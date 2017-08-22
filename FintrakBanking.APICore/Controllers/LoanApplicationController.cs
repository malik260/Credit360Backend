using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
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
        private ILoanRepository loanRepository;
        private ICreditLimitValidationsRepository creditLimitValidationsRepository;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanApplicationController(ILoanApplicationRepository _repoApply, ILoanRepository _loanRepository, ICreditLimitValidationsRepository _creditLimitValidationsRepository)
        {
            this.repoApply = _repoApply;
            this.loanRepository = _loanRepository;
            this.creditLimitValidationsRepository = _creditLimitValidationsRepository;
        }

        #region Loan Application
        [HttpGet][Route("loan-application")]
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

        [HttpGet][Route("loan-application/{id}")]
        public HttpResponseMessage GetLoanApplicationById(int id)
        {
            try
            {
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
        public async Task<HttpResponseMessage> AddLoanApplication([FromBody] LoanApplicationViewModel entity)
        {
            try
            {

                if (creditLimitValidationsRepository.ValidateCamsol(entity.customerId.Value) > 0)
                {
                    throw new Exception("Customer '" + entity.customerName + "' has been CAMSOL");
                }

                if (creditLimitValidationsRepository.ValidateWatchList(entity.customerId.Value) > 0)
                {
                    throw new Exception("Customer '" + entity.customerName + "' has been Watchlisted");
                }

                if (creditLimitValidationsRepository.ValidateBlackList(entity.customerId.Value) > 0)
                {
                    throw new Exception("Customer '" + entity.customerName + "' has been Blacklisted");
                }


              //var model =  creditLimitValidationsRepository.ValidateAmountByBranch1(entity.branchId).Difference;


                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.branchId = (short)token.GetBranchId;

                entity.misCode = "001";
                entity.teamMiscode = "004";

                var response = await repoApply.AddLoanApplication(entity);
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
        //[HttpGet][Route("loan/application/pending/page/{page}/itemsPerPage/{itemPerPage}")]
        public HttpResponseMessage GetAllPendingLoanApplications( int page, int itemsPerPage)
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

    }
}