using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common.CustomException;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using FintrakBanking.Interfaces.Setups.General;
using System.Globalization;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using System.Threading;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/creditoperations")]
    public class LoanOperationsController : ApiControllerBase
    {
        private ILoanOperationsRepository repo;
        private ILoanRepository loanRepo;
        private IEndOfDayRepository repoEOD;
        private IGeneralSetupRepository generalSetup;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();


        public LoanOperationsController(ILoanOperationsRepository _repo,
            IEndOfDayRepository _repoEOD, ILoanRepository _loanRepo, IGeneralSetupRepository _genSetup)
        {
            this.repo = _repo;
            this.repoEOD = _repoEOD;
            this.loanRepo = _loanRepo;
            this.generalSetup = _genSetup;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("getcollateralsearchchargeamount{stateId}")]
        public HttpResponseMessage GetCollateralSearchChargeAmount(int stateId)
        {
               var data = repo.GetCollateralSearchChargeAmount(stateId);
               if(data != 0) { 
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
              }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        }




        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-bulk-prepayment-reversal")]
        public HttpResponseMessage AddBulkPrepaymentReversal([FromBody] LoanReviewOperationViewModel model)
        {
            //try
            //{
            model.userBranchId = (short)token.GetBranchId;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.approvalStatusId = (int)ApprovalStatusEnum.Pending;

            try
            {
                var response = repo.AddBulkPrepaymentReversal(model, model.companyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }

            //if (response)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //}
            //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operationtype/isFinalOperation/{isFinalOperation}")]
        public HttpResponseMessage GetOperationType(bool isFinalOperation)
        {
            
                var data = repo.GetOperationType(isFinalOperation);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,  new { success = true, result = data });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operationtypebyoverdraft")]
        public HttpResponseMessage GetOperationTypeByOD()
        {
            
                var data = repo.GetOperationTypeByOD();
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
        [Route("loan-remedial-operationtype")]
        public HttpResponseMessage GetRemedialOperationType()
        {
            
                var data = repo.GetRemedialOperationType();
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
        [Route("loan-operationtype/")]
        public HttpResponseMessage GetOperationTypeByLoanId(int productTypeId, int scheduleTypeId)
        {
           
                var data = repo.GetOperationTypeByLoanId((LoanProductTypeEnum)productTypeId, (LoanScheduleTypeEnum)scheduleTypeId);
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
        [Route("loan-review-approval-operationtype/")]
        public HttpResponseMessage GetReviewApprovalOperationTypeByLoanId(int productTypeId, int scheduleTypeId)
        {
            
                var data = repo.GetReviewApprovalOperationTypeByLoanId((LoanProductTypeEnum)productTypeId, (LoanScheduleTypeEnum)scheduleTypeId);
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
        [Route("loan-search/")]
        public HttpResponseMessage SearchForLoan(string searchQuery)
        {
            
                var data = loanRepo.SearchForLoan(searchQuery);
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
        [Route("loan-search-prepayment")]
        public HttpResponseMessage SearchForLoanPrepayment(string searchQuery)
        {
           
                var data = loanRepo.SearchForLoanPrepayment(searchQuery);
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
        [Route("loan-approval-prepayment")]
        public HttpResponseMessage LoanPrepaymentApprovalList()
        {
           
                var data = loanRepo.LoanPrepaymentApprovalList();
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
        [Route("loan-search-inactive-contingent")]
        public HttpResponseMessage SearchForLoanInactiveContingent(string searchQuery)
        {
           
                var data = loanRepo.SearchForLoanInactiveContingent(searchQuery);
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
        [Route("loan-search-contingent")]
        public HttpResponseMessage SearchForLoanContingent(string searchQuery)
        {
                var data = loanRepo.SearchForLoanContingent(searchQuery);
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
        [Route("fx-revolving-loan-search/")]
        public HttpResponseMessage SearchForFXRevolvingLoan(string searchQuery)
        {
                var data = loanRepo.SearchForFXRevolvingLoan(searchQuery);
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
        [Route("running-commercial-and-fx-loan-search/")]
        public HttpResponseMessage SearchRunningCommercialAndFXLoans(string searchQuery)
        {
                var data = loanRepo.SearchRunningCommercialAndFXLoans(searchQuery);
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
        [Route("term-and-revolving-loan-search/")]
        public HttpResponseMessage SearchForLoanAndRevolvingLoan(int productTypeId, string searchQuery)
        {
             //var data = loanRepo.SearchForLoanAndRevolvingLoan(productTypeId, searchQuery);
                var data = loanRepo.SearchForLoanAndRevolvingLoan(searchQuery);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            
        }

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("term-and-revolving-loan-review-search/")]
        //public HttpResponseMessage SearchForLoanAndRevolvingLoanReviewFeeCharge(int productTypeId, string searchQuery)
        //{
        //    try
        //    {
        //        var data = loanRepo.SearchForLoanAndRevolvingLoanReviewFeeCharge(productTypeId, searchQuery);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}


        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-overdraft-review-application")]
        public HttpResponseMessage GetLoanReviewApplicationOverDraft()
        {
           
                var data = loanRepo.GetLoanReviewApplicationOverDraft(token.GetStaffId, token.GetCompanyId);
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
        [Route("approved-overdraft-route-and-review-application")]
        public HttpResponseMessage GetLoanReviewApplicationOverDraftRouteAndOperations()
        {
                var data = loanRepo.GetLoanReviewApplicationOverDraftRouteAndOperations(token.GetStaffId, token.GetCompanyId);
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
        [Route("approved-loan-review-awaiting-operation")]
        public HttpResponseMessage GetApprovedLoanReviewAwaitingOperation()
        {
                var data = loanRepo.GetApprovedLoanReviewAwaitingOperation(token.GetStaffId, token.GetCompanyId);
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
        [Route("overdraft-detail/")]
        public HttpResponseMessage SearchForOverdraft(int revolvingLoanId)
        {
            
                var data = loanRepo.GetOverdraftDetailsByLoanId(revolvingLoanId);
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
        [Route("approved-loan-review")]
        public HttpResponseMessage GetApprovedLoanReview()
        {
                var data = loanRepo.GetApprovedLoanReview(token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-facility-modification/approval")]
        public HttpResponseMessage ApproveFacilityModification([FromBody] ForwardViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            try
            {
                WorkflowResponse response = loanRepo.ApproveLMSFacilityModification(model);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error approving this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-facility-modification/approval")]
        public HttpResponseMessage GetFacilityModificationsForApproval()
        {
            try
            {
                IEnumerable<FacilityModificationViewModel> response = loanRepo.GetLMSFacilityModificationsForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("modify-lms-facility")]
        public HttpResponseMessage ModifyLMSFacility([FromBody] FacilityModificationViewModel model)
        {
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = loanRepo.AddFacilityModification(model);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Error saving record" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("process-review-loan-data/{searchString}")]
        public HttpResponseMessage GetProcessLoanReviewData(string searchString)
        {
            var data = loanRepo.GetProcessLoanReviewData(token.GetCompanyId, token.GetStaffId, searchString);  
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
        [Route("approved-non-term-loan-review")]
        public HttpResponseMessage GetApprovedNonTermLoansForReview()
        {
                var data = loanRepo.GetApprovedNonTermLoansForReview(token.GetStaffId, token.GetCompanyId);
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
        [Route("approved-non-term-loan-review-approval")]
        public HttpResponseMessage GetApprovedNonTermLoansForReviewAwaitingApproval()
        {
                var data = loanRepo.GetApprovedNonTermLoansForReviewAwaitingApproval(token.GetStaffId,token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-prepayment-reversal")]
        public HttpResponseMessage AddPrepaymentReversal([FromBody] LoanReviewOperationViewModel model)
        {
            //try
            //{
            model.userBranchId = (short)token.GetBranchId;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.approvalStatusId = (int)ApprovalStatusEnum.Pending;

            //if ((int)OperationsEnum.Prepayment == model.operationTypeId)

            //{
            //    model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

            //    if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
            //    }

            //    var response = repo.AddOperationReview(model);
            //    if (response)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}
            //else if (model.operationTypeId == (int)OperationsEnum.Fee_chargeChange)
            //{
            //    if (repo.DoesChargeFeeExist(model.loanId, model.operationTypeId, (int)model.interestFrequencyTypeId))
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested charge fee type already exist and going through approval" });
            //    }
            //    var response = repo.AddOperationReview(model);
            //    if (response)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}
            //else
            //{
            //    if (model.principalFirstPaymentDate < model.proposedEffectiveDate)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Principal First Payment Date cannot be less than Effective date" });
            //    }
            //    if (model.interestFirstPaymentDate < model.proposedEffectiveDate)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Interest First Payment Date cannot be less than Effective date" });
            //    }
            //    if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
            //    }


            //    var response = repo.AddOperationReview(model);

            //    if (response)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}


            var response = repo.AddOperationReview(model);

            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            //}
            //catch (ConditionNotMetException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            //}
            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            //}

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-remove-lien")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddRequestUnLienODAccount()
        {
                   

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            Task.Factory
                .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning, // guarantees separate thread
                    TaskScheduler.Default)
                .Wait();


            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try
            {
                var entity = new RemoveLienViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.comment = provider.FormData["comment"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.fileSizeUnit = provider.FormData["fileSizeUnit"];
                entity.casaLienAccountId = Convert.ToInt32(provider.FormData["casaLienAccountId"]);
                entity.loanReferenceNumber = provider.FormData["loanReferenceNumber"];
                entity.overwrite = provider.FormData["overwrite"] == "true";
                var requestDate = provider.FormData["requestDate"];
                var actualrequestDate = requestDate.Substring(0, 15);
                entity.requestDate = DateTime.ParseExact(actualrequestDate, "ddd MMM dd yyyy", CultureInfo.InvariantCulture);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.approvalStatusId = (int)ApprovalStatusEnum.Pending;


                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var response = repo.AddRequestUnLienODAccount(entity, buffer);


                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and waiting for approval" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating request:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating request" });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-search-prepayment-reversal")]
        public HttpResponseMessage SearchForLoanPrepaymentReversal(string searchQuery)
        {
            try
            {

                var data = loanRepo.SearchForLoanPrepaymentReversal(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-loan-review-route")]
        public HttpResponseMessage GetApprovedLoanReviewAwaitingRoute()
        {
                var data = loanRepo.GetApprovedLoanReviewAwaitingRoute(token.GetStaffId, token.GetCompanyId);
                if (data.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
           
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-line-review")]
        public HttpResponseMessage GetApprovedLineReview()
        {
           
                var data = loanRepo.GetApprovedLineReview(token.GetStaffId,token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            
        }


        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("approved-line-review-search/{searchString}")]
        //public HttpResponseMessage GetApprovedLineReviewSerach(string searchString)
        //{

        //    var data = loanRepo.GetApprovedLineReviewSearch(token.GetStaffId, token.GetCompanyId, searchString);
        //    if (data == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = "No record found" });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = true, result = data });

        //}

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("approved-line-review")]
        //public HttpResponseMessage GetApprovedLineReview()
        //{
        //    try
        //    {
        //        var data = loanRepo.GetApprovedLineReview();
        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //               new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //               new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //              new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("approved-fx-revolving-loan-review")]
        //public HttpResponseMessage GetApprovedFXRevolvingLoanReview()
        //{
        //    try
        //    {
        //        var data = loanRepo.GetApprovedFXRevolvingLoanReview();
        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //               new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //               new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //              new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet]
        [Route("approved-loan-review-remedial")]
        public HttpResponseMessage GetApprovedLoanReviewRemedial()
        {
                var userId = token.GetStaffId;

                var data = loanRepo.GetApprovedLoanReviewRemedial(userId, token.GetCompanyId);
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
        [Route("loan-search/full-and-final/{searchQuery}")]
        public HttpResponseMessage SearchForFullAndFinalLoan(string searchQuery)
        {
           
                var data = loanRepo.SearchForFullAndFinalLoan(searchQuery);
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
        [Route("cancel-full-and-final/{loanId}")]
        public HttpResponseMessage CancelFullAndFinal(int loanId)
        {
                var data = loanRepo.CancelFullAndFinal(loanId);
                if (data == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("undisbursed-loan-details/")]
        public HttpResponseMessage SearchForLoansUnDisbursed(int loanId, int loanType)
        {
            var data = loanRepo.GetUnDisbursedLoanByLoanId(loanId, loanType);
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
        [Route("cancel-full-and-final/{loanId}/status/{statusId}")]
        public HttpResponseMessage CancelFullAndFinal(int loanId, int statusId)
        {
            bool response = false;
                if (statusId == (int)FullAndFinalStatusEnum.Cancelled)
                {
                     response = loanRepo.CancelFullAndFinal(loanId, statusId);
                }
                else if (statusId == (int)FullAndFinalStatusEnum.Completed)
                {
                    var loan = repo.GetLoanInformation(loanId);

                LoanReviewOperationViewModel model = new LoanReviewOperationViewModel();

                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.approvalStatusId = (int)ApprovalStatusEnum.Pending;
                model.loanId = loanId;
                model.productTypeId = loan.PRODUCTID;
                model.loanSystemTypeId = loan.LOANSYSTEMTYPEID;
                model.operationTypeId = (int)OperationsEnum.FullAndFinalCompleteWriteOff;
                model.reviewDetails = "Full And Final Complete Write-Off";
                model.approvalStatusId = 0;
                model.operationCompleted = false;
                model.proposedEffectiveDate = generalSetup.GetApplicationDate();

                     response = repo.AddOperationReview(model);
                        
                }

            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, message = "Operation successfully" });
            }

             return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = response.ToString() });
           
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("disbursed-loan-details/")]
        public HttpResponseMessage SearchForLoan(int loanId, int loanType)
        {
                var data = loanRepo.GetDisbursedLoanByLoanId(loanId, loanType);
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
        [Route("disbursed-loan-tranche-details/{loanId}")]
        public HttpResponseMessage getDisbursedCommercialLoanTrancheDetailsById(int loanId)
        {
           
                var data = loanRepo.getDisbursedCommercialLoanTrancheDetailsById(loanId);
                if (data.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("group-customer-loan-details/")]
        public HttpResponseMessage SearchForGroupLoan(int loanId)
        {
            
                var data = loanRepo.GetGroupLoanByLoanId(loanId);
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
        [Route("overdraft-search/")]
        public HttpResponseMessage SearchAllOverdraft(string searchQuery)
        {
           
            var data = loanRepo.SearchAllOverdraft(searchQuery);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-collateral-od/")]
        public HttpResponseMessage GetLoanCollateral(int loanId, int loanType)
        {
           
                var data = loanRepo.GetLoanCollateral(loanId, loanType);
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
        [Route("loan-convenant-od/")]
        public HttpResponseMessage GetLoanConvenant(int loanId, int loanType)
        {
           
                var data = loanRepo.GetLoanCovenant(loanId, loanType);
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
        [Route("loan-chargefee-od/")]
        public HttpResponseMessage GetLoanChargeFee(int loanId, int loanType)
        {
            
                var data = loanRepo.GetLoanChargeFee(loanId, loanType);
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
        [Route("loan-convenant/")]
        public HttpResponseMessage GetLoanConvenant(int loanId)
        {
           
                var data = loanRepo.GetLoanCovenant(loanId);
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
        [Route("loan-chargefee/")]
        public HttpResponseMessage GetLoanChargeFee(int loanId)
        {
            
                var data = loanRepo.GetLoanChargeFee(loanId);
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
        [Route("loan-schedule-details/")]
        public HttpResponseMessage GetOperationTypeByLoanId(int loanId)
        {
            
                var data = loanRepo.GetLoanScheduleByLoanId(loanId);
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
        [Route("loan-operation/awaiting-approval")]
        public HttpResponseMessage GetLoanOperationAwaitingApproval(){
            var data = repo.GetLoanOperationAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/by-loanreference-awaiting-approval/{loanReference}")]
        public HttpResponseMessage GetLoanOperationByLoanReferenceAwaitingApproval(string loanReference)
        {
            var data = repo.GetLoanOperationByLoanReferenceAwaitingApproval(token.GetStaffId, token.GetCompanyId, loanReference);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-removal-operation/awaiting-approval")]
        public HttpResponseMessage GetLienRemovalAwaitingApproval()
        {
            var data = repo.GetLienRemovalAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-search/{searchString}")]
        public HttpResponseMessage GetLienSearchData(string searchString)
        {
            var data = repo.GetLienSearchData(searchString);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-assignment-to-agent/awaiting-approval-list/{source}")]
        public HttpResponseMessage GetLienRemovalAwaitingApprovalList(string source)
        {
            var data = repo.GetBulkRecoveryToAgentAwaitingApprovalList(source, token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-assignment-to-agent/awaiting-approval")]
        public HttpResponseMessage GetBulkRecoveryToAgentAwaitingApproval()
        {
            var data = repo.GetBulkRecoveryToAgentAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-retail-recovery-assignment-to-agent/awaiting-approval")]
        public HttpResponseMessage GetBulkRetailRecoveryToAgentAwaitingApproval()
        {
            var data = repo.GetBulkRetailRecoveryToAgentAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-unassignment-from-agent/awaiting-approval")]
        public HttpResponseMessage getBulkUnassignmentRecoveryFromAgentAwaitingApproval()
        {
            var data = repo.GetBulkUnassignmentRecoveryFromAgentAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-retail-recovery-unassignment-from-agent/awaiting-approval")]
        public HttpResponseMessage getBulkUnassignmentRetailRecoveryFromAgentAwaitingApproval()
        {
            var data = repo.GetBulkUnassignmentRetailRecoveryFromAgentAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-assignment-to-agent/application-list/{source}")]
        public HttpResponseMessage BulkRecoveryToAgentAwaitingApprovalList(string source)
        {
            var data = repo.BulkRecoveryToAgentAwaitingApprovalList(source, token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-removal-operation/documents/{lienRemovalId}")]
        public HttpResponseMessage GetLienRemovalDocuments(int lienRemovalId)
        {
            var data = repo.GetLienRemovalDocuments(lienRemovalId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/awaiting-documentation")]
        public HttpResponseMessage GetLoanOperationAwaitingDocumentation()
        {
            var data = repo.GetLoanOperationDocumentation(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/awaiting-documentation-search/{searchString}")]
        public HttpResponseMessage GetLoanOperationAwaitingDocumentation(string searchString)
        {
            var data = repo.GetLoanOperationDocumentationSearch(token.GetStaffId, token.GetCompanyId, searchString);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-operation/lms-completed-documentation")]
        public HttpResponseMessage GetLMSCompletedLoanOperationAwaitingDocumentation([FromBody] CompletedCreditDocumentationModel obj)
        {
            var data = repo.GetCompletedLoanOperationDocumentation(token.GetStaffId, token.GetCompanyId, obj.startDate, obj.endDate);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/awaiting-documentation-los")]
        public HttpResponseMessage GetLoanOperationAwaitingDocumentationLos()
        {
            var data = repo.GetLoanOperationDocumentationLos(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/awaiting-documentation-los-search/{searchString}")]
        public HttpResponseMessage GetLoanOperationAwaitingDocumentationLosSearch(string searchString)
        {
            var data = repo.GetLoanOperationDocumentationLosSearch(token.GetStaffId, token.GetCompanyId, searchString);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-operation/completed-documentation-los")]
        public HttpResponseMessage GetAllCompletedLoanOperationDocumentationLos([FromBody] CompletedCreditDocumentationModel obj)
        {
            var data = repo.GetAllCompletedLoanOperationDocumentationLos(token.GetStaffId, token.GetCompanyId, obj.startDate, obj.endDate);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/awaiting-documentation-los-approval")]
        public HttpResponseMessage GetLoanOperationDocumentationLosApproval()
        {
            var data = repo.GetLoanOperationDocumentationLosApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-insurance-upload-awaiting-approval")]
        public HttpResponseMessage GetBulkInsuranceUploadAwaitingApproval()
        {
            var data = repo.GetBulkInsuranceUploadAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-insurance-upload-rejected-approval")]
        public HttpResponseMessage GetBulkInsuranceUploadRejectedApproval()
        {
            var data = repo.GetBulkInsuranceUploadRejectedApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-write-off-analysis")]
        public HttpResponseMessage GetAllLoansOperationWriteOffAnalysis()
        {
            var data = repo.GetAllLoansOperationWriteOffAnalysis(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-analysis")]
        public HttpResponseMessage GetLoanOperationRecoveryAnalysis()
        {
            var data = repo.GetLoanOperationRecoveryAnalysis(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/get-delinquent-accounts")]
        public HttpResponseMessage GetDelinquentAccounts()
        {
            var data = repo.GetDelinquentAccounts(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/get-delinquent-digital-accounts")]
        public HttpResponseMessage GetDelinquentDigitalAccounts()
        {
            var data = repo.GetDelinquentDigitalAccounts(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-analysis-agents/{source}/{start}")]
        public HttpResponseMessage getAllLoansOperationRecoveryAnalysisByAgent(string source, DateTime start)
        {
            var data = repo.getAllLoansOperationRecoveryAnalysisByAgent(source, token.GetStaffId, token.GetCompanyId, start );
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-computation-variables")]
        public HttpResponseMessage getAllRecoveryComputationVariables()
        {
            var data = repo.getAllRecoveryComputationVariables(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [Route("unassignment-loan-operation/recovery-operation-agents/{source}")]
        public HttpResponseMessage getAllUnassignedRecoveryOperationByAgent(string source)
        {
            var data = repo.getAllUnassignedRecoveryOperationByAgent(source, token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-report-collection-analysis-agents/{source}/{start}")]
        public HttpResponseMessage getAllLoansForRecoveryAnalysisByAgent(string source, DateTime start)
        {
            var data = repo.getAllLoansForRecoveryAnalysisByAgent(source, token.GetStaffId, token.GetCompanyId, start);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-assignment-by-single-agents/{source}")]
        public HttpResponseMessage getAllLoansForRecoveryAnalysisBySingleAgent(string source)
        {
            var data = repo.getAllLoansForRecoveryAnalysisBySingleAgent(source, token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-commission-external-analysis-agents/{source}/{start}")]
        public HttpResponseMessage getAllLoansForExternalRecoveryAnalysisByAgent(string source, DateTime start)
        {
            var data = repo.getAllLoansForExternalRecoveryAnalysisByAgent(source, token.GetStaffId, token.GetCompanyId, start);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/generate-recovery-mail-to-agents/{source}")]
        public HttpResponseMessage generateRecoveryMailToAgents(string source)
        {
            bool data = repo.generateRecoveryMailToAgents(source, token.GetStaffId, token.GetCompanyId);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "mail sent successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = data });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/pending-recovery-mail-to-agents/{source}")]
        public HttpResponseMessage getAllPendingEmailAlert(string source)
        {
            var data = repo.getAllPendingEmailAlert(source, token.GetStaffId, token.GetCompanyId);
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, result =data });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-commission-agents-retail/{start}")]
        public HttpResponseMessage getAllRecoveryCommissonByAgents(DateTime start)
        {
            var data = repo.getAllRecoveryCommissonByAgents(token.GetStaffId, token.GetCompanyId, start);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-commission-agents-internal/{start}")]
        public HttpResponseMessage getAllInternalRecoveryCommissonByAgents(DateTime start)
        {
            var data = repo.getAllInternalRecoveryCommissonByAgents(token.GetStaffId, token.GetCompanyId, start);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-report-collection-agents-retail/{start}")]
        public HttpResponseMessage getAllRecoveryReportCollectionByAgents(DateTime start)
        {
            var data = repo.getAllRecoveryReportCollectionByAgents(token.GetStaffId, token.GetCompanyId, start);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovered-report-all-agents")]
        public HttpResponseMessage GetAllLoansRecoveredByAgent()
        {
            var data = repo.GetAllLoansRecoveredByAgents(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-report-all-agents")]
        public HttpResponseMessage GetAllLoansRecoveredByAgents()
        {
            var data = repo.GetAllLoansRecoveredByAgentForReporting(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-recovery-approval/{accreditedConsultantId}/{referenceId}")]
        public HttpResponseMessage GetAllBulkLoansRecoveredByAgent(int accreditedConsultantId, string referenceId)
        {
            var data = repo.getAllLoansRecoveryAnalysisByAgent(token.GetStaffId, token.GetCompanyId, accreditedConsultantId, referenceId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-unassign-recovery-approval/{accreditedConsultantId}/{referenceId}")]
        public HttpResponseMessage GetAllBulkUnassignLoansRecoveredByAgent(int accreditedConsultantId, int referenceId)
        {
            var data = repo.getAllUnassignLoansRecoveryAnalysisByAgent(token.GetStaffId, token.GetCompanyId, accreditedConsultantId, referenceId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-recovery-approval-remedial/{accreditedConsultantId}/{referenceId}")]
        public HttpResponseMessage getAllLoansRecoveryAnalysisByAgentRemedial(int accreditedConsultantId, string referenceId)
        {
            var data = repo.getAllLoansRecoveryAnalysisByAgentRemedial(token.GetStaffId, token.GetCompanyId, accreditedConsultantId, referenceId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/approved-loan-review")]
        public HttpResponseMessage GetApprovedLoanReviewed()
        {
           
                var data = repo.GetApprovedLoanOperationReview();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
           
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/approval-detail/")]
        public HttpResponseMessage GetApprovalDetails(int loanId, int operationId)
        {
            
                var data = repo.GetApprovalDetails(loanId, operationId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
           
        }
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-loan-review")]
        public HttpResponseMessage AddOperationReview([FromBody] LoanReviewOperationViewModel model)
        {
            
            model.userBranchId = (short)token.GetBranchId;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.approvalStatusId = (int)ApprovalStatusEnum.Pending;

            //if ((int)OperationsEnum.Prepayment == model.operationTypeId)

            //{
            //    model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

            //    if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
            //    }

            //    var response = repo.AddOperationReview(model);
            //    if (response)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}
            //else if (model.operationTypeId == (int)OperationsEnum.Fee_chargeChange)
            //{
            //    if (repo.DoesChargeFeeExist(model.loanId, model.operationTypeId, (int)model.interestFrequencyTypeId))
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested charge fee type already exist and going through approval" });
            //    }
            //    var response = repo.AddOperationReview(model);
            //    if (response)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}
            //else
            //{
            //    if (model.principalFirstPaymentDate < model.proposedEffectiveDate)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Principal First Payment Date cannot be less than Effective date" });
            //    }
            //    if (model.interestFirstPaymentDate < model.proposedEffectiveDate)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Interest First Payment Date cannot be less than Effective date" });
            //    }
            //    if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
            //    }


            //    var response = repo.AddOperationReview(model);

            //    if (response)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}


            var response = repo.AddOperationReview(model);

            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record created successfully, now waiting for approval" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            //}
            //catch (ConditionNotMetException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            //}
            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            //}

        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("operation-approval")]
        public HttpResponseMessage GoForApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            WorkflowResponse data = repo.GoForApproval(entity);

            if (data.stateId == (int)ApprovalState.Ended)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation has been approved successfully. Sent to Credit Documentation for filling" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = data.responseMessage });
            }
               /*if (entity.operationId != (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && entity.operationId != (int)OperationsEnum.CompleteWriteOff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation has been approved successfully. Sent to Credit Documentation for filling" });
                }else if (entity.operationId != (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && entity.currentUserCode == "COA" && entity.operationId == (int)OperationsEnum.CompleteWriteOff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successfully. Sent to Domestic Operation Inputer" });
                }
                else if (entity.operationId != (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && entity.currentUserCode != "COA" && entity.operationId == (int)OperationsEnum.CompleteWriteOff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation has been approved successfully. Sent to Credit Documentation for filling" });
                }
                else
                {
                    var newRef = repo.GetCollateralLoanNewRefernceNumber(entity);
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, message = "Operation has been approved successfully, With New Reference Number " + newRef });
                }

            }
            else if (data == 2)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation has been disapproved successfully." });
            }
            else if (data == 3 && entity.operationId != (int)OperationsEnum.CompleteWriteOff)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Operation successful, Sent to Credit Documentation for filling" });
            }
            else if (data == 3 && entity.operationId == (int)OperationsEnum.CompleteWriteOff && entity.currentUserCode != "COA")
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Operation successful, Sent to Credit Documentation for filling" });
            }
            else if (data == 3 && entity.operationId == (int)OperationsEnum.CompleteWriteOff && entity.currentUserCode == "COA")
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Operation successful, Sent to Domestic Operation Inputer" });
            }
            else if (data == 4)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Operation Has Been Refered Back" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }*/
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lien-removal-approval")]
        public HttpResponseMessage GoForLienRemovalApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            var data = repo.GoForLienRemovalApproval(entity);

            if (data == 1)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation has been approved successfully." });
            }
            else if (data == 2)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation has been disapproved successfully." });
            }
            else if (data == 3)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            else if (data == 4)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Operation Has Been Refered Back" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("assign-nplloans-to-agent-approval")]
        public HttpResponseMessage GoForAssignLoansToAgentApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            WorkflowResponse data = repo.GoForAssignLoansToAgentApproval(entity);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = data.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("unassign-nplloans-from-agent-approval")]
        public HttpResponseMessage GoForUnassignLoansFromAgentApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            WorkflowResponse data = repo.GoForUnassignLoansFromAgentApproval(entity);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = data.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("assign-bulk-nplloans-to-agent-approval/{approvalStatusId}/{comment}")]
        public HttpResponseMessage GoForBulkAssignLoansToAgentApproval(int approvalStatusId, string comment, [FromBody] List<BulkRecoveryApprovalViewModel> entity)
        {

            var user = new UserInfo
            {
                BranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            WorkflowResponse res = repo.GoForBulkAssignLoansToAgentApproval(entity,user, approvalStatusId,comment);

            if (res != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = res.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("unassign-bulk-nplloans-from-agent-approval/{approvalStatusId}/{comment}")]
        public HttpResponseMessage GoForBulkUnassignLoansFromAgentApproval(int approvalStatusId, string comment, [FromBody] List<BulkRecoveryApprovalViewModel> entity)
        {

            var user = new UserInfo
            {
                BranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            WorkflowResponse res = repo.GoForBulkUnassignLoansFromAgentApproval(entity, user, approvalStatusId, comment);

            if (res != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = res.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operationtypebycontingent")]
        public HttpResponseMessage getOperationTypeByContingent()
        {
            var data = repo.GetOperationTypeByContingent();
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
        [Route("contingent-for-termination-application")]
        public HttpResponseMessage GetContingentApprovedExpiredApplication()
        {
            var data = loanRepo.GetContingentApprovedExpiredApplication(token.GetStaffId, token.GetCompanyId);
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
        [Route("approved-contingent-application")]
        public HttpResponseMessage GetContingentApprovedApplication()
        {
            var data = loanRepo.GetContingentApprovedApplication(token.GetStaffId, token.GetCompanyId);
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
        [Route("contingent-detail/")]
        public HttpResponseMessage SearchForContingent(int revolvingLoanId)
        {
            var data = loanRepo.GetContingentByLoanId(revolvingLoanId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }


        [HttpPost, Route("add-loan-contingent-with-attachment")]
        public async Task<HttpResponseMessage> AddOperationReviewContingentWithAttachment()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }
            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            Task.Factory
                .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning, // guarantees separate thread
                    TaskScheduler.Default)
                .Wait();

            var formData = provider.FormData["formData"];

            var errors = new List<string>();
            LoanReviewOperationViewModel model = JsonConvert.DeserializeObject<LoanReviewOperationViewModel>(formData,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg)
                    {
                        errors.Add(earg.ErrorContext.Member.ToString());
                        earg.ErrorContext.Handled = true;
                    }
                });


            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            model.userBranchId = (short)token.GetBranchId;
            model.companyId = token.GetCompanyId;
            model.createdBy = token.GetStaffId;
            model.applicationUrl = HttpContext.Current.Request.Path;

            var file = provider.Contents.FirstOrDefault();
            var buffer = await file.ReadAsByteArrayAsync();

            if ((int)OperationsEnum.ContingentLiabilityRenewal == model.operationTypeId)

            {


                //if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                //}
                var response = repo.SaveDocument(model, buffer);
                //var response = repo.AddOperationReviewContingentWithImage(model, buffer);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            else if ((int)OperationsEnum.ContingentLiabilityTermination == model.operationTypeId)
            {

                //model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

                //if (repo.DoesOperationExist(model.loanId, model.operationTypeId, (short)model.loanSystemTypeId))
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                //}
                var response = repo.SaveDocument(model, buffer);
                //var response = repo.AddOperationReviewContingentWithImage(model,buffer);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            else if ((int)OperationsEnum.ContingentLiabilityTenorExtension == model.operationTypeId || (int)OperationsEnum.ContingentLiabilityAmountReduction == model.operationTypeId || (int)OperationsEnum.ContingentLiabilityAmountAddition == model.operationTypeId)
            {

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            else if ((int)OperationsEnum.ContingentLiabilityTerminateAndRebook == model.operationTypeId)
            {

                //model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

                //if (repo.DoesOperationExist(model.loanId, model.operationTypeId, (short)model.loanSystemTypeId))
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                //}

                var response = repo.SaveDocument(model, buffer);//AddOperationReviewContingentWithImage(model, buffer);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            else if ((int)OperationsEnum.CancelContingentLiability == model.operationTypeId)
            {
                model.approvalStatusId = model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

                if (repo.DoesOperationExist(model.loanId, model.operationTypeId, (short)model.loanSystemTypeId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                }

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-loan-contingent")]
        public HttpResponseMessage AddOperationReviewContingent([FromBody] LoanReviewOperationViewModel model)
        {
            model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = Request.RequestUri.Host;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.userBranchId = (short)token.GetBranchId;

            if ((int)OperationsEnum.ContingentLiabilityRenewal == model.operationTypeId)

            {

                //if (model.maturityDate < model.proposedEffectiveDate)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Maturity Date cannot be less than Effective date" });
                //}

                //if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                //}

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            else if ((int)OperationsEnum.ContingentLiabilityTermination == model.operationTypeId)
            {

                model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

                if (repo.DoesOperationExist(model.loanId, model.operationTypeId, (short)model.loanSystemTypeId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                }

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            else if ((int)OperationsEnum.ContingentLiabilityTenorExtension == model.operationTypeId || (int)OperationsEnum.ContingentLiabilityAmountReduction == model.operationTypeId || (int)OperationsEnum.ContingentLiabilityAmountAddition == model.operationTypeId)
            {

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            else if ((int)OperationsEnum.ContingentLiabilityTerminateAndRebook == model.operationTypeId)
            {

                model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

                if (repo.DoesOperationExist(model.loanId, model.operationTypeId, (short)model.loanSystemTypeId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                }

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            else if ((int)OperationsEnum.CancelContingentLiability == model.operationTypeId)
            {
                model.approvalStatusId = model.approvalStatusId = (int)ApprovalStatusEnum.Processing;

                if (repo.DoesOperationExist(model.loanId, model.operationTypeId, (short)model.loanSystemTypeId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                }

                var response = repo.AddOperationReviewContingent(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });


        }


        // [HttpPost] [ClaimsAuthorization]
        //[Route("operation-loan-rephrasement")]
        //public HttpResponseMessage LoanRephrasementOperation([FromBody]LoanReviewOperationViewModel entity)
        //{
        //    try
        //    {

        //        if (entity.loanReviewOperationsId == 0 || entity.loanId == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Please reconfirm your request and try again" });
        //        }
        //        var data = repo.LoanRephasementProcess((short)entity.loanReviewOperationsId, entity.loanId, token.GetStaffId);

        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, message = "Operation successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = false, message = "Operation not successful" });
        //    }
        //    catch (System.Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
        //    }
        //}

        [HttpGet]
        [ClaimsAuthorization]
        [Route("sent-email-for-recovery/{accreditedConsultantId}")]
        public HttpResponseMessage SendEmailForRecovery( int accreditedConsultantId)
        {
            
                var data = repo.SendEmailToRecoveryAgent(token.GetCompanyId,token.GetStaffId,(short)token.GetBranchId, accreditedConsultantId);
                if (data == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-reporting/application-list")]
        public HttpResponseMessage BulkRecoveryReportingAwaitingApprovalList()
        {
            var data = repo.BulkRecoveryReportingApplicationList(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-reporting/awaiting-approval-list")]
        public HttpResponseMessage getBulkRecoveryReportingAwaitingApprovalList()
        {
            var data = repo.GetBulkRecoveryReportingAwaitingApprovalList(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-recovery-reporting-approval/{reference}")]
        public HttpResponseMessage getAllLoansRecoveryReportingByReference(string reference)
        {
            var data = repo.getAllLoansRecoveryReportingByReference(token.GetStaffId, token.GetCompanyId, reference);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-recovery-reporting-approval")]
        public HttpResponseMessage GoForRecoveryReportingApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            WorkflowResponse data = repo.GoForRecoveryReportingApproval(entity);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = data.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-reporting/awaiting-approval")]
        public HttpResponseMessage GetBulkRecoveryReportingAwaitingApproval()
        {
            var data = repo.GetBulkRecoveryReportingAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }



        //====================== recovery commission

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/recovery-commission-by-agents")]
        public HttpResponseMessage getAllPaymentRecoveredCommissionByAgent()
        {
            var data = repo.getAllPaymentRecoveredCommissionByAgent(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-commission/application-list")]
        public HttpResponseMessage BulkRecoveryCommissionAwaitingApprovalList()
        {
            var data = repo.BulkRecoveryCommissionApplicationList(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-commission/awaiting-approval-list")]
        public HttpResponseMessage getBulkRecoveryCommissionAwaitingApprovalList()
        {
            var data = repo.GetBulkRecoveryCommissionAwaitingApprovalList(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-operation/bulk-recovery-commission-approval/{reference}")]
        public HttpResponseMessage getAllLoansRecoveryCommissionByReference(string reference)
        {
            var data = repo.getAllLoansRecoveryCommissionByReference(token.GetStaffId, token.GetCompanyId, reference);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("bulk-recovery-commission/awaiting-approval")]
        public HttpResponseMessage GetBulkRecoveryCommissionAwaitingApproval()
        {
            var data = repo.GetBulkRecoveryCommissionAwaitingApproval(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-recovery-commission-approval")]
        public HttpResponseMessage GoForRecoveryApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            WorkflowResponse data = repo.GoForRecoveryCommissionApproval(entity);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = data.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("documentation-filling-approval")]
        public HttpResponseMessage GoForDocumentationFillingApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            var data = repo.GoForDocumentationFillingApproval(entity);

            if (data == 1)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Approved" });
            }
            else if (data == 2)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Disapproved" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("bulk-insurance-upload-approval")]
        public HttpResponseMessage GoForBulkInsuranceUploadApproval([FromBody]ApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            WorkflowResponse data = repo.GoForBulkInsuranceUploadApproval(entity);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = data.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = data.responseMessage });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("multiple-bulk-insurance-upload-approval/{approvalStatusId}/{comment}")]
        public HttpResponseMessage GoForMultipleBulkInsuranceUploadApproval(int approvalStatusId, string comment, [FromBody] List<MultipleInsuranceOutputViewModel> entity)
        {

            var user = new UserInfo
            {
                BranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            WorkflowResponse res = repo.GoForMultipleBulkInsuranceUploadApproval(entity, user, approvalStatusId, comment);

            if (res != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = res.responseMessage });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Approval failed" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-agents-list")]
        public HttpResponseMessage GetAllRecoveryAgents()
        {
            var data = repo.GetAllRecoveryAgents(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-agents-customers-list/{recoveryAgent}")]
        public HttpResponseMessage GetAllRecoveryCustomersAssignedToAgent(int recoveryAgent)
        {
            var data = repo.GetAllRecoveryCustomersAssignedToAgent(recoveryAgent);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-internal-agents-list/{start}")]
        public HttpResponseMessage GetAllInternalRecoveryAgents([FromUri] DateTime start)
        {
            var data = repo.GetAllInternalRecoveryAgents(token.GetStaffId, token.GetCompanyId, start);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("approved-overdraft-review-application-search/{searchString}")]
        //public HttpResponseMessage GetLoanReviewApplicationOverDraftSearch(string searchString)
        //{

        //    var data = loanRepo.GetLoanReviewApplicationOverDraftSearch(token.GetStaffId, token.GetCompanyId, searchString);
        //    if (data == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = "No record found" });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = true, result = data });
        //}


        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("approved-loan-review-search/{searchString}")]
        //public HttpResponseMessage GetApprovedLoanReviewSearch(string searchString)
        //{
        //    var data = loanRepo.GetApprovedLoanReviewSearch(token.GetCompanyId, token.GetStaffId, searchString);
        //    if (data == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = "No record found" });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = true, result = data });

        //}


        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-contingent-application-search/{searchString}")]
        public HttpResponseMessage GetContingentApprovedApplicationSearch(string searchString)
        {
            var data = loanRepo.GetContingentApprovedApplication(token.GetStaffId, token.GetCompanyId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }

    }
}