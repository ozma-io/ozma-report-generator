using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class Instance
    {
        public Instance()
        {
            ReportTemplateSchemas = new HashSet<ReportTemplateSchema>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<ReportTemplateSchema> ReportTemplateSchemas { get; set; }
    }
}
