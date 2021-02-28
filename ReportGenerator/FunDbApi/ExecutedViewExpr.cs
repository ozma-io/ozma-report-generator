﻿using System.Dynamic;

namespace ReportGenerator.FunDbApi
{
    public class ExecutedViewExpr
    {
        public ExpandoObject attributes { get; set; } = null!;
        public ExpandoObject[] columnAttributes { get; set; } = null!;
        public ExecutedRow[] rows { get; set; } = null!;
    }
}
