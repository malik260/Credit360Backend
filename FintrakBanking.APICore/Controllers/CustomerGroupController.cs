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

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/customers")]
    public class CustomerGroupController : ApiControllerBase
    {
        private ICustomerGroupRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CustomerGroupController(ICustomerGroupRepository _repo)
        {
            this.repo = _repo;
        }
        
        #region Customer Group
        [HttpPost]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpDelete]
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
            catch (Exception e)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error deleting this record {e.InnerException}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-group/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupByCustomerId(int customerGroupId)
        {
            try
            {
                var data = repo.GetCustomerGroupByCustomerId(customerGroupId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        [HttpPut]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("customer-group/approval")]
        public async Task<HttpResponseMessage> GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var data = await repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "customer group has been approved successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An error occured: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-group/search/")]
        public HttpResponseMessage SearchCustomerGroup(string searchQuery)
        {
            try
            {
                var data = repo.SearchForCustomerGroup(token.GetCompanyId, searchQuery);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data.ToList() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e}" });
            }
        }

        #endregion

        #region Customer Group Mapping
        [HttpPost]
        [Route("customer-group-mapping")]
        public HttpResponseMessage AddCustomerGroupMapping([FromBody] CustomerGroupMappingViewModel entity)
        {
            try
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("customer-group-mapping/multiple")]
        public HttpResponseMessage AddMultipleCustomerGroupMapping([FromBody] List<CustomerGroupMappingViewModel> customerGroups)
        {
            try
            {
                var data = repo.AddMultipleCustomerGroupMapping(customerGroups);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, new { success = true, result = data, message = "The record has been created successfully" });

                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.InnerException}" });
            }
        }

        [HttpDelete]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error deleting this record {e.InnerException}" });
            }
        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("customer-group-members/{groupid}")]
        public HttpResponseMessage GetGroupMembersByGroupId(int groupid)
        {
            try
            {
                var data = repo.GetGroupMembersByGroupId(groupid, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-group-mapping/{groupMapId}")]
        public HttpResponseMessage GetCustomerGroupMappingByGroupMapId(int groupMapId)
        {
            try
            {
                var data = repo.GetCustomerGroupMappingByGroupMapId(groupMapId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        [HttpGet]
        [Route("customer-group-mapping/customers/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupMappingByGroupId(int customerGroupId)
        {
            try
            {
                var data = repo.GetCustomerGroupMappingByGroupId(customerGroupId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-group-mapping/relationship-types")]
        public HttpResponseMessage GetCustomerGroupRelationshipTypes()
        {
            try
            {
                var data = repo.GetCustomerGroupRelationshipTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion
    }
}