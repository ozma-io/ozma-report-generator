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

        private async Task<string?> ExecuteRequest(string url, RestRequest request, int retryCount = 0)
        {
            var client = new RestClient(url);
            var response = await client.ExecuteAsync(request);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return response.Content;
                case System.Net.HttpStatusCode.Unauthorized:
                    if (retryCount == 0)
                    {
                        retryCount++;
                        await tokenProcessor.RefreshToken();
                        return await ExecuteRequest(url, request, retryCount);
                    }
                    else throw new Exception("FunDb query execution error: Unauthorized. " + response.Content);
                default:
                    throw new Exception("FunDb query execution error. Response: " + response.Content);
            }
        }

        public async Task<string?> LoadQueryAnonymous(string queryText, Dictionary<string, object> parameterValues)
        {
            string? result = null;
            if (!string.IsNullOrEmpty(tokenProcessor.AccessToken))
            {
                var request = PrepareRequest(parameterValues);
                request.AddParameter("__query", queryText, ParameterType.QueryString);
                result = await ExecuteRequest(GetApiUrl() + "/views/anonymous/entries", request);
            }
            return result;
        }

        public async Task<string?> LoadQueryNamed(string queryText, Dictionary<string, object> parameterValues)
        {
            string? result = null;
            if (!string.IsNullOrEmpty(tokenProcessor.AccessToken))
            {
                var request = PrepareRequest(parameterValues);
                result = await ExecuteRequest(GetApiUrl() + queryText, request);
            }
            return result;
        }
    }
}
