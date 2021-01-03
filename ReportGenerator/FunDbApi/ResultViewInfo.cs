using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class ResultViewInfo
    {
        public AttributeTypesMap attributeTypes { get; set; }
        public AttributeTypesMap rowAttributeTypes { get; set; }
        public dynamic domains { get; set; }
        public EntityRef? mainEntity { get; set; }
        public ResultColumnInfo[] columns { get; set; }
    }
}
