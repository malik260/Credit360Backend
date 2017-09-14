using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class PublicHolidayController : ApiControllerBase
    {
        private readonly IPublicHolidayRepository repo;
        private readonly IAuditTrailRepository audit;
        public PublicHolidayController(IPublicHolidayRepository _repo,
                                IAuditTrailRepository _audit)
        {
            this.repo = _repo;
            this.audit = _audit;
        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        [HttpGet]
        [Route("public-holiday")]
        public HttpResponseMessage GetAllPublicHoliday()
        {
            try
            {
              
                var data = repo.GetAllPublicHoliday();
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data});
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
        [Route("public-holiday")]
        public HttpResponseMessage AddPublicHoliday([FromBody] PublicHolidayViewModel enitity)
        {
            try
            {
                if (repo.isHolidayExist(enitity.Description))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { suucess = false, message = $"{enitity.Description} already exit" });
                }
                enitity.createdBy = token.GetStaffId;
                enitity.userBranchId = (short)token.GetBranchId;
                enitity.applicationUrl = HttpContext.Current.Request.Path;
                enitity.companyId = token.GetCompanyId;

                var data = repo.AddPublicHoliday(enitity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = enitity, message = "Public Holiday has been created successfully" });
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

        [HttpPut]
        [Route("public-holiday/{id}")]
        public HttpResponseMessage UpdatePublicHoliday([FromBody] PublicHolidayViewModel enitity, int id)
        {
            try
            {
                enitity.createdBy = token.GetStaffId;
                enitity.userBranchId = (short)token.GetBranchId;
                enitity.applicationUrl = HttpContext.Current.Request.Path;
                enitity.companyId = token.GetCompanyId;

                var data = repo.UpdatePublicHoliday(enitity, id);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = enitity, message = "Public Holiday has been updated successfully" });
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
    }
}
