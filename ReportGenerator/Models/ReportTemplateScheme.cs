using System;
using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class ReportTemplateScheme
    {
        public ReportTemplateScheme()
        {
            ReportTemplates = new HashSet<ReportTemplate>();
        }

        public int Id { get; set; }
        public int InstanceId { get; set; }
        public string Name { get; set; } = null!;

        public virtual Instance Instance { get; set; } = null!;
        public virtual ICollection<ReportTemplate> ReportTemplates { get; set; }
    }
}
