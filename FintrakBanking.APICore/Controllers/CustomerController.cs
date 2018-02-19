using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.WorkFlow;
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
        private ICustomerStagingRepository stagingRepo;
        private ICustomerProductFeeRepository proRepo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CustomerController(ICustomerRepository _repo, ICustomerStagingRepository _stagingRepo, ICustomerProductFeeRepository _proRepo)
        {
            this.repo = _repo;
            this.stagingRepo = _stagingRepo;
            this.proRepo = _proRepo;
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
               if (entity.customerTypeId == (int)CustomerTypeEnum.Individual)
                {
                    entity.subSectorId = 389;
                }
                if (repo.ValidateCustomerCode(entity.customerCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                      new { success = false, message = $"Customer with code {entity.customerCode} already exist" });
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
        [Route("simple-details/{customerId}")]
        public HttpResponseMessage GetSimpleCustomerDetailsByCustomerId(int customerId)
        {
            try
            {
                var data = repo.GetSimpleCustomerDetailsByCustomerId(customerId);
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
        [Route("customer-staging/")]
        public HttpResponseMessage GetStagedCustomer(string searchTerm)
        {

            try
            {
                var data = stagingRepo.GetIntegratedCustomerInformation(searchTerm);
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                entity.companyId = (short)token.GetCompanyId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                if (repo.ValidateModifiedCustomerRecord(entity.customerId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "Customer General Information is already undergoing approval." });
                }
                var data = repo.UpdateCustomer(customerId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been updated successfully" });
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
                if (repo.ValidateModifiedCompanyRecord(entity.customerId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "Customer Company Information is already undergoing approval." });
                }
                entity.companyId = (short)token.GetCompanyId;
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
                if (repo.ValidateModifiedCustomerRecord(entity.customerId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "Customer Address Information is already undergoing approval." });
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
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

        [HttpPost]
        [Route("customer-nextofkin")]
        public HttpResponseMessage AddCustomerNextOfKin([FromBody]CustomerNextOfKinViewModels entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.nextOfKinId != 0 || entity.nextOfKinId < 0)
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

                var data = repo.AddCustomerNextOfKin(entity);
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
        [Route("customer-children/{childId}")]
        public HttpResponseMessage DeleteCustomerChild( int childId)
        {
            try
            {
             
                var data = repo.DeleteChild(childId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Child Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this child" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
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
        [Route("single-customer-general-info-by-customerid/")]
        public HttpResponseMessage GetSingleCustomerGeneralInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerGeneralInfoByCustomerId(customerId);
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
        [Route("single-customer-nextofkin-info/")]
        public HttpResponseMessage GetSingleCustomerNextOfKinInfo(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerNextOfKinInfo(customerId);
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

        #region Customer Product Fee ( aka Fee Concession)

        [HttpGet]
        [Route("customer-product-fee/customer/{customerId}")]
        public HttpResponseMessage GetCustomerProductFeeByCustomerId(int customerId)
        {
            try
            {
                var data = proRepo.GetCustomerProductFeeByCustomerId(token.GetCompanyId,customerId);
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
        [Route("customer-product-fee/product/{productId}")]
        public HttpResponseMessage GetCustomerProductFeeByProductId(int productId)
        {
            try
            {
                var data = proRepo.GetCustomerProductFeeByProductId(token.GetCompanyId,productId);
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
        [Route("customer-product-fee/{customerProductFeeId}")]
        public HttpResponseMessage UpdateCustomerProductFee(int customerProductFeeId, CustomerProductFeeViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = proRepo.UpdateCustomerProductFee(customerProductFeeId, entity);
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
        [Route("customer-product-fee")]
        public HttpResponseMessage AddCustomerProductFee([FromBody]CustomerProductFeeViewModel entity)
        {
            try
            {
                

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = proRepo.AddCustomerProductFee(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"Customer Product Fee has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error created this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpGet]
        [Route("validate-new-customer/{customerCode}")]
        public HttpResponseMessage ValidateNewCustomerCode(string customerCode)
        {
            try
            {
              
                var data = repo.ValidateCustomerCode(customerCode);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                         new { success = true, message = $"Customer with code {customerCode} already exist on Fintrak" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpGet]
        [Route("validate-customer-modification/{customerId}")]
        public HttpResponseMessage ValidateCustomerModification(int customerId)
        {
            try
            {

                var data = repo.ValidateCustomerModification(customerId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                         new { success = true, message = "Modified Customer Information is undergoing approval. Please contact approving authority." });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "" });
            }
        }
        [HttpGet]
        [Route("customer-information-completed/{customerId}")]
        public HttpResponseMessage CustomerInformationCompleted(int customerId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };

                var data = repo.CustomerInformationCompleted(customerId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
        [HttpDelete]
        [Route("customer-product-fee/{customerProductFeeId}")]
        public HttpResponseMessage DeleteCustomerProductFee(int customerProductFeeId)
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

                var data = proRepo.DeleteCustomerProductFee(customerProductFeeId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error deleting this record" });
            }
            catch (Exception e)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        #endregion

        [HttpGet]
        [Route("customer/approvals/temp")]
        public HttpResponseMessage GetAllCustomerInformationAwaitingApproval()
        {
            try 
            {
                var custInfo = repo.GetAllCustomerInformationAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (custInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = custInfo });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("approval")]
        public HttpResponseMessage GoForApproval([FromBody]ApprovalViewModel entity)
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
                        new { success = true, message = "Customer record has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
//Models
//FinTrakBankingContext