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
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class AppraisalMemorandumController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IAppraisalMemorandumRepository repo;

        public AppraisalMemorandumController(IAppraisalMemorandumRepository repo_)
        {
            this.repo = repo_;
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-application/{loanApplicationId}")]
        public HttpResponseMessage GetAppraisalMemorandumByLoanApplicationId(int loanApplicationId)
        {
            var data = repo.GetAppraisalMemorandum(loanApplicationId, token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-application/{loanApplicationId}/documentation")]
        public HttpResponseMessage GetAppraisalMemorandumDocumentation(int loanApplicationId)
        {
            var data = repo.GetAllDocumentation(loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [Route("appraisal-memorandum")]
        public HttpResponseMessage AddAppraisalMemorandum([FromBody] AppraisalMemorandumViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddAppraisalMemorandum(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut]
        [Route("appraisal-memorandum/{appraisalMemorandumId}")]
        public HttpResponseMessage UpdateAppraisalMemorandum([FromBody] AppraisalMemorandumViewModel entity, int appraisalMemorandumId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateAppraisalMemorandum(entity, appraisalMemorandumId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("appraisal-memorandum/forward")]
        public HttpResponseMessage ForwardAppraisalMemorandum([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            try
            {
                WorkflowResponse response = repo.ForwardAppraisalMemorandum(entity);
                if (response != null)
                {
                    if(entity.subTransId != null) { repo.UpdateSubsidiaryBasicTransaction((int)entity.subTransId, entity); }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application has been acted on successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error acting on this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error acting on this record {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("adhoc-appraisal/forward")]
        public HttpResponseMessage AdhocAppraisalMemorandum([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.AdhocAppraisalMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "ADHOC APPLICATION") });
        }

        [HttpPost]
        [Route("lc-approval/forward")]
        public HttpResponseMessage LcAppraisalMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcAppraisalMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC ISSUANCE") });
        }

        [HttpPost]
        [Route("lc-release/forward")]
        public HttpResponseMessage LcReleaseMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcReleaseMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC RELEASE") });
        }

        [HttpPost]
        [Route("lc-cancelation/forward")]
        public HttpResponseMessage LcCancelationMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcCancelationMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC CANCELATION") });
        }

        [HttpPost]
        [Route("lc/enhancement-forward")]
        public HttpResponseMessage LcEnhancementMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcEnhancementMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC ENHANCEMENT") });
        }

        [HttpPost]
        [Route("lc/extension-forward")]
        public HttpResponseMessage LcExtensionMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcIssuanceExtensionMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC EXTENSION") });
        }

        [HttpPost]
        [Route("lc/ussance-extension-forward")]
        public HttpResponseMessage LcUsanceExtensionMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcUsanceExtensionMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC USSANCE EXTENSION") });
        }

        [HttpPost]
        [Route("lc/ussance-forward")]
        public HttpResponseMessage LcUssanceMemorandum([FromBody] LcForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LcUssanceMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LC USSANCE") });
        }

        [HttpPost]
        [Route("letter-gen-request/forward")]
        public HttpResponseMessage LetterGenerationRequestMemorandum([FromBody] LetterGenerationRequestViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.LetterGenerationRequestMemorandum(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "LETTER GENERATION") });
        }

        //[HttpPost]
        //[Route("collateral-swap/forward-for-approval")]
        //public HttpResponseMessage CollateralSwapMemorandum([FromBody] CollateralSwapViewModel entity)
        //{
        //    entity.userBranchId = (short)token.GetBranchId;
        //    entity.companyId = token.GetCompanyId;
        //    entity.createdBy = token.GetStaffId;
        //    entity.staffId = token.GetStaffId;
        //    entity.applicationUrl = HttpContext.Current.Request.Path;

        //    WorkflowResponse response = repo.CollateralSwapMemorandum(entity);

        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "COLLATERAL SWAP") });
        //}


        [HttpGet]
        [Route("appraisal-memorandum/trail/{applicationId}/operation/{operationId}/all/{all}")]
        public HttpResponseMessage GetAppraisalMemorandumTrail(int applicationId, int operationId, bool all)
        {
            var data = repo.GetAppraisalMemorandumTrail(applicationId, operationId, all);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpGet]
        [Route("appraisal-memorandum-lms/trail/{applicationId}/operation/{operationId}")]
        public HttpResponseMessage GetAppraisalMemorandumTrailLms(int applicationId, int operationId)
        {
            var data = repo.GetCallmemoApprovalTrail(applicationId, operationId);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpGet]
        [Route("call-memo/trail/{applicationId}/operation/{operationId}")]
        public HttpResponseMessage GetCallMemoApprovalTrail(int applicationId, int operationId)
        {
            var data = repo.GetCallmemoApprovalTrail(applicationId, operationId);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpGet]
        [Route("appraisal-memorandum/trail/{applicationId}/operation/{operationId}/currentLevel/{currentLevelId}/all/{all}/isClassified/{isClassified}")]
        public HttpResponseMessage GetTrailForReferBack(int applicationId, int operationId, int currentLevelId, bool all, bool isClassified)
        {
            var data = repo.GetTrailForReferBack(applicationId, operationId, currentLevelId, all, isClassified);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpGet]
        [Route("appraisal-memorandum/trail/{applicationId}/operation/{operationId}/currentLevel/{currentLevelId}/all/{all}/isClassified/{isClassified}/isLMSCrossWorkflow/{isLMSCrossWorkflow}")]
        public HttpResponseMessage GetTrailForReferBack(int applicationId, int operationId, int currentLevelId, bool all, bool isClassified, bool isLMSCrossWorkflow = false)
        {
            var data = repo.GetTrailForReferBack(applicationId, operationId, currentLevelId, all, isClassified, isLMSCrossWorkflow);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }


        [HttpGet]
        [Route("appraisal-memorandum/trail/{operationId}")]
        public HttpResponseMessage GetAppraisalMemorandumTrailCallMemo(int operationId)
        {
            //var data = repo.GetAppraisalMemorandumTrailCallMemo(operationId);
            //if (data.Any())
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            //}
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpPost]
        [Route("appraisal-memorandum/privilege")]
        public HttpResponseMessage GetUserPrivilege([FromBody] AuthoritySignatureViewModel entity)
        {
         
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                var data = repo.GetUserPrivilege(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
 
        }

        [HttpPost]
        [Route("appraisal-memorandum/privilege-by-code")]
        public HttpResponseMessage GetUserPrivilegeByCode([FromBody] AuthoritySignatureViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffRoleCode = token.GetStaffRoleCode;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            var data = repo.GetUserPrivilegeByCode(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-detail/{loanApplicationId}")]
        public HttpResponseMessage GetApprovedLoanDetail(int loanApplicationId)
        {
            LoanApplicationDetailsViewModel data = repo.GetLoanApplicationDetail(loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("appraisal-memorandum/tranche-detail/{bookingRequestId}")]
        public HttpResponseMessage GetApprovedTrancheDetail(int bookingRequestId)
        {
            LoanApplicationDetailsViewModel data = repo.GetApprovedTrancheDetail(bookingRequestId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-detail-refnumber/{applicationReferenceNumber}")]
        public HttpResponseMessage GetApprovedLoanDetailByReferenceNumber(string applicationReferenceNumber)
        {
            LoanApplicationDetailsViewModel data = repo.GetLoanApplicationDetailByRefNo(applicationReferenceNumber);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("appraisal-memorandum/single-detail/{detailId}")]
        public HttpResponseMessage GetSingleApprovedLoanDetail(int detailId)
        {
            LoanApplicationDetailsViewModel data = repo.GetSingleLoanApplicationDetail(detailId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-secured-collateral-type")]
        public HttpResponseMessage GetAllCRMSSecuredCollateralType()
        {
            var data = repo.GetAllCRMSSecuredCollateralType(token.GetCompanyId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-all-collateral-type")]
        public HttpResponseMessage GetAllCRMSCollateralType()
        {
            var data = repo.GetAllCRMSAllCollateralType(token.GetCompanyId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-unsecured-collateral-type")]
        public HttpResponseMessage GetAllCRMSUnsecuredCollateralType()
        {
            var data = repo.GetAllCRMSUnsecuredCollateralType(token.GetCompanyId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }
        [HttpGet]
        [Route("appraisal-memorandum/loan-detail-fees/{loanApplicationId}")]
        public HttpResponseMessage GetLoanDetailsFee(int loanApplicationId)
        {
            var data = repo.GetLoanDetailsFee(loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-detail-change-log/{loanApplicationId}")]
        public HttpResponseMessage GetLoanDetailChangeLog(int loanApplicationId)
        {
            var data = repo.GetLoanDetailChangeLog(loanApplicationId);
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = "No records found" });
        }

        [HttpGet, Route("loan-application-approval-process")]
        public HttpResponseMessage GetPendingLoanApplications([FromUri] int operationId, [FromUri] int page, [FromUri] int itemsPerPage, [FromUri] int? classId, [FromUri] string searchString, [FromUri] bool isSpecific)
        {
            IQueryable<LoanApplicationViewModel> items;
            items = repo.GetPendingLoanApplications(operationId, token.GetCompanyId, token.GetBranchId, token.GetStaffId, classId, isSpecific);


            if (!String.IsNullOrEmpty(searchString))
            {

                searchString = searchString.Trim().ToLower();
                items = (from x in items
                         where x.applicationReferenceNumber.ToLower().StartsWith(searchString)
                         || x.applicantName.ToLower().StartsWith(searchString)
                         || x.applicationAmount.ToString() == searchString
                         //|| x.customerGroupName.ToLower().StartsWith(searchString)
                         select x);

                items = items.Take(itemsPerPage);

                //items = items.Where(x =>
                //    (searchString.StartsWith(x.applicationReferenceNumber))
                //    || (searchString.StartsWith(x.customerName.ToLower()))
                //    || (searchString.StartsWith(x.customerGroupName.ToLower()))
                //    ).Take(itemsPerPage);
            }

            var data = items
                .OrderByDescending(x => x.timeIn) // OrderBy() must be called for Skip() to work!
                //.OrderByDescending(x => x.applicationReferenceNumber) // OrderBy() must be called for Skip() to work!
                .Skip(page)
                .Take(itemsPerPage)
                .ToList();
           data = repo.CalculateSLA(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
        }

        [HttpGet, Route("pool-application-approval-process")]
        public HttpResponseMessage GetPoolApplications([FromUri] int operationId, [FromUri] int? classId, [FromUri] string searchString)
        {
            IQueryable<LoanApplicationViewModel> items;
            items = repo.GetPoolApplications(operationId, token.GetCountryId, token.GetBranchId, token.GetStaffId, classId);


            if (!String.IsNullOrEmpty(searchString))
            {

                searchString = searchString.Trim().ToLower();
                items = (from x in items
                         where x.applicationReferenceNumber.ToLower().StartsWith(searchString)
                         || x.applicantName.ToLower().StartsWith(searchString)
                         || x.applicationAmount.ToString() == searchString
                         //|| x.customerGroupName.ToLower().StartsWith(searchString)
                         select x);

            }

            var data = items.OrderByDescending(x => x.timeIn).ToList();
            data = repo.CalculateSLA(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
        }

        [HttpGet, Route("subsidiaries-loan-applications")]
        public HttpResponseMessage GetSubsidiaryPendingLoanApplications([FromUri] int operationId, [FromUri] int page, [FromUri] int itemsPerPage, [FromUri] int? classId, [FromUri] string searchString, [FromUri] bool isSpecific)
        {
            var staffRoleCode = token.GetStaffRoleCode;
            IQueryable<SubsidiaryViewModel> items;
            items = repo.GetSubsidiaryPendingLoanApplications(operationId, token.GetCountryId, token.GetBranchId, token.GetStaffId, classId, staffRoleCode, isSpecific);
            if (items != null)
            {
                if (!String.IsNullOrEmpty(searchString))
                {

                    searchString = searchString.Trim().ToLower();
                    items = (from x in items
                             where x.applicationReferenceNumber.ToLower().StartsWith(searchString)

                             || x.applicationAmount.ToString() == searchString
                             select x);
                    items = items.Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.timeIn)
                    .Skip(page)
                    .Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
        }
        [HttpGet, Route("subsidiaries")]
        public async Task<HttpResponseMessage> GetSubsidiaries()
        {
            var data = await repo.GetSubsidiaries();
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
        }
        [HttpPut, Route("reassign-application/owner/{staffId}")]
        public HttpResponseMessage ChangeApplicationOwner([FromBody] int loanApplicationId, int staffId)
        {
            var entity = new GeneralEntity
            {
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path
            };

            var reassigned = repo.ChangeApplicationOwner(loanApplicationId, staffId, entity);
            if (reassigned)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Ownership was reassigned successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured when trying to reassign" });
        }

        [HttpPut, Route("reassign-application/{staffId}")]
        public HttpResponseMessage ReassignApplication([FromBody] int approvalTrailId, int staffId)
        {
            var entity = new GeneralEntity
            {
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path
            };

            var reassigned = repo.AssignApplication(approvalTrailId, staffId, entity);
            if (reassigned)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Request was reassigned successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured when trying to reassign" });
        }

        [HttpPut, Route("reassign-multiple-requests/{staffId}")]
        public HttpResponseMessage ReassignMultipleRequests([FromBody] List<int> model, int staffId)
        {
            var entity = new GeneralEntity
            {
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path
            };

            var reassigned = repo.ReassignMultipleRequests(model, entity, staffId);
            if (reassigned)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Request(s) were assigned successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured when trying to reassign" });
        }

        [HttpPut, Route("self-assign-multiple-approval-item")]
        public HttpResponseMessage SelfAssignmultipleApprovalItem([FromBody] List<ForwardViewModel> model)
        {
            var entity = new GeneralEntity
            {
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path
            };

            var reassigned = repo.SelfAssignMultpleApplication(model, entity);
            if (reassigned)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Request assigned successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured when trying to reassign" });
        }

        [HttpPut, Route("revert-transaction-to-general-pool/{trailId}")]
        public HttpResponseMessage ReturnAssignApplicationToPool([FromBody] List<ForwardViewModel> model, int trailId)
        {
            var entity = new GeneralEntity
            {
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path
            };

            var reassigned = repo.ReturnAssignApplicationToPool(trailId, entity);
            if (reassigned)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Request has been returned to general pool successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured when trying to reassign" });
        }

        [HttpPut, Route("selfAssign-application")]
        public HttpResponseMessage AssignApplication([FromBody] int approvalTrailId)
        {
            var entity = new GeneralEntity
            {
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path
            };

            var reassigned = repo.AssignApplication(approvalTrailId, token.GetStaffId, entity);
            if (reassigned)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Request was assigned successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured" });
        }

        [HttpGet, Route("adhoc-approval/{operationId}/class/{classId}")]
        public HttpResponseMessage getApplicationsToBeAdhocApprovedForInitiateBooking(int operationId, int? classId)
        {
            IQueryable<LoanApplicationViewModel> items;
            items = repo.GetPendingAdhocApplications(operationId, token.GetCountryId, token.GetBranchId, token.GetStaffId, classId);


            //if (!String.IsNullOrEmpty(searchString))
            //{

            //    searchString = searchString.Trim().ToLower();
            //    items = (from x in items
            //             where x.applicationReferenceNumber.ToLower().StartsWith(searchString)
            //             || x.applicantName.ToLower().StartsWith(searchString)
            //             || x.applicationAmount.ToString() == searchString
            //             //|| x.customerGroupName.ToLower().StartsWith(searchString)
            //             select x);

            //    items = items.Take(itemsPerPage);

            //    //items = items.Where(x =>
            //    //    (searchString.StartsWith(x.applicationReferenceNumber))
            //    //    || (searchString.StartsWith(x.customerName.ToLower()))
            //    //    || (searchString.StartsWith(x.customerGroupName.ToLower()))
            //    //    ).Take(itemsPerPage);
            //}

            var data = items.OrderByDescending(x => x.applicationReferenceNumber).ToList(); // OrderBy() must be called for Skip() to work!
                                                                                //.Skip(page)
                                                                                //.Take(itemsPerPage)
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
        }

        [HttpGet]
        [Route("current-committee/application/{loanApplicationId}")]
        public HttpResponseMessage GetCurrentCommitteeByLoanApplicationId(int loanApplicationId)
        {
            var data = repo.GetCurrentCommittee(loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("appraisal-memorandum/forward-secretariat")]
        public HttpResponseMessage SecretariatForwardAppraisalMemorandum([FromBody] ForwardCommitteeCamViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            var response = repo.SecretariatForwardAppraisalMemorandum(entity);

            if (response == true)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpGet, Route("regional-loan-application")]
        public HttpResponseMessage GetRegionalLoanApplications([FromUri] int page, [FromUri] int itemsPerPage, [FromUri] string searchString)
        {
            var items = repo.GetRegionalLoanApplications(token.GetStaffId);

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                items = items.Where(x =>
                    x.applicationReferenceNumber.Contains(searchString)
                    || x.customerName.ToLower().Contains(searchString)
                    || x.customerGroupName.ToLower().Contains(searchString)
                    ).Take(itemsPerPage);
            }

            var data = items
                .OrderByDescending(x => x.timeIn) // OrderBy() must be called for Skip() to work!
                .ThenByDescending(x => x.loanApplicationId)
                .Skip(page)
                .Take(itemsPerPage)
                .ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
        }

        [HttpGet]
        [Route("appraisal-memorandum/pending-product-program")]
        public HttpResponseMessage GetPendingProductProgram()
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };

            var data = repo.GetPendingProductProgram(user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("untenored-status/application/{applicationId}")]
        public HttpResponseMessage GetUntenoredStatus(int applicationId)
        {
            bool status = repo.GetUntenoredStatus(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
        }

        #region MONITORING TRIGGERS

        [HttpGet]
        [Route("application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage GetApplicationMonitoringTriggers(int applicationId)
        {
            var response = repo.GetApplicationMonitoringTriggers(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [Route("application-monitoring-triggers-aps/{operationId}/applicationDetailId/{applicationDetailId}")]
        public HttpResponseMessage GetASP_MonitoringTriggers(int operationId,int applicationDetailId)
        {
            var response = repo.GetApplicationMonitoringTriggersByOperationId(operationId, applicationDetailId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage SaveApplicationMonitoringTriggers(int applicationId, [FromBody] List<MonitoringTriggersViewModel> entity)
        {
            var response = repo.SaveApplicationMonitoringTriggers(applicationId, entity, token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        #endregion MONITORING TRIGGERS

        [HttpGet]
        //[ClaimsAuthorization]
        [Route("repayment-schedule-terms")]
        public HttpResponseMessage GetAllSetupRepaymentTerms()
        {
            //try
            //{
            var response = repo.GetAllSetupRepaymentTerms();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            //}
            //catch (SecureException ex)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            //}
        }

        [HttpGet]
        [Route("get-old-application-reference/{data}")]
        public HttpResponseMessage GetAllOldApplicationReference(string data)
        {
            var response = repo.GetAllOldApplicationReference(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        //[ClaimsAuthorization]
        [Route("repayment-schedule-terms")]
        public HttpResponseMessage SaveRepaymentScheduleAndTerms([FromBody] RepaymentScheduleTermsViewModel entity)
        {
            //try
            //{
                IEnumerable<RepaymentScheduleTermsViewModel> response = repo.SaveRepaymentScheduleAndTerms(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            //}
            //catch (SecureException ex)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            //}
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("product-limit-validation")]
        public HttpResponseMessage SaveProductLimitValidation([FromBody] ProductLimitValidationViewModel entity)
        {
            List<ProductLimitValidationViewModel> response = repo.SaveProductLimitValidation(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [Route("product-limit-validation/{applicationId}/class/{classId}")]
        public HttpResponseMessage GetProductLimitValidation(int applicationId, int classId)
        {
            List<ProductLimitValidationViewModel> response = repo.GetProductLimitValidation(applicationId, classId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }


        [HttpGet]
        [Route("appraisal-memorandum/workflow-test")]
        public HttpResponseMessage WorkflowTest()
        {
            bool data = repo.WorkflowTest();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [Route("approval-trail/{approvalTrailId}")]
        public HttpResponseMessage GetapprovalTrailByTrailId(int approvalTrailId)
        {
            var data = repo.GetapprovalTrailByTrailId(approvalTrailId);
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
        }

        #region recommended collateral

        [HttpGet]
        [Route("recommended-collateral/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateral(int applicationId)
        {
            List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateral(applicationId, token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [Route("recommended-collateral-history/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateralHistory(int applicationId)
        {
            List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateralHistory(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recommended-collateral")]
        public HttpResponseMessage AddRecommendedCollateral([FromBody] RecommendedCollateralViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.createdBy = (short)token.GetStaffId;
            List<RecommendedCollateralViewModel> response = repo.AddRecommendedCollateral(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("recommended-collateral")]
        public HttpResponseMessage UpdateRecommendedCollateral([FromBody] RecommendedCollateralViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.createdBy = (short)token.GetStaffId;
            List<RecommendedCollateralViewModel> response = repo.UpdateRecommendedCollateral(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        #endregion recommended collateral



        #region LMS APPROVAL

        [HttpGet]
        [Route("lms-application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage GetApplicationMonitoringTriggersLms(int applicationId)
        {
            IEnumerable<MonitoringTriggersViewModel> response = repo.GetApplicationMonitoringTriggersLms(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage SaveApplicationMonitoringTriggersLms(int applicationId, [FromBody] List<MonitoringTriggersViewModel> entity)
        {
            IEnumerable<MonitoringTriggersViewModel> response = repo.SaveApplicationMonitoringTriggersLms(applicationId, entity, token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-repayment-schedule-terms")]
        public HttpResponseMessage SaveRepaymentScheduleAndTermsLms([FromBody] RepaymentScheduleTermsViewModel entity)
        {
            List<RepaymentScheduleTermsViewModel> response = repo.SaveRepaymentScheduleAndTermsLms(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [Route("lms-recommended-collateral/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateralLms(int applicationId)
        {
            List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateralLms(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [Route("lms-recommended-collateral-history/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateralHistoryLms(int applicationId)
        {
            List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateralHistoryLms(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-recommended-collateral")]
        public HttpResponseMessage AddRecommendedCollateralLms([FromBody] RecommendedCollateralViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            List<RecommendedCollateralViewModel> response = repo.AddRecommendedCollateralLms(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lms-recommended-collateral")]
        public HttpResponseMessage UpdateRecommendedCollateralLms([FromBody] RecommendedCollateralViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            List<RecommendedCollateralViewModel> response = repo.UpdateRecommendedCollateralLms(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        #endregion LMS APPROVAL
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("tranch-disbursment-approval-level")]
        public HttpResponseMessage saveTranchDisbursmentApprovalLevel([FromBody] TranchDisbursmentViewModel entity)
        {
            bool response = repo.saveTranchDisbursmentApprovalLevel(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [Route("appraisal-memorandum/lms-loan-detail/{loanApplicationId}")]
        public HttpResponseMessage GetApprovedLMSLoanDetail(int loanApplicationId)
        {
            LoanApplicationDetailsViewModel data = repo.GetLMSLoanApplicationDetail(loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [Route("appraisal-memorandum/forward-status")]
        public HttpResponseMessage GetWorkflowNextStatus([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.GetWorkflowNextStatus(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application has been acted on successfully" });
        }

        [HttpPost]
        [Route("appraisal-memorandum/forward-status-lms")]
        public HttpResponseMessage GetWorkflowNextStatusLms([FromBody] ForwardReviewViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.GetWorkflowNextStatusLms(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application has been acted on successfully" });
        }

        [HttpPost]
        [Route("contractor-tiering")]
        public HttpResponseMessage PostContractorTiering([FromBody] ContractorTieringViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            bool response = repo.AddContractorTiering(entity);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Contractor criteria has been added successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Error saving Contractor criteria" });
        }

        [HttpPost]
        [Route("ibl-checklist")]
        public HttpResponseMessage PostIBLCheclistDetail([FromBody] IBLChecklistViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            bool response = repo.AddIBLCheclistDetail(entity);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Contractor criteria has been added successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Error saving Contractor criteria" });
        }

        [HttpGet]
        [Route("global-interest-rate-change-comments/trail/{applicationId}/operation/{operationId}")]
        public HttpResponseMessage GetGlobalInterestRateChangeTrail(int applicationId, int operationId)
        {
            var data = repo.GetGlobalInterestRateChangeTrail(applicationId, operationId);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpPost]
        [Route("add-project-risk-rating")]
        public HttpResponseMessage PostProjectRiskRating([FromBody] ProjectRiskRatingViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            var response = repo.AddProjectRiskRating(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Contractor criteria has been added successfully" });
        }

        [HttpPut]
        //[ClaimsAuthorization]
        [Route("update-subsidiary-basic-transaction/{id}")]
        public HttpResponseMessage UpdateSubsidiaryBasicTransaction(int id, [FromBody] ForwardViewModel entity)
        {

            bool response = repo.UpdateSubsidiaryBasicTransaction(id, entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response });
        }

        [HttpGet]
        //[ClaimsAuthorization]
        [Route("get-subsidiary-basic-approvalLevelId/{id}")]
        public HttpResponseMessage GetSubsidiaryBasicApprovalLevel(int id)
        {

            var response = repo.GetSubsidiaryBasicApprovalLevel(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response });
        }
    }
}
