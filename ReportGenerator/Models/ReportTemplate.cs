using System;
using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class ReportTemplate
    {
        public ReportTemplate()
        {
            ReportTemplateQueries = new HashSet<ReportTemplateQuery>();
        }

        public int Id { get; set; }
        public int InstanceId { get; set; }
        public int SchemeId { get; set; }
        public string Name { get; set; } = null!;
        public string Parameters { get; set; } = null!;
        public byte[] OdtWithoutQueries { get; set; } = null!;

        public virtual Instance Instance { get; set; } = null!;
        public virtual ReportTemplateScheme Scheme { get; set; } = null!;
        public virtual ICollection<ReportTemplateQuery> ReportTemplateQueries { get; set; }
    }
}
