using System;
using System.Collections.Generic;

#nullable disable

namespace ReportGenerator.Models
{
    public partial class ReportTemplate
    {
        public ReportTemplate()
        {
            ReportTemplateQueries = new HashSet<ReportTemplateQuery>();
        }

        public int Id { get; set; }
        public int SchemeId { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public byte[] OdtWithoutQueries { get; set; }

        public virtual ReportTemplateScheme Scheme { get; set; }
        public virtual ICollection<ReportTemplateQuery> ReportTemplateQueries { get; set; }
    }
}
