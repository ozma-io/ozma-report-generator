using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportGenerator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator.Repositories
{
    public class ReportTemplateSchemaRepository : Repository
    {
        public ReportTemplateSchemaRepository(IConfiguration configuration, string instanceName) : base(configuration,
            instanceName)
        {

        }

        public async Task AddSchema(ReportTemplateSchema reportTemplateSchema)
        {
            reportTemplateSchema.Instance = instance;
            await dbContext.ReportTemplateSchemas.AddAsync(reportTemplateSchema);
            await dbContext.SaveChangesAsync();
        }

        public async Task<ReportTemplateSchema?> LoadSchema(int id)
        {
            return await dbContext.ReportTemplateSchemas.AsNoTracking()
                .FirstOrDefaultAsync(p => (p.InstanceId == instance.Id) && (p.Id == id));
        }

        public async Task<List<ReportTemplateSchema>> LoadAllSchemas()
        {
            return await dbContext.ReportTemplateSchemas.Where(p => p.InstanceId == instance.Id).AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteSchema(int id)
        {
            var item = dbContext.ReportTemplateSchemas.FirstOrDefault(
                p => (p.InstanceId == instance.Id) && (p.Id == id));
            if (item != null)
            {
                dbContext.Remove(item);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
