using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using ReportGenerator.FunDbApi;
using ReportGenerator.Models;
using ReportGenerator.Repositories;
using Sandwych.Reporting;
using Sandwych.Reporting.OpenDocument;

namespace ReportGenerator.Controllers
{
    [ApiController]
    public class GenerateController : BaseController
    {
        public GenerateController(IConfiguration configuration) : base(configuration)
        {
        }

        [AllowAnonymous]
        [Route("api/test")]
        public async Task<IActionResult?> Test()
        {
            var codeGenerator = new BarCodeGenerator();
            var barCode = codeGenerator.Generate(BarCodeType.BarCode, "038000356216");
            var qrCode = codeGenerator.Generate(BarCodeType.QrCode, "блабла");
            
            var template = await OdfDocument.LoadFromAsync(@"d:\\test.odt");
            var odtTemplate = new OdtTemplate(template);
            var blob1 = odtTemplate.TemplateDocument.AddOrGetImage(barCode);
            var blob2 = odtTemplate.TemplateDocument.AddOrGetImage(qrCode);
            
            var data = new Dictionary<string, object>()
            {
                { "barCode", blob1.Blob.FileName },
                { "qrCode", blob2.Blob.FileName }
            };
            var imageFileNames = new List<string>();
            imageFileNames.Add(blob1.Blob.FileName);
            imageFileNames.Add(blob2.Blob.FileName);

            var context = new TemplateContext(data);
            var doc = await odtTemplate.RenderAsync(context);
            OpenDocumentTextFunctions.InsertImages(doc, imageFileNames);
            await doc.SaveAsync(@"d:\\test2.odt");
            return null;
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/{filename}.odt")]
        public async Task<IActionResult?> GetOdt(string instanceName, string schemaName, string templateName, string fileName)
        {
            return await GenerateTemplate(instanceName, schemaName, templateName, fileName, "odt");
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/{filename}.pdf")]
        public async Task<IActionResult?> GetPdf(string instanceName, string schemaName, string templateName, string fileName)
        {
            return await GenerateTemplate(instanceName, schemaName, templateName, fileName, "pdf");
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/{filename}.html")]
        public async Task<IActionResult?> GetHtml(string instanceName, string schemaName, string templateName, string fileName)
        {
            return await GenerateTemplate(instanceName, schemaName, templateName, fileName, "html");
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/{filename}.txt")]
        public async Task<IActionResult?> GetTxt(string instanceName, string schemaName, string templateName, string fileName)
        {
            return await GenerateTemplate(instanceName, schemaName, templateName, fileName, "txt");
        }

        private async Task<IActionResult?> GenerateTemplate(string instanceName, string schemaName, string templateName, string fileName,
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

            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("No FileName specified");
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

            var isAuthenticated = CreateTokenProcessor();
            if ((!isAuthenticated) || (TokenProcessor == null))
            {
                return LocalRedirect(HttpContext.Request.Path + HttpContext.Request.QueryString);
            }

            var funDbApiConnector = new FunDbApi.FunDbApiConnector(configuration, instanceName, TokenProcessor);
            var checkAccess = await funDbApiConnector.CheckAccess();
            if (checkAccess == HttpStatusCode.Unauthorized) 
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                return LocalRedirect(HttpContext.Request.Path + HttpContext.Request.QueryString);
            }
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
                        FileDownloadName = fileName + ".odt"
                    };
                }
                else if ((format == "pdf") || (format == "html") || (format == "txt"))
                {
                    var odtFilePath = Path.GetTempFileName(); 
                    var newFilePath = odtFilePath.Replace(".tmp", "." + format);
                    try
                    {
                        await generatedReport.SaveAsync(odtFilePath);
                        if (!System.IO.File.Exists(odtFilePath))
                            throw new Exception("File " + odtFilePath + " was not created");
                        try
                        {
                            var response = await FormatConverter.ConvertOdtByUnoconv(configuration, odtFilePath, format);
                            if (!System.IO.File.Exists(newFilePath))
                                throw new Exception("File " + newFilePath + " was not created. Error message: " +
                                                    response);
                            byte[] bytes = await System.IO.File.ReadAllBytesAsync(newFilePath);
                            MediaTypeHeaderValue? mediaType = null;
                            switch (format)
                            {
                                case "pdf":
                                    mediaType = new MediaTypeHeaderValue("application/pdf");
                                    break;
                                case "html":
                                    mediaType = new MediaTypeHeaderValue("text/html");
                                    break;
                                case "txt":
                                    mediaType = new MediaTypeHeaderValue("text/html");
                                    break;

                            }
                            result = new FileContentResult(bytes, mediaType)
                            {
                                FileDownloadName = fileName + "." + format
                            };
                        }
                        finally
                        {
                            System.IO.File.Delete(newFilePath);
                        }
                    }
                    finally
                    {
                        System.IO.File.Delete(odtFilePath);
                    }
                }
                else 
                    throw new Exception("Unsupported file format: " + format);
                
                return result;
            }
            return null;
        }
    }
}
