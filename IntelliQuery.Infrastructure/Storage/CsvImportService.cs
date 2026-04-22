using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Globalization;
using CsvHelper;
using IntelliQuery.Application.Abstractions;
using IntelliQuery.Domain.Entities;
using IntelliQuery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntelliQuery.Infrastructure.Storage
{
    public class CsvImportService : ICsvImportService
    {
        private readonly AppDbContext _dbContext;

        public CsvImportService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ImportAsync(Guid datasetId, Guid importJobId, string filePath, CancellationToken cancellationToken = default)
        {
            var importJob = await _dbContext.ImportJobs.FirstOrDefaultAsync(x => x.Id == importJobId, cancellationToken);

            if (importJob is null)
            {
                throw new InvalidOperationException("Import job not found.");
            }

            try
            {
                importJob.Status = "Processing";
                await _dbContext.SaveChangesAsync(cancellationToken);

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                if (headers is null || headers.Length == 0)
                {
                    throw new InvalidOperationException("CSV file does not contain headers.");
                }

                var samples = new List<Dictionary<string, string?>>();
                var datasetRows = new List<DatasetRow>();
                var rowNumber = 1;
                const int sampleSize = 25;

                while (await csv.ReadAsync())
                {
                    var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

                    foreach (var header in headers)
                    {
                        row[header] = csv.GetField(header);
                    }

                    if (samples.Count < sampleSize)
                    {
                        samples.Add(new Dictionary<string, string?>(row, StringComparer.OrdinalIgnoreCase));
                    }

                    datasetRows.Add(new DatasetRow
                    {
                        Id = Guid.NewGuid(),
                        DatasetId = datasetId,
                        RowNumber = rowNumber++,
                        JsonData = JsonSerializer.Serialize(row),
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }

                var existingColumns = await _dbContext.DatasetColumns
                    .Where(x => x.DatasetId == datasetId)
                    .ToListAsync(cancellationToken);

                if (existingColumns.Count > 0)
                {
                    _dbContext.DatasetColumns.RemoveRange(existingColumns);
                }

                var existingRows = await _dbContext.DatasetRows
                    .Where(x => x.DatasetId == datasetId)
                    .ToListAsync(cancellationToken);

                if (existingRows.Count > 0)
                {
                    _dbContext.DatasetRows.RemoveRange(existingRows);
                }

                var columns = headers.Select(header =>
                {
                    var values = samples
                        .Select(x => x.TryGetValue(header, out var value) ? value : null)
                        .ToList();

                    var inferredType = InferDataType(values);
                    var isNullable = values.Any(string.IsNullOrWhiteSpace);

                    double? minValue = null;
                    double? maxValue = null;

                    if (inferredType == "number")
                    {
                        var numericValues = values
                            .Where(v => double.TryParse(v, out _))
                            .Select(v => double.Parse(v!))
                            .ToList();

                        if (numericValues.Count > 0)
                        {
                            minValue = numericValues.Min();
                            maxValue = numericValues.Max();
                        }
                    }

                    return new DatasetColumn
                    {
                        Id = Guid.NewGuid(),
                        DatasetId = datasetId,
                        Name = header,
                        DataType = inferredType,
                        IsNullable = isNullable,
                        MinValue = minValue,
                        MaxValue = maxValue
                    };
                }).ToList();

                _dbContext.DatasetColumns.AddRange(columns);
                _dbContext.DatasetRows.AddRange(datasetRows);

                importJob.Status = "Completed";
                importJob.CompletedAtUtc = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                importJob.Status = "Failed";
                importJob.ErrorMessage = ex.Message;
                importJob.CompletedAtUtc = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);
                throw;
            }
        }

        private static string InferDataType(IEnumerable<string?> values)
        {
            var nonEmptyValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            if (nonEmptyValues.Count == 0)
            {
                return "string";
            }

            if (nonEmptyValues.All(v => double.TryParse(v, out _)))
            {
                return "number";
            }

            if (nonEmptyValues.All(v => DateTime.TryParse(v, out _)))
            {
                return "datetime";
            }

            if (nonEmptyValues.All(v => bool.TryParse(v, out _)))
            {
                return "boolean";
            }

            return "string";
        }
    }
}
