using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using ReportGenerator.Models;
using ReportGenerator.Repositories;
using Sandwych.Reporting.OpenDocument;
using Test.Models;

namespace ReportGenerator.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController(IConfiguration configuration) : base(configuration)
        {
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<bool> HasAdminRightsForInstance(string instanceName)
        {
            var tokenProcessor = CreateTokenProcessor();
            var funDbApiConnector = new FunDbApi.FunDbApiConnector(configuration, instanceName, tokenProcessor);
            var adminRights = await funDbApiConnector.GetUserIsAdmin();
            return adminRights;
        }

        private async Task<SelectList> LoadSchemaNamesList(string instanceName)
        {
            var list = new List<ReportTemplateSchema>();
            using (var repository = new ReportTemplateSchemaRepository(configuration, instanceName))
            {
                list = await repository.LoadAllSchemas();
            }
            var selectList = new SelectList(list, "Id", "Name");
            return selectList;
        }

        [HttpGet]
        [Route("admin/{instanceName}/GetSchemaNamesList")]
        public async Task<JsonResult> GetSchemaNamesList(string instanceName)
        {
            var selectList = await LoadSchemaNamesList(instanceName);
            return Json(selectList);
        }

        [Route("admin/{instanceName}")]
        public async Task<IActionResult> Index(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new Exception("Instance name cannot be empty");

            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized("User has no admin rights for this instance");

            new ReportTemplateRepository(configuration, instanceName, true);
            ViewBag.SchemaId = await LoadSchemaNamesList(instanceName);
            ViewBag.instanceName = instanceName;
            return View();
        }

        private static string RemoveRestrictedSymbols(string text)
        {
            return text.Replace(" ", "").Replace("/", "").Replace("__", "");
        }

        #region Схемы шаблонов 
        [HttpGet]
        [Route("admin/{instanceName}/LoadSchemas")]
        public async Task<IActionResult> LoadSchemas(string instanceName)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            var list = new List<ReportTemplateSchema>();
            using (var repository = new ReportTemplateSchemaRepository(configuration, instanceName))
            {
                list = await repository.LoadAllSchemas();
            }
            return PartialView("~/Views/Admin/PartialViews/SchemasListPartial.cshtml", list);
        }

        [HttpPost]
        [Route("admin/{instanceName}/AddSchema")]
        public async Task<IActionResult> AddSchema(string instanceName, ReportTemplateSchema model)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            using (var repository = new ReportTemplateSchemaRepository(configuration, instanceName))
            {
                try
                {
                    model.Name = RemoveRestrictedSymbols(model.Name);
                    await repository.AddSchema(model);
                    return Ok();
                }
                catch (Exception e)
                {
                    string msg;
                    if (e.InnerException != null) msg = e.InnerException.Message;
                    else msg = e.Message;
                    return StatusCode(500, msg);
                }
            }
        }

        [AllowAnonymous]
        [HttpDelete]
        [Route("admin/{instanceName}/DeleteSchema")]
        public async Task<IActionResult> DeleteSchemaAnonymous(string instanceName, int id)
        {
            return await DeleteSchema(instanceName, id);
        }

        [HttpDelete]
        //[Route("admin/{instanceName}/DeleteSchema")]
        public async Task<IActionResult> DeleteSchema(string instanceName, int id)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            using (var repository = new ReportTemplateSchemaRepository(configuration, instanceName))
            {
                try
                {
                    await repository.DeleteSchema(id);
                    return Ok();
                }
                catch (Exception e)
                {
                    string msg;
                    if (e.InnerException != null) msg = e.InnerException.Message;
                    else msg = e.Message;
                    return StatusCode(500, msg);
                }
            }
        }
        #endregion

        #region Шаблоны
        [HttpGet]
        [Route("admin/{instanceName}/LoadTemplates")]
        public async Task<IActionResult> LoadTemplates(string instanceName)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            var list = new List<VReportTemplate>();
            using (var repository = new ReportTemplateRepository(configuration, instanceName))
            {
                list = await repository.LoadAllTemplates();
            }
            return PartialView("~/Views/Admin/PartialViews/TemplatesListPartial.cshtml", list);
        }

        [HttpPost]
        [Route("admin/{instanceName}/AddTemplate")]
        public async Task<IActionResult> AddTemplate(string instanceName, IFormFile UploadedOdtFile, ReportTemplate model)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            OdfDocument? odtWithQueries = null;
            await using (var stream = new MemoryStream())
            {
                await UploadedOdtFile.CopyToAsync(stream);
                odtWithQueries = await OdfDocument.LoadFromAsync(stream);
            }
            if (odtWithQueries == null) throw new Exception("Error processing odt file");

            var queries = OpenDocumentTextFunctions.GetQueriesFromOdt(odtWithQueries);
            foreach (var query in queries)
            {
                var newQuery = new ReportTemplateQuery
                {
                    Name = query.Name,
                    QueryText = query.QueryTextWithoutParameterValues,
                    QueryType = (short) query.QueryType
                };
                model.ReportTemplateQueries.Add(newQuery);
            }
            var odtWithoutQueries = OpenDocumentTextFunctions.RemoveQueriesFromOdt(odtWithQueries);
            await using (var stream = new MemoryStream())
            {
                await odtWithoutQueries.SaveAsync(stream);
                model.OdtWithoutQueries = stream.ToArray();
            }
            using (var repository = new ReportTemplateRepository(configuration, instanceName))
            {
                try
                {
                    model.Name = RemoveRestrictedSymbols(model.Name);
                    await repository.AddTemplate(model);
                    return Ok();
                }
                catch (Exception e)
                {
                    string msg;
                    if (e.InnerException != null) msg = e.InnerException.Message;
                    else msg = e.Message;
                    return StatusCode(500, msg);
                }
            }
        }

        [HttpPost]
        [Route("admin/{instanceName}/UpdateTemplateFile")]
        public async Task<IActionResult> UpdateTemplateFile(string instanceName, int templateId, IFormFile UploadedOdtFile)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            OdfDocument? odtWithQueries = null;
            await using (var stream = new MemoryStream())
            {
                await UploadedOdtFile.CopyToAsync(stream);
                odtWithQueries = await OdfDocument.LoadFromAsync(stream);
            }
            if (odtWithQueries == null) throw new Exception("Error processing odt file");

            using (var repository = new ReportTemplateRepository(configuration, instanceName))
            {
                var model = await repository.LoadTemplate(templateId);
                if (model == null) throw new Exception("Template with id=" + templateId + " not found");
                model.ReportTemplateQueries.Clear();
                var queries = OpenDocumentTextFunctions.GetQueriesFromOdt(odtWithQueries);
                foreach (var query in queries)
                {
                    var newQuery = new ReportTemplateQuery
                    {
                        Name = query.Name,
                        QueryText = query.QueryTextWithoutParameterValues,
                        QueryType = (short)query.QueryType
                    };
                    model.ReportTemplateQueries.Add(newQuery);
                }
                var odtWithoutQueries = OpenDocumentTextFunctions.RemoveQueriesFromOdt(odtWithQueries);
                await using (var stream = new MemoryStream())
                {
                    await odtWithoutQueries.SaveAsync(stream);
                    model.OdtWithoutQueries = stream.ToArray();
                }
                try
                {
                    await repository.UpdateTemplate(model);
                    return Ok();
                }
                catch (Exception e)
                {
                    string msg;
                    if (e.InnerException != null) msg = e.InnerException.Message;
                    else msg = e.Message;
                    return StatusCode(500, msg);
                }
            }
        }

        [HttpDelete]
        [Route("admin/{instanceName}/DeleteTemplate")]
        public async Task<IActionResult> DeleteTemplate(string instanceName, int id)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            using (var repository = new ReportTemplateRepository(configuration, instanceName))
            {
                try
                {
                    await repository.DeleteTemplate(id);
                    return Ok();
                }
                catch (Exception e)
                {
                    string msg;
                    if (e.InnerException != null) msg = e.InnerException.Message;
                    else msg = e.Message;
                    return StatusCode(500, msg);
                }
            }
        }
        #endregion
    }
}
