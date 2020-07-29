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
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Extensions;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using System.Globalization;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/loan")]
    public class LoanController : ApiControllerBase
    {
        private ILoanRepository repo;
        private ICustomerCollateralRepository repoCollateral;
        private ICustomerRepository repoCustomer;
        private ILoanScheduleRepository scheduleRepo;
        private ILoanOperationsRepository loanoperations;
        private IProductRepository productRepo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ExportDataTableToExcel export = new ExportDataTableToExcel();


        //private IHostingEnvironment _hostingEnvironment;
        //private IHostingEnvironment _hostingEnvironment;
        //TokenDecryptionHelper token = new TokenDecryptionHelper();
        public LoanController(ILoanRepository _repo,
                              ICustomerCollateralRepository _repoCollateral,
                              ICustomerRepository _repoCustomer,
                               ILoanScheduleRepository _scheduleRepo,
                               IProductRepository _productRepo, ILoanOperationsRepository _loanoperations)
        {
            this.repo = _repo;
            this.repoCollateral = _repoCollateral;
            this.repoCustomer = _repoCustomer;
            this.scheduleRepo = _scheduleRepo;
            this.productRepo = _productRepo;
            this.loanoperations = _loanoperations;

            //this._hostingEnvironment = hostingEnvironment;
        }

        #region Loan


        [HttpPost]
        [ClaimsAuthorization]
        [Route("current-exposure/customer/{loanTypeId}")]
        public HttpResponseMessage GetCurrentCustomerExposure([FromBody] List<CustomerExposure> customer, int loanTypeId)
        {
            var data = repo.GetCurrentCustomerExposure(customer, loanTypeId, token.GetCompanyId);
            //if (!data.Any())
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            //}

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("current-camsol/customer/{loanTypeId}")]
        public HttpResponseMessage GetCurrentCamsolByCustomer([FromBody] List<CustomerExposure> customer, int loanTypeId)
        {
            var data = repo.GetCurrentCamsolByCustomer(customer, loanTypeId, token.GetCompanyId);
            //if (!data.Any())
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            //}
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("facility-summary/application/{applicationId}")]
        public HttpResponseMessage GetApplicationFacilitySummary(int applicationId)
        {
            List<CurrentCustomerExposure> data = repo.GetApplicationFacilitySummary(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }


        [HttpGet]
        [Route("running-loan/customer/{id}")]
        public HttpResponseMessage GetAllLoanTypes(int id)
        {
            var data = repo.RunningLoans(id, token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("exchange-rate/{fromCode}/{toCode}/{rateCode}")]
        public HttpResponseMessage GetExchangeRate(string fromCode ,string toCode,string rateCode)
        {
            var data = repo.GetExchangeRate(fromCode, toCode, rateCode);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-accounts/balance/{casaAccountId}")]
        public HttpResponseMessage GetCASABalanceById(int casaAccountId)
        {
            var data = repo.GetCASABalanceById(casaAccountId,token.GetCompanyId);
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Account Number do not exist" });
            }
                
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("revolving-types")]
        public HttpResponseMessage GetRevolvingLoanTypes()
        {
            var data = repo.GetRevolvingLoanTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-transaction-dynamics/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanTransactionDynamics(int loanApplicationDetailId)
        {
            var data = repo.GetLoanTransactionDynamics(loanApplicationDetailId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("temporary-overdraft-revolving-types")]
        public HttpResponseMessage GetTemporaryOverdrafts()
        {
            var data = repo.GetTemporaryOverdrafts();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-types")]
        public HttpResponseMessage GetLoanApplicationTypes()
        {
            var data = repo.GetLoanApplicationTypes();
            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-detail-covenant/{applicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationDetailCovenantById(int applicationDetailId)
        {
            var data = repo.GetLoanApplicationDetailCovenantById(applicationDetailId);
            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-product-fees/booking-request/{loanBookingRequestId}")]
        public HttpResponseMessage GetLoanProductFees(int loanBookingRequestId)
        {
            var response = repo.GetLoanProductFees(loanBookingRequestId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

       

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-schedule-category")]
        public HttpResponseMessage GetAllLoanScheduleCategory()
        {
            var data = scheduleRepo.GetAllLoanScheduleCategory();
            //if (!data.Any())
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            //}

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-schedule-types")]
        public HttpResponseMessage GetAllLoanScheduleType()
        {
            var data = scheduleRepo.GetAllLoanScheduleType();
            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-schedule-types/{productTypeId}")]
        public HttpResponseMessage GetAllLoanScheduleType(short? productTypeId)
        {
            var data = scheduleRepo.GetAllLoanScheduleType(productTypeId);
            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-schedule-types/category/{categoryId}")]
        public HttpResponseMessage GetLoanScheduleTypeByCategory(short categoryId)
        {
            var data = scheduleRepo.GetLoanScheduleTypeByCategory(categoryId);
            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-booking")]
        public HttpResponseMessage AddLoanBooking([FromBody] LoanViewModel entity)
        {
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;



            var data = repo.AddLoanBooking(entity);
            if (data != "")
            {
                if (entity.productTypeId == (short)LoanProductTypeEnum.CommercialLoan
                    || entity.productTypeId == (short)LoanProductTypeEnum.TermLoan
                    || entity.productTypeId == (short)LoanProductTypeEnum.SelfLiquidating
                    || entity.productTypeId == (short)LoanProductTypeEnum.ForeignXRevolving
                    || entity.productTypeId == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                {
                    if (entity.isInEditMode)
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = $"Loan Loan with Account Number: '{ data}' was successfully modified." });
                    else
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Loan booking was successfully initiated and is waiting authorization.\r\n Loan Account Number: " + data });
                }

                if (entity.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                {
                    if (entity.isInEditMode)
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = $"Overdaft with Account Number: '{ data}' was successfully modified." });
                    else
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Revolving facility booking was successfully initiated and is awaiting authorization.\r\n Facility Account Number: " + data });
                }

                if (entity.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                {
                    if (entity.isInEditMode)
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = $"contigent facility with Account Number: '{ data}' was successfully modified." });
                    else
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Contingent facility booking was successfully initiated and is awaiting authorization.\r\n Facility Account Number: " + data });
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-existing-loan")]
        public HttpResponseMessage AddExistingLoan([FromBody] LoanViewModel entity)
        {
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.customerSensitivityLevelId = 1;

            var data = repo.AddExistingLoan(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = entity.productTypeName + " successfully imported" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error importing this record" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("maintain-facility-line")]
        public HttpResponseMessage UpdateFacilityLineStatus([FromBody] LoanViewModel entity)
        {
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;



            var data = repo.UpdateFacilityLineStatus(entity);
            if (data)
            {
              return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Customer's line facility successfully maintained." });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Line could not be maintained. An error occured." });
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-customer-accounts/{customerId}/application-detail/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanCustomerAccounts(int customerId, int loanApplicationDetailId)
        {
            var data = repo.GetLoanCustomerAccounts(customerId, loanApplicationDetailId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-tranches/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanByApplicationDetailId(int loanApplicationDetailId)
        {
            var data = repo.GetLoanByApplicationDetailId(loanApplicationDetailId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-lines/{customerId}")]
        public HttpResponseMessage GetCustomerLines(int customerId)
        {
            var data = repo.GetCustomerLines(customerId);
            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [Route("loan-tranche-history/{loanReferenceNumber}")]
        public HttpResponseMessage GetLoanHistoryByLoanAccountNumber(string loanReferenceNumber)
        {
            var data = repo.GetLoanHistoryByLoanAccountNumber(loanReferenceNumber);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("loan-request/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanRequestsByApplicationDetailId(int loanApplicationDetailId)
        {
            var data = repo.GetLoanRequestsByApplicationDetailId(loanApplicationDetailId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("gaurantor/product-type/{productTypeId}/application/{applicationReferenceNumber}")]
        public HttpResponseMessage AddLoanGuarantor([FromBody] LoanGuarantorViewModel entity, short productTypeId, int applicationReferenceNumber)
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


        [HttpGet]
        [Route("appraisal-loan-details-updates/{appraisalMemorandumId}")]
        public HttpResponseMessage GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId)
        {
            var data = repo.GetAppraisalMemorandumLoanUpdates(appraisalMemorandumId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }



        [HttpGet]
        [Route("monitoring-trigger")]
        public HttpResponseMessage GetLoanMonitoringTrigger()
        {
            var data = repo.GetLoanMonitoringTrigger();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("loan-status")]
        public HttpResponseMessage GetLoanStatus()
        {
            var data = repo.GetLoanStatus(token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("monitoring-trigger/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanMonitoringTriggerByLoanApplicationDetailId(int loanApplicationDetailId)
        {
            var data = repo.GetLoanMonitoringTriggerByLoanApplicationDetailId(loanApplicationDetailId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("loan-application-collateral/{loanApplicationId}")]
        public HttpResponseMessage GetLoanApplicationCollateralsByApplicationId(int loanApplicationId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetLoanApplicationCollateralsByApplicationId(loanApplicationId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        //[HttpGet]
        //[Route("loan-booking/request/approval")]
        //public HttpResponseMessage GetInitiatedLoanApplicationAwaitingApproval()
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //        var data = repo.GetBookingRequestAwaitingApproval(token.GetStaffId, token.GetCompanyId, false);

        //        if (data.Any() == false)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        //    }
        //    catch (ConditionNotMetException ce)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
        //    }
        //    catch (BadLogicException be)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
        //    }
        //}

        [HttpGet]
        [Route("number-of-installments/tenor-mode/{tenorModeId}/frequency-type/{frequencyTypeId}/tenor/{tenor}")]
        public HttpResponseMessage GetNumberOfInstallments(short tenorModeId, short frequencyTypeId, int tenor)
        {
            var data = scheduleRepo.CalculateNumberOfInstallments((TenorModeEnum)tenorModeId, frequencyTypeId, tenor);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("{loanId}")]
        public HttpResponseMessage GetLoan(int loanId)
        {
            var data = repo.GetLoan(loanId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }


        [HttpGet]
        [Route("loan-booking/approvers/{operationId}")]
        public HttpResponseMessage GetLoanOperationApprovers(int operationId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetLoanOperationApprovers(operationId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }


        [HttpGet]
        [Route("loan-booking/term/awaiting-approval")]
        public HttpResponseMessage GetLoanBookingAwaitingApproval()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetLoanFacilityBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("loan-booking/verification/awaiting-approval")]
        public HttpResponseMessage GetBookedLoanApplicationForBookingVerification()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetBookedLoanApplicationForBookingVerification(token.GetStaffId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("loan-booking/verification/awaiting-approval-param/{searchString}")]
        public HttpResponseMessage getBookedLoanApplicationsForVerificationAwaitingApprovalParam(string searchString)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetBookedLoanApplicationForBookingVerificationParam(token.GetStaffId, token.GetCompanyId, searchString);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("facility-line-maintenance-awaiting-approval")]
        public HttpResponseMessage GetFacilityLineAwaitingApproval()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetFacilityLineAwaitingMaintenanceApproval(token.GetStaffId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("loans-disbursed")]
        public HttpResponseMessage GetdisbursedLoansApplicationDetails()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetdisbursedLoansApplicationDetails(token.GetStaffId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("loan-booking/revolving/awaiting-approval")]
        public HttpResponseMessage GetRevolvingFacilityBookingAwaitingApproval()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetRevolvingFacilityBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found", });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("loan-booking/contingent/awaiting-approval")]
        public HttpResponseMessage GetContingentFacilityBookingAwaitingApproval()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetContingentFacilityBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpGet]
        [Route("commercial-loans")]
        public HttpResponseMessage GetLoanCommercialLoans()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetLoanCommercialLoans(token.GetCompanyId);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }


        [HttpGet]
        [Route("full-and-final-status")]
        public HttpResponseMessage GetFullAndFinalStatus()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetFullAndFinalStatus();

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-booking/approval/{loanBookingRequestId}/{isManual}")]
        public HttpResponseMessage ApproveLoanBooking([FromBody] ApprovalViewModel model, int loanBookingRequestId, bool isManual)
        {
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.BranchId = (short)token.GetBranchId;
            model.staffId = token.GetStaffId;

            var responseId = repo.GoForApproval(model, loanBookingRequestId, isManual);
            var dynamicMessage = string.Empty;
            if (responseId == 1)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            else if (responseId == 2)
            {
                dynamicMessage = "Loan has been successfully disbursed and forward for Filling";
                if (model.operationId == (short)OperationsEnum.RevolvingLoanBooking)
                    dynamicMessage = "Overdraft facility grant successfully committed and forward for Filling";
                if (model.operationId == (short)OperationsEnum.ContigentLoanBooking)
                    dynamicMessage = "Contingent Liability has been committed successfully and forward for Filling";
                if (model.operationId == (short)OperationsEnum.TermLoanBooking)
                    dynamicMessage = "Loan has been successfully disbursed and forward for Filling";
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = dynamicMessage });
            }
            else if (responseId == 3)
            {
                dynamicMessage = "Loan disapproval was successful";
                if (model.operationId == (short)OperationsEnum.RevolvingLoanBooking)
                    dynamicMessage = "Overdraft facility grant disapproved";
                if (model.operationId == (short)OperationsEnum.ContigentLoanBooking)
                    dynamicMessage = "Contingent Liability disapproved";
                if (model.operationId == (short)OperationsEnum.TermLoanBooking)
                    dynamicMessage = "Loan disapproval was successful";

                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = dynamicMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Operation unsuccessful, an error occured while saving changes. " });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-booking/verification/{loanBookingRequestId}")]
        public HttpResponseMessage ApproveLoanBookingVerification([FromBody] ApprovalViewModel model, int loanBookingRequestId)
        {
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.BranchId = (short)token.GetBranchId;
            model.staffId = token.GetStaffId;

            var responseId = repo.GoForApproval(model, loanBookingRequestId);
            var dynamicMessage = string.Empty;
            if (responseId == 1)
            {
                dynamicMessage = "Facility booking has been successfully authorized";
                if (model.operationId == (short)OperationsEnum.RevolvingLoanBooking)
                    dynamicMessage = "Overdraft booking successfully authorized";
                if (model.operationId == (short)OperationsEnum.ContigentLoanBooking)
                    dynamicMessage = "Contingent Liability booking successfully authorized";
                if (model.operationId == (short)OperationsEnum.TermLoanBooking)
                    dynamicMessage = "Loan booking has been successfully authorized";
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = dynamicMessage });
            }
            else if (responseId == 2)
            {
                dynamicMessage = "Loan booking has been successfully completed";
                if (model.operationId == (short)OperationsEnum.RevolvingLoanBooking)
                    dynamicMessage = "Overdraft booking successfully completed";
                if (model.operationId == (short)OperationsEnum.ContigentLoanBooking)
                    dynamicMessage = "Contingent Liability successfully released";
                if (model.operationId == (short)OperationsEnum.TermLoanBooking)
                    dynamicMessage = "Loan booking has been successfully completed";
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = dynamicMessage });
            }
            else if (responseId == 3)
            {
                dynamicMessage = "Loan booking authorization declined.";
                if (model.operationId == (short)OperationsEnum.RevolvingLoanBooking)
                    dynamicMessage = "Overdraft facility booking authorization declined.";
                if (model.operationId == (short)OperationsEnum.ContigentLoanBooking)
                    dynamicMessage = "Contingent Liability booking authorization declined.";
                if (model.operationId == (short)OperationsEnum.TermLoanBooking)
                    dynamicMessage = "Loan booking authorization declined.";

                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = dynamicMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Operation unsuccessful, an error occured while saving changes. " });
            }
        }

        [HttpGet]
        [Route("loan-facility-awaiting-booking/{searchString}")]
        public HttpResponseMessage getLoanFacilitiesAwaitingApprovalByParam(string searchString)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.getLoanFacilitiesAwaitingApprovalByParam(token.GetCompanyId, token.GetStaffId, searchString);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });

        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("loan-request/approval/{loanBookingRequestId}")]
        //public HttpResponseMessage ApproveInitiatedLoanBooking([FromBody] ApprovalViewModel model, int loanBookingRequestId)
        //{
        //    model.applicationUrl = HttpContext.Current.Request.Path;
        //    model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //    model.createdBy = token.GetStaffId;
        //    model.companyId = token.GetCompanyId;
        //    model.BranchId = (short)token.GetBranchId;
        //    model.staffId = token.GetStaffId;

        //    var responseId = repo.GoForBookingRequestApproval(model, loanBookingRequestId);

        //    //try
        //    //{
        //    //    model.applicationUrl = HttpContext.Current.Request.Path;
        //    //    model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //    //    model.createdBy = token.GetStaffId;
        //    //    model.companyId = token.GetCompanyId;
        //    //    model.BranchId = (short) token.GetBranchId;
        //    //    model.staffId = token.GetStaffId;

        //    //    WorkflowResponse response = repo.GoForBookingRequestApproval(model, loanBookingRequestId);

        //    //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Operation successful, request has been routed to the next approving office" });
        //    //}
        //    //catch (SecureException ex)
        //    //{
        //    //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    //}

        //    if (responseId == 1)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, message = "Operation successful, request has been routed to the next approving office" });
        //    }
        //    else if (responseId == 0)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //                                new { success = true, message = "Loan request has been successfully approved" });
        //    }
        //    else if (responseId == 3)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //                                new { success = true, message = "Loan request was successfully disapproved" });
        //    }
        //    else
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = false, message = "Operation unsuccessful, an error occured while saving changes. " });
        //    }
        //}

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-booking/fee-override/approval")]
        public HttpResponseMessage ApproveLoaFeeOverride([FromBody] ApprovalViewModel model)
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
        
        [HttpGet]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetCustomerLoans(int customerId)
        {
            var data = repo.GetLoanByCustomer(customerId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }


        [HttpGet]
        [Route("customer-loan-booking-override/{customerCode}")]
        public HttpResponseMessage getBookingOverride(string customerCode)
        {
            var data = repo.getBookingOverride(customerCode);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }


        [HttpGet]
        [Route("existing-loans/{applicationId}")]
        public HttpResponseMessage GetLoanApplicationExistingLoans(int applicationId)
        {
            List<LoanViewModel> data = repo.GetLoanApplicationExistingLoans(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [Route("customer-group/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupLoans(int customerGroupId)
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

        [HttpGet]
        [Route("find/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            var data = repo.FindLoan(searchCriteria, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-search")]
        public HttpResponseMessage SearchLoan([FromBody] LoanSearchViewModel searchModel)
        {
            var data = repo.LoanSearch(token.GetCompanyId, searchModel);
            //if (!data.Any())
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            //}

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-collaterals/{loanId}/{loanSystemTypeId}")]
        public HttpResponseMessage GetLoanCollateral(int loanId, int loanSystemTypeId)
        {
            var data = repo.GetLoanCollateral(loanId, loanSystemTypeId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("foreign-loan-naration/{loanId}")]
        public HttpResponseMessage GetForeignLoanBeneficiaryNaration(int loanId)
        {
            var data = repo.GetForeignLoanBeneficiaryNaration(loanId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-monitoring-triggers/{loanId}/{loanSystemTypeId}")]
        public HttpResponseMessage GetLoanMonitoringTriggers(int loanId, int loanSystemTypeId)
        {
            var data = repo.GetLoanMonitoringTriggers(loanId, loanSystemTypeId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }

        [HttpGet]
        [Route("first-pay-date/effective-date/{effectiveDate}/frequency-type/{frequencyTypeId}")]
        public HttpResponseMessage GetFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            var data = scheduleRepo.CalculateFirstPayDate(effectiveDate, frequencyTypeId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("periodic-schedule")]
        public HttpResponseMessage GeneratePeriodicLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            var data = scheduleRepo.GeneratePeriodicLoanSchedule(loanInput);

            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("periodic-prepayment-schedule")]
        public HttpResponseMessage GeneratePeriodicPrepaymentLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            var data = loanoperations.GeneratePrepaymentSchedule(loanInput);

            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("daily-schedule")]
        public HttpResponseMessage GenerateDailyLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            var data = scheduleRepo.GenerateDailyLoanSchedule(loanInput);

            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("customer-collateral/")]
        public HttpResponseMessage SearchCustomerCollateral(string searchQuery)
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

        [HttpGet]
        [Route("customer-collateral/search")]
        public HttpResponseMessage SearchCustomer(string q)
        {
            var data = repo.SearchCustomerCollateral(token.GetCompanyId, q).ToList();
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
        }

        [HttpGet]
        [Route("detail/{param}")]
        public HttpResponseMessage GetBookedLoanDetails(string param)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();

            var data = repo.GetBookedLoanDetailsWithParameters(token.GetCompanyId, param).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-schedule")]
        public HttpResponseMessage GetBookedLoanDetailsForReport(ReportSearchParamViewModel param)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();

            var data = repo.GetBookedLoanDetails(token.GetCompanyId, param).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("details/customer/{customerCode}")]
        public HttpResponseMessage GetBookedLoanDetail(string customerCode)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();

            var data = repo.GetBookedLoanDetailsByCustomerCode(customerCode, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [Route("details/reference-number/{loanReferenceNumber}")]
        public HttpResponseMessage GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();

            var data = repo.GetBookedLoanDetailsByLoanReferenceNumber(loanReferenceNumber, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("schedule/export")]
        public HttpResponseMessage ExportScheduleToExcel([FromBody] LoanPaymentScheduleInputViewModel model)
        {
            var fileBytes = scheduleRepo.GenerateLoanScheduleExport(model);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-booking-modification/refer-back")]
        public HttpResponseMessage ReferBackBooking([FromBody] ApprovalViewModel entity)
        {
            entity.BranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            var response = repo.ReferBackBooking(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Request was sent to " + response.nextPersonName + " successful" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("referred-booked-facility-record")]
        public HttpResponseMessage GetReferedBookingFacilityRecordsById([FromBody] CamProcessedLoanViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            var data = repo.GetReferedBookingFacilityRecordsById(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }


        #endregion Loan

        #region Frequency Type
        [HttpGet]
        [ClaimsAuthorization]
        [Route("limit-frequency-type")]
        public HttpResponseMessage GetAllFrequencyType()
        {
            var response = repo.GetAllFrequencyType();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }
        #endregion

        #region (Loan Application Date) Pre - Loan booking

        //[HttpGet]
        //[Route("loan-application/availment-completed")]
        //public HttpResponseMessage GetAvailedLoanApplicationsDueForInitiateBooking()
        //{
        //    TokenDecryptionHelper token = new TokenDecryptionHelper();
        //    try
        //    {
        //        var response = repo.GetAvailedLoanApplicationsDueForInitiateBooking(token.GetCompanyId, token.GetStaffId, token.GetBranchId);
        //        if (!response.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        //    }
        //    catch (ConditionNotMetException ce)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
        //    }
        //    catch (BadLogicException be)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

        //[HttpGet]
        //[Route("loan-application/adhoc-approval")]
        //public HttpResponseMessage getApplicationsToBeAdhocApprovedForInitiateBooking()
        //{
        //    TokenDecryptionHelper token = new TokenDecryptionHelper();
        //    try
        //    {
        //        var response = repo.getApplicationsToBeAdhocApprovedForInitiateBooking(token.GetCompanyId, token.GetStaffId, token.GetBranchId);
        //        if (!response.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        //    }
        //    catch (ConditionNotMetException ce)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
        //    }
        //    catch (BadLogicException be)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}


        [HttpGet]
        [Route("loan-application-details/{applicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationDetails(int applicationDetailId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetLoanApplicationDetails(applicationDetailId, token.GetCompanyId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpGet]
        [Route("availed-loan-applications/crms-code-ready")]
        public HttpResponseMessage GetAvailedLoanApplicationsReadyForCrmsCode()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetAvailedLoanApplicationsReadyForCrmsCode(token.GetCompanyId, token.GetStaffId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });

        }

        [HttpGet]
        [Route("loan-application-detail")]
        public HttpResponseMessage GetApprovedLoanApplicationsDetail()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetAvailedLoanApplicationsReadyForBooking(token.GetCompanyId, token.GetStaffId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });

        }


        [HttpGet]
        [Route("availed-loan-applications/booking-ready")]
        public HttpResponseMessage GetAvailedLoanApplicationsReadyForBooking()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetAvailedLoanApplicationsReadyForBooking(token.GetCompanyId, token.GetStaffId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });

        }

        [HttpGet]
        [Route("availed-contingent-facility-for-release")]
        public HttpResponseMessage GetAvailedContingentFacilityBooking()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetAvailedContingentFacilityBooking(token.GetCompanyId, token.GetStaffId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });

        }

        [HttpGet]
        [Route("commercial-loans/application-detail/{loanApplicationDetailId}")]
        public HttpResponseMessage GetAvailedLoanApplicationsDueForInitiateBooking(int loanApplicationDetailId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetCommercialLoanByApplicationDetailId(loanApplicationDetailId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [Route("requested-loan-booking/{loanBookingRequestId}/application-detail/{applicationDetailId}")]
        public HttpResponseMessage GetAvailedLoanApplicationDetailById(int applicationDetailId, int loanBookingRequestId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetAvailedLoanApplicationDetailById(token.GetStaffId, token.GetCompanyId, applicationDetailId, loanBookingRequestId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [Route("loan-customer-company-information/{customerId}")]
        public HttpResponseMessage getLoanCustomerCompanyInformation(int customerId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.getLoanCustomerCompanyInformation(customerId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

  


        [HttpGet]
        [Route("loan-application/collateral/customer/{customerId}")]
        public HttpResponseMessage GetCollateralCustomer(int customerId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repoCollateral.GetCustomerCollateral(customerId, null, token.GetCompanyId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [Route("loan-application/charge-fee/{chargeFeeId}/product/{productId}")]
        public HttpResponseMessage GetLoanProductChargeFee(int chargeFeeId, int productId)
        {
            var response = repo.GetLoanProductChargeFee(chargeFeeId, productId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [Route("loan-product-fees/{productId}")]
        public HttpResponseMessage GetProductFees(int productId)
        {
            var response = repo.GetProductFees(productId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
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
            catch (SecureException ex)
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-interest-rate-amount")]
        public HttpResponseMessage getDiscountedCPInterestAmount([FromBody] LoanViewModel entity)
        {
            var loanProductInfo = productRepo.GetProductById(entity.productId);
            var isDicounted = false;
                if(loanProductInfo != null) isDicounted = loanProductInfo.dealTypeId == (short)DealTypeEnum.Upfront ? true : false;

            var data = repo.getLoanInterestRateAmount(entity.principalAmount, entity.interestRate, entity.effectiveDate, entity.maturityDate,(DayCountConventionEnum)entity.scheduleDayCountConventionId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation successful" , result = data, isDicounted = isDicounted });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-repricing-mode")]
        public HttpResponseMessage GetLoanRepricingModes()
        {
            try
            {
                var data = repo.GetLoanRepricingModes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("loan-disbursement")]
        //public HttpResponseMessage AddUpdateLoanDisbursement([FromBody]LoanDisbursementViewModel entity)
        //{
        //    try
        //    {
        //        string createUpdate = "";
        //        if (entity.loanDisbursementId != 0 || entity.loanDisbursementId < 0)
        //        {
        //            createUpdate = "updated";
        //        }
        //        else
        //        {
        //            createUpdate = "created";
        //        }
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.companyId = (short)token.GetCompanyId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;
        //        entity.staffId = token.GetStaffId;

        //        var data = repo.AddUpdateLoanDisbursement(entity);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = $"There was an error {createUpdate} this record" });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = $"There was an error creating this record {e.Message}" });
        //    }
        //}
        #endregion

        #region

        [HttpGet]
        [ClaimsAuthorization]
        [Route("completed-loan")]
        public HttpResponseMessage GetCompletedLoan()
        {
            try
            {
                var data = repo.GetCompletedLoans();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("completed-loan/search/{value}")]
        public HttpResponseMessage GetCompletedLoan(string value)
        {
            try
            {
                var data = repo.GetCompletedLoan(value);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("completed-loan-status")]
        public HttpResponseMessage GetChangeLoanStatusOfACompletedLoan([FromBody]int loanid)
        {
            try
            {
                var data = repo.GetChangeLoanStatusOfACompletedLoan(loanid);
                if (data == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion




        //    [HttpPost]
        //    [ClaimsAuthorization]
        //    [Route("two-factor-auth-enabled-fee-override")]
        //    public HttpResponseMessage TwoFactorAuthenticationEnabledWithoutFeeOverride([FromBody]LoanViewModel entity)
        //    {
        //        try
        //        {
        //            TokenDecryptionHelper token = new TokenDecryptionHelper();

        //            entity.userBranchId = (short)token.GetBranchId;
        //            entity.applicationUrl = HttpContext.Current.Request.Path;
        //            entity.createdBy = token.GetStaffId;
        //            entity.companyId = token.GetCompanyId;

        //            var data = repo.TwoFactorAuthenticationEnabledWithoutFeeOverride(entity);
        //            if (!data)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK,
        //                   new { success = false, result = data, message = "" });
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                   new { success = true, result = data });
        //        }
        //        catch (System.Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                  new { success = false, message = ex.Message });
        //        }
        //    }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-balance/{loanId}")]
        public HttpResponseMessage GetLoanBalances(int loanId)
        {
            var response = repo.GetLoanBalances(loanId, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("legal-contingent-code-validation/{legalContingentCode}/{loanApplicationDetailId}")]
        public HttpResponseMessage VerifyLegalContingentCode(string legalContingentCode, int loanApplicationDetailId)
        {
            var response = repo.VerifyLegalContingentCode(legalContingentCode, loanApplicationDetailId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, data = response, message = "Success" });
            }
            else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Failed" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("bulk-loan-entries")]
        public HttpResponseMessage saveBulkLoanDisbursementEntries([FromBody] List<multipleDisbursementOutputViewModel> models)
        {
            UserInfo user = new UserInfo();
            user.BranchId = (short)token.GetBranchId;
            user.applicationUrl = HttpContext.Current.Request.Path;
            user.createdBy = token.GetStaffId;
            user.companyId = token.GetCompanyId;

            var data = repo.saveBulkLoanDisbursementEntries(models,user);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, data = data, message = "Bulk loan was successfully submitted for disbursement approval" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,

                new { success = false, message = "saving bulk loan for disbursement approval was unsuccessfully" });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("multiple-disbursement")]
        public HttpResponseMessage disburseMultipleLoans([FromBody] List<multipleDisbursementOutputViewModel> models)
        {
            UserInfo user = new UserInfo();
            user.staffId = token.GetStaffId;
            user.BranchId = (short)token.GetBranchId;
            user.companyId = token.GetCompanyId;
            user.createdBy = token.GetStaffId;

            var data = repo.startBulkLoanDisbursement(models, user);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
           
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("bulk-loan-recovery-assignment/{accreditedConsultant}/{expCompletionDate}")]
        public HttpResponseMessage saveBulkLoanAssignmentToAgent(int accreditedConsultant, DateTime? expCompletionDate, [FromBody] List<LoanRecoveryAssignmentViewModel> models)
        {
            UserInfo user = new UserInfo();
            user.staffId = token.GetStaffId;
            user.BranchId = (short)token.GetBranchId;
            user.companyId = token.GetCompanyId;
            user.createdBy = token.GetStaffId;

            var data = repo.saveBulkLoanAssignmentToAgent(models, accreditedConsultant, expCompletionDate, user);

            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, data = data, message = "Bulk Recovery Successfully Saved" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,

                new { success = false, message = "saving loan recovery assignment unsuccessfully" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("bulk-loan-recovery-assignment-initiate-approval")]
        public HttpResponseMessage bulkLoanAssignmentToAgentGoForApproval([FromBody] LoanRecoveryAssignmentViewModel models)
        {
            UserInfo user = new UserInfo();
            user.staffId = token.GetStaffId;
            user.BranchId = (short)token.GetBranchId;
            user.companyId = token.GetCompanyId;
            user.createdBy = token.GetStaffId;

            var data = repo.bulkLoanAssignmentToAgentGoForApproval(models, user);

            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, data = data, message = "Bulk Recovery Successfully forwarded for approval" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,

                new { success = false, message = "Error occur forwarding for approval" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("collateral-liquidation-recovery-without-file")]
        public HttpResponseMessage AddCollateralLiquidationRecovery([FromBody] CollateralLiquidationRecoveryViewModel models)
        {
            try { 
            UserInfo user = new UserInfo();
            user.staffId = token.GetStaffId;
            user.BranchId = (short)token.GetBranchId;
            user.companyId = token.GetCompanyId;
            user.createdBy = token.GetStaffId;
            models.createdBy = token.GetStaffId;
                var response = repo.AddCollateralLiquidationRecoveryWithoutFile(models);
                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Receipt has been uploaded successfully" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Receipt already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this Receipt:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this Receipt" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("collateral-liquidation-recovery")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddCollateralLiquidationRecovery()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try
            {
                var entity = new CollateralLiquidationRecoveryViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.fileSizeUnit = provider.FormData["fileSizeUnit"];
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.applicationReferenceNumber = provider.FormData["applicationReferenceNumber"];
                entity.loanId = Convert.ToInt32(provider.FormData["loanId"]);
                entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
                entity.accreditedConsultant = Convert.ToInt32(provider.FormData["accreditedConsultant"]);
                entity.loanAssignId = Convert.ToInt32(provider.FormData["loanAssignId"]);
                entity.totalRecoveryAmount = Convert.ToDecimal(provider.FormData["totalRecoveryAmount"]);
                entity.recoveredAmount = Convert.ToDecimal(provider.FormData["recoveredAmount"]);
                entity.collateralCode = provider.FormData["collateralCode"];
                entity.collectionMode = provider.FormData["collectionMode"];
                var receiptDate = provider.FormData["receiptDate"];
                var receiptDateSub = receiptDate.Substring(0, 15);
                entity.receiptDate = DateTime.ParseExact(receiptDateSub, "ddd MMM dd yyyy", CultureInfo.InvariantCulture);
                entity.percentageCommission = Convert.ToDecimal(provider.FormData["percentageCommission"]);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                int response = repo.AddCollateralLiquidationRecovery(entity, buffer);

                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Receipt has been uploaded successfully" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Receipt already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this Receipt:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this Receipt" });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("liquidation-receipt-download/{liquidationRecoveryReceiptId}")]
        public HttpResponseMessage GetLiquidationReceipt(int liquidationRecoveryReceiptId)
        {
            CollateralLiquidationRecoveryViewModel data = repo.GetLiquidationReceipt(liquidationRecoveryReceiptId);
            if (data == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-document-download/{lienRemovalId}")]
        public HttpResponseMessage GetLienReovalLetter(int lienRemovalId)
        {
            RemoveLienViewModel data = repo.GetLienRemovalLetter(lienRemovalId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("saved-multiple-disbursement")]
        public HttpResponseMessage GetpendingMultipleDisbursement()
        {
            var response = repo.GetpendingMultipleDisbursement();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }
        

        [HttpPost]
        [ClaimsAuthorization]
        [Route("pre-multiple-disbursement")]  
        public async Task<HttpResponseMessage> UploadBulkDisbursementData()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);


            var isFinal = Convert.ToBoolean(provider.FormData["isFinal"]);

            var entity = new UserInfo
            {
                BranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            var file = provider.Contents.FirstOrDefault();
            var buffer = await file.ReadAsByteArrayAsync();
            var data = repo.preBulkLoanDisbursement(buffer, entity, isFinal);

            if (buffer != null)
            {
                bool success = true;
                if (data.Item2 == false && isFinal) { success = false; }
                if (!success) { return Request.CreateResponse(HttpStatusCode.OK, new { success = success, result = data.Item1, message = "Bulk loan disbursement failed to uploaded." }); }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = success, result = data.Item1, message = "Bulk Disbursement data was successfully uploaded" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error uploading Bulk Disbursement data" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("work-flow-tracker-booking/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetApprovalTrailByOperationIdAndTargetIdBooking(int operationId, int targetId)
        {
            var data = repo.GetApprovalTrailByOperationIdAndTargetId(operationId, targetId, token.GetCompanyId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

    }
}