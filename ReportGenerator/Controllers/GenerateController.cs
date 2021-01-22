using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ReportGenerator.FunDbApi;
using ReportGenerator.Models;
using ReportGenerator.Repositories;
using Sandwych.Reporting.OpenDocument;

namespace ReportGenerator.Controllers
{
    [ApiController]
    public class GenerateController : ControllerBase
    {
        private readonly ILogger<GenerateController> _logger;

        public GenerateController(ILogger<GenerateController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("api/test")]
        public async Task Get()
        {
            //var apiConnector = new FunDbApiConnector();
            //var q = await apiConnector.LoadQueryAnonymous("{$id int, $month int, $year int }: SELECT \"transaction_date\",\"account_from\"=>contact => main_name as contact_from,\"account_to\"=>contact => main_name as contact_to,\"amount\",\"name\" FROM fin.transactions WHERE NOT is_deleted AND transactions.account_to=>contact = $id AND date_part('month', transaction_date) = $month AND date_part('year', transaction_date) = $year ORDER BY transaction_date, id",
            //    new Dictionary<string, object>
            //    {
            //        {"id", 45},
            //        {"month", 6},
            //        {"year", 2020},
            //    });
            var odtWithQueries = await OdfDocument.LoadFromAsync(@"d:\\template_with_queries.odt");
            var queries = OpenDocumentTextFunctions.GetQueriesFromOdt(odtWithQueries);
        }

        [HttpGet]
        [Route("api/{templateName}/generate")]
        public async Task<FileResult?> Get(string templateName, int? count, string? format)
        {
            return await GenerateTemplate(templateName, count, format);
        }

        private async Task<FileResult?> GenerateTemplate(string templateName, int? count, string? format)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                throw new Exception("No TemplateName specified");
            }

            var paramsWithValues = new Dictionary<string, object>();
            var requestQuery = HttpContext.Request.Query;
            foreach (var queryParam in requestQuery)
            {
                if ((queryParam.Key != "templateName") && (queryParam.Key != "format") && (queryParam.Key != "count"))
                    paramsWithValues.Add(queryParam.Key, queryParam.Value.ToString());
            }

            ReportTemplate? template = null;
            using (var repository = new ReportTemplateRepository())
            {
                template = await repository.LoadTemplate(templateName);
            }
            if (template == null)
            {
                throw new Exception("Template '" + templateName + "' not found");
            }

            var generatedReport = await ReportTemplateFunctions.GenerateReport(template, paramsWithValues);
            if (generatedReport != null)
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await generatedReport.SaveAsync(stream);
                    bytes = stream.ToArray();
                }

                FileContentResult? result = null;
                if (format == "pdf") 
                {
                    byte[] bytesPdf;
                    await using (var stream = new MemoryStream(bytes))
                    {
                        var html = FormatConverter.OdtToHtml(stream);
                        bytesPdf = FormatConverter.HtmlToPdf(html);
                    }
                    result = new FileContentResult(bytesPdf,
                        new MediaTypeHeaderValue("application/pdf"))
                    {
                        FileDownloadName = templateName +".pdf"
                    };
                }
                else
                {
                    result = new FileContentResult(bytes,
                        new MediaTypeHeaderValue("application/vnd.oasis.opendocument.text"))
                    {
                        FileDownloadName = templateName + ".odt"
                    };
                }
                return result;
            }
            return null;
        }
    }
}
