using System;
using System.Collections.Generic;

#nullable disable

namespace ReportGenerator.Models
{
    public partial class ReportTemplateScheme
    {
        public ReportTemplateScheme()
        {
            ReportTemplates = new HashSet<ReportTemplate>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ReportTemplate> ReportTemplates { get; set; }
    }
}
