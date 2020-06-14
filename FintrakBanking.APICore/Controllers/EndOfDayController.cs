using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/admin")]
    public class EndOfDayController : ApiControllerBase
    {
        private IEndOfDayRepository repoEOD;
        private ILoanOperationsRepository repoLoanOP;
        private IAdminRepository adminRepo;
        private IFinacleIntegrationRepository finacleIntegration;

        public EndOfDayController(IEndOfDayRepository _repoEOD, ILoanOperationsRepository _repoLoanOP, IAdminRepository _adminRepo, IFinacleIntegrationRepository _finacleIntegration)
        {
            this.repoEOD = _repoEOD;
            this.repoLoanOP = _repoLoanOP;
            this.adminRepo = _adminRepo;
            this.finacleIntegration = _finacleIntegration;
        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        [HttpGet]
        [ClaimsAuthorization]
        [Route("end-of-day")]
        public HttpResponseMessage GetFinanceEndofday()
        {

            var data = repoEOD.GetFinanceEndofday(token.GetCompanyId);
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "An unknown error has occured" });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("end-of-day")]
        public HttpResponseMessage RunEndOfDay([FromBody] EndOfDayViewModel model)
        {

            model.companyId = token.GetCompanyId;
            model.createdBy = token.GetStaffId;
            model.userBranchId = (short)token.GetBranchId;
            var data = repoEOD.RunEndOfDay(model);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = true, message = "End of day transaction completed successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "End of day transaction failed" });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("refresh-loan-classification")]
        public HttpResponseMessage RefreshLoanClassification()
        {


            var data = repoEOD.RefreshLoanClassification(token.GetCompanyId);

            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = true, message = "Refresh Finacle Bulk Posting Transaction Completed Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "Refresh Finacle Bulk Posting Transaction Failed" });

        }



        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("change-application-date")]
        //public HttpResponseMessage ChangeApplicationDate([FromBody] EndOfDayViewModel model)
        //{

        //    model.companyId = token.GetCompanyId;
        //    model.createdBy = token.GetStaffId;
        //    model.userBranchId = (short)token.GetBranchId;
        //    var data = repoEOD.ChangeApplicationDate(model);
        //    if (data)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //                   new { success = true, message = "Application Date Change has been completed successfully" });
        //    }
        //    else
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //                   new { success = false, message = "Application Date Change failed" });

        //}


        [HttpGet]
        [ClaimsAuthorization]
        [Route("process-repayment-from-staging")]
        public HttpResponseMessage ProcessRepaymentFromStaging()
        {

            //try
            //{
            // model.companyId = token.GetCompanyId;
            //model.createdBy = token.GetStaffId;
            //model.userBranchId = (short)token.GetBranchId;
            var data = repoLoanOP.GetRepaymentFromStaging();

            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = true, message = "Refresh Finacle Bulk Posting Transaction Completed Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "Refresh Finacle Bulk Posting Transaction Failed" });
            //}
            //catch (ConditionNotMetException ce)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ce.Message });
            //}
            //catch (BadLogicException be)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = be.Message });
            //}
            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unhandled error occured. The system cannot complete the process." });
            //}
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("deactivate-inactive-users")]
        public HttpResponseMessage DeactivateInactiveUsers()
        {


            adminRepo.DeactivateInactiveUsers();


            return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "Inactive Users Deactivated successfully" });


            //}
            //catch (ConditionNotMetException ce)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ce.Message });
            //}
            //catch (BadLogicException be)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = be.Message });
            //}
            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unhandled error occured. The system cannot complete the process." });
            //}
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("get_endofday_operation_log")]
        public HttpResponseMessage GetEndofdayOperationLog([FromBody] FinanceEndofdayViewModel model)
        {
            var data = repoEOD.GetEndofdayOperationLog((DateTime)model.eodDate, token.GetCompanyId);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "An unknown error has occured" });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-endofday-operation-log-monitoring")]
        public HttpResponseMessage GetEndofdayOperationLogMonitoring()
        {
            var data = repoEOD.GetEndofdayOperationLogMonitoring(token.GetCompanyId);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "An unknown error has occured" });

        }


    }

}