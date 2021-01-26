using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportGenerator.FunDbApi;
using ReportGenerator.Models;
using Sandwych.Reporting;
using Sandwych.Reporting.OpenDocument;

namespace ReportGenerator
{
    public static class ReportTemplateFunctions
    {
        private static Dictionary<string, string> GetParameters(ReportTemplate template)
        {
            var result = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(template.Parameters))
            {
                var list = template.Parameters.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in list)
                {
                    var nameAndType = item.Split(':');
                    if (nameAndType.Count() == 2)
                    {
                        var parameterName = nameAndType[0];
                        var parameterType = nameAndType[1];
                        if (!result.Any(p => p.Key == parameterName))
                            result.Add(parameterName, parameterType);
                    }
                }
            }
            return result;
        }

        private static List<FunDbQuery> GetQueriesAsFunDb(ReportTemplate template)
        {
            var result = new List<FunDbQuery>();
            if ((template.ReportTemplateQueries != null) && (template.ReportTemplateQueries.Any()))
            {
                foreach (var query in template.ReportTemplateQueries)
                {
                    var funDbQuery = new FunDbQuery(query.Name, query.QueryText);
                    result.Add(funDbQuery);
                }
            }
            return result;
        }

        private static async Task<OdfDocument?> PrepareOdtWithoutQueries(ReportTemplate template)
        {
            OdfDocument? odt = null;
            await using (var stream = new MemoryStream(template.OdtWithoutQueries))
            {
                odt = await OdfDocument.LoadFromAsync(stream);
            }
            return odt;
        }

        public static async Task<OdfDocument?> GenerateReport(ReportTemplate template, Dictionary<string, object> parametersWithValues, string instanceName, string token)
        {
            var parameters = GetParameters(template);
            OdfDocument? result = null;
            foreach (var parameterName in parameters.Select(p => p.Key))
            {
                var passedParameterWithValue = parametersWithValues.FirstOrDefault(p => p.Key == parameterName);
                if ((passedParameterWithValue.Key == null) || (passedParameterWithValue.Value == null))
                {
                    throw new Exception("No value passed for required parameter $" + parameterName);
                }
            }

            var odtWithoutQueries = await PrepareOdtWithoutQueries(template);
            var queriesFromOdt = GetQueriesAsFunDb(template);
            if (!queriesFromOdt.Any()) return odtWithoutQueries;

            foreach (var funDbQuery in queriesFromOdt)
            {
                await funDbQuery.LoadDataAsync(parametersWithValues, instanceName, token);
            }

            var loadedQueries = queriesFromOdt.Where(p => p.IsLoaded).ToList();
            if (loadedQueries.Count == queriesFromOdt.Count)
            {
                var data = new Dictionary<string, object>();
                foreach (var parameterWithValue in parametersWithValues)
                {
                    if (parameters.Any(p => p.Key == parameterWithValue.Key))
                    {
                        if (!data.Any(p => p.Key == parameterWithValue.Key))
                            data.Add(parameterWithValue.Key, parameterWithValue.Value);
                    }
                }
                foreach (var loadedQuery in loadedQueries)
                {
                    if (loadedQuery.Result != null)
                        data.Add(loadedQuery.Name, loadedQuery.Result);
                }
                if (odtWithoutQueries != null)
                {
                    var context = new TemplateContext(data);
                    var odtTemplate = new OdtTemplate(odtWithoutQueries);
                    result = await odtTemplate.RenderAsync(context);
                }
            }
            else
            {
                throw new Exception("One or more FunDb query failed");
            }
            return result;
        }
    }
}
