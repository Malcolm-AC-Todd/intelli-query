using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Application.Contracts_Uploads
{
    public class DatasetQueryRequest
    {
        public string QueryType { get; set; } = string.Empty;
        public string? GroupByColumn { get; set; }
        public string? ValueColumn { get; set; }
        public int Take { get; set; } = 5;
    }
}
