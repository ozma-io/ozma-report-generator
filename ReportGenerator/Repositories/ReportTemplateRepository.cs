using System;
using System.Threading.Tasks;
using Sandwych.Reporting.OpenDocument;

namespace ReportGenerator.Repositories
{
    public class ReportTemplateRepository : IDisposable
    {
        public void Dispose()
        {
            //ToDo
        }

        public async Task SaveTemplate(ReportTemplate template, OdfDocument odtWithQueries)
        {
            //ToDo: сохранять в бд
            var odtWithoutQueries = ReportTemplate.RemoveQueriesFromOdt(odtWithQueries);
            await odtWithoutQueries.SaveAsync("template_without_queries.odt");
        }

        public async Task<OdfDocument> LoadOdtWithoutQueries(ReportTemplate template)
        {
            //ToDo: загружать из бд
            return await OdfDocument.LoadFromAsync("template_without_queries.odt");
        }
    }
}
