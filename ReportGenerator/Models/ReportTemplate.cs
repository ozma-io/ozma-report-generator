using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class ReportTemplate
    {
        public int Id { get; set; }
        public int SchemaId { get; set; }
        public string Name { get; set; } = null!;
        public byte[] OdtWithoutQueries { get; set; } = null!;

        public ReportTemplateSchema Schema { get; set; } = null!;
        public ICollection<ReportTemplateQuery> ReportTemplateQueries { get; set; } = null!;
    }
}
