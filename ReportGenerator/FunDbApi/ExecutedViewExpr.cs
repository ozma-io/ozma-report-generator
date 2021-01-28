namespace ReportGenerator.FunDbApi
{
    public class ExecutedViewExpr
    {
        public AttributesMap attributes { get; set; } = null!;
        public AttributesMap[] columnAttributes { get; set; } = null!;
        public ExecutedRow[] rows { get; set; } = null!;
    }
}
