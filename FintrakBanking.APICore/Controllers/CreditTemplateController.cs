using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class CreditTemplateController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ICreditTemplateRepository repo;
        private IMemorandumRepository _memoRepo;
        private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

        public CreditTemplateController(ICreditTemplateRepository repo, IMemorandumRepository memoRepo)
        {
            this.repo = repo;
            this._memoRepo = memoRepo;
        }

        #region DOCUMENT TEMPLATE DEPRECATED

        [HttpGet] [ClaimsAuthorization]  
        [Route("credit-template")]
        public HttpResponseMessage GetCreditTemplate()
        {
            try
            {
                var data = repo.GetAllCreditTemplate();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("credit-template/{creditTemplateId}")]
        public HttpResponseMessage GetCreditTemplate(int creditTemplateId)
        {
            try
            {
                var data = repo.GetCreditTemplate(creditTemplateId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("credit-template/by-level-product")]
        public HttpResponseMessage GetCreditTemplateLevelProduct(int approvalLevelId, int productClassId)
        {
            try
            {
                var data = repo.GetAllCreditTemplateByLevelProduct(approvalLevelId, productClassId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("credit-template/level/{approvalLevelId}")]
        public HttpResponseMessage GetCreditTemplateLevelId(int approvalLevelId)
        {
            try
            {
                var data = repo.GetCreditTemplateByLevelId(approvalLevelId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("credit-template/product-class")]
        public HttpResponseMessage GetCreditTemplateProductClass(int productClassId)
        {
            try
            {
                var data = repo.GetAllCreditTemplateByProductClass(productClassId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("credit-template")]
        public HttpResponseMessage AddCreditTemplate([FromBody] CreditTemplateViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddCreditTemplate(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("credit-template/{creditTemplateId}")]
        public HttpResponseMessage UpdateCreditTemplate([FromBody] CreditTemplateViewModel entity, int creditTemplateId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateCreditTemplate(entity, creditTemplateId);
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

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("credit-template/")]
        public HttpResponseMessage DeleteCreditTemplate( int creditTemplateId)
        {
            try
            {
             
                var data = repo.DeleteCreditTemplate(creditTemplateId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true,  message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        #endregion DOCUMENT TEMPLATE DEPRECATED

        #region DOCUMENT TEMPLATE SETUP
        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-template-setup")]
        public HttpResponseMessage GetAllDocumentTemplateSetup()

        {
            try
            {
                var data = repo.GetAllDocumentTemplateSetup();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-template-section-setup")]
        public HttpResponseMessage GetAllDocumentTemplateSectionSetup(int templateId)

        {
            try
            {
                var data = repo.GetAllDocumentTemplateSectionSetup(templateId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-template-section-role-setup")]
        public HttpResponseMessage GetAllDocumentTemplateSectionRoleSetup(int templatesectionId)

        {
            try
            {
                var data = repo.GetAllDocumentTemplateSectionRoleSetup(templatesectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-template")]
        public HttpResponseMessage AddDocumentTemplate([FromBody] DocumentTemplateViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.AddDocumentTemplate(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-template/{documentTemplateId}")]
        public HttpResponseMessage UpdateDocumentTemplate([FromBody] DocumentTemplateViewModel entity, int documentTemplateId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.UpdateDocumentTemplate(entity, documentTemplateId);
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
        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-template/")]
        public HttpResponseMessage DeleteDocumentTemplate(int documentTemplateId)
        {
            try
            {

                var data = repo.DeleteDocumentTemplate(documentTemplateId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }




        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-template-section")]
        public HttpResponseMessage AddDocumentTemplateSection([FromBody] DocumentTemplateSectionViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.AddDocumentTemplateSection(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-template-section/{documentTemplateSectionId}")]
        public HttpResponseMessage UpdateDocumentTemplateSection([FromBody] DocumentTemplateSectionViewModel entity, int documentTemplateSectionId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.UpdateDocumentTemplateSection(entity, documentTemplateSectionId);
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
        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-template-section/")]
        public HttpResponseMessage DeleteDocumentTemplateSection(int documentTemplateSectionId)
        {
            try
            {
                var userBranchId = (short)token.GetBranchId;
                var companyId = token.GetCompanyId;
                var lastUpdatedBy = token.GetStaffId;
                var applicationUrl = HttpContext.Current.Request.Path;
                var userIPAddress = Request.RequestUri.Host;
                var data = repo.DeleteDocumentTemplateSection(documentTemplateSectionId, userBranchId, companyId, lastUpdatedBy, applicationUrl, userIPAddress);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-template-section-role")]
        public HttpResponseMessage AddDocumentTemplateSectionRole([FromBody] DocumentTemplateSectionRoleViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.AddDocumentTemplateSectionRole(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-template-section-role/{sectionRoleId}")]
        public HttpResponseMessage UpdateDocumentTemplateSectionRole([FromBody] DocumentTemplateSectionRoleViewModel entity, int sectionRoleId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;


                entity.sectionRoleId = sectionRoleId;
                var data = repo.UpdateDocumentTemplateSectionRole(entity);
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
        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-template-section-role/")]
        public HttpResponseMessage DeleteDocumentTemplateSectionRole(int sectionRoleId)
        {
            try
            {
                var userBranchId = (short)token.GetBranchId;
                var companyId = token.GetCompanyId;
                var lastUpdatedBy = token.GetStaffId;
                var applicationUrl = HttpContext.Current.Request.Path;
                var userIPAddress = Request.RequestUri.Host;
                var data = repo.DeleteDocumentTemplateSectionRole(sectionRoleId, userBranchId, companyId, lastUpdatedBy, applicationUrl, userIPAddress);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion DOCUMENT TEMPLATE SETUP



        #region DOCUMENT TEMPLATE IMPL
        // added by Ade for drawdown memo
        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-drawdown-memo-html/{targetId}/targetId")]
        public HttpResponseMessage GetDrawdownMemoHtml(int targetId)
        {
            try
            {
                var response = _memoRepo.GetDrawdownMemoHtml(token.GetStaffId, targetId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-call-memo-html/{id}")]
        public HttpResponseMessage GetCallMemoHtml([FromUri] int id)
        {
            try
            {
                var response = _memoRepo.GetCallMemoMarkup(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("confirm-insurance-policy-approval-status/{appDetailId}")]
        public HttpResponseMessage GetInsurancePolicyConfirmationStatus(int appDetailId)
        {
            try
            {

                var data = repo.GetInsurancePolicyConfirmationStatus(token.GetStaffId, appDetailId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("confirm-insurance-policy-approval-status-by-appdetail-id/{appDetailId}")]
        public HttpResponseMessage GetInsurancePolicyConfirmationStatusByAppDetailId(int appDetailId)
        {
            try
            {

                var data = repo.GetInsurancePolicyConfirmationStatusByAppDetailId(token.GetStaffId, appDetailId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-cashback-memo-html/{operationId}/operationId/{targetId}/targetId")]
        public HttpResponseMessage GetCashBackMemoHtml(int operationId, int targetId)
        {
            try
            {
                var response = _memoRepo.CashBackMemoMarkupHtml(token.GetStaffId, operationId, targetId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-drawdown-memo/{operationId}/operationId/{targetId}/targetId")]
        public HttpResponseMessage GetDrawdownMemo([FromUri] int operationId, [FromUri] int targetId)
        {
            try
            {
                var roleId = token.GetRoleId;      
                var response = _memoRepo.DocumentationDeferralWaiverFormHtml(token.GetStaffId, operationId, targetId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-section/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetLoadedDocumentSections(int operationId, int targetId)
        {
            try
            {
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentSections(token.GetStaffId, operationId, targetId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-section-bulk-liquidation/operation/{operationId}/target/{targetId}/section/{sectionId}")]
        public HttpResponseMessage GetDocumentSectionBulkLiquidation(int operationId, int targetId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetDocumentSectionBulkLiquidation(token.GetStaffId,operationId,targetId,sectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-section/operation/{operationId}/target/{targetId}/section/{sectionId}")]
        public HttpResponseMessage GetDocumentSection(int operationId, int targetId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetDocumentSection(token.GetStaffId, operationId, targetId, sectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-section/operation/{operationId}/target/{targetId}/targetIdForWorkFlow/{targetIdForWorkFlow}/section/{sectionId}/generic/{customerId}")]
        public HttpResponseMessage GetDocumentSectionGeneric(int operationId, int targetId, int targetIdForWorkFlow, int sectionId, int customerId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetDocumentSection(token.GetStaffId, operationId, targetId, sectionId, customerId, targetIdForWorkFlow, true);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-exception-section/operation/{operationId}/target/{targetId}/section/{sectionId}")]
        public HttpResponseMessage GetExceptionDocumentSection(int operationId, int targetId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetExceptionDocumentSection(token.GetStaffId, operationId, targetId, sectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-analysis-document-section/operation/{operationId}/target/{targetId}/referenceId/{referenceId}/section/{sectionId}")]
        public HttpResponseMessage GetRecoveryAnalysisDocumentSection(int operationId, int targetId, string referenceId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetRecoveryAnalysisDocumentSection(token.GetStaffId, operationId, targetId, referenceId,  sectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("third-party-loan-document-section/operation/{operationId}/target/{targetId}/section/{sectionId}")]
        public HttpResponseMessage GetThirdpartLoanDocumentSection(int operationId, int targetId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetThirdPartyLoanDocumentSection(token.GetStaffId, operationId, targetId, sectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-template/operation/{operationId}")]
        public HttpResponseMessage GetDocumentTemplates(int operationId)
        {
            try
            {
                List<DocumentTemplateViewModel> response = repo.GetDocumentTemplates(token.GetStaffId, operationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        
        [HttpGet]
        [ClaimsAuthorization]
        [Route("preview-document-bulk-liquidation/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetLoadedDocumentBulkLiquidation(int operationId, int targetId)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentBulkLiquidation(token.GetStaffId,operationId, targetId,user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("documentation/operation/{operationId}/target/{targetId}/{isThirdPartyFacility}")]
        public HttpResponseMessage GetLoadedDocumentation(int operationId, int targetId, bool isThirdPartyFacility)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentation(token.GetStaffId, operationId, targetId, user, isThirdPartyFacility);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("documentation/operation/{operationId}/target/{targetId}/approvalLevelId/{approvalLevelId}/{isThirdPartyFacility}")]
        public HttpResponseMessage GetLoadedDocumentationStamped(int operationId, int targetId, int approvalLevelId, bool isThirdPartyFacility)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentationStamped(token.GetStaffId, operationId, targetId, user, approvalLevelId, isThirdPartyFacility);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("remove-documentation/operation/{operationId}/target/{targetId}/approvalLevelId/{approvalLevelId}/{isThirdPartyFacility}")]
        public HttpResponseMessage RemoveLoadedDocumentationStamped(int operationId, int targetId, int approvalLevelId, bool isThirdPartyFacility)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.RemoveLoadedDocumentationStamped(token.GetStaffId, operationId, targetId, user, approvalLevelId, isThirdPartyFacility);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("documentation/operation/{operationId}/target/{targetId}/targetIdForWorkFlow/{targetIdForWorkFlow}/generic/{customerId}")]
        public HttpResponseMessage GetLoadedDocumentationGeneric(int operationId, int targetId, int targetIdForWorkFlow, int customerId)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentationGeneric(token.GetStaffId, operationId, targetId, targetIdForWorkFlow, user, customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("exception-documentation/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetLoadedExceptionDocumentation(int operationId, int targetId)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedExceptionDocumentation(token.GetStaffId, operationId, targetId, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-analysis-documentation/operation/{operationId}/target/{targetId}/referenceId/{referenceId}/templateId/{templateId}")]
        public HttpResponseMessage getRecoveryAnalysisDocumentation(int operationId, int targetId, string referenceId, int templateId)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.getRecoveryAnalysisDocumentation(token.GetStaffId, operationId, targetId, referenceId, user, templateId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("documentation-bulk-liquidation/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetLoadedDocumentationBulkLiquidation(int operationId, int targetId)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentationBulkLiquidation(token.GetStaffId, operationId, targetId, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-template/load")]
        public HttpResponseMessage LoadDocumentTemplate([FromBody] DocumentTemplateViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                bool response = repo.LoadDocumentTemplate(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-template-lms/load")]
        public HttpResponseMessage LoadDocumentTemplateLms([FromBody] DocumentTemplateViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                bool response = repo.LoadDocumentTemplateLMS(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-section")]
        public HttpResponseMessage SaveLoadedDocumentSection([FromBody] LoadedDocumentSectionViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                bool response = repo.SaveLoadedDocumentSection(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("append-digital-stamp")]
        public HttpResponseMessage AppendDigitalStamp([FromBody] LoadedDocumentSectionViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                bool response = repo.AppendDigitalStamp(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("remove-digital-stamp")]
        public HttpResponseMessage RemoveDigitalStamp([FromBody] LoadedDocumentSectionViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                bool response = repo.RemoveDigitalStamp(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("isLLLViolated/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetIsLLLViolated(int operationId, int targetId)
        {
            try
            {
                dynamic response = repo.GetIsLLLViolated(operationId, targetId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion DOCUMENT TEMPLATE IMPL

    }
}
