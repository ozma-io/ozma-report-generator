using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace ReportGenerator.Repositories
{
    public class ReportTemplateSchemeRepository : Repository
    {
        public ReportTemplateSchemeRepository(string instanceName) : base(instanceName)
        {

        }

        public async Task AddScheme(ReportTemplateScheme reportTemplateScheme)
        {
            reportTemplateScheme.Instance = instance;
            await dbContext.ReportTemplateSchemes.AddAsync(reportTemplateScheme);
            await dbContext.SaveChangesAsync();
        }

        public async Task<ReportTemplateScheme?> LoadScheme(int id)
        {
            return await dbContext.ReportTemplateSchemes.AsNoTracking()
                .FirstOrDefaultAsync(p => (p.InstanceId == instance.Id) && (p.Id == id));
        }

        public async Task<List<ReportTemplateScheme>> LoadAllSchemes()
        {
            return await dbContext.ReportTemplateSchemes.Where(p => p.InstanceId == instance.Id).AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteScheme(int id)
        {
            var item = dbContext.ReportTemplateSchemes.FirstOrDefault(
                p => (p.InstanceId == instance.Id) && (p.Id == id));
            if (item != null)
            {
                dbContext.Remove(item);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
