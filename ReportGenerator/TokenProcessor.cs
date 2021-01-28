using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator
{
    public class TokenProcessor
    {
        public string AccessToken { get; private set; }
        private readonly HttpContext httpContext;
        private readonly IConfiguration configuration;

        private TokenProcessor(IConfiguration configuration, HttpContext httpContext, string accessToken)
        {
            this.httpContext = httpContext;
            AccessToken = accessToken;
            this.configuration = configuration;
        }

        public static async Task<TokenProcessor> Create(IConfiguration configuration, HttpContext httpContext)
        {
            var accessToken =
                await httpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "access_token");
            return new TokenProcessor(configuration, httpContext, accessToken);
        }

        public async Task RefreshToken()
        {
            var refreshToken =
                await httpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "refresh_token");
            if (string.IsNullOrEmpty(refreshToken)) return;
            var response = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration["AuthSettings:OpenIdConnectUrl"] + "protocol/openid-connect/token",
                ClientId = configuration["AuthSettings:ClientId"],
                ClientSecret = configuration["AuthSettings:ClientSecret"],
                RefreshToken = refreshToken
            });
            if (!response.IsError)
            {
                AccessToken = response.AccessToken;
                refreshToken = response.RefreshToken;
            }
        }
    }
}
