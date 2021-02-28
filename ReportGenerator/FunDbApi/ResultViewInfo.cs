using System.Dynamic;

namespace ReportGenerator.FunDbApi
{
    public class ResultViewInfo
    {
        public AttributeTypesMap attributeTypes { get; set; } = null!;
        public AttributeTypesMap rowAttributeTypes { get; set; } = null!;
        public ExpandoObject domains { get; set; } = null!;
        public EntityRef? mainEntity { get; set; }
        public ResultColumnInfo[] columns { get; set; } = null!;
    }
}
