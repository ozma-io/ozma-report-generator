using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace ReportGenerator.FunDbApi
{
    public class FunDbApiConnector
    {
        private readonly IConfiguration configuration;
        private readonly TokenProcessor tokenProcessor;
        private readonly string instanceName;

        public FunDbApiConnector(IConfiguration configuration, string instanceName, TokenProcessor tokenProcessor)
        {
            this.configuration = configuration;
            this.tokenProcessor = tokenProcessor;
            this.instanceName = instanceName;
        }

        private string GetApiUrl()
        {
            var dbUrl = configuration["FunDbSettings:DatabaseServerUrl"];
            return dbUrl.Replace("{instanceName}", instanceName);
        }

        public async Task<bool> GetUserIsAdmin(int retryCount = 0)
        {
            var values = new Dictionary<string, object>();
            var request = PrepareRequest(values);
            var client = new RestClient(GetApiUrl() + "/permissions");
            var response = await client.ExecuteAsync(request);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var responseJson = response.Content;
                    var permissions = JsonConvert.DeserializeObject<PermissionsResponseJson>(responseJson);
                    return permissions.IsRoot;
                case System.Net.HttpStatusCode.Forbidden:
                    return false;
                case System.Net.HttpStatusCode.Unauthorized:
                    if (retryCount == 0)
                    {
                        retryCount++;
                        await tokenProcessor.RefreshToken();
                        return await GetUserIsAdmin(retryCount);
                    }
                    else throw new Exception("Getting /permissions error: Unauthorized. " + response.Content);
                default:
                    throw new Exception("Getting /permissions error. Response status code: " + response.StatusCode);
            }
        }

        private RestRequest PrepareRequest(Dictionary<string, object> parameterValues)
        {
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "Bearer " + tokenProcessor.AccessToken);
            foreach (var parameterValue in parameterValues)
            {
                if (parameterValue.Value != null)
                {
                    var value = parameterValue.Value.ToString();
                    if (!string.IsNullOrEmpty(value))
                        request.AddParameter(parameterValue.Key, value, ParameterType.QueryString);
                }
            }
            return request;
        }

        private async Task<dynamic?> ExecuteRequest(string url, RestRequest request, int retryCount = 0)
        {
            dynamic? result = null;
            var client = new RestClient(url);
            var response = await client.ExecuteAsync(request);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var responseJson = response.Content;
                    var viewExprResult = JsonConvert.DeserializeObject<ViewExprResult>(responseJson);
                    if (viewExprResult?.info != null && viewExprResult.result != null)
                    {
                        var columnsNames = viewExprResult.info.columns.Select(p => p.name).ToList();

                        if (columnsNames.Count() == 1 && viewExprResult.result.rows.Length == 1)
                        {
                            dynamic? value;
                            if (viewExprResult.result.rows[0].values[0].pun != null)
                                value = viewExprResult.result.rows[0].values[0].pun;
                            else value = viewExprResult.result.rows[0].values[0].value;
                            result = value;
                        }
                        else
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
                    }
                    else
                    {
                        throw new Exception("FunDb query execution error " + responseJson);
                    }
                    return result;
                case System.Net.HttpStatusCode.Unauthorized:
                    if (retryCount == 0)
                    {
                        retryCount++;
                        await tokenProcessor.RefreshToken();
                        return await ExecuteRequest(url, request, retryCount);
                    }
                    else throw new Exception("FunDb query execution error: Unauthorized. " + response.Content);
                default:
                    throw new Exception("FunDb query execution error. Response status code: " + response.StatusCode);
            }
        }

        private async Task<dynamic?> LoadQueryAnonymous(string queryText, Dictionary<string, object> parameterValues)
        {
            dynamic? result = null;
            if (!string.IsNullOrEmpty(tokenProcessor.AccessToken))
            {
                var request = PrepareRequest(parameterValues);
                request.AddParameter("__query", queryText, ParameterType.QueryString);
                result = await ExecuteRequest(GetApiUrl() + "/views/anonymous/entries", request);
            }
            return result;
        }

        private async Task<dynamic?> LoadQueryNamed(string queryText, Dictionary<string, object> parameterValues)
        {
            dynamic? result = null;
            if (!string.IsNullOrEmpty(tokenProcessor.AccessToken))
            {
                var request = PrepareRequest(parameterValues);
                if (!queryText.EndsWith("/entries")) queryText = queryText + "/entries";
                result = await ExecuteRequest(GetApiUrl() + queryText, request);
            }
            return result;
        }

        public async Task LoadQuery(FunDbQuery query, Dictionary<string, object> queryParametersWithValues)
        {
            var queryTextToRun = query.QueryTextWithoutParameterValues;
            dynamic? result = null;
            if (queryTextToRun.StartsWith("/views/"))
                result = await LoadQueryNamed(queryTextToRun, queryParametersWithValues);
            else
                result = await LoadQueryAnonymous(queryTextToRun, queryParametersWithValues);
            query.SetResult(result);
        }
    }
}
