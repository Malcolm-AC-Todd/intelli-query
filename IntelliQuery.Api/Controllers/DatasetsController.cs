using IntelliQuery.Application.Abstractions;
using IntelliQuery.Application.Contracts_Uploads;
using IntelliQuery.Domain.Entities;
using IntelliQuery.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliQuery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatasetsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICsvImportService _csvImportService;

        public DatasetsController(AppDbContext dbContext, IFileStorageService fileStorageService, ICsvImportService csvImportService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
            _csvImportService = csvImportService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<UploadDatasetResponse>> Upload(
            [FromForm] UploadDatasetRequest request,
            CancellationToken cancellationToken)
        {
            var file = request.File;

            if (file is null || file.Length == 0)
            {
                return BadRequest("A file is required.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CSV files are supported.");
            }

            var dataset = new Dataset
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileNameWithoutExtension(file.FileName),
                CreatedAtUtc = DateTime.UtcNow
            };

            await using var fileStream = file.OpenReadStream();
            var storagePath = await _fileStorageService.SaveFileAsync(
                fileStream,
                file.FileName,
                dataset.Id,
                cancellationToken);

            var datasetFile = new DatasetFile
            {
                Id = Guid.NewGuid(),
                DatasetId = dataset.Id,
                FileName = file.FileName,
                StoragePath = storagePath,
                UploadedAtUtc = DateTime.UtcNow
            };

            var importJob = new ImportJob
            {
                Id = Guid.NewGuid(),
                DatasetId = dataset.Id,
                Status = "Pending",
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Datasets.Add(dataset);
            _dbContext.DatasetFiles.Add(datasetFile);
            _dbContext.ImportJobs.Add(importJob);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _csvImportService.ImportAsync(dataset.Id, importJob.Id, storagePath, cancellationToken);

            var response = new UploadDatasetResponse
            {
                DatasetId = dataset.Id,
                ImportJobId = importJob.Id,
                FileName = file.FileName,
                Message = "Dataset uploaded successfully. Import job created."
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetAll(CancellationToken cancellationToken)
        {
            var datasets = await _dbContext.Datasets
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(datasets);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<object>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var dataset = await _dbContext.Datasets
                .Include(x => x.Columns)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (dataset is null)
            {
                return NotFound();
            }

            var importJob = await _dbContext.ImportJobs
                .Where(x => x.DatasetId == id)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            return Ok(new
            {
                dataset.Id,
                dataset.Name,
                dataset.CreatedAtUtc,
                ImportJob = importJob is null ? null : new
                {
                    importJob.Id,
                    importJob.Status,
                    importJob.ErrorMessage,
                    importJob.CreatedAtUtc,
                    importJob.CompletedAtUtc
                },
                Columns = dataset.Columns
                    .OrderBy(x => x.Name)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.DataType,
                        x.IsNullable,
                        x.MinValue,
                        x.MaxValue
                    })
            });
        }

        [HttpGet("{id:guid}/rows")]
        public async Task<ActionResult<object>> GetRows(Guid id, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
        {
            if (take <= 0)
            {
                take = 50;
            }

            if (take > 200)
            {
                take = 200;
            }

            var datasetExists = await _dbContext.Datasets
                .AnyAsync(x => x.Id == id, cancellationToken);

            if (!datasetExists)
            {
                return NotFound();
            }

            var rows = await _dbContext.DatasetRows
                .Where(x => x.DatasetId == id)
                .OrderBy(x => x.RowNumber)
                .Take(take)
                .ToListAsync(cancellationToken);

            var result = rows.Select(x => new
            {
                x.RowNumber,
                Data = JsonSerializer.Deserialize<Dictionary<string, object?>>(x.JsonData)
            });

            return Ok(result);
        }

        [HttpPost("{id:guid}/query")]
        public async Task<ActionResult<DatasetQueryResponse>> QueryDataset(Guid id, [FromBody] DatasetQueryRequest request, CancellationToken cancellationToken)
        {
            var datasetExists = await _dbContext.Datasets
                .AnyAsync(x => x.Id == id, cancellationToken);

            if (!datasetExists)
            {
                return NotFound();
            }

            var rows = await _dbContext.DatasetRows
                .Where(x => x.DatasetId == id)
                .OrderBy(x => x.RowNumber)
                .ToListAsync(cancellationToken);

            var data = rows
                .Select(x => JsonSerializer.Deserialize<Dictionary<string, string?>>(x.JsonData))
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            var queryType = request.QueryType.Trim().ToLowerInvariant();

            return queryType switch
            {
                "row_count" => Ok(new DatasetQueryResponse
                {
                    QueryType = queryType,
                    Result = new
                    {
                        Count = data.Count
                    }
                }),

                "sum_by_group" => Ok(new DatasetQueryResponse
                {
                    QueryType = queryType,
                    Result = SumByGroup(data, request.GroupByColumn, request.ValueColumn)
                }),

                "top_numeric" => Ok(new DatasetQueryResponse
                {
                    QueryType = queryType,
                    Result = TopNumeric(data, request.ValueColumn, request.Take)
                }),

                _ => BadRequest($"Unsupported query type: {request.QueryType}")
            };
        }

        //Helper functions for query processing
        private static object SumByGroup(List<Dictionary<string, string?>> rows, string? groupByColumn, string? valueColumn)
        {
            if (string.IsNullOrWhiteSpace(groupByColumn))
            {
                return new { Error = "GroupByColumn is required." };
            }

            if (string.IsNullOrWhiteSpace(valueColumn))
            {
                return new { Error = "ValueColumn is required." };
            }

            var result = rows
                .Where(row => row.ContainsKey(groupByColumn) && row.ContainsKey(valueColumn))
                .GroupBy(row =>
                {
                    var groupValue = row[groupByColumn];
                    return string.IsNullOrWhiteSpace(groupValue) ? "(blank)" : groupValue;
                })
                .Select(group => new
                {
                    Group = group.Key,
                    Sum = group.Sum(row =>
                    {
                        var value = row[valueColumn];
                        return double.TryParse(value, out var number) ? number : 0;
                    })
                })
                .OrderByDescending(x => x.Sum)
                .ToList();

            return result;
        }

        private static object TopNumeric(List<Dictionary<string, string?>> rows, string? valueColumn, int take)
        {
            if (string.IsNullOrWhiteSpace(valueColumn))
            {
                return new { Error = "ValueColumn is required." };
            }

            if (take <= 0)
            {
                take = 5;
            }

            if (take > 50)
            {
                take = 50;
            }

            var result = rows
                .Where(row => row.ContainsKey(valueColumn))
                .Select(row => new
                {
                    Row = row,
                    Value = double.TryParse(row[valueColumn], out var number) ? number : (double?)null
                })
                .Where(x => x.Value.HasValue)
                .OrderByDescending(x => x.Value)
                .Take(take)
                .Select(x => new
                {
                    x.Value,
                    Data = x.Row
                })
                .ToList();

            return result;
        }
    }
}
