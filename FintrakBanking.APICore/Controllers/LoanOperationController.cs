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
using FintrakBanking.Repositories.Credit;
using System.Collections.Generic;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/operations")]
    public class LoanOperationController : ApiControllerBase
    {
        private ILoanOperationsRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        public LoanOperationController(ILoanOperationsRepository _repo)
        {
            this.repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("bulk-rate-customer-excemptions")]
        public HttpResponseMessage GetLoanRateCustomerExcemptions()
        {
            try
            {
                var data = repo.GetLoanRateCustomerExcemptions(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("getrunningloan/{refNo}")]
        public HttpResponseMessage GetRunningLoans(string refNo)
        {
            try
            {
                var data = repo.GetRunningLoans(token.GetCompanyId, refNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-running-fx-revolving-loan/{refNo}")]
        public HttpResponseMessage GetRunningFXLoans(string refNo)
        {
            try
            {
                var data = repo.GetRunningFXLoans(token.GetCompanyId, refNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("commercial-loans-lines")]
        public HttpResponseMessage GetCommercialLoansLines()
        {
            try
            {
                var data = repo.GetCommercialLoansLines(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("maturity-instruction-type")]
        public HttpResponseMessage GetMaturityInstructionType()
        {
            try
            {
                var data = repo.GetMaturityInstructionType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("due-commercial-loans")]
        public HttpResponseMessage GetDueCommercialLoans()
        {
            try
            {
                var data = repo.GetDueCommercialLoans(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("due-commercial-loans/detail/{loanApplicationDetailId}")]
        public HttpResponseMessage GetDueCommercialLoansByApplicationDetailId(int loanApplicationDetailId)
        {
            try
            {
                var data = repo.GetDueCommercialLoansByApplicationDetailId(token.GetCompanyId, loanApplicationDetailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("running-commercial-loans/detail")]
        public HttpResponseMessage GetRunningCommercialLoanLines()
        {
            try
            {
                var data = repo.GetRunningCommercialLoanLines(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("running-commercial-loans/{refNo}")]
        public HttpResponseMessage GetRunningCommercialLoans(string refNo)
        {
            try
            {
                var data = repo.GetRunningCommercialLoans(token.GetCompanyId, refNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

    

        [HttpGet]
        [Route("loan-maturity-instructions")]
        public HttpResponseMessage GetLoanMaturityInstructions()
        {
            try
            {
                var data = repo.GetLoanMaturityInstructions();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("commercial-loan-maturity-instruction")]
        public HttpResponseMessage addMaturityInstruction([FromBody] MaturityIntructionViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;


                //var data = repo.addMaturityInstruction(entity);

                var data = repo.addMaturityInstructionApprove(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Maturity Instruction Successfully Sent For Approval " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("non-term-loan-interest-rate-change")]
        public HttpResponseMessage NonTermLoanInterestRateChange([FromBody] LoanReviewViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                var data = repo.addNonTermLoanLoanRateChangeApprove(entity);

                //var data = repo.addNonTermLoanLoanRateChange(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Interest Rate Change was Successful " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error running this update" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-line-interest-rate-change")]
        public HttpResponseMessage ApplicationLineRateChange([FromBody] LoanReviewViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                //var data = repo.addApplicationLineRateChange(entity);
                var data = repo.addApplicationLineRateChangeApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Interest Rate Change was successfully sent for Approval." });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error running this update" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-line-facility-amount-change")]
        public HttpResponseMessage changeApplicationLineAmount([FromBody] LoanReviewViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                var data = repo.addApplicationLineAmountApproval(entity);

                //var data = repo.changeApplicationLineAmount(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Facility amount change was successfully sent for Approval. " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error running this update" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("commercial-loan-prepayment/{loanReferenceNumber}")]
        public HttpResponseMessage CommercialLoanPrepayment([FromBody] loanPrepaymentViewModel entity, string loanReferenceNumber)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.addCommercialLoanPrepayment(loanReferenceNumber,entity);
                if (data.saveStatus.ToLower() =="saved")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Payment was successful" });
                }
                else if(data.saveStatus == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New payment result generated" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error running this update" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("commercial-loan-roll-over")]
        public HttpResponseMessage ProcessCommercialPaperRollOver([FromBody] MaturityIntructionViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.ProcessCommercialPaperManualRollOverApproval(entity, null);

                //var data = repo.ProcessCommercialPaperManualRollOver(entity, null);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Loan Rollover process was Sent For Approval." });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error processing rollover for this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-go-for-approval")]
        public HttpResponseMessage CPFXApplicationGoForApproval([FromBody] ApprovalViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.BranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;

                // var data = repo.addApplicationLineTenorChangeApproval(entity);

                var data = repo.addApplicationGoForApproval(entity);

                if (data == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Operation Approved Successfully." });
                }
                else if (data == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation details has been disapproved." });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office." });
                }
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("non-term-loan-tenor-extension")]
        public HttpResponseMessage addNonTermLoanTenorReview([FromBody] LoanReviewViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                //var data = repo.addNonTermLoanTenorReview(entity);
                var data = repo.addNonTermLoanTenorReviewApprove(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Tenor Change successfully sent for Approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error processing tenor extension for this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception ab)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: { ab.Message } an error occured" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("line-operation-awaiting-approval")]
        public HttpResponseMessage GetApplicationLineTenorChangeAwaitingApproval()
        {
            try
            {
                var data = repo.GetApplicationLineTenorChangeAwaitingApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-line-tenor-extension")]
        public HttpResponseMessage addApplicationLineTenorChange([FromBody] LoanReviewViewModel entity )
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                var data = repo.addApplicationLineTenorChangeApproval(entity);

                //var data = repo.addApplicationLineTenorChange(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Tenor successfully sent for Approval." });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Record is Still Being Processed For Approval." });

                //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error processing tenor extension for this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-line-tenor-extension-approve")]
        public HttpResponseMessage LineTenorChangeGoForApproval([FromBody] ApprovalViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.BranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;

                // var data = repo.addApplicationLineTenorChangeApproval(entity);

                var data = repo.addApplicationLineTenorChange(entity);

                if (data == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Tenor Approved Successfully." });
                }
                else if (data == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Tenor Change details has been disapproved." });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office." });
                }
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("commercial-loan-sub-allocation")]
        public HttpResponseMessage CommercialPaperSubAllocation([FromBody] List<subAllocationViewModel> models)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                foreach (var entity in models)
                {
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;
                }

                var data = repo.CommercialPaperSubAllocation(models);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Sub-Allocation was successfull." });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error committing this transaction" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }


        [HttpGet] [ClaimsAuthorization]  
        [Route("new-interest-rate-review")]
        public HttpResponseMessage GetNewInterestRateReviews()
        {
            try
            {
                var data = repo.GetNewInterestRateReviews(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-charge-fee-byloanid/")]
        public HttpResponseMessage GetLoanChargeFeeByLoanId(int loanId)
        {
            try
            {
                var data = repo.GetLoanChargeFeeByLoanId(loanId);
                if(data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data,  });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record found."});

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }

         [HttpPost] [ClaimsAuthorization]
        [Route("add-bulk-rate-excemption")]
        public HttpResponseMessage addBulkLoanRateExcemptions([FromBody] LoanViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.branchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.addBulkRateLoanExcemptions(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Bulk Rate Loan Excemption Successfully Added " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("interest-rate-change")]
        public HttpResponseMessage addBulkInterestRateChange([FromBody] LoanBulkInterestReviewViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.addInterestRateChange(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Interst Rate Successfully Added " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        //
         [HttpPost] [ClaimsAuthorization]
        [Route("bulk-interest-rate-change/application")]
        public HttpResponseMessage AddLoanScheduleByBulkRate([FromBody] LoanBulkInterestReviewViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.BulkRateReview(entity.productPriceIndexId, entity.newInterestRate, entity.effectiveDate, token.GetStaffId, (int)OperationsEnum.TermLoanBooking);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Interst Rate Successfully Added " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

    }
}