namespace ReportGenerator.Models
{
    public partial class ReportTemplateQuery
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string QueryText { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ReportTemplate Template { get; set; } = null!;
    }
}
