using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReportGenerator.FunDbApi;
using ReportGenerator.Repositories;
using Sandwych.Reporting;
using Sandwych.Reporting.OpenDocument;

namespace ReportGenerator
{
    public class ReportTemplate
    {
        public string Name { get; private set; }
        private readonly Dictionary<string,Type> _parameters;
        private readonly List<FunDbQuery> _queriesFromOdt;

        private ReportTemplate(string templateName, Dictionary<string, Type> parameters, List<FunDbQuery> queriesFromOdt)
        {
            Name = templateName;
            _parameters = parameters;
            _queriesFromOdt = queriesFromOdt;
        }

        public static ReportTemplate CreateFromOdt(string templateName, Dictionary<string, Type> parameters, OdfDocument odt)
        {
            if (odt == null)
            {
                throw new Exception("No odt file. Template was not loaded correctly");
            }

            var contentXml = odt.ReadMainContentXml();
            var contentAsString = contentXml.InnerText;

            var queriesFromOdt = new List<FunDbQuery>();
            const string patternForQueries = @"<query ([A-Za-z0-9]+)>(.*?)<\/query>";
            var regexForQueries = new Regex(patternForQueries, RegexOptions.Singleline | RegexOptions.Compiled);

            //var parameterNamesFromOdt = new List<string>();
            //const string patternForParameters = @"{{([A-Za-z0-9]+)}}";
            //var regexForParameters = new Regex(patternForParameters, RegexOptions.Singleline | RegexOptions.Compiled);

            var matchesForQueries = regexForQueries.Matches(contentAsString);
            foreach (Match matchForQueries in matchesForQueries)
            {
                if ((matchForQueries.Success) && (matchForQueries.Groups.Count == 3))
                {
                    var queryName = matchForQueries.Groups[1].ToString();
                    var queryText = FormatQueryText(matchForQueries.Groups[2].ToString());

                    //var parameterNamesForQuery = new List<string>();
                    //var matchesForParameters = regexForParameters.Matches(queryText);
                    //foreach (Match matchForParameters in matchesForParameters)
                    //{
                    //    if ((matchForParameters.Success) && (matchForParameters.Groups.Count == 2))
                    //    {
                    //        var parameterName = matchForParameters.Groups[1].ToString();
                    //        if (parameterNamesFromOdt.All(p => p != parameterName))
                    //            parameterNamesFromOdt.Add(parameterName);
                    //        if (parameterNamesForQuery.All(p => p != parameterName))
                    //            parameterNamesForQuery.Add(parameterName);
                    //    }
                    //}

                    var query = new FunDbQuery(queryName, queryText);
                    queriesFromOdt.Add(query);
                }
            }
            return new ReportTemplate(templateName, parameters, queriesFromOdt);
        }

        public static OdfDocument RemoveQueriesFromOdt(OdfDocument odtWithQueries)
        {
            var contentXml = odtWithQueries.ReadMainContentXml();
            const string patternForQueries = @"(&lt;query [A-Za-z0-9]+&gt;.*?&lt;/query&gt;)";
            var newXml = Regex.Replace(contentXml.InnerXml, patternForQueries, "", RegexOptions.Singleline | RegexOptions.Compiled);
            contentXml.InnerXml = newXml;
            odtWithQueries.WriteMainContentXml(contentXml);
            return odtWithQueries;
        }

        private static string FormatQueryText(string queryText)
        {
            if (string.IsNullOrEmpty(queryText)) return queryText;
            var newText = new StringBuilder(queryText);
            newText = newText.Replace("SELECT", " SELECT ");
            newText = newText.Replace("AS", " AS ");
            newText = newText.Replace("FROM", " FROM ");
            newText = newText.Replace("WHERE", " WHERE ");
            newText = newText.Replace("NOT", " NOT ");
            newText = newText.Replace("AND", " AND ");
            newText = newText.Replace("ORDER BY", " ORDER BY ");
            newText = newText.Replace("ORDERBY", " ORDER BY ");
            newText = newText.Replace("  ", " ");
            return newText.ToString();
        }

        public async Task<OdfDocument> GenerateReport(Dictionary<string, object> parametersWithValues)
        {
            OdfDocument result = null;
            foreach (var parameterName in _parameters.Select(p => p.Key))
            {
                var passedParameterWithValue = parametersWithValues.FirstOrDefault(p => p.Key == parameterName);
                if ((passedParameterWithValue.Key == null) || (passedParameterWithValue.Value == null))
                {
                    throw new Exception("No value passed for required parameter $" + parameterName);
                }
            }

            foreach (var funDbQuery in _queriesFromOdt)
            {
                await funDbQuery.LoadDataAsync(parametersWithValues);
            }

            var loadedQueries = _queriesFromOdt.Where(p => p.IsLoaded).ToList();
            if (loadedQueries.Any())
            {
                var data = new Dictionary<string, object>();
                foreach (var parameterWithValue in parametersWithValues)
                {
                    if (_parameters.Any(p => p.Key == parameterWithValue.Key))
                    {
                        if (!data.Any(p => p.Key == parameterWithValue.Key))
                            data.Add(parameterWithValue.Key, parameterWithValue.Value);
                    }
                }
                foreach (var loadedQuery in loadedQueries)
                {
                    data.Add(loadedQuery.Name, loadedQuery.Result);
                }
                var context = new TemplateContext(data);
                OdfDocument odtWithoutQueries = null;
                using (var repository = new ReportTemplateRepository())
                {
                    odtWithoutQueries = await repository.LoadOdtWithoutQueries(this);
                }
                if (odtWithoutQueries != null)
                {
                    var template = new OdtTemplate(odtWithoutQueries);
                    result = await template.RenderAsync(context);
                }
            }
            return result;
        }
    }
}
