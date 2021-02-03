﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using ReportGenerator.Models;
using ReportGenerator.Repositories;

namespace ReportGenerator.Controllers
{
    [ApiController]
    public class GenerateController : BaseController
    {
        public GenerateController(IConfiguration configuration) : base(configuration)
        {
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/odt")]
        public async Task<IActionResult?> GetOdt(string instanceName, string schemaName, string templateName)
        {
            return await GenerateTemplate(instanceName, schemaName, templateName, "odt");
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/pdf")]
        public async Task<IActionResult?> GetPdf(string instanceName, string schemaName, string templateName)
        {
            return await GenerateTemplate(instanceName, schemaName, templateName, "pdf");
        }

        private async Task<IActionResult?> GenerateTemplate(string instanceName, string schemaName, string templateName,
            string? format)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                throw new Exception("No InstanceName specified");
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                throw new Exception("No SchemaName specified");
            }

            if (string.IsNullOrEmpty(templateName))
            {
                throw new Exception("No TemplateName specified");
            }

            var paramsWithValues = new Dictionary<string, object>();
            var requestQuery = HttpContext.Request.Query;
            foreach (var queryParam in requestQuery)
            {
                paramsWithValues.Add(queryParam.Key, queryParam.Value.ToString());
            }

            ReportTemplate? template = null;
            using (var repository = new ReportTemplateRepository(configuration, instanceName))
            {
                template = await repository.LoadTemplate(schemaName, templateName);
            }

            if (template == null)
            {
                return NotFound("Template '" + templateName + "' not found");
            }

            var tokenProcessor = CreateTokenProcessor();
            var funDbApiConnector = new FunDbApi.FunDbApiConnector(configuration, instanceName, tokenProcessor);
            var generatedReport =
                await ReportTemplateFunctions.GenerateReport(funDbApiConnector, template, paramsWithValues);
            if (generatedReport != null)
            {
                FileContentResult? result = null;
                if (format == "odt")
                {
                    byte[] bytes;
                    await using (var stream = new MemoryStream())
                    {
                        await generatedReport.SaveAsync(stream);
                        bytes = stream.ToArray();
                    }
                    result = new FileContentResult(bytes,
                        new MediaTypeHeaderValue("application/vnd.oasis.opendocument.text"))
                    {
                        FileDownloadName = templateName + ".odt"
                    };
                }
                else if (format == "pdf")
                {
                    var fileName = instanceName + "_" + schemaName + "_" +  templateName;
                    var odtFilePath = Path.GetTempPath() + fileName + ".odt";
                    var pdfFilePath = Path.GetTempPath() + fileName + ".pdf";
                    await generatedReport.SaveAsync(odtFilePath);
                    if (!System.IO.File.Exists(odtFilePath))
                        throw new Exception("File " + odtFilePath + " was not created");
                    var response = FormatConverter.OdtToPdf(configuration, odtFilePath);
                    if (!System.IO.File.Exists(pdfFilePath))
                        throw new Exception("File " + pdfFilePath + " was not created. Error message: " + response);
                    byte[] bytesPdf = await System.IO.File.ReadAllBytesAsync(pdfFilePath);
                    result = new FileContentResult(bytesPdf,
                        new MediaTypeHeaderValue("application/pdf"))
                    {
                        FileDownloadName = templateName + ".pdf"
                    };
                    System.IO.File.Delete(odtFilePath);
                    System.IO.File.Delete(pdfFilePath);
                }
                else 
                    throw new Exception("Unsupported file format: " + format);
                
                return result;
            }
            return null;
        }
    }
}
