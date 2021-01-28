namespace ReportGenerator.Models
{
    public partial class VReportTemplate
    {
        public int? Id { get; set; }
        public string Name { get; set; } = null!;
        public int? SchemaId { get; set; }
        public string SchemaName { get; set; } = null!;
        public int? InstanceId { get; set; }
    }
}
