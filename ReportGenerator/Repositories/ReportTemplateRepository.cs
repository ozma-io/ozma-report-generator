using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ReportGenerator.Models;

namespace ReportGenerator.Repositories
{
    public class ReportTemplateRepository : Repository
    {
        public ReportTemplateRepository(IConfiguration configuration, string instanceName) : base(configuration,
            instanceName)
        {

        }

        public async Task AddTemplate(ReportTemplate template)
        {
            await dbContext.ReportTemplates.AddAsync(template);
            await dbContext.SaveChangesAsync();
        }
        
        public async Task<List<VReportTemplate>> LoadAllTemplates()
        {
            return await dbContext.VReportTemplates.Where(p => p.InstanceId == instance.Id).AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteTemplate(int id)
        {
            var item = dbContext.ReportTemplates.FirstOrDefault(p =>
                (p.Schema.InstanceId == instance.Id) && (p.Id == id));
            if (item != null)
            {
                dbContext.Remove(item);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<ReportTemplate?> LoadTemplate(string schemaName, string templateName)
        {
            return await dbContext.ReportTemplates.AsNoTracking().Include(p => p.ReportTemplateQueries)
                .FirstOrDefaultAsync(p =>
                    (p.Schema.InstanceId == instance.Id) && (p.Schema.Name == schemaName) && (p.Name == templateName));
        }
    }
}
