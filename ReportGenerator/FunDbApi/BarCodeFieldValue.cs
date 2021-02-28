namespace ReportGenerator.FunDbApi
{
    public class BarCodeFieldValue
    {
        public string QueryFieldName { get; set; } = null!;
        public BarCodeType CodeType { get; set; }
        public string ValueToEncode { get; set; } = null!;
        public string FieldValue { get; set; } = null!;
    }
}
