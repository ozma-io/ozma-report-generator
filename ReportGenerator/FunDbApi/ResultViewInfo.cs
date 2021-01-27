using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class ResultViewInfo
    {
        public AttributeTypesMap attributeTypes { get; set; } = null!;
        public AttributeTypesMap rowAttributeTypes { get; set; } = null!;
        public dynamic domains { get; set; } = null!;
        public EntityRef? mainEntity { get; set; }
        public ResultColumnInfo[] columns { get; set; } = null!;
    }
}
