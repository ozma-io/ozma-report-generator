using System.Dynamic;

namespace ReportGenerator.OzmaDBApi
{
    public class ExecutedValue
    {
        public dynamic value { get; set; } = null!; // Значение в ячейке.
        public ExpandoObject? attributes { get; set; }
        public dynamic? pun { get; set; }
    }
}
