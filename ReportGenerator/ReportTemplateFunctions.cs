using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReportGenerator.FunDbApi;
using ReportGenerator.Models;
using Sandwych.Reporting.OpenDocument;
using TemplateContext = Sandwych.Reporting.TemplateContext;

namespace ReportGenerator
{
    public static class ReportTemplateFunctions
    {
        //private static Dictionary<string, string> GetParameters(ReportTemplate template)
        //{
        //    var result = new Dictionary<string, string>();
        //    if (!string.IsNullOrEmpty(template.Parameters))
        //    {
        //        var list = template.Parameters.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var item in list)
        //        {
        //            var nameAndType = item.Split(':');
        //            if (nameAndType.Count() == 2)
        //            {
        //                var parameterName = nameAndType[0];
        //                var parameterType = nameAndType[1];
        //                if (!result.Any(p => p.Key == parameterName))
        //                    result.Add(parameterName, parameterType);
        //            }
        //        }
        //    }
        //    return result;
        //}

        private static List<FunDbQuery> GetQueriesAsFunDb(ReportTemplate template)
        {
            var result = new List<FunDbQuery>();
            if ((template.ReportTemplateQueries != null) && (template.ReportTemplateQueries.Any()))
            {
                foreach (var query in template.ReportTemplateQueries)
                {
                    var funDbQuery = new FunDbQuery(query.Name, query.QueryText, (QueryType)query.QueryType);
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

        public static async Task<OdfDocument?> GenerateReport(FunDbApiConnector funDbApiConnector, ReportTemplate template, Dictionary<string, object> parametersWithValues)
        {
            //var parameters = GetParameters(template);
            //foreach (var parameterName in parameters.Select(p => p.Key))
            //{
            //    var passedParameterWithValue = parametersWithValues.FirstOrDefault(p => p.Key == parameterName);
            //    if ((passedParameterWithValue.Key == null) || (passedParameterWithValue.Value == null))
            //    {
            //        throw new Exception("No value passed for required parameter $" + parameterName);
            //    }
            //}

            OdfDocument? result = null;
            var odtWithoutQueries = await PrepareOdtWithoutQueries(template);
            if (odtWithoutQueries == null) throw new Exception("Template odt is null");

            var queriesFromOdt = GetQueriesAsFunDb(template);
            if (!queriesFromOdt.Any()) return odtWithoutQueries;

            foreach (var funDbQuery in queriesFromOdt)
            {
                await funDbQuery.LoadDataAsync(funDbApiConnector, parametersWithValues);
            }

            var loadedQueries = queriesFromOdt.Where(p => p.IsLoaded).ToList();
            if (loadedQueries.Count == queriesFromOdt.Count)
            {
                var data = new Dictionary<string, object>();
                foreach (var parameterWithValue in parametersWithValues)
                {
                    //if (parameters.Any(p => p.Key == parameterWithValue.Key))
                    //{
                    if (!data.Any(p => p.Key == parameterWithValue.Key))
                        data.Add(parameterWithValue.Key, parameterWithValue.Value);
                    //}
                }

                var templateExpressions = OpenDocumentTextFunctions.GetTemplateExpressionsFromOdt(odtWithoutQueries);
                if (!templateExpressions.Any())
                    throw new Exception("No {{ }} expressions found in template");

                #region syntax check
                foreach (var templateExpression in templateExpressions)
                {
                    var loadedQuery = loadedQueries.FirstOrDefault(p => p.Name == templateExpression.QueryName);
                    if (loadedQuery == null)
                        throw new Exception("Query " + templateExpression.QueryName +
                                            " from template not found in FunDb queries");
                    if (loadedQuery.QueryType != templateExpression.QueryType)
                        throw new Exception("Query " + templateExpression.QueryName +
                                            " return type is different from FunDb query type");

                    switch (templateExpression.QueryType)
                    {
                        case QueryType.SingleRow:
                            if (!(loadedQuery.Result is ExpandoObject))
                                throw new Exception("Query " + templateExpression.QueryName +
                                                    " from template return result is not ExpandoObject");
                            foreach (var fieldName in templateExpression.FieldNames)
                            {
                                if (!((ExpandoObject) loadedQuery.Result).Any(p => p.Key == fieldName))
                                    throw new Exception("Query " + templateExpression.QueryName +
                                                        " error: field " + fieldName +
                                                        " not found in FunDb query results");
                            }

                            break;
                        case QueryType.ManyRows:
                            if (!(loadedQuery.Result is List<ExpandoObject>))
                                throw new Exception("Query " + templateExpression.QueryName +
                                                    " from template return result is not List<ExpandoObject>");
                            foreach (var fieldName in templateExpression.FieldNames)
                            {
                                foreach (var item in (List<ExpandoObject>) loadedQuery.Result)
                                {
                                    if (!item.Any(p => p.Key == fieldName))
                                        throw new Exception("Query " + templateExpression.QueryName +
                                                            " error: field " + fieldName +
                                                            " not found in FunDb query results");
                                }
                            }

                            break;
                    }
                }
                #endregion

                var odtTemplate = new OdtTemplate(odtWithoutQueries);
                var imagesToInsertFileNames = new List<string>();

                foreach (var loadedQuery in loadedQueries)
                {
                    if (loadedQuery.Result != null)
                    {
                        if (loadedQuery.BarCodeFieldValues.Any())
                        {
                            var barCodeGenerator = new BarCodeGenerator();
                            foreach (var barCodeFieldValue in loadedQuery.BarCodeFieldValues)
                            {
                                var imageBlob =
                                    barCodeGenerator.Generate(barCodeFieldValue.CodeType, barCodeFieldValue.ValueToEncode);
                                var documentBlob = odtTemplate.TemplateDocument.AddOrGetImage(imageBlob);
                                loadedQuery.ChangeBarCodeFieldValueInResult(barCodeFieldValue,
                                    documentBlob.Blob.FileName);
                                imagesToInsertFileNames.Add(documentBlob.Blob.FileName);
                            }
                        }
                        data.Add(loadedQuery.Name, loadedQuery.Result);
                    }
                }
                var context = new TemplateContext(data);
                result = await odtTemplate.RenderAsync(context);
                if (imagesToInsertFileNames.Any())
                    OpenDocumentTextFunctions.InsertImages(result, imagesToInsertFileNames);
            }
            else
            {
                throw new Exception("One or more FunDb query failed");
            }

            return result;
        }
    }
}
