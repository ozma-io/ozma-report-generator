using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class ExecutedRow
    {
        public ExecutedValue[] values { get; set; } = null!; // Значения в строке.
        public int domainId { get; set; }
        public AttributesMap? attributes { get; set; }
        public dynamic entityIds { get; set; } = null!;
        public int? mainId { get; set; }
        public EntityRef? mainSubEntity { get; set; }
    }
}
