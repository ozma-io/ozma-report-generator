using System;
using System.Collections.Generic;

#nullable disable

namespace ReportGenerator.Models
{
    public partial class VReportTemplate
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? SchemeId { get; set; }
        public string Parameters { get; set; }
        public string SchemeName { get; set; }
    }
}
