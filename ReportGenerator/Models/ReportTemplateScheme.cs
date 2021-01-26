﻿using System;
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
        public string Name { get; set; }

        public virtual Instance Instance { get; set; }
        public virtual ICollection<ReportTemplate> ReportTemplates { get; set; }
    }
}
