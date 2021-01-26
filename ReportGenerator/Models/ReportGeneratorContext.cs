using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator.Models
{
    public partial class ReportGeneratorContext : DbContext
    {
        public ReportGeneratorContext()
        {
        }

        public ReportGeneratorContext(DbContextOptions<ReportGeneratorContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Instance> Instances { get; set; }
        public virtual DbSet<ReportTemplate> ReportTemplates { get; set; }
        public virtual DbSet<ReportTemplateQuery> ReportTemplateQueries { get; set; }
        public virtual DbSet<ReportTemplateScheme> ReportTemplateSchemes { get; set; }
        public virtual DbSet<VReportTemplate> VReportTemplates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
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
                entity.HasIndex(e => e.InstanceId, "reporttemplates_instanceid_index");

                entity.HasIndex(e => new { e.Name, e.InstanceId, e.SchemeId }, "reporttemplates_name_instanceid_schemeid_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.SchemeId, "reporttemplates_schemeid_index");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OdtWithoutQueries).IsRequired();

                entity.Property(e => e.Parameters).HasColumnType("character varying");

                entity.HasOne(d => d.Instance)
                    .WithMany(p => p.ReportTemplates)
                    .HasForeignKey(d => d.InstanceId)
                    .HasConstraintName("reporttemplates_instances_id_fk");

                entity.HasOne(d => d.Scheme)
                    .WithMany(p => p.ReportTemplates)
                    .HasForeignKey(d => d.SchemeId)
                    .HasConstraintName("reporttemplates_reporttemplateschemes_id_fk");
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

            modelBuilder.Entity<ReportTemplateScheme>(entity =>
            {
                entity.HasIndex(e => e.InstanceId, "reporttemplateschemes_instanceid_index");

                entity.HasIndex(e => new { e.Name, e.InstanceId }, "reporttemplateschemes_name_instanceid_uindex")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Instance)
                    .WithMany(p => p.ReportTemplateSchemes)
                    .HasForeignKey(d => d.InstanceId)
                    .HasConstraintName("reporttemplateschemes_instances_id_fk");
            });

            modelBuilder.Entity<VReportTemplate>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("vReportTemplates");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Parameters).HasColumnType("character varying");

                entity.Property(e => e.SchemeName).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
