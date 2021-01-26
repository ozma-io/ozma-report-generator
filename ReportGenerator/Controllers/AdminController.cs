using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using ReportGenerator.Models;
using ReportGenerator.Repositories;
using Sandwych.Reporting.OpenDocument;
using Test.Models;

namespace ReportGenerator.Controllers
{
    public class AdminController : BaseController
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<bool> HasAdminRightsForInstance(string instanceName)
        {
            var token = await GetToken();
            var adminRights = await new FunDbApi.FunDbApiConnector(instanceName, token).GetUserIsAdmin();
            return adminRights;
        }

        [Route("admin/{instanceName}")]
        public async Task<IActionResult> Index(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new Exception("Instance name cannot be empty");

            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            ViewBag.instanceName = instanceName;
            var list = new List<ReportTemplateScheme>();
            using (var repository = new ReportTemplateSchemeRepository(instanceName))
            {
                list = await repository.LoadAllSchemes();
            }
            var selectList = new SelectList(list, "Id", "Name");
            ViewBag.SchemeId = selectList;
            return View();
        }

        private static string RemoveRestrictedSymbols(string text)
        {
            return text.Replace(" ", "").Replace("/", "").Replace("__", "");
        }

        #region Схемы шаблонов 
        [HttpGet]
        [Route("admin/{instanceName}/LoadSchemes")]
        public async Task<IActionResult> LoadSchemes(string instanceName)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            var list = new List<ReportTemplateScheme>();
            using (var repository = new ReportTemplateSchemeRepository(instanceName))
            {
                list = await repository.LoadAllSchemes();
            }
            return PartialView("~/Views/Admin/PartialViews/SchemesListPartial.cshtml", list);
        }

        [HttpPost]
        [Route("admin/{instanceName}/AddScheme")]
        public async Task<IActionResult> AddScheme(string instanceName, ReportTemplateScheme model)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            using (var repository = new ReportTemplateSchemeRepository(instanceName))
            {
                try
                {
                    model.Name = RemoveRestrictedSymbols(model.Name);
                    await repository.AddScheme(model);
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
        [Route("admin/{instanceName}/DeleteScheme")]
        public async Task<IActionResult> DeleteScheme(string instanceName, int id)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            using (var repository = new ReportTemplateSchemeRepository(instanceName))
            {
                try
                {
                    await repository.DeleteScheme(id);
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
            using (var repository = new ReportTemplateRepository(instanceName))
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
                    QueryText = query.QueryTextWithoutParameterValues
                };
                model.ReportTemplateQueries.Add(newQuery);
            }
            var odtWithoutQueries = OpenDocumentTextFunctions.RemoveQueriesFromOdt(odtWithQueries);
            await using (var stream = new MemoryStream())
            {
                await odtWithoutQueries.SaveAsync(stream);
                model.OdtWithoutQueries = stream.ToArray();
            }
            using (var repository = new ReportTemplateRepository(instanceName))
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

        [HttpDelete]
        [Route("admin/{instanceName}/DeleteTemplate")]
        public async Task<IActionResult> DeleteTemplate(string instanceName, int id)
        {
            var hasAdminRights = await HasAdminRightsForInstance(instanceName);
            if (!hasAdminRights) return Unauthorized();

            using (var repository = new ReportTemplateRepository(instanceName))
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
