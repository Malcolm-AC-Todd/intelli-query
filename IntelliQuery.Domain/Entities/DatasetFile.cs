using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Domain.Entities
{
    public class DatasetFile
    {
        public Guid ID { get; set; }
        public Guid DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public DateTime UploadedAtUtc { get; set; }
    }
}
