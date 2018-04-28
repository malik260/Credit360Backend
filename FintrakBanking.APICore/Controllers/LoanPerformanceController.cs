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
    [RoutePrefix("api/v1/loan-management")]
    public class LoanPerformanceController : ApiControllerBase
    {
        private ILoanPerformanceRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        LoanPerformanceController(ILoanPerformanceRepository _repo)
        {
            this.repo = _repo;
        }
        [HttpGet]
        [Route("loan-prudential-guildline-type")]
        public HttpResponseMessage GetPrudGuildlineType()
        {
            try
            {
                var type = repo.GetPrudGuildlineType();

                if (type == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = type });
            }
            catch (System.Exception ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("get-all-loans")]
        public HttpResponseMessage GetAllLoanPerformance([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var loans = repo.GetAllLoan();
                int totalItems = loans.Count();

                loans = loans.OrderBy(x => x.maturityDate).Skip(page).Take(itemsPerPage);

                var data = loans.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = totalItems });

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }
    }
}
