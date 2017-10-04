using FintrakBanking.APICore.Filters;
using System.Security;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FintrakBanking.APICore.core
{
    [JWTAuthorize]
    //[EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ApiControllerBase : ApiController
    {
        protected void ValidateAuthorizedUser(string userRequested)
        {
            string userLoggedIn = User.Identity.Name;
            if (userLoggedIn != userRequested)
                throw new SecurityException("Attempting to access data for another user.");
        }
    }
}