﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReportGenerator.FunDbApi
{
    public class FunDbQuery
    {
        public string Name { get; private set; }
        public string QueryTextWithoutParameterValues { get; private set; }

        public bool IsLoaded { get; private set; }
        private object? _result;
        public QueryType QueryType { get; private set; }

        public object? Result
        {
            get
            {
                if (!IsLoaded) return null;
                return _result;
            }
        }

        public List<BarCodeFieldValue> BarCodeFieldValues { get; private set; }

        public FunDbQuery(string name, string queryTextWithoutParameterValues, QueryType queryType)
        {
            Name = name;
            QueryTextWithoutParameterValues = queryTextWithoutParameterValues;
            QueryType = queryType;
            BarCodeFieldValues = new List<BarCodeFieldValue>();
        }

        public async Task LoadDataAsync(FunDbApiConnector funDbApiConnector, Dictionary<string, object> queryParametersWithValues)
        {
            var queryTextToRun = QueryTextWithoutParameterValues;
            dynamic? result = null;
            if (queryTextToRun.StartsWith("/views/"))
            {
                const string pattern = @"/views/(?<schema>[^/]+)/(?<name>[^/]+)$";
                var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled);
                var matchesForQueries = regex.Matches(queryTextToRun);
                if (matchesForQueries.Count() != 1) throw new Exception("Syntax error in query " + Name);
                var namedQueryText = "/views/by_name/" + matchesForQueries[0].Groups[1].ToString() + "/" +
                                     matchesForQueries[0].Groups[2].ToString();
                if (!namedQueryText.EndsWith("/entries")) namedQueryText = namedQueryText + "/entries";
                result = await funDbApiConnector.LoadQueryNamed(namedQueryText, queryParametersWithValues);
            }
            else
                result = await funDbApiConnector.LoadQueryAnonymous(queryTextToRun, queryParametersWithValues);
            if (result != null)
            {
                IsLoaded = true;

                var responseJson = (string) result;
                var viewExprResult = JsonConvert.DeserializeObject<ViewExprResult>(responseJson);
                if (viewExprResult?.info != null && viewExprResult.result != null)
                {
                    var columnsNames = viewExprResult.info.columns.Select(p => p.name).ToList();

                    if (QueryType == QueryType.SingleValue)
                    {
                        if ((columnsNames.Count() > 1) || (viewExprResult.result.rows.Length > 1))
                        {
                            throw new Exception("FunDb query type (SingleValue) does not match returned results");
                        }
                        dynamic? value;
                        if (viewExprResult.result.rows[0].values[0].pun != null)
                            value = viewExprResult.result.rows[0].values[0].pun;
                        else value = viewExprResult.result.rows[0].values[0].value;
                        result = value;
                    }
                    else if (QueryType == QueryType.SingleRow)
                    {
                        if (viewExprResult.result.rows.Length != 1)
                        {
                            throw new Exception("FunDb query type (SingleRow) does not match returned results");
                        }
                        var row = viewExprResult.result.rows[0];
                        var newItem = new ExpandoObject();
                        for (var i = 0; i < columnsNames.Count(); i++)
                        {
                            dynamic? value;
                            if (row.values[i].pun != null) value = row.values[i].pun;
                            else value = row.values[i].value;
                            ((IDictionary<string, object>)newItem).Add(columnsNames[i], value);
                        }
                        result = newItem;
                    }
                    else if (QueryType == QueryType.ManyRows)
                    {
                        result = new List<ExpandoObject>();
                        foreach (var row in viewExprResult.result.rows)
                        {
                            var newItem = new ExpandoObject();
                            for (var i = 0; i < columnsNames.Count(); i++)
                            {
                                dynamic? value;
                                if (row.values[i].pun != null) value = row.values[i].pun;
                                else value = row.values[i].value;
                                ((IDictionary<string, object>)newItem).Add(columnsNames[i], value);
                            }
                            ((List<ExpandoObject>)result).Add(newItem);
                        }
                    }
                    else
                    {
                        throw new Exception("Wrong FunDb query type: " + QueryType);
                    }
                    BarCodeFieldValues = GetBarCodeFieldValues(viewExprResult);
                }
                else
                {
                    throw new Exception("FunDb query execution error " + responseJson);
                }
            }
            _result = result;
        }

        private static List<BarCodeFieldValue> GetBarCodeFieldValues(ViewExprResult viewExprResult)
        {
            var result = new List<BarCodeFieldValue>();
            foreach (var row in viewExprResult.result.rows)
            {
                for (var colNum = 0; colNum < viewExprResult.info.columns.Count(); colNum++)
                {
                    var columnName = viewExprResult.info.columns[colNum].name;
                    var uvAttributes = (IDictionary<string, object>) viewExprResult.result.attributes;
                    var columnAttributes = (IDictionary<string, object>) viewExprResult.result.columnAttributes[colNum];
                    var rowAttributes = (IDictionary<string, object>?) row.attributes;
                    var valueAttributes = (IDictionary<string, object>?) row.values[colNum].attributes;

                    object? controlAttribute = null;
                    if ((valueAttributes != null) && (valueAttributes.ContainsKey("control")))
                        controlAttribute = valueAttributes["control"];
                    else if ((rowAttributes != null) && (rowAttributes.ContainsKey("control")))
                        controlAttribute = rowAttributes["control"];
                    else if ((columnAttributes != null) && (columnAttributes.ContainsKey("control")))
                        controlAttribute = columnAttributes["control"];
                    else if ((uvAttributes != null) && (uvAttributes.ContainsKey("control")))
                        controlAttribute = uvAttributes["control"];

                    if ((controlAttribute != null) && ((controlAttribute.ToString() == "barcode") ||
                                                       (controlAttribute.ToString() == "qrcode")))
                    {
                        var domainId = row.domainId.ToString();
                        if (domainId != null)
                        {
                            var domain = ((IDictionary<string, object>) viewExprResult.info.domains)[domainId];
                            if (domain != null)
                            {
                                var columnInfo = ((IDictionary<string, object>) domain)[columnName];
                                if (columnInfo != null)
                                {
                                    string? entityName = null;
                                    string? schemaName = null;
                                    var _ref =
                                        (IDictionary<string, object>) ((IDictionary<string, object>) columnInfo)["ref"];
                                    if (_ref["name"].ToString() == "id")
                                    {
                                        var entity = (IDictionary<string, object>) _ref["entity"];
                                        if (entity != null)
                                        {
                                            entityName = entity["name"].ToString();
                                            schemaName = entity["schema"].ToString();
                                        }
                                    }
                                    else
                                    {
                                        var field =
                                            (IDictionary<string, object>) ((IDictionary<string, object>) columnInfo)[
                                                "field"];
                                        if (field != null)
                                        {
                                            var fieldType = (IDictionary<string, object>) field["fieldType"];
                                            if ((fieldType != null) && (fieldType["type"].ToString() == "reference"))
                                            {
                                                var entity = (IDictionary<string, object>) fieldType["entity"];
                                                if (entity != null)
                                                {
                                                    entityName = entity["name"].ToString();
                                                    schemaName = entity["schema"].ToString();
                                                }
                                            }
                                        }
                                    }

                                    if ((entityName != null) && (schemaName != null))
                                    {
                                        var fieldValue = row.values[colNum].value;
                                        string fieldValueToDisplay;
                                        if (row.values[colNum].pun != null)
                                            fieldValueToDisplay = row.values[colNum].pun.ToString();
                                        else fieldValueToDisplay = row.values[colNum].value.ToString();

                                        if (controlAttribute.ToString() == "barcode")
                                        {
                                            result.Add(new BarCodeFieldValue
                                            {
                                                QueryFieldName = columnName,
                                                CodeType = BarCodeType.BarCode,
                                                ValueToEncode = fieldValue.ToString(),
                                                FieldValue = fieldValueToDisplay
                                            });

                                        }
                                        else if (controlAttribute.ToString() == "qrcode")
                                        {
                                            var valueToEncode = "1/" + schemaName + "/" + entityName + "/" + fieldValue.ToString();
                                            result.Add(new BarCodeFieldValue
                                            {
                                                QueryFieldName = columnName,
                                                CodeType = BarCodeType.QrCode,
                                                ValueToEncode = valueToEncode,
                                                FieldValue = fieldValueToDisplay
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public void ChangeBarCodeFieldValueInResult(BarCodeFieldValue barCodeFieldValue, string newValue)
        {
            if (_result != null)
            {
                if (QueryType == QueryType.SingleValue)
                {
                    if (_result.ToString() == barCodeFieldValue.FieldValue) _result = newValue;
                }
                else if (QueryType == QueryType.SingleRow)
                {
                    if (((ExpandoObject) _result).Any(p =>
                        (p.Key == barCodeFieldValue.QueryFieldName) &&
                        (p.Value.ToString() == barCodeFieldValue.FieldValue)))
                    {
                        ((IDictionary<string, object>) _result)[barCodeFieldValue.QueryFieldName] = newValue;
                    }
                }
                else if (QueryType == QueryType.ManyRows)
                {
                    foreach (ExpandoObject item in (List<ExpandoObject>) _result)
                    {
                        if (item.Any(p =>
                            (p.Key == barCodeFieldValue.QueryFieldName) &&
                            (p.Value.ToString() == barCodeFieldValue.FieldValue)))
                            ((IDictionary<string, object>) item)[barCodeFieldValue.QueryFieldName] = newValue;
                    }
                }
            }
        }
    }
}
