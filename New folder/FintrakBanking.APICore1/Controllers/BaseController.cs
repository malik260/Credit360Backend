using FintrakBanking.APICore.middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FintrakBanking.APICore.Controllers
{
    [Authorize]
    
    public class BaseController : Controller
    {

    }
}