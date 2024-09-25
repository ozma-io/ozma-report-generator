namespace ReportGenerator.OzmaDBApi
{
    public class ResultColumnInfo
    {
        public string name { get; set; } = null!;
        public AttributeTypesMap attributeTypes { get; set; } = null!;
        public AttributeTypesMap cellAttributeTypes { get; set; } = null!;
        public dynamic valueType { get; set; } = null!;
        public dynamic? punType { get; set; }
        public MainFieldInfo mainField { get; set; } = null!;
    }
}
