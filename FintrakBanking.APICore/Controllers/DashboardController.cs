using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/dashboard")]
    public class DashboardController : ApiController
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IDashboardRepository dashboard;
        public DashboardController(IDashboardRepository _dashboard)
        {
            dashboard = _dashboard;
        }

        [HttpPost]
        [Route("loan-application-sector")]
        public HttpResponseMessage GetAppraisalMemorandumDocumentation(DateRange val)
        {
            try
            {
                var data = dashboard.LoanApplicationsBySector(val.startDate, val.endDate);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
