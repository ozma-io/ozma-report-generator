using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace ReportGenerator
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async x =>
                        {
                            if ((x.Properties.IssuedUtc != null) && (x.Properties.ExpiresUtc != null))
                            {
                                var now = DateTimeOffset.UtcNow;
                                var timeElapsed = now.Subtract(x.Properties.IssuedUtc.Value);
                                var timeRemaining = x.Properties.ExpiresUtc.Value.Subtract(now);

                                if (timeElapsed > timeRemaining)
                                {
                                    var identity = (ClaimsIdentity) x.Principal.Identity;
                                    var accessTokenClaim = identity.FindFirst("access_token");
                                    var refreshTokenClaim = identity.FindFirst("refresh_token");

                                    var refreshToken = refreshTokenClaim.Value;
                                    var response = await new HttpClient().RequestRefreshTokenAsync(
                                        new RefreshTokenRequest
                                        {
                                            Address = Configuration["AuthSettings:OpenIdConnectUrl"] +
                                                      "protocol/openid-connect/token",
                                            ClientId = Configuration["AuthSettings:ClientId"],
                                            ClientSecret = Configuration["AuthSettings:ClientSecret"],
                                            RefreshToken = refreshToken
                                        });

                                    if (!response.IsError)
                                    {
                                        identity.RemoveClaim(accessTokenClaim);
                                        identity.RemoveClaim(refreshTokenClaim);

                                        identity.AddClaims(new[]
                                        {
                                            new Claim("access_token", response.AccessToken),
                                            new Claim("refresh_token", response.RefreshToken)
                                        });

                                        x.ShouldRenew = true;
                                    }
                                }
                            }
                        }
                    };
                })
                .AddOpenIdConnect(options =>
                    {
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.ClientId = Configuration["AuthSettings:ClientId"];
                        options.ClientSecret = Configuration["AuthSettings:ClientSecret"];
                        options.Authority = Configuration["AuthSettings:OpenIdConnectUrl"];
                        options.SaveTokens = true;
                        options.RequireHttpsMetadata = !Environment.IsDevelopment();
                        options.ResponseType = "code";
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        options.Events = new OpenIdConnectEvents
                        {
                            OnTokenValidated = x =>
                            {
                                var identity = (ClaimsIdentity) x.Principal.Identity;
                                identity.AddClaims(new[]
                                {
                                    new Claim("access_token", x.TokenEndpointResponse.AccessToken),
                                    new Claim("refresh_token", x.TokenEndpointResponse.RefreshToken)
                                });
                                x.Properties.IsPersistent = true;
                                var accessToken = new JwtSecurityToken(x.TokenEndpointResponse.AccessToken);
                                x.Properties.ExpiresUtc = accessToken.ValidTo;
                                return Task.CompletedTask;
                            }
                        };
                    }
                );

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Admin/Error");
            }
            app.UseStatusCodePages();

            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            //app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Admin}/{action=Index}/{id?}");
            });
        }
    }
}
