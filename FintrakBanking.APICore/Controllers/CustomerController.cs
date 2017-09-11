using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/customer")]
    public class CustomerController : ApiControllerBase
    {
        private ICustomerRepository repo;
        string addedUpdated = "";
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CustomerController(ICustomerRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage AddCustomer([FromBody]CustomerViewModels entity)
        {
            try
            {

                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomer(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been created successfully" });
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
        [Route("{customerId}")]
        public HttpResponseMessage DeleteCustomer(int customerId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteCustomer(customerId, user).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The record has been created successfully" });
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

        [HttpGet]
        [Route("")]
        public HttpResponseMessage GetCustomer(int custormerId)
        {

            try
            {
                var data = repo.GetCustomer(custormerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-by-branch")]
        public HttpResponseMessage GetCustomerByBranchId()
        {
            try
            {
                var data = repo.GetCustomerByBranchId(token.GetBranchId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpGet] [Route("customer-by-branch")]
        //public HttpResponseMessage GetCustomerByBranchId()
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        var data = repo.GetCustomerByBranchId(token.GetBranchId);
        //        if (!data.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }

        //}

        [HttpGet]
        [Route("customer")]
        public HttpResponseMessage SearchCustomer(string search)
        {
            try
            {
                var data = repo.CustomerSearch(token.GetCompanyId, search);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customers-in-group/{groupId}")]
        public HttpResponseMessage GetCustomerInGroupByGroupId(int groupId)
        {

            try
            {
                var data = repo.GetCustomerInGroupByGroupId(groupId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-search/realtime/")]
        public HttpResponseMessage SearchCustomerRealTime(string searchQuery)
        {
            try
            {
                var data = repo.CustomerSearchRealTime(token.GetCompanyId, searchQuery);
                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data.ToList() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("customer-search")]
        public HttpResponseMessage SearchCustomer([FromBody] CustomerSearchItemViewModels search)
        {
            try
            {
                var data = repo.CustomerSearch(token.GetCompanyId, search);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-by-company/{companyId}")]
        public HttpResponseMessage GetCustomerByCompanyId(int companyId)
        {
            try
            {
                var data = repo.GetCustomerByCompanyId(companyId);
                if (data == null )
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-by-customer-type/{customertypeId}")]
        public HttpResponseMessage GetCustomerByType(int customertypeId)
        {
            try
            {
                var data = repo.GetCustomerByTypeId(customertypeId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [Route("customertype")]
        public HttpResponseMessage GetCustomerType()
        {
            try
            {
                var data = repo.GetCustomerType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("{customerId}")]
        public HttpResponseMessage UpdateCustomer(int customerId, CustomerViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.UpdateCustomer(customerId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been created successfully" });
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

        [HttpGet]
        [Route("suppliertype")]
        public HttpResponseMessage ClientSupplierType()
        {
            try
            {
                var data = repo.GetClientSupplierType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("identificationMode")]
        public HttpResponseMessage GetIdentificationMode()
        {
            try
            {
                var data = repo.GetIdentificationMode();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("directorsType")]
        public HttpResponseMessage GetDirectorsTypes()
        {
            try
            {
                var data = repo.GetDirectorsTypes();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
        //[HttpPost]
        //[Route("customer-company")]
        //public HttpResponseMessage UpdateCustomerCompanyInformation([FromBody] CustomerCompanyInfomationViewModels entity)
        //{
        //    try
        //    {
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;

        //        var data = repo.UpdateCustomerCompanyInfomation(entity);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, result = data, message = "The record has been updated successfully" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = "There was an error updating this record" });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = $"There was an error updating this record {e.Message}" });
        //    }
        //}
        [HttpPost]
        [Route("customer-identification")]
        public HttpResponseMessage AddCustomerIdentification([FromBody] CustomerIdentificationViewModels entity)
        {
            try
            {
                if (entity.identificationId == 0)
                {
                    addedUpdated = "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data =  repo.AddCustomerIdentification(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("customer-employmentHistory")]
        public HttpResponseMessage AddCustomerEmploymentHistory([FromBody] CustomerEmploymentHistoryViewModels entity)
        {
            try
            {
                if (entity.placeOfWorkId == 0)
                {
                    addedUpdated = "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerEmploymentHistory(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("customer-bvn")]
        public HttpResponseMessage AddCustomerBVN([FromBody] CustomerBvnViewModels entity)
        {
            try
            {
                if (entity.customerBvnid == 0 )
                {
                     addedUpdated= "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                    entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerBvn(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-clientsupplier")]
        public HttpResponseMessage AddCustomerClientSupplier([FromBody] CustomerClientOrSupplierViewModels entity)
        {
            try
            {

                if (entity.client_SupplierId == 0)
                {
                    addedUpdated = "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerClientSupplier(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-companydirectors")]
        public HttpResponseMessage AddCustomerCompanyDiector([FromBody] CustomerCompanyDirectorsViewModels entity)
        {
            try
            {
                if (entity.companyDirectorId == 0)
                {
                    addedUpdated = "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerCompanyDiector(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-address")]
        public HttpResponseMessage AddCustomerAddress([FromBody] CustomerAddressViewModels entity)
        {
            try
            {
                if (entity.addressId == 0)
                {
                    addedUpdated = "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerAddresses(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-phonecontact")]
        public HttpResponseMessage AddCustomerPhoneContact([FromBody] CustomerPhoneContactViewModels entity)
        {
            try
            {
                if (entity.phoneContactId == 0)
                {
                    addedUpdated = "created";
                }
                else
                {
                    addedUpdated = "updated";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerPhoneContact(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {addedUpdated} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {addedUpdated} this record {e.Message}" });
            }
        }
    }
}
