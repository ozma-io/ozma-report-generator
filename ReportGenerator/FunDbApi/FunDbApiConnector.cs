using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace ReportGenerator.FunDbApi
{
    public class FunDbApiConnector : IDisposable
    {
        private const string BaseApiUrl = "https://anton-laptev.api.ozma.org";
        private const string TokenUrl = "https://account.ozma.io/auth/realms/default/protocol/openid-connect/token";
        private const string ClientId = "template-generator";
        private const string ClientSecret = "31fb5f48-e8c9-4e5e-83d3-b3a4b8f41384";
        private const string UserName = "anton.laptev@gmail.com";
        private const string Password = "testpwd";

        public void Dispose()
        {
            //ToDo
        }

        private async Task<string> GetToken() 
        {
            var client = new RestClient(TokenUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded",
                "grant_type=password" +
                "&client_id=" + ClientId +
                "&client_secret=" + ClientSecret +
                "&username=" + UserName +
                "&password=" + Password,
                ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            var responseJson = response.Content;
            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)["access_token"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthenticationException("ozma.io API authentication failed.");
            }
            return token;
        }

        public async Task<dynamic> LoadQueryAnonymous(string queryText, Dictionary<string, object> parameterValues)
        {
            dynamic result = null;

            var token = await GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                var client = new RestClient(BaseApiUrl + "/views/anonymous/entries");
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("authorization", "Bearer " + token);
                request.AddParameter("__query", queryText, ParameterType.QueryString);
                foreach (var parameterValue in parameterValues)
                {
                    if (parameterValue.Value != null)
                        request.AddParameter(parameterValue.Key, parameterValue.Value.ToString(),
                            ParameterType.QueryString);
                }
                var response = await client.ExecuteAsync(request);
                var responseJson = response.Content;
                var viewExprResult = JsonConvert.DeserializeObject<ViewExprResult>(responseJson);
                if (viewExprResult?.info != null && viewExprResult.result != null)
                {
                    var columnsNames = viewExprResult.info.columns.Select(p => p.name).ToList();

                    if (columnsNames.Count() == 1 && viewExprResult.result.rows.Length == 1)
                        result = viewExprResult.result.rows[0].values[0].value;
                    else
                    {
                        result = new List<ExpandoObject>();
                        foreach (var row in viewExprResult.result.rows)
                        {
                            var newItem = new ExpandoObject();
                            for (var i = 0; i < columnsNames.Count(); i++)
                            {
                                ((IDictionary<string, object>) newItem).Add(columnsNames[i], row.values[i].value);
                            }
                            ((List<ExpandoObject>)result).Add(newItem);
                        }
                    }
                }
            }
            return result;
        }
    }
}
