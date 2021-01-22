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
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Схемы шаблонов 
        public IActionResult Schemes()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadSchemes()
        {
            var list = new List<ReportTemplateScheme>();
            using (var repository = new ReportTemplateSchemeRepository())
            {
                list = await repository.LoadAllSchemes();
            }
            return PartialView("~/Views/Admin/PartialViews/SchemesListPartial.cshtml", list);
        }

        [HttpPost]
        public async Task<IActionResult> AddScheme(ReportTemplateScheme model)
        {
            using (var repository = new ReportTemplateSchemeRepository())
            {
                try
                {
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
        public async Task<IActionResult> DeleteScheme(int id)
        {
            using (var repository = new ReportTemplateSchemeRepository())
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
        public async Task<IActionResult> Templates()
        {
            var list = new List<ReportTemplateScheme>();
            using (var repository = new ReportTemplateSchemeRepository())
            {
                list = await repository.LoadAllSchemes();
            }
            var selectList = new SelectList(list, "Id", "Name");
            ViewBag.SchemeId = selectList;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadTemplates()
        {
            var list = new List<VReportTemplate>();
            using (var repository = new ReportTemplateRepository())
            {
                list = await repository.LoadAllTemplates();
            }
            return PartialView("~/Views/Admin/PartialViews/TemplatesListPartial.cshtml", list);
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplate(IFormFile UploadedOdtFile, ReportTemplate model)
        {
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
            using (var repository = new ReportTemplateRepository())
            {
                try
                {
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
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            using (var repository = new ReportTemplateRepository())
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
