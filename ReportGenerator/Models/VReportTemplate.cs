namespace ReportGenerator.Models
{
    public partial class VReportTemplate
    {
        public int Id { get; set; }
        public int SchemaId { get; set; }
        public string Name { get; set; } = null!;

        public ReportTemplateSchema Schema { get; set; } = null!;
    }
}
