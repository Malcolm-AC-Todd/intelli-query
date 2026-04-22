using IntelliQuery.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _environment;

        public LocalFileStorageService(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid datasetId, CancellationToken cancellationToken = default)
        {
            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads");
            var datasetFolder = Path.Combine(uploadsRoot, datasetId.ToString());

            Directory.CreateDirectory(datasetFolder);

            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(datasetFolder, safeFileName);

            await using var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(outputStream, cancellationToken);

            return filePath;
        }
    }
}
