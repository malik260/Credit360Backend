using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.ViewModels.Credit;
using System.Web;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.APICore.Filters;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Formatting;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/contingent")]
    public class ContingentLoanUsageController : ApiControllerBase
    {
        private IContingentLoanUsageRepository repo;

        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ContingentLoanUsageController(IContingentLoanUsageRepository repo)
        {
            this.repo = repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getcontingentloan")]
        public HttpResponseMessage GetAllContingentLoans()
        {
          
            try
            {
                var data = repo.GetAllContingentLoans(token.GetStaffId, token.GetCompanyId); 

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loanusage")]
        public async Task<HttpResponseMessage> SaveContigentLoans()
        {

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var formData = provider.FormData["formData"];

                var errors = new List<string>();
                ContingentLoanUsageViewModel incomingData = JsonConvert.DeserializeObject<ContingentLoanUsageViewModel>(formData,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg)
                        {
                            errors.Add(earg.ErrorContext.Member.ToString());
                            earg.ErrorContext.Handled = true;
                        }
                    });


                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();

                incomingData.applicationUrl = HttpContext.Current.Request.Path;
                incomingData.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                incomingData.userBranchId = (short)token.GetBranchId;
                incomingData.createdBy = token.GetStaffId;
                incomingData.companyId = token.GetCompanyId;
                incomingData.staffId = token.GetStaffId;
                incomingData.userBranchId = (short)token.GetBranchId;

                var data = repo.SaveContigentLoans(incomingData, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

            
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approvals/loanusage")]
        public HttpResponseMessage GetPendingRequest()
        {

            try
            {
                var data = repo.GetPendingRequest(token.GetStaffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("contingent/document/{loanId}")]
        public HttpResponseMessage GetContingentDocument(int loanId)
        {

            try
            {
                var data = repo.GetContingentUsageDocument(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loanusage-approval")]
        public HttpResponseMessage SaveContigentLoansUsageApproval(ApproveAPSRequestViewModel entity)
        {
            var responseMessage = string.Empty;

            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            entity.userBranchId = (short)token.GetBranchId;
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;

            bool response = repo.SaveContigentLoansUsageApproval(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-contingent-usage/{loanId}")]
        public HttpResponseMessage GetContingentUsage(int loanId)
        {
            try
            {
                var data = repo.GetContingentUsage(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
