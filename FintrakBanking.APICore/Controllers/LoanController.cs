using FintrakBanking.APICore.JWTAuth;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.Customer;
using System.Collections.Generic;
using FintrakBanking.ViewModels.Reports;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Extensions;
using System.Data;
using System.Net.Http.Headers;

using System.IO;

using System.Drawing;

namespace FintrakBanking.APICore.Controllers //D:\Projects\FintrakBanking\FintrakBankingAPIFW\FintrakBankingAPI462\FintrakBanking.APICore\Controllers\LoanController.cs
{
    [RoutePrefix("api/v1/loan")]
    public class LoanController : ApiControllerBase
    {
        private ILoanRepository repo;
        private ICustomerCollateralRepository repoCollateral;
        private ICustomerRepository repoCustomer;
        private ILoanScheduleRepository scheduleRepo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
      private  ExportDataTableToExcel export = new ExportDataTableToExcel();

        //private IHostingEnvironment _hostingEnvironment;
        //private IHostingEnvironment _hostingEnvironment;
        //TokenDecryptionHelper token = new TokenDecryptionHelper();
        public LoanController(ILoanRepository _repo,
                              ICustomerCollateralRepository _repoCollateral,
                              ICustomerRepository _repoCustomer,
                               ILoanScheduleRepository _scheduleRepo)
        {
            this.repo = _repo;
            this.repoCollateral = _repoCollateral;
            this.repoCustomer = _repoCustomer;
            this.scheduleRepo = _scheduleRepo;

            //this._hostingEnvironment = hostingEnvironment;
        }

        #region Loan


         [HttpPost] [ClaimsAuthorization]
        [Route("current-exposure/customer")]
        public HttpResponseMessage GetCurrentCustomerExposure([FromBody] List<CustomerExposure> customer)
        {
            try
            {
                var data = repo.GetCurrentCustomerExposure(customer, token.GetCompanyId);
                //if (!data.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                //}

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("running-loan/customer/{id}")]
        public HttpResponseMessage GetAllLoanTypes(int id)
        {
            try
            {
                var data = repo.RunningLoans(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("revolving-types")]
        public HttpResponseMessage GetRevolvingLoanTypes(int id)
        {
            try
            {
                var data = repo.GetRevolvingLoanTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }


        [HttpGet]
        [Route("loan-application-types")]
        public HttpResponseMessage GetLoanApplicationTypes()
        {
            try
            {
                var data = repo.GetLoanApplicationTypes();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-category")]
        public HttpResponseMessage GetAllLoanScheduleCategory()
        {
            try
            {
                var data = scheduleRepo.GetAllLoanScheduleCategory();
                //if (!data.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                //}

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-types")]
        public HttpResponseMessage GetAllLoanScheduleType()
        {
            try
            {
                var data = scheduleRepo.GetAllLoanScheduleType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-types/{productTypeId}")]
        public HttpResponseMessage GetAllLoanScheduleType(short? productTypeId)
        {
            try
            {
                var data = scheduleRepo.GetAllLoanScheduleType(productTypeId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-types/category/{categoryId}")]
        public HttpResponseMessage GetLoanScheduleTypeByCategory(short categoryId)
        {
            try
            {
                var data = scheduleRepo.GetLoanScheduleTypeByCategory(categoryId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-booking")]
        public  HttpResponseMessage AddLoanBooking([FromBody] LoanViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data =  repo.AddLoanBooking(entity);
                if (data != "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The Loan booking was successful and and is waiting for approval.\r\n Loan Reference Number: " + data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-customer-accounts/{customerId}/application-detail/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanCustomerAccounts(int customerId, int loanApplicationDetailId)
        {
            try
            {
                var data = repo.GetLoanCustomerAccounts(customerId, loanApplicationDetailId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("gaurantor/product-type/{productTypeId}/application/{applicationReferenceNumber}")]
        public HttpResponseMessage AddLoanGuarantor( [FromBody] LoanGuarantorViewModel entity, short productTypeId, int applicationReferenceNumber)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = false; //repo.AddLoanGuarantor(entity, productTypeId, applicationReferenceNumber);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The Loan Gaurantor successful added " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error saving gaurantor" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }


        [HttpGet]
        [Route("appraisal-loan-details-updates/{appraisalMemorandumId}")]
        public HttpResponseMessage GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId)
        {
            try
            {
                var data = repo.GetAppraisalMemorandumLoanUpdates(appraisalMemorandumId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }



        [HttpGet]
        [Route("monitoring-trigger")]
        public HttpResponseMessage GetLoanMonitoringTrigger()
        {
            try
            {
                var data = repo.GetLoanMonitoringTrigger();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-status")]
        public HttpResponseMessage GetLoanStatus()
        {
            try
            {
                var data = repo.GetLoanStatus(token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("monitoring-trigger/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanMonitoringTriggerByLoanApplicationDetailId(int loanApplicationDetailId)
        {
            try
            {
                var data = repo.GetLoanMonitoringTriggerByLoanApplicationDetailId(loanApplicationDetailId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        //[HttpGet]
        //[Route("loan-application-collateral/{loanApplicationId}")]
        //public HttpResponseMessage GetLoanBookingAwaitingApproval(int loanApplicationId)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //        var data = repo.GetLoanApplicationCollateralsByApplicationId(loanApplicationId);

        //        if (data.Any() == false)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        //    }
        //    catch (ConditionNotMetException ce)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            //}
            //catch (BadLogicException be)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            //}
            //catch (Exception )
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            //}
        //}

        [HttpGet]
        [Route("number-of-installments/tenor-mode/{tenorModeId}/frequency-type/{frequencyTypeId}/tenor/{tenor}")]
        public HttpResponseMessage GetNumberOfInstallments(short tenorModeId, short frequencyTypeId, int tenor)
        {
            try
            {
                var data = scheduleRepo.CalculateNumberOfInstallments((TenorModeEnum)tenorModeId, frequencyTypeId, tenor);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("{loanId}")]
        public HttpResponseMessage GetLoan(int loanId)
        {
            try
            {
                var data = repo.GetLoan(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/term/awaiting-approval")]
        public HttpResponseMessage GetLoanBookingAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetTermLoanBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/approvers/{operationId}")]
        public HttpResponseMessage GetLoanOperationApprovers(int operationId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetLoanOperationApprovers(operationId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/revolving/awaiting-approval")]
        public HttpResponseMessage GetRevolvingLoanBookingAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetRevolvingLoanBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/contingent/awaiting-approval")]
        public HttpResponseMessage GetContingentLoanBookingAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetContingentLoanBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/term/deffered-fees/awaiting-approval")]
        public HttpResponseMessage GetDeferredTermLoanFeeAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetDeferredTermLoanFeeAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/revolving/deffered-fees/awaiting-approval")]
        public HttpResponseMessage GetDeferredRevolvingLoanFeeAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetDeferredRevolvingLoanFeeAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-booking/contingent/deffered-fees/awaiting-approval")]
        public HttpResponseMessage GetDeferredContingentLoanFeeAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetDeferredContingentLoanFeeAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }



         [HttpPost] [ClaimsAuthorization]
        [Route("loan-booking/approval/{loanBookingRequestId}")]
        public HttpResponseMessage ApproveLoanBooking(ApprovalViewModel model, int loanBookingRequestId)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = (short)token.GetBranchId;
                model.staffId = token.GetStaffId;

                var responseId = repo.GoForApproval(model, loanBookingRequestId);

                if (responseId == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
                else if(responseId == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, message = "Loan has been successfully disbursed" });
                }
                else if (responseId == 3)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, message = "Loan disapproval was successful" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Operation unsuccessful, an error occured while saving changes. " });
                }
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-booking/fee-override/approval")]
        public HttpResponseMessage ApproveLoaFeeOverride(ApprovalViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = (short)token.GetBranchId;
                model.staffId = token.GetStaffId;

                var data = repo.GoForFeeOverrideApproval(model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, message = "Loan fee override has been approved successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }


        [HttpGet]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetCustomerLoans(int customerId)
        {
            try
            {
                var data = repo.GetLoanByCustomer(customerId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("customer-group/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupLoans(int customerGroupId)
        {
            try
            {
                var data = repo.GetLoanByCustomerGroup(customerGroupId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        result = data.ToList(),
                        message = "No record found"
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    result = data.ToList(),
                    count = data.Count()
                });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("find/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {
                var data = repo.FindLoan(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-search")]
        public HttpResponseMessage SearchLoan([FromBody] LoanSearchViewModel searchModel)
        {
            try
            {
                var data = repo.LoanSearch(token.GetCompanyId, searchModel);
                //if (!data.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                //}

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("first-pay-date/effective-date/{effectiveDate}/frequency-type/{frequencyTypeId}")]
        public HttpResponseMessage GetFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            try
            {
                var data = scheduleRepo.CalculateFirstPayDate(effectiveDate, frequencyTypeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("periodic-schedule")]
        public HttpResponseMessage GeneratePeriodicLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            try
            {
                var data = scheduleRepo.GeneratePeriodicLoanSchedule(loanInput);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("daily-schedule")]
        public HttpResponseMessage GenerateDailyLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            try
            {
                var data = scheduleRepo.GenerateDailyLoanSchedule(loanInput);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("customer-collateral/")]
        public HttpResponseMessage SearchCustomerCollateral(string searchQuery)
        {
            try
            {
                var data = repo.SearchCustomerCollateral(token.GetCompanyId, searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("customer-collateral/search")]
        public HttpResponseMessage SearchCustomer(string q)
        {
            try
            {
                var data = repo.SearchCustomerCollateral(token.GetCompanyId, q).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("detail/{param}")]
        public HttpResponseMessage GetBookedLoanDetails(string param)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetailsWithParameters(token.GetCompanyId, param).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-schedule")]
        public HttpResponseMessage GetBookedLoanDetailsForReport(ReportSearchParamViewModel param)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetails(token.GetCompanyId, param).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("details/customer/{customerCode}")]
        public HttpResponseMessage GetBookedLoanDetail(string customerCode)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetailsByCustomerCode(customerCode, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("details/reference-number/{loanReferenceNumber}")]
        public HttpResponseMessage GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetailsByLoanReferenceNumber(loanReferenceNumber, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("schedule/export")]
        public HttpResponseMessage ExportScheduleToExcel([FromBody] LoanPaymentScheduleInputViewModel model)
        {
            try
            {
                var fileBytes = scheduleRepo.GenerateLoanScheduleExport(model);

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }

        }

        #endregion Loan

        #region (Loan Application Date) Pre - Loan booking

        [HttpGet]
        [Route("loan-application/availment-completed")]
        public HttpResponseMessage GetAvailedLoanApplicationsDueForInitiateBooking()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetAvailedLoanApplicationsDueForInitiateBooking(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application-details/{applicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationDetails(int applicationDetailId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetLoanApplicationDetails(applicationDetailId, token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception )
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("availed-loan-applications/booking-ready")]
        public HttpResponseMessage GetAvailedLoanApplicationsReadyForBooking()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetAvailedLoanApplicationsReadyForBooking(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }

        }

        [HttpGet]
        [Route("availed-loan-application/booking-ready/{applicationDetailId}")]
        public HttpResponseMessage GetAvailedLoanApplicationDetailById(int applicationDetailId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetAvailedLoanApplicationDetailById(token.GetCompanyId, applicationDetailId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-customer-company-information/{customerId}")]
        public HttpResponseMessage getLoanCustomerCompanyInformation(int customerId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.getLoanCustomerCompanyInformation(customerId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-application/request-booking/{applicationId}")]
        public HttpResponseMessage AddLoanBookingRequest(int applicationId, [FromBody] LoanBookingRequestViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.AddLoanBookingRequest(applicationId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, data = data, message = "Booking successfully initiated!" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = false, message = "Initiating Booking was unsuccessful!" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-application/collateral/customer/{customerId}")]
        public HttpResponseMessage GetCollateralCustomer(int customerId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repoCollateral.GetCustomerCollateral(customerId, null, token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-application/charge-fee/{chargeFeeId}/product/{productId}")]
        public HttpResponseMessage GetLoanProductChargeFee(int chargeFeeId, int productId)
        {
            try
            {
                var response = repo.GetLoanProductChargeFee(chargeFeeId, productId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("loan-product-fees/{productId}")]
        public HttpResponseMessage GetProductFees(int productId)
        {
            try
            {
                var response = repo.GetProductFees(productId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        #endregion (Loan Application Date) Pre - Loan booking


        #region Workflow Tracker

        [HttpGet]
        [Route("work-flow-tracker/operation/{operationId}/target/{targetId}")]
        public async Task<HttpResponseMessage> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId)
        {
            try
            {
                var data = await repo.GetApprovalTrailByOperationIdAndTargetId(operationId, targetId, token.GetCompanyId, token.GetStaffId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        #endregion Workflow Tracker

        #region Loan Disbursement 

        //IEnumerable<LoanDisbursementViewModel> GetAllLoanDisbursement(int loanId);
        //bool AddUpdateLoanDisbursement(LoanDisbursementViewModel entity);
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-disbursement")]
        public HttpResponseMessage GetAllLoanDisbursement(int loanId)
        {
            try
            {
                var data = repo.GetAllLoanDisbursement(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-disbursement")]
        public HttpResponseMessage AddUpdateLoanDisbursement([FromBody]LoanDisbursementViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.loanDisbursementId != 0 || entity.loanDisbursementId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddUpdateLoanDisbursement(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        #endregion
    }
}