using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class ExecutedValue
    {
        public dynamic value { get; set; } = null!; // Значение в ячейке.
        public AttributesMap? attributes { get; set; }
        public dynamic? pun { get; set; }
}
}
