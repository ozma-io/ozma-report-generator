using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator.Models
{
    public partial class ReportGeneratorContext : DbContext
    {
        private readonly IConfiguration configuration;

        public ReportGeneratorContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //public ReportGeneratorContext(DbContextOptions<ReportGeneratorContext> options)
        //    : base(options)
        //{
        //}


        public virtual DbSet<Instance> Instances { get; set; } = null!;
        public virtual DbSet<ReportTemplate> ReportTemplates { get; set; } = null!;
        public virtual DbSet<ReportTemplateQuery> ReportTemplateQueries { get; set; } = null!;
        public virtual DbSet<ReportTemplateSchema> ReportTemplateSchemas { get; set; } = null!;
        public virtual DbSet<VReportTemplate> VReportTemplates { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(configuration.GetConnectionString("postgreSql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Russian_Russia.1251");

            modelBuilder.Entity<Instance>(entity =>
            {
                entity.HasIndex(e => e.Name, "instances_name_uindex")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ReportTemplate>(entity =>
            {
                entity.HasIndex(e => new { e.Name, e.SchemaId }, "reporttemplates_name_schemaid_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.SchemaId, "reporttemplates_schemaid_index");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OdtWithoutQueries).IsRequired();

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.ReportTemplates)
                    .HasForeignKey(d => d.SchemaId)
                    .HasConstraintName("reporttemplates_reporttemplateschemas_id_fk");
            });

            modelBuilder.Entity<ReportTemplateQuery>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.QueryText)
                    .IsRequired()
                    .HasColumnType("character varying");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.ReportTemplateQueries)
                    .HasForeignKey(d => d.TemplateId)
                    .HasConstraintName("reporttemplatequeries_reporttemplates_id_fk");
            });

            modelBuilder.Entity<ReportTemplateSchema>(entity =>
            {
                entity.HasIndex(e => e.InstanceId, "reporttemplateschemas_instanceid_index");

                entity.HasIndex(e => new { e.Name, e.InstanceId }, "reporttemplateschemas_name_instanceid_uindex")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Instance)
                    .WithMany(p => p.ReportTemplateSchemas)
                    .HasForeignKey(d => d.InstanceId)
                    .HasConstraintName("reporttemplateschemas_instances_id_fk");
            });

            modelBuilder.Entity<VReportTemplate>(entity =>
            {
                entity.ToTable("ReportTemplates");
                entity.HasOne(o => o.ReportTemplate).WithOne()
                    .HasForeignKey<ReportTemplate>(o => o.Id);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
