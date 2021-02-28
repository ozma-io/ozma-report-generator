using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ReportGenerator.FunDbApi;
using Sandwych.Reporting.OpenDocument;

namespace ReportGenerator
{
    public static class OpenDocumentTextFunctions
    {
        private static string GetOdtXmlAsText(OdfDocument odtWithQueries)
        {
            var contentXml = odtWithQueries.ReadMainContentXml();
            XmlNode? bodyNode = null;
            foreach (XmlNode? node in contentXml.DocumentElement.ChildNodes)
            {
                if ((node != null) && (node.Name == "office:body"))
                {
                    bodyNode = node;
                    break;
                }
            }
            if (bodyNode == null)
            {
                throw new Exception("Error parsing odt file. Cant find office:body tag");
            }
            XmlNode? textNode = null;
            foreach (XmlNode? node in bodyNode.ChildNodes)
            {
                if ((node != null) && (node.Name == "office:text"))
                {
                    textNode = node;
                    break;
                }
            }
            if (textNode == null)
            {
                throw new Exception("Error parsing odt file. Cant find office:text tag");
            }
            var sb = new StringBuilder();
            foreach (XmlNode? node in textNode.ChildNodes)
            {
                if (node != null)
                {
                    sb.AppendLine(node.InnerText);
                    sb.Append(' ');
                }
            }
            sb = sb.Replace(Environment.NewLine, string.Empty);
            var result = sb.ToString();
            return result;
        }

        public static List<FunDbQuery> GetQueriesFromOdt(OdfDocument odtWithQueries)
        {
            if (odtWithQueries == null)
            {
                throw new Exception("No odt file. Template was not loaded correctly");
            }
            var contentAsString = GetOdtXmlAsText(odtWithQueries);
            var queriesFromOdt = new List<FunDbQuery>();
            const string patternForQueries = "<query name=\"([A-Za-z0-9]+)\" type=\"([A-Za-z]+)\">(.*?)<\\/query>";
            var regexForQueries = new Regex(patternForQueries, RegexOptions.Singleline | RegexOptions.Compiled);
            var matchesForQueries = regexForQueries.Matches(contentAsString);
            foreach (Match? matchForQueries in matchesForQueries)
            {
                if ((matchForQueries != null) && (matchForQueries.Success) && (matchForQueries.Groups.Count == 4))
                {
                    var queryName = matchForQueries.Groups[1].ToString();
                    var queryTypeText = matchForQueries.Groups[2].ToString();
                    QueryType queryType;
                    switch (queryTypeText)
                    {
                        case "SingleValue":
                            queryType = QueryType.SingleValue;
                            break;
                        case "SingleRow":
                            queryType = QueryType.SingleRow;
                            break;
                        case "ManyRows":
                            queryType = QueryType.ManyRows;
                            break;
                        default:
                            throw new Exception("Wrong query type in template: " + queryTypeText);
                    }
                    var queryText = matchForQueries.Groups[3].ToString();
                    var query = new FunDbQuery(queryName, queryText, queryType);
                    queriesFromOdt.Add(query);
                }
            }
            return queriesFromOdt;
        }

        public static OdfDocument RemoveQueriesFromOdt(OdfDocument odtWithQueries)
        {
            var contentXml = odtWithQueries.ReadMainContentXml();
            const string patternForQueries = @"(&lt;query (.*?)+&gt;.*?&lt;/query&gt;)";
            var newXml = Regex.Replace(contentXml.InnerXml, patternForQueries, "", RegexOptions.Singleline | RegexOptions.Compiled);
            contentXml.InnerXml = newXml;
            odtWithQueries.WriteMainContentXml(contentXml);
            return odtWithQueries;
        }

        public static List<TemplateExpression> GetTemplateExpressionsFromOdt(OdfDocument odt)
        {
            var result = new List<TemplateExpression>();
            var contentAsString = GetOdtXmlAsText(odt);
            const string patternForStatements = @"{{(.*?)}}";
            var regex = new Regex(patternForStatements, RegexOptions.Multiline | RegexOptions.Compiled);
            var matches = regex.Matches(contentAsString);

            const string patternForManyRows = @"{\% for ([a-zA-Z0-9]+) in ([a-zA-Z0-9]+) \%}";
            var regexForManyRows = new Regex(patternForManyRows, RegexOptions.Multiline | RegexOptions.Compiled);
            var matchesForManyRows = regexForManyRows.Matches(contentAsString);

            foreach (Match? match in matchesForManyRows)
            {
                if ((match != null) && (match.Success) && (match.Groups.Count == 3))
                {
                    var subQueryName = match.Groups[1].ToString();
                    var queryName = match.Groups[2].ToString();
                    if (!result.Any(p => p.QueryName == queryName))
                    {
                        var templateExpression = new TemplateExpression();
                        templateExpression.QueryType = QueryType.ManyRows;
                        templateExpression.QueryName = queryName;
                        templateExpression.SubQueryName = subQueryName;
                        result.Add(templateExpression);
                    }
                }
            }

            foreach (Match? match in matches)
            {
                if ((match != null) && (match.Success) && (match.Groups.Count == 2))
                {
                    var templateExpression = new TemplateExpression();
                    
                    var expression = match.Groups[1].ToString();
                    if (expression.Contains("."))
                    {
                        var parts = expression.Split('.');
                        var queryName = parts[0];
                        var fieldName = parts[1];
                        var manyRowsquery = result.FirstOrDefault(p =>
                            (p.QueryType == QueryType.ManyRows) && (p.SubQueryName == queryName));
                        if (manyRowsquery != null)
                        {
                            if (!manyRowsquery.FieldNames.Any(p => p == fieldName))
                                manyRowsquery.FieldNames.Add(fieldName);
                        }
                        else
                        {
                            var singleRowQuery = result.FirstOrDefault(p => p.QueryName == queryName);
                            if (singleRowQuery == null)
                            {
                                templateExpression.QueryType = QueryType.SingleRow;
                                templateExpression.QueryName = queryName;
                                templateExpression.FieldNames.Add(fieldName);
                                result.Add(templateExpression);
                            }
                            else
                            {
                                if (!singleRowQuery.FieldNames.Any(p => p == fieldName))
                                    singleRowQuery.FieldNames.Add(fieldName);
                            }
                        }
                    }
                    else
                    {
                        if (!result.Any(p => p.QueryName == expression))
                        {
                            templateExpression.QueryType = QueryType.SingleValue;
                            templateExpression.QueryName = expression;
                            result.Add(templateExpression);
                        }
                    }
                }
            }
            return result;
        }

        public static OdfDocument InsertImages(OdfDocument odt, List<string> imageFileNames)
        {
            var contentXml = odt.ReadMainContentXml();
            foreach (var imageFileName in imageFileNames)
            {
                var stringFrom = imageFileName;
                var stringTo =
                    "<draw:frame svg:width=\"150px\" svg:height=\"150px\" draw:z-index=\"0\"><draw:image xlink:href=\"Pictures/" +
                    imageFileName +
                    "\" xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuate=\"onLoad\" draw:mime-type=\"image/png\"/></draw:frame>";
                contentXml.InnerXml = contentXml.InnerXml.Replace(stringFrom, stringTo);
            }
            odt.WriteMainContentXml(contentXml);
            return odt;
        }
    }
}
