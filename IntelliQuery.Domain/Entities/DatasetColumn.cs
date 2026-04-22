using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Domain.Entities
{
    public class DatasetColumn
    {
        public Guid Id { get; set; }
        public Guid DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public string Datatype { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
    }
}
