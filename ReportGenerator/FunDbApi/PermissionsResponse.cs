using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class PermissionsResponse
    {
        public bool IsAdmin { get; set; }
        public PermissionsResponseJson? ResponseJson { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
    }
}
