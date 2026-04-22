using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Application.Abstractions
{
    public interface ICsvImportService
    {
        Task ImportAsync(Guid datasetId, Guid importJobId, string filePath, CancellationToken cancellationToken = default);
    }
}
