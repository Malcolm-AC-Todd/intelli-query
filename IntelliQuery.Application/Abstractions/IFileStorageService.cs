using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Application.Abstractions
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid datasetId, CancellationToken cancellationToken = default);
    }
}
