using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/admin")]
    public class EndOfDayController : ApiControllerBase
    {
        private IEndOfDayRepository repoEOD;
        
        public EndOfDayController( IEndOfDayRepository _repoEOD)
        {
            this.repoEOD = _repoEOD;
        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        [HttpGet]
        [Route("end-of-day")]
        public HttpResponseMessage GetFinanceEndofday()
        {
            try
            {
                var data = repoEOD.GetFinanceEndofday(token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "An unknown error has occured" });

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }

        }


        [HttpPost]
        [Route("end-of-day")]
        public HttpResponseMessage RunEndOfDay([FromBody] EndOfDayViewModel model)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short) token.GetBranchId;
                var data = repoEOD.RunEndOfDay(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = true, message = "End of day transaction completed successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "End of day transaction failed" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



    }

}