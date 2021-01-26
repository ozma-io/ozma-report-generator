using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReportGenerator.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected async Task<string> GetToken()
        {
            return await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "access_token");
        }
    }
}
