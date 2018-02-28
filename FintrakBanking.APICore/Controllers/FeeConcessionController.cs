using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/fees")]
    public class FeeConcessionController : ApiControllerBase
    {
         private IFeeConcessionRepository repo;
       

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public FeeConcessionController(IFeeConcessionRepository _repo)
        {
            repo = _repo;
        }

    }
}
