using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class ExecutedViewExpr
    {
        public AttributesMap attributes { get; set; }
        public AttributesMap[] columnAttributes { get; set; }
        public ExecutedRow[] rows { get; set; }
    }
}
