namespace ReportGenerator.OzmaDBApi
{
    public class ColumnField
    {
        public dynamic fieldType { get; set; } = null!;
        public dynamic valueType { get; set; } = null!;
        public dynamic defaultValue { get; set; } = null!;
        public bool isNullable { get; set; }
        public bool isImmutable { get; set; }
        public EntityRef? inheritedFrom { get; set; }
    }
}
