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
using FintrakBanking.Common.CustomException;

using System.Web;
using System.Web.Http;
using FintrakBanking.ViewModels.Credit;
using System.Threading;

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
        [ClaimsAuthorization]
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
                entity.customerSensitivityLevelId = 1;
                var data = repo.AddCustomer(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-type-legal-status")]
        public HttpResponseMessage GetAllCRMSLegalStatus()
        {
            try
            {
                var data = repo.GetAllCRMSLegalStatus();

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
        [Route("politically-exposed-person/{customerCode}")]
        public HttpResponseMessage GetPoliticallyExposedPerson(string customerCode)
        {
            var data = repo.GetPoliticallyExposedPerson(customerCode);

            if (!data)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("update-customer-information/customerCode/{customerCode}/accountNumber/{accountNumber}")]
        public HttpResponseMessage UpdateCustomerInformation(string customerCode, string accountNumber)
        {
            var result = repo.UpdateCustomerInformation(customerCode, accountNumber, token.GetStaffId);

            if (!result)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Customer information update failed" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Customer information updated successfully" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-type-company-size")]
        public HttpResponseMessage GetAllCRMSCompanySize()
        {
            try
            {
                var data = repo.GetAllCRMSCompanySize();

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
        [Route("refresh-customer-account/{customerId}")]
        public HttpResponseMessage RefreshCustomerAccount(int customerId)
        {
            try
            {
                var result = repo.refreshCustomerAccount(customerId);
                if(result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Customer Account Refreshed Successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error occurred, Please Contact the System Administrator" });
                }
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, errorCode = "99" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
           
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-type-relationship-type")]
        public HttpResponseMessage GetAllCRMSRelationshipType()
        {
            try
            {
                var data = repo.GetAllCRMSRelationshipType();

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
        [Route("crms-type-relationship-type-by-customer-type/")]
        public HttpResponseMessage GetAllCRMSRelationshipTypeByType(int type)
        {
            try
            {
                var data = repo.GetAllCRMSRelationshipTypeByType(type);

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
        [Route("crms-type-legal-status-by-customer-type/")]
        public HttpResponseMessage GetAllCRMSLegalStatusByType(int type)
        {
            try
            {

                var data = repo.GetAllCRMSLegalStatusByType(type);

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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-staging/isProspectConversion/{isProspectConversion}/")]
        public HttpResponseMessage GetStagedCustomer(string searchTerm, [FromUri] bool isProspectConversion)
        {
            try
            {
                var data = stagingRepo.GetIntegratedCustomerInformation(searchTerm, isProspectConversion);
                if (data != null && data.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (APIErrorException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $" {e.Message}" });
            }
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("merge-duplicate-customers/accounNumber/{accounNumber}/prospectiveCustomerCode/{prospectiveCustomerCode}")]
        public HttpResponseMessage MergeDuplicateCustomers(string accounNumber, string prospectiveCustomerCode)
        {
            try
            {
                var result = stagingRepo.MergeDuplicateCustomers(accounNumber, prospectiveCustomerCode, token.GetStaffId);

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "Duplicate customer records have been merged successfully!" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "Duplicate customer records failed to merge!" });
            }
            catch (APIErrorException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $" {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("customerRating/{id}")]
        public HttpResponseMessage GetCustomerRating(int id)
        {

                var data = repo.GetCustomerRating(id);
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
        [Route("customer-casa-information/")]
        public HttpResponseMessage GetCustomerCASAInformation(int customerId)
        {
                var data = repo.GetCustomerCASAInformation(customerId);
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
        [Route("customer-casa-information/{customerCode}")]
        public HttpResponseMessage GetCustomerCASAInformationByCustomerCode(string customerCode)
        {
            
                var data = repo.GetCustomerCASAInformation(customerCode);
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
        [Route("customer-by-loanapplication/")]
        public HttpResponseMessage GetCustomerByLoanapplicationId(int loanApplicationId)
        {

                var data = repo.GetCustomerGeneralInfoByLoanId(loanApplicationId);
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
        [Route("customer-by-lms-loanapplication/")]
        public HttpResponseMessage GetCustomerByLMSLoanapplicationId(int loanApplicationId)
        {

                var data = repo.GetCustomerGeneralInfoByLMSLoanId(loanApplicationId);
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customers-information/")]
        public HttpResponseMessage SearchRandomSingleCustomersBySearchQuery(string searchQuery)
        {
            try
            {
                var data = repo.SearchRandomSingleCustomersBySearchQuery(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("group-customer-members/{groupId}/")]
        public HttpResponseMessage SearchGroupCustomersBySearchQuery(string searchQuery, int groupId)
        {
            var data = repo.SearchGroupCustomersBySearchQuery(searchQuery, groupId);
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
        [Route("customers-information/")]
        public HttpResponseMessage SearchRandomCustomersBySearchQuery(string searchQuery)
        {
            try
            {
                var data = repo.SearchRandomCustomersBySearchQuery(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-corporate-customers-information/")]
        public HttpResponseMessage SearchRandomSingleCorporateCustomersBySearchQuery(string searchQuery)
        {
            try
            {
                var data = repo.SearchRandomSingleCorporateCustomersBySearchQuery(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("group-customers-information/")]
        public HttpResponseMessage SearchRandomGroupCustomersBySearchQuery(string searchQuery)

        {
            try
            {
                var data = repo.SearchRandomGroupCustomersBySearchQuery(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-search/realtime/")]
        public HttpResponseMessage SearchCustomerRealTime(string searchQuery)
        {
            try
            {
                var data = repo.CustomerSearchRealTime(token.GetCompanyId, searchQuery);
                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data.ToList() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-search")]
        public HttpResponseMessage SearchCustomer([FromBody] CustomerSearchItemViewModels search)
        {
            try
            {
                var data = repo.CustomerSearch(token.GetCompanyId, search);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-ibl-eligibility/{customerId}")]
        public HttpResponseMessage GetCustomerIBLEligibility(int customerId)
        {
            try
            {
                var data = repo.GetCustomerIBLEligibility(customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut, Route("customer-ibl-eligibility-update")]
        public HttpResponseMessage AssignApplication([FromBody] int iblEligibilityId)
        {

            var updated = repo.updateIBLEligibility(iblEligibilityId);
            if (updated)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "updated successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("eligibility-search")]
        public HttpResponseMessage EligibilitySearch([FromBody] CustomerEligibilityViewModels search)
        {
            try
            {
                var data = repo.EligibilitySearch(token.GetCompanyId, search);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = data.response_descr });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("corporate-customer-type")]
        public HttpResponseMessage GetCorporateCustomerType()
        {
            try
            {
                var data = repo.GetCorporateCustomerType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customertype-with-hybrid")]
        public HttpResponseMessage GetCustomerTypeWithHybrid()
        {
            try
            {
                var data = repo.GetCustomerTypeWithHybrid();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
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
                entity.customerSensitivityLevelId = 1;
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerCompanyInfomation(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                if (repo.ValidateModifiedPhoneRecord(entity.customerId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                           new { success = false, message = "Customer Phone Contact Information is already undergoing approval." });
                }
                var data = repo.AddCustomerPhoneContact(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                if (repo.ValidateModifiedAddressRecord(entity.customerId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "Customer Address Information is already undergoing approval." });
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerAddresses(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerEmploymentHistory(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                    //if (repo.ValidateCustomerBVN(entity.customerId, entity.bankVerificationNumber))
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK,
                    //       new { success = false, message = "The BVN you entered already exist" });
                    //}
                    //if (entity.rcNumber != null && entity.customerTypeId == (int)CustomerTypeEnum.Corporate)
                    //{
                    //    if (repo.ValidateCustomerRCnumber(entity.customerId, entity.rcNumber))
                    //    {
                    //        return Request.CreateResponse(HttpStatusCode.OK,
                    //           new { success = false, message = "The Registration Number you entered already exist" });
                    //    }
                    //}
                    //if (repo.ValidateCustomerEmail(entity.customerId, entity.email))
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK,
                    //       new { success = false, message = "The Email Address you entered already exist" });
                    //}
                    //if (entity.taxNumber != null && entity.taxNumber != "")
                    //{
                    //    if (repo.ValidateCustomerTIN(entity.customerId, entity.taxNumber))
                    //    {
                    //        return Request.CreateResponse(HttpStatusCode.OK,
                    //           new { success = false, message = "The Tax Identification Number you entered already exist" });
                    //    }
                    //}
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerCompanyDirector(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"{e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                    //if (repo.ValidateClientSupplierEmail(entity.customerId, entity.client_SupplierEmail))
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK,
                    //       new { success = false, message = "The Email Address you entered already exist" });
                    //}
                    //if (entity.taxNumber != null && entity.taxNumber != "")
                    //{
                    //    if (repo.ValidateClientSupplierTIN(entity.customerId, entity.taxNumber))
                    //    {
                    //        return Request.CreateResponse(HttpStatusCode.OK,
                    //           new { success = false, message = "The Tax Identification Number you entered already exist" });
                    //    }
                    //}
                    //if (entity.rcNumber != "" && entity.rcNumber != null)
                    //{
                    //    if (repo.ValidateClientSupplierRCnumber(entity.customerId, entity.rcNumber))
                    //    {
                    //        return Request.CreateResponse(HttpStatusCode.OK,
                    //           new { success = false, message = "The Registration Number you entered already exist" });
                    //    }
                    //}
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerClientSupplier(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerIdentification(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddCustomerNextOfKin(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpDelete]
        [ClaimsAuthorization]
        [Route("customer-children/{childId}")]
        public HttpResponseMessage DeleteCustomerChild(int childId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                  //  userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteChild(childId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Child Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this child" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }
        [HttpDelete]
        [ClaimsAuthorization]
        [Route("company-ultimate-beneficial/{companyBeneficialId}")]
        public HttpResponseMessage DeleteUltimateBeneficial(int companyBeneficialId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //  userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteUltimateBeneficial(companyBeneficialId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        #region Single Customer Information By CustomerID
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-general-info-by-customerid/")]
        public HttpResponseMessage GetSingleCustomerGeneralInfo(int customerId, int targetId)
        {
            try
            {
                var data = repo.GetSingleCustomerGeneralInfoByCustomerId(customerId, targetId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-company-info/")]
        public HttpResponseMessage GetSingleCustomerCompanyInfo(int customerId, int targetId)
        {
            try
            {
                var data = repo.GetSingleCustomerCompanyInfo(customerId, targetId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-address-info/")]
        public HttpResponseMessage GetSingleCustomerAddressInfo(int customerId, int targetId)
        {
            try
            {
                var data = repo.GetSingleCustomerAddressInfo(customerId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-phonecontact-info/")]
        public HttpResponseMessage GetSingleCustomerPhoneContactInfo(int customerId, int targetId)
        {
            try
            {
                var data = repo.GetSingleCustomerPhoneContactInfo(customerId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-related-employer/{customerId}")]
        public HttpResponseMessage GetSingleCustomerRelatedEmployer(int customerId)
        {
            try
            {
                var data = repo.GetSingleCustomerRelatedEmployer(customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-employment-info/")]
        public HttpResponseMessage GetSingleCustomerEmploymentHistoryInfo(int customerId, int targetId)
        {
            try
            {
                var data = repo.GetSingleCustomerEmploymentHistoryInfo(customerId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-board-info/")]
        public HttpResponseMessage GetSingleCustomerBoardInfo(int customerId, int targetId)
        {
            try
            {
                var directorTypeId = (short)CompanyDirectorTypeEnum.BoardMember;
                var data = repo.GetSingleCustomerDirectorInfo(customerId, directorTypeId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-shareholder-individual/")]
        public HttpResponseMessage GetSingleCustomerShareholderIndividual(int customerId, int targetId)
        {
            try
            {
                var customerTypeId = (short)CustomerTypeEnum.Individual;
                var data = repo.GetSingleCustomerShareholderInfo(customerId, customerTypeId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-shareholder-corporate/")]
        public HttpResponseMessage GetSingleCustomerShareholderCorporate(int customerId, int targetId)
        {
            try
            {
                var customerTypeId = (short)CustomerTypeEnum.Corporate;
                var data = repo.GetSingleCustomerShareholderInfo(customerId, customerTypeId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-accountsignatory-info/")]
        public HttpResponseMessage GetSingleCustomerAccountSignatoryInfo(int customerId, int targetId)
        {
            try
            {
                var directorTypeId = (short)CompanyDirectorTypeEnum.Account_Signatory;
                var data = repo.GetSingleCustomerDirectorInfo(customerId, directorTypeId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-client-info/")]
        public HttpResponseMessage GetSingleTempCustomerClientInfo(int customerId, int targetId)
        {
            try
            {
                var clientTypeId = (short)CompanyClientOrSupplierTypeEnum.Client;
                var data = repo.GetSingleCustomerClientOrSupplierInfo(customerId, clientTypeId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-supplier-info/")]
        public HttpResponseMessage GetSingleCustomerSupplierInfo(int customerId, int targetId)
        {
            try
            {
                var supplierTypeId = (short)CompanyClientOrSupplierTypeEnum.Supplier;
                var data = repo.GetSingleCustomerClientOrSupplierInfo(customerId, supplierTypeId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-customer-nextofkin-info/")]
        public HttpResponseMessage GetSingleCustomerNextOfKinInfo(int customerId, int targetId)
        {
            try
            {
                var data = repo.GetSingleCustomerNextOfKinInfo(customerId, targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #endregion

        #region Customer Product Fee ( aka Fee Concession)

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-product-fee/customer/{customerId}")]
        public HttpResponseMessage GetCustomerProductFeeByCustomerId(int customerId)
        {
            try
            {
                var data = proRepo.GetCustomerProductFeeByCustomerId(token.GetCompanyId, customerId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-product-fee/product/{productId}")]
        public HttpResponseMessage GetCustomerProductFeeByProductId(int productId)
        {
            try
            {
                var data = proRepo.GetCustomerProductFeeByProductId(token.GetCompanyId, productId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
        [HttpDelete]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        #endregion

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
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

                if (data == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer record has been approved successfully." });
                }
                else if (data == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer record has been disapproved successfully." });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #region  Customer Related Party
        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-related-party")]
        public HttpResponseMessage GetCustomerRelatedParty(int custormerId) 
        {
            try
            {
                var data = repo.GetCustomerRelatedParty(custormerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-related-party/{relatedPartyId}")]
        public HttpResponseMessage DeleteRelatedParty(int relatedPartyId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //  userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteRelatedParty(relatedPartyId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer party Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this party" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-address/{addressId}")]
        public HttpResponseMessage DeleteAddress(int addressId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //  userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.Deleteaddress(addressId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer address Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this address" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-contact/{phoneContactId}")]
        public HttpResponseMessage DeleteContact(int phoneContactId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //  userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.Deletcontact(phoneContactId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer contact Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this address" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-employment-history/{placeOfWorkId}")]
        public HttpResponseMessage DeleteEmployment(int placeOfWorkId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //  userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteEmployment(placeOfWorkId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer contact Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error delete this address" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-nextOfKin-history/{nextOfKinId}")]
        public HttpResponseMessage DeleteNextOfKin(int nextOfKinId)
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

                var data = repo.DeleteNextOfKin(nextOfKinId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Customer Next of Kin deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error deleting this Next of Kin" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-related-party")]
        public HttpResponseMessage AddUpdateCustomerRelatedParty([FromBody]CustomerRelatedPartyViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.relatedPartyId != 0 || entity.relatedPartyId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateRelatedPartyEntry(entity.customerId, entity.companyDirectorId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Selected Company Director already exist for this Customer." });
                    }
                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.staffId = token.GetStaffId;

                var data = repo.AddUpdateCustomerRelatedParty(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        #endregion

        #region Prospective Customer
        [HttpGet]
        [ClaimsAuthorization]
        [Route("prospect-customer")]
        public HttpResponseMessage GetAllProspectiveCustomer()
        {
            try
            {
                var custInfo = repo.GetAllProspectiveCustomer();

                if (custInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = custInfo });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPut]
        [ClaimsAuthorization]
        [Route("prospect-customer/")]
        public HttpResponseMessage UpdatePropectToCustomer(int customerId, CustomerViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.customerSensitivityLevelId = 1;
              
                var data =  repo.UpdatePropectToCustomer(customerId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been updated successfully and undergoing approval." });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = ce.Message });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }
        #endregion
        [HttpGet]
        [ClaimsAuthorization]
        [Route("director-related-customer")]
        public HttpResponseMessage DirectorRelatedCustomer(string bvn)
        {
            try
            {
                var custInfo = repo.DirectorRelatedCustomer(bvn);

                if (custInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = custInfo });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [Route("pre-multiple-fs-caption")]
        public async Task<HttpResponseMessage> UploadBulkFsCaptionData()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            Task.Factory
                .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning, // guarantees separate thread
                    TaskScheduler.Default)
                .Wait();


            var isFinal = Convert.ToBoolean(provider.FormData["isFinal"]);
            var customerId = Convert.ToInt32(provider.FormData["customerId"]);

            var entity = new UserInfo
            {
                BranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            var file = provider.Contents.FirstOrDefault();
            var buffer = await file.ReadAsByteArrayAsync();
            var data = repo.preBulkFsCaption(buffer, entity, isFinal, customerId);

            if (buffer != null)
            {
                bool success = true;
                if (data.Item2 == false && isFinal) { success = false; }
                if (!success) { return Request.CreateResponse(HttpStatusCode.OK, new { success = success, result = data.Item1, message = "Pre Bulk insurance failed to upload." }); }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = success, result = data.Item1, message = "Pre Bulk Insurance data was successfully uploaded" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error uploading Pre Bulk Insurance data" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("bulk-fs-caption-entries")]
        public HttpResponseMessage saveBulkInsurancePolicyEntries([FromBody] List<MultipleFsCaptionOutputViewModel> models)
        {
            UserInfo user = new UserInfo();
            user.BranchId = (short)token.GetBranchId;
            user.applicationUrl = HttpContext.Current.Request.Path;
            user.createdBy = token.GetStaffId;
            user.companyId = token.GetCompanyId;

            var response = repo.saveBulkFsCaptionEntries(models, user);

            return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, data = response, message = "Records saves successfully" });
        }






    }
}
//Models
//FinTrakBankingContext