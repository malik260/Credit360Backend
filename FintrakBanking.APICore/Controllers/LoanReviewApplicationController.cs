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
using FintrakBanking.APICore.Filters;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    [SecureExceptionFilterAttribute]
    public class LoanReviewApplicationController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ILoanReviewApplicationRepository repo;
        private ILoanRepository loanRepo;
        public LoanReviewApplicationController(ILoanReviewApplicationRepository _repo, ILoanRepository _loanRepo)
        {
            this.repo = _repo;
            this.loanRepo = _loanRepo;
        }

        [HttpGet, Route("review-application")]
        [ClaimsAuthorization]

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

            
                IQueryable<LoanReviewApplicationViewModel> items;
                items = repo.GetApplications(user, operationId, classId);

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    items = items.Where(x =>
                        x.referenceNumber.Contains(searchString)
                        || x.customerName.Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.approvalTrailId) 
                    //.OrderByDescending(x => x.loanReviewApplicationId)
                    .Skip(page).Take(itemsPerPage).ToList();
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            /*data =*/ repo.CalculateSLA(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            
        }
        [HttpGet, Route("review-subsidiary-application")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetSubsidiaryApplications(
            [FromUri] int page,
            [FromUri] int itemsPerPage,
            [FromUri] int operationId,
            [FromUri] int? classId,
            [FromUri] string searchString
            )
        {
            var staffRoleCode = token.GetStaffRoleCode;
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };

            
                IEnumerable<SubsidiaryViewModel> items;
                items = repo.GetSubsidiaryApplications(user, operationId, classId, staffRoleCode);

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    items = items.Where(x =>
                        x.referenceNumber.Contains(searchString)
                        || x.customerName.Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.subBasicId) 
                    //.OrderByDescending(x => x.loanReviewApplicationId)
                    .Skip(page).Take(itemsPerPage).ToList();
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            /*data =*/ //repo.CalculateSLA(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            
        }

        [HttpGet, Route("review-application/id/{lmsApplicationId}")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetApplications(int lmsApplicationId)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };


            List<applicationDetails> data;
            data = repo.GetApplicationsById(user, lmsApplicationId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });

        }

        [HttpGet, Route("review-availment/crms")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetLoanReviewForCRMS(
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


            IQueryable<LoanReviewApplicationViewModel> items;
            items = repo.GetLoanReviewForCRMS(user, operationId, classId);

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                items = items.Where(x =>
                    x.referenceNumber.Contains(searchString)
                    || x.customerName.Contains(searchString)
                    ).Take(itemsPerPage);
            }

            var data = items
                .OrderByDescending(x => x.loanReviewApplicationId)
                .Skip(page).Take(itemsPerPage).ToList(); ;
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            /*data =*/ repo.CalculateSLA(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });

        }

        [HttpGet, Route("review-availment")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetLoanReviewAvailmentAwaitingApproval(
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

            
                IQueryable<LoanReviewApplicationViewModel> items;
                items = repo.GetLoanReviewAvailmentAwaitingApproval(user, operationId, classId);

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    items = items.Where(x =>
                        x.referenceNumber.Contains(searchString)
                        || x.customerName.Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.timeIn) 
                    .Skip(page).Take(itemsPerPage).ToList(); ;
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            /*data =*/ repo.CalculateSLA(data);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            
        }

        [HttpGet, Route("review-drawdown")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetLoanReviewDrawdownAwaitingApproval(
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


            IQueryable<LoanReviewApplicationViewModel> items;
            items = repo.GetLoanReviewDrawdownApproval(user, classId);

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                items = items.Where(x =>
                    x.referenceNumber.Contains(searchString)
                    || x.customerName.Contains(searchString)
                    ).Take(itemsPerPage);
            }

            var data = items
                .OrderByDescending(x => x.loanReviewApplicationId)
                .Skip(page).Take(itemsPerPage);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });

        }


        [HttpGet, Route("loan-review-application/select-list")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetAllSelectList()
        {
            
            var data = repo.GetAllSelectList();
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
           
        }


        [HttpGet, Route("loan-review-application-approval/select-list")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetAllLMSApprovalOperationList()
        {
           
                var data = repo.GetAllLMSApprovalOperationList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            
        }

        [HttpGet, Route("loan-review-application-approval/select-list/productTypeId/{productTypeId}")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetAllLMSApprovalOperationListByProductTypeId(int productTypeId)
        {

            var data = repo.GetAllLMSApprovalOperationListByProductTypeId(productTypeId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });

        }

        [HttpGet, Route("loan-review-application/chargefeeid/{id}")]
        [ClaimsAuthorization]

        public HttpResponseMessage GetChargeFeeById(int id)
        {
           
            var data = repo.GetChargeFeeDetails(id);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            
        }

        [HttpPost] [ClaimsAuthorization]
        [Route("loan-review-application/submit")]
        public HttpResponseMessage SubmitLoanReviewApplication([FromBody] LoanReviewApplicationViewModel entity)
        {
            try
            {
                entity.createdBy =  token.GetStaffId;
                entity.companyId =  token.GetCompanyId;
                entity.branchId = (short)token.GetBranchId;
                string response = repo.SubmitLoanReviewApplication(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message});
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-review-application/validatecustomer/{loanApplicationDetailId}/{customerId}")]
        public HttpResponseMessage validateCustomer(int loanApplicationDetailId,int customerId)
        {
                bool response = repo.ValidateSubAllocationOperation(loanApplicationDetailId, customerId);
                if (response == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response, result = response });
            
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-review-application/validatesuballocation/{loanApplicationDetailId}/{customerId}/{loanSystemTypeId}")]
        public HttpResponseMessage validatesuballocation(int loanApplicationDetailId, int customerId, int loanSystemTypeId)
        {

            bool response = repo.ValidateNewSubAllocationOperation(loanApplicationDetailId, customerId, loanSystemTypeId);
            if (response == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response, result = response });
        }

        [HttpPost] [ClaimsAuthorization]
        [Route("loan-review-application/loan-search")]
        public HttpResponseMessage LoanSearch([FromBody] SearchViewModel search)
        {
            var searchString = search.searchString.Trim();
            //var data = loanRepo.SearchForLoanAndRevolvingLoan(searchString);
            var data = loanRepo.SearchForLoanAndRevolvingLoan(searchString, search.statusId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-review-application/loan-search-fee")]
        public HttpResponseMessage LoanSearchFee([FromBody] SearchViewModel search)
        {
            var searchString = search.searchString.Trim();
            var data = loanRepo.SearchForLoanAndRevolvingLoanFeeCharge(search.loanSystemTypeId, searchString);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpPost] [ClaimsAuthorization]
        [Route("loan-review-application/forward-application")]
        public HttpResponseMessage ForwardApplication([FromBody] ForwardReviewViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.lastUpdatedBy = token.GetStaffId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;

                WorkflowResponse response = repo.ForwardApplication(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
           
            catch(SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
        }

        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-review-application/forward-appraisal")]
        public HttpResponseMessage ForwardApplicationAppraisal([FromBody] ForwardReviewViewModel model)
        {
           
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.lastUpdatedBy = token.GetStaffId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;

                WorkflowResponse response = repo.ForwardApplicationAppraisal(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-detail/loan/{loanId}/loan-type/{loanTypeId}")]
        public HttpResponseMessage GetLoanApplicationDetail(int loanId, int loanTypeId)
        {
            
            var data = repo.GetLoanApplicationDetail(loanId,loanTypeId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            
        }


        [HttpGet, Route("lms-regional-loan-application")]
        public HttpResponseMessage GetRegionalLoanApplications([FromUri] int page, [FromUri] int itemsPerPage, [FromUri] string searchString)
        {
           
                IQueryable<LoanReviewApplicationViewModel> items = repo.GetRegionalLoanApplications(token.GetStaffId);

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    items = items.Where(x =>
                        x.referenceNumber.Contains(searchString)
                        || x.customerName.ToLower().Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.timeIn) // OrderBy() must be called for Skip() to work!
                    .ThenByDescending(x => x.loanReviewApplicationId)
                    .Skip(page)
                    .Take(itemsPerPage)
                    .ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-review-application-detail-search")]
        public HttpResponseMessage LoanReviewApplicationSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.Search(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = e.Message });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = e.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("exceptional-loan-application-detail-search")]
        public HttpResponseMessage ExceptionalLoanApplicationSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.ExceptionalSearch(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-review-contingent-application-detail-search")]
        public HttpResponseMessage LoanReviewContingentApplicationSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.ContingentSearch(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-lien-application-detail-search")]
        public HttpResponseMessage LoanLienApplicationSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.SearchLien(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-lien-applications")]
        public HttpResponseMessage LoanLienApplications()
        {

            var data = repo.GetAllLienRemovalApplications(token.GetStaffId, token.GetCompanyId);
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
        [Route("appraisal-review-referback")]
        public HttpResponseMessage AppraisalReviewReferBack([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            bool response = repo.AppraisalReviewReferBack(entity);
            if (response == true)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("management-position")]
        public HttpResponseMessage UpdateManagementPosition([FromBody] ManagementPositionViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            bool response = repo.UpdateManagementPosition(entity);
            if (response == true) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("management-position/detailId/{detailId}")]
        public HttpResponseMessage GetManagementPosition(int detailId)
        {
            ManagementPositionViewModel data = repo.GetManagementPosition(detailId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-operation/loanId/{loanId}/loanSystemTypeId/{loanSystemTypeId}")]
        public HttpResponseMessage GetLmsOperation(int loanId, short loanSystemTypeId)
        {
            
                var data = repo.GetLMSOperation(loanId, loanSystemTypeId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }else
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("maturityinstruction-operation/loanId/{loanId}/loanSystemTypeId/{loanSystemTypeId}")]
        public HttpResponseMessage GetMaturityInstruction(int loanId, short loanSystemTypeId)
        {
           
                var data = repo.GetMaturityInstruction(loanId, loanSystemTypeId);
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
        [Route("written-off-accrual-amount/loanId/{loanId}/loanSystemTypeId/{loanSystemTypeId}")]
        public HttpResponseMessage GetWrittenOffAccrualAmount(int loanId, short loanSystemTypeId)
        {
            decimal? data = repo.GetWrittenOffAccrualAmount(loanId, loanSystemTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("maximum-application-outstanding-balance/{applicationId}")]
        public HttpResponseMessage GetMaximumApplicationOutstandingBalance(int applicationId)
        {
            decimal data = repo.GetMaximumApplicationOutstandingBalance(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("contingent-used-amount/{contingentLoanId}")]
        public HttpResponseMessage GetContingentTotoalUsed(int contingentLoanId)
        {
            var data = repo.GetContingentTotoalUsed(contingentLoanId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }


    }
}