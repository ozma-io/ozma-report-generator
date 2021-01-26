using System;
using System.Collections.Generic;


namespace ReportGenerator.Models
{
    public partial class Instance
    {
        public Instance()
        {
            ReportTemplateSchemes = new HashSet<ReportTemplateScheme>();
            ReportTemplates = new HashSet<ReportTemplate>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ReportTemplateScheme> ReportTemplateSchemes { get; set; }
        public virtual ICollection<ReportTemplate> ReportTemplates { get; set; }
    }
}
