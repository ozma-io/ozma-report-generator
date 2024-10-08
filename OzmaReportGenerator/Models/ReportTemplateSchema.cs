﻿using System.Collections.Generic;

namespace ReportGenerator.Models
{
    public partial class ReportTemplateSchema
    {
        public int Id { get; set; }
        public int InstanceId { get; set; }
        public string Name { get; set; } = null!;

        public virtual Instance Instance { get; set; } = null!;
        public virtual IList<ReportTemplate> ReportTemplates { get; set; } = new List<ReportTemplate>();
    }
}
