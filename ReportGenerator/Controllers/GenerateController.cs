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

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generate/{fileName}.{format}")]
        public IActionResult? GetReport(string instanceName, string schemaName, string templateName, string fileName, string format)
        {
            return CheckAuthAndGetReport(instanceName, schemaName, templateName, fileName, format);
        }

        [HttpGet]
        [Route("api/{instanceName}/{schemaName}/{templateName}/generateDirectly/{fileName}.{format}")]
        public async Task<IActionResult?> GetReportDirectly(string instanceName, string schemaName, string templateName, string fileName, string format)
        {
            // FIXME: use RouteUrl instead!
            var generatePath = $"api/{instanceName}/{schemaName}/{templateName}/generateDirectly/{fileName}.{format}";
            return await GenerateTemplate(instanceName, schemaName, templateName, fileName, format, generatePath + HttpContext.Request.QueryString);
        }

        /* This mess is intented to fix unwanted behavior with blank pages when server redirects to auth and then back */
        private IActionResult? CheckAuthAndGetReport(string instanceName, string schemaName, string templateName, string fileName, string format)
        {
            var isAuthenticated = CreateTokenProcessor();
            if ((!isAuthenticated) || (TokenProcessor == null))
            {
                return LocalRedirect(HttpContext.Request.Path + HttpContext.Request.QueryString);
            }

            return new ContentResult
                {
                    ContentType = "text/html",
                    Content = $@"<div>Redirecting...</div>
                        <script>
                            var oldWindow = window;
                            var newWindow = window.open('/api/{instanceName}/{schemaName}/{templateName}/generateDirectly/{fileName}.{format}{HttpContext.Request.QueryString}', '_blank');
                            newWindow.onblur = () => {{
                                newWindow.close();
                            }};
                            if (newWindow) {{
                              oldWindow.close();
                            }}
                        </script>"
                };
        }

        private async Task<IActionResult?> GenerateTemplate(string instanceName, string schemaName, string templateName, string fileName, string format, PathString? redirectIfErrorTo=null)
        {
            var redirectIfErrorPath = redirectIfErrorTo == null ? HttpContext.Request.Path + HttpContext.Request.QueryString : redirectIfErrorTo.ToString();
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

            try
            {
                using (var repository = new ReportTemplateRepository(configuration, instanceName))
                {
                    template = await repository.LoadTemplate(schemaName, templateName);
                }
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }

            if (template == null)
            {
                return NotFound("Template '" + templateName + "' not found");
            }

            var isAuthenticated = CreateTokenProcessor();
            if ((!isAuthenticated) || (TokenProcessor == null))
            {
                return LocalRedirect(redirectIfErrorPath);
            }

            var funDbApiConnector = new FunDbApi.FunDbApiConnector(configuration, instanceName, TokenProcessor);
            var checkAccess = await funDbApiConnector.CheckAccess();
            if (checkAccess == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                return LocalRedirect(redirectIfErrorPath);
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
                                    mediaType = new MediaTypeHeaderValue("text/plain; charset=utf-8");
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
