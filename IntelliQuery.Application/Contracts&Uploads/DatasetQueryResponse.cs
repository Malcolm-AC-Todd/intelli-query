using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliQuery.Application.Contracts_Uploads
{
    public class DatasetQueryResponse
    {
        public string QueryType { get; set; } = string.Empty;
        public object Result { get; set; } = new();
    }
}
