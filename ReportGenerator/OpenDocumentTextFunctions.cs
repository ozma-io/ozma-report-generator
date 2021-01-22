using System;
using System.Collections.Generic;
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
            const string patternForQueries = @"<query ([A-Za-z0-9]+)>(.*?)<\/query>";
            var regexForQueries = new Regex(patternForQueries, RegexOptions.Singleline | RegexOptions.Compiled);
            var matchesForQueries = regexForQueries.Matches(contentAsString);
            foreach (Match? matchForQueries in matchesForQueries)
            {
                if ((matchForQueries != null) && (matchForQueries.Success) && (matchForQueries.Groups.Count == 3))
                {
                    var queryName = matchForQueries.Groups[1].ToString();
                    var queryText = matchForQueries.Groups[2].ToString();
                    var query = new FunDbQuery(queryName, queryText);
                    queriesFromOdt.Add(query);
                }
            }
            return queriesFromOdt;
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
    }
}
