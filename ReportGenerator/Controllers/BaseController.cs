using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected IConfiguration configuration;
        public BaseController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected TokenProcessor CreateTokenProcessor()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                throw new Exception("User is not authenticated");
            }
            return TokenProcessor.Create(configuration, HttpContext);
        }
    }
}
