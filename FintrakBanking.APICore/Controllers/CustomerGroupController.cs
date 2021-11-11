using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/customers")] 
    public class CustomerGroupController : ApiControllerBase
    {
        private ICustomerGroupRepository repo;
        private IErrorLogRepository errorLogger;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CustomerGroupController(ICustomerGroupRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region Customer Group
         [HttpPost] [ClaimsAuthorization]
        [Route("customer-group")]
        public HttpResponseMessage AddCustomerGroup([FromBody] CustomerGroupViewModel entity)
        {

            try
            {

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                if (repo.DoesGroupNameExist(entity.groupName, entity.groupCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "The Group Code or Group Name you entered already exists" });
                }
                var data = repo.AddTempCustomerGroup(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.Created,
                        new
                        {
                            success = true,
                            result = data,
                            message = "The record has been created successfully, now awaiting approval"
                        });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                //errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("customer-group/{groupId}")]
        public HttpResponseMessage DeleteCustomerGroup(short groupId)

        {
            try
            {

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host
                };

                var data = repo.DeleteCustomerGroup(groupId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException e)
            {
               // errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error deleting this record {e.InnerException}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group")]
        public HttpResponseMessage GetCustomerGroup()
        {

            try
            {
                var data = repo.GetCustomerGroup();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
               // errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-temp-customer-groups")]
        public HttpResponseMessage GetTempCustomerGroups()
        {

            try
            {
                var data = repo.GetAllTempCustomerGroups();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                // errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group/awaiting-approval")]
        public HttpResponseMessage GetCustomerGroupAwaitingApproval()
        {
            try
            {

                var data = repo.GetCustomerGroupsAwaitingApprovals(token.GetStaffId, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException e)
            {
                //errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupByCustomerId(int customerGroupId)
        {
            try
            {
                var data = repo.GetCustomerGroupByCustomerId(customerGroupId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


       [HttpPut] [ClaimsAuthorization]
        [Route("customer-group/{customerGroupId}")]
        public HttpResponseMessage UpdateCustomerGroup(int customerGroupId, CustomerGroupViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.UpdateCustomerGroupForApproval(customerGroupId, entity);

                if (data)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully, now awaiting approval" });

                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
               // errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("customer-group/approval")]
        public HttpResponseMessage GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer Group has been approved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
               // errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An error occured: {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-group-mapping/approval")]
        public HttpResponseMessage GoForGroupMappingApproval([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForGroupMappingApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer Group Mapping has been approved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                // errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An error occured: {ex.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group/search/")]
        public HttpResponseMessage SearchForCustomerGroupRealtime(string searchQuery)
        {
            try
            {
                var data = repo.SearchForCustomerGroup(token.GetCompanyId, searchQuery);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data?.ToList() });
                //var data = repo.SearchForCustomerGroupRealtime(token.GetCompanyId, searchQuery);
                //return Request.CreateResponse(HttpStatusCode.OK,
                //    new { success = true, result = data.ToList() });
            }
            catch (SecureException e)
            {
               // errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-group-mapping/awaiting-approval")]
        public HttpResponseMessage GetCustomerGroupMapsAwaitingApprovals()
        {
            var data = repo.GetCustomerGroupMapsAwaitingApprovals(token.GetStaffId, token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = data.ToList() });
        }

        //   [HttpGet] [ClaimsAuthorization]  
        //[Route("all-customer-group-mapping")]
        //public HttpResponseMessage GetAllCustomerGroupMappingByGroupId(int customerGroupId)
        //{
        //    try
        //    {
        //        var data = repo.GetAllCustomerGroupMappingByGroupId(customerGroupId);
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, result = data });
        //    }
        //    catch (SecureException e)
        //    {
        //        //errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = $"Error: {e}" });
        //    }
        //}
        [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group/")]
        public HttpResponseMessage CustomerGroupSearch(string searchQuery)
        {
            try
            {
                var data = repo.CustomerGroupSearch(searchQuery);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data.ToList() });
            }
            catch (SecureException e)
            {
               // errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group/{customerGroupId}/mapping-details")]
        public HttpResponseMessage GetCustomerGroupDetailedMapping(int customerGroupId)
        {
            try
            {
                var data = repo.GetCustomerGroupDetailsByGroupId(customerGroupId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException e)
            {
                //errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e}" });
            }
        }

        #endregion

        #region Customer Group Mapping
         [HttpPost] [ClaimsAuthorization]
        [Route("customer-group-mapping")]
        public HttpResponseMessage AddCustomerGroupMapping([FromBody] CustomerGroupMappingViewModel entity)
        {
          
                var token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.AddTempCustomerGroupMapping(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, new { success = true, result = data, message = "The record has been created successfully, now awaiting approval" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
          
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("customer-group-mapping/multiple")]
        public HttpResponseMessage AddMultipleCustomerGroupMapping([FromBody] List<CustomerGroupMappingViewModel> customerGroups)
        {
            
                var token = new TokenDecryptionHelper();
                var data = repo.AddMultipleCustomerGroupMapping(customerGroups, token.GetStaffId, (short)token.GetBranchId, token.GetCompanyId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, new { success = true, result = data, message = "The record has been created successfully, now awaiting approval." });

                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("customer-group-mapping/{groupMapId}")]
        public HttpResponseMessage DeleteCustomerGroupMapping(int groupMapId)

        {
            var token = new TokenDecryptionHelper();
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host
                };

                var data = repo.DeleteCustomerGroupMapping(groupMapId, user);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException e)
            {
                errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error deleting this record {e.InnerException}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group-mapping")]
        public HttpResponseMessage GetCustomerGroupMapping()
        {

            try
            {
                var data = repo.GetCustomerGroupMapping();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group-members/{groupid}")]
        public HttpResponseMessage GetGroupMembersByGroupId(int groupid)
        {
            try
            {
                var data = repo.GetGroupMembersByGroupId(groupid, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group-mapping/{groupMapId}")]
        public HttpResponseMessage GetCustomerGroupMappingByGroupMapId(int groupMapId)
        {
            try
            {
                var data = repo.GetCustomerGroupMappingByGroupMapId(groupMapId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group-mapping/customers/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupMappingByGroupId(int customerGroupId)
        {
            try
            {
                var data = repo.GetCustomerGroupMappingByGroupId(customerGroupId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-temp-customer-group-mapping/{customerGroupId}")]
        public HttpResponseMessage GetTempCustomerGroupMappingByGroupId(int customerGroupId)
        {
            try
            {
                var data = repo.GetTempCustomerGroupMappingByGroupId(customerGroupId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("customer-group-mapping/relationship-types")]
        public HttpResponseMessage GetCustomerGroupRelationshipTypes()
        {
            try
            {
                var data = repo.GetCustomerGroupRelationshipTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("customer-relationship-type")]
        public HttpResponseMessage AddCustomerGroupRelationshipTypes([FromBody] LookupViewModel entity)
        {

            try
            {
                var data = repo.AddCustomerGroupRelationshipTypes(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.Created,
                        new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("customer-group-mapping/{groupMapId}")]
        public HttpResponseMessage UpdateCustomerGroupMaping(int groupMapId, [FromBody] CustomerGroupMappingViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.UpdateCustomerGroupMapping(groupMapId, entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion
        #region KYC Item Setup
      [HttpGet] [ClaimsAuthorization]  
        [Route("Kycitem")]
        public HttpResponseMessage GetKYCItem()
        {
            try
            {

                var data = repo.GetKYCItems(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("Kycitem")]
        public HttpResponseMessage AddKYCItem([FromBody] KYCItemViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.AddKycItem(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.Created,
                        new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                errorLogger.LogError(e, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
       [HttpPut] [ClaimsAuthorization]
        [Route("Kycitem/{kYCItemId}")]
        public HttpResponseMessage UpdateKYCItem(int kYCItemId, [FromBody] KYCItemViewModel entity)
        {
            try
            {
              
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.UpdatedKycItem(kYCItemId, entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion
    }
}