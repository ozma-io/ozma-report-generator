using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ReportGenerator.Models;
using ReportGenerator.Repositories;

namespace ReportGenerator.Controllers
{
    [ApiController]
    public class GenerateController : BaseController
    {
        private readonly ILogger<GenerateController> _logger;

        public GenerateController(ILogger<GenerateController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("api/{instanceName}/{templateName}/generate")]
        public async Task<FileResult?> Get(string instanceName, string templateName, int? count, string? format)
        {
            return await GenerateTemplate(instanceName, templateName, count, format);
        }

        private async Task<FileResult?> GenerateTemplate(string instanceName, string templateName, int? count, string? format)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                throw new Exception("No InstanceName specified");
            }
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
            using (var repository = new ReportTemplateRepository(instanceName))
            {
                template = await repository.LoadTemplate(templateName);
            }
            if (template == null)
            {
                throw new Exception("Template '" + templateName + "' not found");
            }

            var token = await GetToken();
            var generatedReport = await ReportTemplateFunctions.GenerateReport(template, paramsWithValues, instanceName, token);
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
