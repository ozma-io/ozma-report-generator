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

        protected async Task<TokenProcessor> CreateTokenProcessor()
        {
            return await TokenProcessor.Create(configuration, HttpContext);
        }
    }
}
