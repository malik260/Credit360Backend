using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.Repositories;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.ViewModels.Business;
using System.Web;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/credit")]
    public class LoanApplicationController : ApiControllerBase
    {
        private ILoanApplicationRepository repoApply;

        public LoanApplicationController(ILoanApplicationRepository _repoApply)
        {
            this.repoApply = _repoApply;
        }

        #region Loan Application
        [HttpGet][Route("loan-application")]
        public HttpResponseMessage GetAllLoanApplications()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var response = repoApply.GetAllLoanApplications(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet][Route("loan-application/{id}")]
        public HttpResponseMessage GetLoanApplicationById(int id)
        {
            try
            {
                TokenDecryptionHelper token = null ;
                var response = repoApply.GetLoanApplicationById(id,token.GetCompanyId);
                if (response!= null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet][Route("loan-product-class")]
        public HttpResponseMessage GetProductClass()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var response = repoApply.GetProductClass();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        
        [HttpGet][Route("loan-application/search/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {
                TokenDecryptionHelper token = null;

                var response = repoApply.FindLoanApplication(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpDelete("loan-application/{aid}/approval-status/{id}")]
        //public IActionResult UpdateApprovalStatus(int aid, ApprovalStatusEnum id)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = HttpContext.Current.Request.Path,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };

        //        repoApply.UpdateApprovalStatus(aid,id, user);

        //        return new { success = true, result = id, message = "record has been deleted successfully" });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost][Route("loan/application")]
        public async Task<HttpResponseMessage> LoanBooking([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = null;

                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.branchId = (short)token.GetBranchId;

                entity.misCode = "001";
                entity.teamMiscode = "004";

                var response = await repoApply.CreateLoanApplication(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation completed successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet][Route("loan/application/pending")]
        public HttpResponseMessage GetAllPendingLoanApplications( int page, int itemsPerPage)
        {
            try
            {
                TokenDecryptionHelper token = null ;
                var response = repoApply.GetAllLoanApplications(token.GetCompanyId).Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending).ToList();
                int totalItems = response.Count();
                response = response.OrderBy(x=>x.applicationDate).Skip(page).Take(itemsPerPage).ToList();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, totalItems = totalItems });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #endregion

    }
}