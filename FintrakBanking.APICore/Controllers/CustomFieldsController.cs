using System.Collections.Generic;
using System.Threading.Tasks; 
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit; 
using System;
using FintrakBanking.APICore.core;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using System.Net;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class CustomFieldController : ApiControllerBase
    {
        TokenDecryptionHelper token = null;
        private ICustomFieldsRepository repo;
        IErrorLogRepository errorLogger;
        public CustomFieldController(ICustomFieldsRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region   Custom Fields

        // ossy

         [HttpPost] [ClaimsAuthorization] [Route("custom-field")]
        public HttpResponseMessage AddCustomField([FromBody] AddCustomFieldViewModel model)
        {

            try
            {
                token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                var data = repo.AddCustomField(model).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = model, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
               
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("custom-field")]
        public HttpResponseMessage UpdateCustomField([FromBody] AddCustomFieldViewModel model, int id)
        {
           
              try
              {
                  token = new TokenDecryptionHelper();

                  model.userBranchId = (short)token.GetBranchId;
                  model.companyId = token.GetCompanyId;
                  model.lastUpdatedBy = token.GetStaffId;
                  model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                var data = repo.UpdateCustomField(model, id).IsCompleted;

                  if (data)
                  {
                    return Request.CreateResponse(HttpStatusCode.OK,   new { success = true, result = model, message = "The record has been Update successfully" });
                  }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
              }
              catch (SecureException ex)
              {
                  this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
              }
          
        }

            //ossy


         [HttpPost] [ClaimsAuthorization][Route("custom-field-multiple")]
        public HttpResponseMessage AddCustomFields([FromBody] List<CustomFieldViewModel> listEntity)
        {
            
                try
                {
                    token = new TokenDecryptionHelper();
                    foreach (var entity in listEntity)
                    {
                        entity.userBranchId = (short)token.GetBranchId;
                        entity.companyId = token.GetCompanyId;
                        entity.createdBy = token.GetStaffId;
                        entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                        entity.applicationUrl = HttpContext.Current.Request.Path;
                    }
                    var data = repo.AddCustomFields(listEntity).IsCompleted;
                    if (data)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK,  new { success = true, result = listEntity, message = "The record has been created successfully" });
                    }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
                }
              
        }

        [HttpDelete] [ClaimsAuthorization][Route("custom-field")]
        public HttpResponseMessage DeleteCustomFields([FromBody] List<CustomFieldViewModel> customFields)
        {
             
                try
                {
                    token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path,
                        userIPAddress = HttpContext.Current.Request.UserHostAddress
            };

                    var data = repo.DeleteCustomFields(customFields, user).IsCompleted;
                    if (data)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK,   new { success = true, result = data, message = "Deleted successfully" });
                    }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
                
        }

       [HttpPut] [ClaimsAuthorization] [Route("custom-field-multiple")]
        public HttpResponseMessage UpdateCustomFields([FromBody] List<CustomFieldViewModel> listEntity)
        {
           
                try
                {
                    token = new TokenDecryptionHelper();
                    foreach (var entity in listEntity)
                    {

                        entity.userBranchId = (short)token.GetBranchId;
                        entity.companyId = token.GetCompanyId;
                        entity.lastUpdatedBy = token.GetStaffId;
                      entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                        entity.applicationUrl = HttpContext.Current.Request.Path;
                    }
                    var data =   repo.UpdateCustomFields(listEntity).IsCompleted;

                    if (data)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK,  new { success = true, result = listEntity, message = "The record has been Update successfully" });
                    }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
                }
              
        }

      [HttpGet] [ClaimsAuthorization]   [Route("custom-field/hostPage/{id}")]
        public HttpResponseMessage GetCustomFieldsByHostPageId(int id)
        {            
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.CustomFieldsByHostPageId(id, token.GetCompanyId);
                    if (data == null)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                    }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
              
            }

        #endregion   Custom Fields
        #region   Custom Fields Data
         [HttpPost] [ClaimsAuthorization]
        [Route("custom-field-data")]
        public HttpResponseMessage AddCustomFieldsData([FromBody] List<CustomFieldsDataViewModel> listEntity)
        {
           
            
            
                try
                {
                    token = new TokenDecryptionHelper();
                    foreach (var entity in listEntity)
                    {
                        entity.userBranchId = (short)token.GetBranchId;
                        entity.companyId = token.GetCompanyId;
                        entity.createdBy = token.GetStaffId;
                        entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                        entity.applicationUrl = HttpContext.Current.Request.Path;
                    }
                    var data = repo.AddCustomFieldsData(listEntity).IsCompleted;
                    if (data)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK,   new { success = true, result = listEntity, message = "The record has been created successfully" });
                    }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
                }
          
        }

        [HttpDelete] [ClaimsAuthorization] [Route("custom-field-data")]
        public HttpResponseMessage DeleteCustomFieldsData([FromBody] List<CustomFieldsDataViewModel> listEntity)
        {
           
             
                try
                {
                    token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path ,
                         userIPAddress = HttpContext.Current.Request.UserHostAddress
            };

                    var data =   repo.DeleteCustomFieldsData(listEntity, user).IsCompleted;
                    if (data)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK,   new { success = true, result = data, message = "Deleted successfully" });
                    }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
           
            }

      [HttpGet] [ClaimsAuthorization]  
        [Route("custom-field-data/hostpage/{id}/{customerId}")]
        public HttpResponseMessage GetCustomFieldsDataByCustomField(int id, int customerId)
        {
           
            
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.GetCustomFieldsDataByHostPage(id, customerId, token.GetCompanyId);
                    if (data == null)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                    }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
             
        }

       [HttpPut] [ClaimsAuthorization] [Route("custom-field-data")]
        public HttpResponseMessage UpdateCustomFieldsData([FromBody] List<CustomFieldsDataViewModel> listEntity)
        {
           
               try
                {
                    token = new TokenDecryptionHelper( );
                    foreach (var entity in listEntity)
                    {
                        entity.userBranchId = (short)token.GetBranchId;
                        entity.companyId = token.GetCompanyId;
                        entity.lastUpdatedBy = token.GetStaffId;
                       entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                        entity.applicationUrl = HttpContext.Current.Request.Path;
                    }
                    var data =  repo.UpdateCustomFieldsData(listEntity).IsCompleted;
                    if (data)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK,   new { success = true, result = listEntity, message = "The record has been Update successfully" });
                    }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
                }
            
            }
        #endregion   Custom Fields Data

        #region   host page 

      [HttpGet] [ClaimsAuthorization]   [Route("hostPage/hostpage/{id}")]
        public HttpResponseMessage GetHostPagesChildrenOnly(int id)
        {
           
           
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.GetHostPagesChildrenOnly(id);
                    if (data == null)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                    }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
            

            }

      [HttpGet] [ClaimsAuthorization]  [Route("hostPage")]
        public HttpResponseMessage GetHostPagesParentOnly( )
        {
            
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.GetHostPagesParentOnly();
                    if (data == null)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                    }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (SecureException ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
               
        }
        #endregion   host page 
    }
}
