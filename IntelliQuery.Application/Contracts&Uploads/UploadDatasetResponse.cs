using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Application.Contracts_Uploads
{
    public class UploadDatasetResponse
    {
        public Guid DatasetId { get; set; }
        public Guid ImportJobId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
