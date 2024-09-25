using System.Collections.Generic;
using ReportGenerator.OzmaDBApi;

namespace ReportGenerator
{
    public class TemplateExpression
    {
        public string QueryName { get; set; } = null!;
        public QueryType QueryType { get; set; }
        public List<string> FieldNames { get; set; }
        public string? SubQueryName { get; set; }

        public TemplateExpression()
        {
            FieldNames = new List<string>();
        }
    }
}
