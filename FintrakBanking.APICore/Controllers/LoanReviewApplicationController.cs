using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LoanReviewApplicationController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ILoanReviewApplicationRepository repo;

        public LoanReviewApplicationController(ILoanReviewApplicationRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet, Route("review-application")]
        public HttpResponseMessage GetApplications(
            [FromUri] int page,
            [FromUri] int itemsPerPage,
            [FromUri] int operationId,
            [FromUri] int? classId,
            [FromUri] string searchString
            )
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };

            try
            {
                IQueryable<LoanReviewApplicationViewModel> items;
                items = repo.GetApplications(user, operationId, classId);

                if (!String.IsNullOrEmpty(searchString))
                {
                    items = items.Where(x =>
                        x.referenceNumber.Contains(searchString)
                        || x.principalAmount.ToString().Contains(searchString)
                        || x.customerName.Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.applicationDate) // OrderBy() must be called for Skip() to work!
                    .ThenByDescending(x => x.loanReviewApplicationId)
                    .Skip(page)
                    .Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("loan-review-application/select-list")]
        public HttpResponseMessage GetAllSelectList()
        {
            try
            {
                var data = repo.GetAllSelectList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("loan-review-application/submit")]
        public HttpResponseMessage SubmitLoanReviewApplication([FromBody] LoanReviewApplicationViewModel entity)
        {
            try
            {
                entity.createdBy =  token.GetStaffId;
                entity.branchId =  token.GetBranchId;
                bool response = repo.SubmitLoanReviewApplication(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Application submitted successfully.", result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }
}