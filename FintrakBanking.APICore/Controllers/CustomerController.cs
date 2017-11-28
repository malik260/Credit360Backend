using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
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
                string createUpdate = "";
                if (entity.customerId != 0 || entity.customerId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomer(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
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
        [Route("{customerId}")]
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
        [Route("customerbyid/{id}")]
        public HttpResponseMessage GetCustomerById(int id)
        {

            try
            {
                var data = repo.GetCustomerAndType(id);
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
        [Route("customerRating/{id}")]
        public HttpResponseMessage GetCustomerRating(int id)
        {

            try
            {
                var data = repo.GetCustomerRating(id);
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
        [Route("customer-casa-information/")]
        public HttpResponseMessage GetCustomerCASAInformation(int customerId)
        {
            try
            {
                var data = repo.GetCustomerCASAInformation(customerId);
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
        [Route("customer-by-loanapplication/")]
        public HttpResponseMessage GetCustomerByLoanapplicationId(int loanApplicationId)
        {

            try
            {
                var data = repo.GetCustomerGeneralInfoByLoanId(loanApplicationId);
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
        [Route("customerbyid/")]
        public HttpResponseMessage GetCustomerByCustomerId(int custormerId)
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
        [Route("customer-information/")]
        public HttpResponseMessage SearchRandomCustomerBySearchQuery(string searchQuery)

        {
            try
            {
                var data = repo.SearchRandomCustomerBySearchQuery(searchQuery);
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
                if (data == null)
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
        [Route("kyc-document-type")]
        public HttpResponseMessage GetKYCDocumentTypes()
        {
            try
            {
                var data = repo.GetKYCDocumentType();
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
        [HttpGet]
        [Route("suppliertype")]
        public HttpResponseMessage GetClientSupplierType()
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
        [Route("customer-address-type")]
        public HttpResponseMessage GetCustomerAddressType()
        {
            try
            {
                var data = repo.GetCustomerAddressType();
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
        [Route("customer-risk-rating")]
        public HttpResponseMessage GetCustomerRiskRating()
        {
            try
            {
                var data = repo.GetCustomerRiskRating();
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

        [HttpPost]
        [Route("customer-company-information")]
        public HttpResponseMessage AddCustomerCompanyInformation([FromBody]CustomerCompanyInfomationViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.companyInfomationId != 0 || entity.companyInfomationId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerCompanyInfomation(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-phonecontact")]
        public HttpResponseMessage AddCustomerPhoneContact([FromBody]CustomerPhoneContactViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.phoneContactId != 0 || entity.phoneContactId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerPhoneContact(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-address")]
        public HttpResponseMessage AddCustomerAddresses([FromBody]CustomerAddressViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.addressId != 0 || entity.addressId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                if (entity.addressTypeId == 0)
                {
                    entity.addressTypeId = (int)CustomerAddressTypeEnum.Corporate;
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerAddresses(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
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
        [HttpPost]
        [Route("customer-bvn")]
        public HttpResponseMessage AddCustomerBvn([FromBody]CustomerBvnViewModels entity)
        {
            try
            {

                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerBvn(entity);
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
        [HttpPost]
        [Route("customer-children")]
        public HttpResponseMessage AddCustomerChildren([FromBody] List<CustomerChildrenViewModel> entity)
        {
            try
            {
                if (entity.Count <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Please select add Children to continue" });
                }


                var data = repo.AddCustomerChildren(entity, token.GetStaffId, (short)token.GetBranchId);
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
        [HttpPost]
        [Route("customer-employmentHistory")]
        public HttpResponseMessage AddCustomerEmploymentHistory([FromBody]CustomerEmploymentHistoryViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.placeOfWorkId != 0 || entity.placeOfWorkId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerEmploymentHistory(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-companydirectors")]
        public HttpResponseMessage AddCustomerCompanyDirector([FromBody]CustomerCompanyDirectorsViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.companyDirectorId != 0 || entity.companyDirectorId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateCustomerBVN(entity.customerId, entity.bankVerificationNumber))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "The BVN you entered already exist" });
                    }
                    if (entity.rcNumber != null)
                    {
                        if (repo.ValidateCustomerRCnumber(entity.customerId, entity.rcNumber))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "The Registration Number you entered already exist" });
                        }
                    }
                    if (repo.ValidateCustomerEmail(entity.customerId, entity.email))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "The Email Address you entered already exist" });
                    }
                    if (entity.taxNumber != null && entity.taxNumber != "")
                    {
                        if (repo.ValidateCustomerTIN(entity.customerId, entity.taxNumber))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "The Tax Identification Number you entered already exist" });
                        }
                    }
                }

                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerCompanyDirector(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-clientsupplier")]
        public HttpResponseMessage AddCustomerClientSupplier([FromBody]CustomerClientOrSupplierViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.client_SupplierId != 0 || entity.client_SupplierId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateClientSupplierEmail(entity.customerId, entity.client_SupplierEmail))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "The Email Address you entered already exist" });
                    }
                    if (entity.taxNumber != null && entity.taxNumber != "")
                    {
                        if (repo.ValidateClientSupplierTIN(entity.customerId, entity.taxNumber))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "The Tax Identification Number you entered already exist" });
                        }
                    }
                    if (entity.rcNumber != "" && entity.rcNumber != null)
                    {
                        if (repo.ValidateClientSupplierRCnumber(entity.customerId, entity.rcNumber))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "The Registration Number you entered already exist" });
                        }
                    }
                }

                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerClientSupplier(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("customer-identification")]
        public HttpResponseMessage AddCustomerIdentification([FromBody]CustomerIdentificationViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.identificationModeId != 0 || entity.identificationModeId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomerIdentification(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


        #region Single Customer Information By CustomerID
        [HttpGet]
        [Route("single-customer-general-info/")]
        public HttpResponseMessage GetSingleCustomerGeneralInfo(string customerCode)
        {
            try
            {
                var data = repo.GetSingleCustomerGeneralInfo(customerCode);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("single-customer-company-info/")]
        public HttpResponseMessage GetSingleCustomerCompanyInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerCompanyInfo(customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("single-customer-address-info/")]
        public HttpResponseMessage GetSingleCustomerAddressInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerAddressInfo(customerId);
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
        [Route("single-customer-phonecontact-info/")]
        public HttpResponseMessage GetSingleCustomerPhoneContactInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerPhoneContactInfo(customerId);
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
        [Route("single-customer-bvn-info/")]
        public HttpResponseMessage GetSingleCustomerBVNInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerBVNInfo(customerId);
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
        [Route("single-customer-identification-info/")]
        public HttpResponseMessage GetSingleCustomerIdentificationInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerIdentificationInfo(customerId);
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
        [Route("single-customer-employment-info/")]
        public HttpResponseMessage GetSingleCustomerEmploymentHistoryInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerEmploymentHistoryInfo(customerId);
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
        [Route("single-customer-board-info/")]
        public HttpResponseMessage GetSingleCustomerBoardInfo(int customerId)
        {
            try
            {
                var directorTypeId = (short)CompanyDirectorTypeEnum.BoardMember;
                var data = repo.GetSingleCustomerDirectorInfo(customerId, directorTypeId);
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
        [Route("single-customer-shareholder-individual/")]
        public HttpResponseMessage GetSingleCustomerShareholderIndividual(int customerId)
        {
            try
            {
                var customerTypeId = (short)CustomerTypeEnum.Individual;
                var data = repo.GetSingleCustomerShareholderInfo(customerId, customerTypeId);
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
        [Route("single-customer-shareholder-corporate/")]
        public HttpResponseMessage GetSingleCustomerShareholderCorporate(int customerId)
        {
            try
            {
                var customerTypeId = (short)CustomerTypeEnum.Corporate;
                var data = repo.GetSingleCustomerShareholderInfo(customerId, customerTypeId);
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
        [Route("customer-shareholder-beneficial/")]
        public HttpResponseMessage GetCustomerShareholderUltimateBeneficial(int companyDirectorId)
        {
            try
            {
                var data = repo.GetShareholderUltimateBeneficial(companyDirectorId);
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
        [Route("single-customer-accountsignatory-info/")]
        public HttpResponseMessage GetSingleCustomerAccountSignatoryInfo(int customerId)
        {
            try
            {
                var directorTypeId = (short)CompanyDirectorTypeEnum.Account_Signatory;
                var data = repo.GetSingleCustomerDirectorInfo(customerId, directorTypeId);
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
        [Route("single-customer-client-info/")]
        public HttpResponseMessage GetSingleCustomerClientInfo(int customerId)
        {
            try
            {
                var clientTypeId = (short)CompanyClientOrSupplierTypeEnum.Client;
                var data = repo.GetSingleCustomerClientOrSupplierInfo(customerId, clientTypeId);
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
        [Route("single-customer-supplier-info/")]
        public HttpResponseMessage GetSingleCustomerSupplierInfo(int customerId)
        {
            try
            {
                var supplierTypeId = (short)CompanyClientOrSupplierTypeEnum.Supplier;
                var data = repo.GetSingleCustomerClientOrSupplierInfo(customerId, supplierTypeId);
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
        [Route("single-customer-children-info/")]
        public HttpResponseMessage GetSingleCustomerChildrenInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerChildrenInfo(customerId);
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

        #endregion

    }
}
//Models
//FinTrakBankingContext