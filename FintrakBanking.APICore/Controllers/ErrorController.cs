using FintrakBanking.APICore.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    public class ErrorController : ApiControllerBase
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions]
        public IHttpActionResult NotFound(string path)
        {
            // log error to ELMAH
            Elmah.ErrorSignal.FromCurrentContext().Raise(new HttpException(404, "404 Not Found: /" + path));

            // return 404
            return NotFound();
        }
    }
}
