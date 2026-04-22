using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Domain.Entities
{
    public class DatasetRow
    {
        public Guid Id { get; set; }

        public Guid DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;

        public int RowNumber { get; set; }

        public string JsonData { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
    }
}
