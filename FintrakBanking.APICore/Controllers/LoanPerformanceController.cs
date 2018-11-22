using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Repositories.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
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
    [RoutePrefix("api/v1/loan-management")]
    public class LoanPerformanceController : ApiControllerBase
    {
       private ILoanPerformanceRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
       public LoanPerformanceController(ILoanPerformanceRepository _repo)
        {
            this.repo = _repo;
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-prudential-guildline-status")]
        public HttpResponseMessage GetPrudGuildlineType()
       {
            try
            {
                var type = repo.GetPrudGuildlineStatus();

                if (type == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = type });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("get-all-loans")]
        public HttpResponseMessage GetAllLoanPerformance([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var loans = repo.GetAllLoans();
                int totalItems = loans.Count();

                loans = loans.OrderBy(x => x.maturityDate).Skip(page).Take(itemsPerPage);

                var data = loans.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = totalItems });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-performance/search-loan")]
        public HttpResponseMessage SearchForFilteredLoanPerformance(string searchQuery)
        {
            try
            {
                var loans = repo.GetAllLoans();

                loans = loans.OrderBy(x => x.maturityDate)
                    .Where(x => x.loanReferenceNumber.ToLower().Contains(searchQuery.ToLower())
                    || x.customerName.ToLower().Contains(searchQuery)
                    || x.customerName.ToUpper().Contains(searchQuery));

                var data = loans.ToList();

                int totalItems = loans.Count();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = loans.ToList(), count = loans.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });

            }
        }





        [HttpGet] [ClaimsAuthorization]  
        [Route("loan-performance/search")]
        public HttpResponseMessage FilteredLoanPerformance([FromUri] int page, string searchQuery)
        {
            try
            {
                var loans = repo.GetAllLoan();

                loans = loans.OrderBy(x => x.maturityDate).Skip(page)
                    .Where(x => x.loanReferenceNumber.ToLower().Contains(searchQuery.ToLower())
                    || x.customerName.ToLower().Contains(searchQuery));
                  

                var data = loans.ToList();

                int totalItems = loans.Count();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = loans.ToList(), count = loans.Count() });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan-performance-status-change")]
        public HttpResponseMessage LoanPerformanceStatusChange([FromBody] PrudGuidelineStatusChangeViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.LoanPerformanceStatusChange(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true,  message = "Performance Status Change Successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = false, message = "Performance Status Change Not Successful" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
