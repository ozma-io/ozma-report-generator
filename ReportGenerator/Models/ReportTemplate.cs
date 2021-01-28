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
        public int SchemaId { get; set; }
        public string Name { get; set; } = null!;
        public byte[] OdtWithoutQueries { get; set; } = null!;

        public virtual ReportTemplateSchema Schema { get; set; } = null!;
        public virtual ICollection<ReportTemplateQuery> ReportTemplateQueries { get; set; }
    }
}
