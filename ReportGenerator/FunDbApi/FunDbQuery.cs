using System;
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

        public FunDbQuery(string name, string queryTextWithoutParameterValues, QueryType queryType)
        {
            Name = name;
            QueryTextWithoutParameterValues = queryTextWithoutParameterValues;
            QueryType = queryType;
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
                }
                else
                {
                    throw new Exception("FunDb query execution error " + responseJson);
                }
            }
            _result = result;
        }
    }
}
