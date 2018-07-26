using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class PublicHolidayController : ApiControllerBase
    {
        private readonly IPublicHolidayRepository repo;
        private IEndOfDayRepository repoEOD;
        private readonly IAuditTrailRepository audit;
        public PublicHolidayController(IPublicHolidayRepository _repo,
                                IAuditTrailRepository _audit, IEndOfDayRepository _repoEOD)
        {
            this.repo = _repo;
            this.audit = _audit;
            this.repoEOD = _repoEOD;
        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

      [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }


        }

       
         [HttpPost] [ClaimsAuthorization]
        [Route("public-holiday")]
        public HttpResponseMessage AddPublicHoliday([FromBody] PublicHolidayViewModel entity)
        {
            try
            {
                if (repo.DoesHolidayExist(entity.date, token.GetCountryId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = $"{entity.description} with {entity.date.ToString("dd/mm/yy")} already exists" });
                }
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.countryId = token.GetCountryId;
                var data = repo.AddPublicHoliday(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = entity, message = "Public Holiday has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "An unknown error has occured" });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("public-holiday/weekends-in-a-year")]
        public HttpResponseMessage AddWeekendsInTheYear([FromBody] PublicHolidayViewModel entity)
        {
            try
            {
               
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.countryId = token.GetCountryId;

                var data = repo.AddWeekendsInTheYear(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = entity, message = "Added all weekends in the year successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "An unknown error has occured" });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }
        [HttpDelete] [ClaimsAuthorization]
        [Route("public-holiday-delete/{id}")]
        public HttpResponseMessage DeletePublicHoliday( int id)
        {
            try
            {

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };


                var data = repo.DeletePublicHoliday(user, id);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true,  message = "Public Holiday has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "An unknown error has occured" });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }
    }
}
