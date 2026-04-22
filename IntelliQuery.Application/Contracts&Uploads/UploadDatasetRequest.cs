using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace IntelliQuery.Application.Contracts_Uploads
{
    public class UploadDatasetRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
