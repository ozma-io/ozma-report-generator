using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator.Controllers
{
    // [Authorize]
    public abstract class BaseController : Controller
    {
        protected IConfiguration configuration;
        public BaseController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected TokenProcessor? TokenProcessor { get; private set; }

        protected bool CreateTokenProcessor()
        {
            // TODO
            /* if (!HttpContext.User.Identity.IsAuthenticated)
             * {
             *     //throw new Exception("User is not authenticated");
             *     return false;
             * } */
            TokenProcessor = TokenProcessor.Create(configuration, HttpContext);
            return true;
        }
    }
}
