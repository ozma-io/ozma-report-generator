using System;
using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class VReportTemplate
    {
        public int? Id { get; set; }
        public string Name { get; set; } = null!;
        public int? SchemeId { get; set; }
        public string Parameters { get; set; } = null!;
        public string SchemeName { get; set; } = null!;
        public int? InstanceId { get; set; }
    }
}
