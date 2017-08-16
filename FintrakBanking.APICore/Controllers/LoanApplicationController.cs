using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/credit")]
    public class LoanApplicationController : ApiControllerBase
    {
        private ILoanApplicationRepository repoApply;
        private ILoanPrelimenaryEvaluationRepository repoLoanPEN;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanApplicationController(ILoanApplicationRepository _repoApply, ILoanPrelimenaryEvaluationRepository _repoLoanPEN)
        {
            this.repoApply = _repoApply;
            repoLoanPEN = _repoLoanPEN;
        }

        #region Loan Application
        [HttpGet]
        [Route("loan-application")]
        public HttpResponseMessage GetAllLoanApplications()
        {
            try
            {
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

        [HttpGet]
        [Route("loan-application/{id}")]
        public HttpResponseMessage GetLoanApplicationById(int id)
        {
            try
            {
                var response = repoApply.GetLoanApplicationById(id, token.GetCompanyId);
                if (response != null)
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

        [HttpGet]
        [Route("loan-product-class")]
        public HttpResponseMessage GetProductClass()
        {
            try
            {
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


        [HttpGet]
        [Route("loan-application/search/")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {
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

        [HttpPost]
        [Route("loan/application")]
        public async Task<HttpResponseMessage> LoanBooking([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
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

        [HttpGet]
        [Route("loan/application/pending")]
        //[HttpGet][Route("loan/application/pending/page/{page}/itemsPerPage/{itemPerPage}")]
        public HttpResponseMessage GetAllPendingLoanApplications(int page, int itemsPerPage)
        {
            try
            {
                var response = repoApply.GetAllLoanApplications(token.GetCompanyId).Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending).ToList();
                int totalItems = response.Count();
                response = response.OrderByDescending(x => x.applicationDate).ThenByDescending(x => x.loanApplicationId).Skip(page).Take(itemsPerPage).ToList();
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

        #region Loan Prelimenary Evaluation

        [HttpPost]
        [Route("loan/prelimenary-evaluation")]
        public HttpResponseMessage AddPrelimenaryEvaluation(LoanPrelimenaryEvaluationViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;

                var response = repoLoanPEN.AddPrelimenaryEvaluation(model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Prelimenary evaluation note created successfully, now awaiting approval" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Prelimenary evaluation note not created" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("loan/prelimenary-evaluation/approval")]
        public HttpResponseMessage ApprovePrelimenaryEvaluation(ApprovalViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = (short)token.GetBranchId;
                model.staffId = token.GetStaffId;

                var data = repoLoanPEN.GoForApproval(model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, message = "Prelimenary evaluation note has been approved successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("loan/prelimenary-evaluation/awaiting-approval")]
        public HttpResponseMessage GetLoanPrelimenaryEvaluationsForAppproval()
        {
            try
            {
                var data = repoLoanPEN.GetPrelimenaryEvaluationsAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        #endregion

    }
}