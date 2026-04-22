using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Domain.Entities
{
    internal class ImportJob
    {
        public Guid Id { get; set; }
        public Guid DatasetId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}
