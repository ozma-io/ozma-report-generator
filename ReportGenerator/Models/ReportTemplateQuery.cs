using System;
using System.Collections.Generic;

#nullable disable

namespace ReportGenerator.Models
{
    public partial class ReportTemplateQuery
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string QueryText { get; set; }
        public string Name { get; set; }

        public virtual ReportTemplate Template { get; set; }
    }
}
