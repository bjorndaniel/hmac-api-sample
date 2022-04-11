using Hmac.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Hmac.Api.Controllers
{
    [HMACAuthenticationFilter]
    public class BaseController : ControllerBase
    {

    }
}