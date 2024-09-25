using System.Dynamic;

namespace ReportGenerator.OzmaDBApi
{
    public class ExecutedRow
    {
        public ExecutedValue[] values { get; set; } = null!; // Значения в строке.
        public int? domainId { get; set; }
        public ExpandoObject? attributes { get; set; }
        public dynamic entityIds { get; set; } = null!;
        public int? mainId { get; set; }
        public EntityRef? mainSubEntity { get; set; }
    }
}
