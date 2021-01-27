using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace ReportGenerator.FunDbApi
{
    public class FunDbApiConnector
    {
        private readonly IConfiguration configuration;
        private readonly string token;
        private readonly string instanceName;

        public FunDbApiConnector(string instanceName, string token)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            this.token = token;
            this.instanceName = instanceName;
        }

        private string GetApiUrl()
        {
            var dbUrl = configuration["FunDbSettings:DatabaseServerUrl"];
            return dbUrl.Replace("{instanceName}", instanceName);
        }

        //private async Task<string> GetToken()
        //{
        //    const string userName = "anton.laptev@gmail.com";
        //    const string password = "testpwd";
        //    var client = new RestClient(configuration["AuthSettings:OpenIdConnectUrl"] + "token");
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //    request.AddParameter("application/x-www-form-urlencoded",
        //        "grant_type=password" +
        //        "&client_id=" + configuration["AuthSettings:ClientId"] +
        //        "&client_secret=" + configuration["AuthSettings:ClientSecret"] +
        //        "&username=" + userName +
        //        "&password=" + password,
        //        ParameterType.RequestBody);
        //    IRestResponse response = await client.ExecuteAsync(request);
        //    var responseJson = response.Content;
        //    var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)["access_token"].ToString();
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        throw new AuthenticationException("ozma.io API authentication failed.");
        //    }
        //    return token;
        //}

        public async Task<bool> GetUserIsAdmin()
        {
            var values = new Dictionary<string, object>();
            //var query = await LoadQueryAnonymous("SELECT is_root FROM public.users WHERE id = $$user_id", values);
            //if (query == true) return true;
            //else return false;
            var request = PrepareRequest(values);
            var client = new RestClient(GetApiUrl() + "/permissions");
            var response = await client.ExecuteAsync(request);
            var responseJson = response.Content;
            if (responseJson == "{\"isRoot\":true}") return true;
            else return false;
        }

        private RestRequest PrepareRequest(Dictionary<string, object> parameterValues)
        {
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "Bearer " + token);
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

        private async Task<dynamic?> ExecuteRequest(string url, RestRequest request)
        {
            dynamic? result = null;
            var client = new RestClient(url);
            var response = await client.ExecuteAsync(request);
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
        }

        public async Task<dynamic?> LoadQueryAnonymous(string queryText, Dictionary<string, object> parameterValues)
        {
            dynamic? result = null;
            if (!string.IsNullOrEmpty(token))
            {
                var request = PrepareRequest(parameterValues);
                request.AddParameter("__query", queryText, ParameterType.QueryString);
                result = await ExecuteRequest(GetApiUrl() + "/views/anonymous/entries", request);
            }
            return result;
        }

        public async Task<dynamic?> LoadQueryNamed(string queryText, Dictionary<string, object> parameterValues)
        {
            dynamic? result = null;
            if (!string.IsNullOrEmpty(token))
            {
                var request = PrepareRequest(parameterValues);
                if (!queryText.EndsWith("/entries")) queryText = queryText + "/entries";
                result = await ExecuteRequest(GetApiUrl() + queryText, request);
            }
            return result;
        }
    }
}
