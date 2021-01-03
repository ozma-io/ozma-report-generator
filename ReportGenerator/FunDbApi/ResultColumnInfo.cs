using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class ResultColumnInfo
    {
        public string name { get; set; }
        public AttributeTypesMap attributeTypes { get; set; }
        public AttributeTypesMap cellAttributeTypes { get; set; }
        public dynamic valueType { get; set; }
        public dynamic? punType { get; set; }
        public MainFieldInfo mainField { get; set; }
}
}
