using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Domain.Entities
{
    public class Dataset
    {
        public Guid Id { get; set; }
        public string Name {  get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }

        // Navigation properties
        public ICollection<DatasetFile> Files { get; set; } = new List<DatasetFile>();
        public ICollection<DatasetColumn> Columns { get; set; } = new List<DatasetColumn>();

    }
}
