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

        public CreditTemplateController(ICreditTemplateRepository repo)
        {
            this.repo = repo;
        }

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
        [Route("document-section/operation/{operationId}/section/{sectionId}")]
        public HttpResponseMessage GetDocumentSection(int operationId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = repo.GetDocumentSection(token.GetStaffId,operationId,sectionId);
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
        [Route("documentation/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetLoadedDocumentation(int operationId, int targetId)
        {
            try
            {
                List<LoadedDocumentSectionViewModel> response = repo.GetLoadedDocumentation(token.GetStaffId,operationId, targetId);
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

        #endregion DOCUMENT TEMPLATE IMPL

    }
}
