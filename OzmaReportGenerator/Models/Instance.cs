using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class Instance
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public IList<ReportTemplateSchema> ReportTemplateSchemas { get; set; } = new List<ReportTemplateSchema>();
    }
}
