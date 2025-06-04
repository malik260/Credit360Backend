using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Repositories.Finance;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/casa")]
    public class CasaController : ApiControllerBase
    {
        private ICasaRepository repo;
        private ICasaLienRepository lienRepo;

        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CasaController(ICasaRepository _repo, ICasaLienRepository _lienRepo)
        {
            this.repo = _repo;
            this.lienRepo = _lienRepo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("{accountId}")]
        public HttpResponseMessage GetAccount(int accountId)
        {
            try
            {
                CasaViewModel data = repo.GetAccount(accountId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-accounts/balance/{accountNumber}")]
        public HttpResponseMessage GetCASABalance(string accountNumber)
        {
            try
            {
                accountNumber = accountNumber.Replace("-", "");
                var data = repo.GetCASABalance(accountNumber, token.GetCompanyId);
                if (data != null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Account Number do not exist" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $" {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{be.Message}" });
            }
            catch (APIErrorException ae)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ae.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error: An unexpected exception  occured" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-accounts/{customerId}/loantype/{loantypeid}")]
        public HttpResponseMessage GetAllCustomerAccount(int customerId, int loanTypeId)
        {
            try
            {
                var data = repo.GetAllCustomerAccount(customerId, loanTypeId, token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-accounts/customer/{id}/currency/{currencyId}")]
        public HttpResponseMessage GetAllCustomerAccountByCustomerIdAndCurrency(int id, int currencyId)
        {
            try
            {
                var data = repo.GetAllCustomerAccountByCustomerIdAndCurrency(id, token.GetCompanyId, currencyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        //[HttpGet]
        [ClaimsAuthorization]
        [Route("customer-accounts/customer/{id}")]
        public HttpResponseMessage GetAllCustomerAccountByCustomerId(int id)
        {
            try
            {
                var data = repo.GetAllCustomerAccountByCustomerId(id, token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });

            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("business-accounts")]
        public HttpResponseMessage GetBusinessAccounts()
        {
            try
            {
                var data = repo.GetBusinessAccounts(token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }
        }

        [Route("account-owner-by-account-number/")]
        public HttpResponseMessage GetAccountOwnerByAccountNumber(string accountNumber)
        {
            try
            {
                var data = repo.GetAccountOwnerByAccountNumber(accountNumber, token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("all-customer-accounts/{customerId}")]
        public HttpResponseMessage GetAllCASAAccount(string casaAccountNumber)
        {
            try
            {
                var data = repo.GetAllCASAAccount(casaAccountNumber, token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetAccountByCustomerId(int customerId)
        {
            try
            {
                var data = repo.GetAccountByCustomerId(customerId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-casa-liens/accountNumber/{accountNumber}")]
        public HttpResponseMessage GetAllCasaLiens(string accountNumber) 
        {
            try
            {
                var data = repo.GetAllCasaLiens(accountNumber);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-casa-lien-types")]
        public HttpResponseMessage GetAllCasaLienTypes()
        {
            try
            {
                var data = repo.GetAllCasaLienTypes(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-casa-loans/casaAccountId/{casaAccountId}")]
        public HttpResponseMessage GetAllCasaLoans(int casaAccountId)
        {
            try
            {
                var data = repo.GetAllCasaLoans(token.GetCompanyId, casaAccountId);
                if (data.Count() < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "There's no Loan Attached to this Account Number" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-consumer-protection")]
        public HttpResponseMessage AddConsumerProtection([FromBody] ConsumerProtectionViewModel model)
        {
            try
            {
                model.branchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var result = lienRepo.AddConsumerProtection(model);

                if (result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = result, message = "Consumer Protection has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Consumer Protection could not be created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-consumer-protections")]
        public HttpResponseMessage GetAllConsumerProtections()
        {
            try
            {
                var data = lienRepo.GetAllConsumerProtections(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-document-section-consumer-protection/operation/{operationId}/target/{targetId}/section/{sectionId}")]
        public HttpResponseMessage GetDocumentSection(int operationId, int targetId, int sectionId)
        {
            try
            {
                LoadedDocumentSectionViewModel response = lienRepo.GetDocumentSectionConsumerProtection(token.GetStaffId, operationId, targetId, sectionId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-consumer-protection-memo/consumerProtectionById/{consumerProtectionById}/operationId/{operationId}")]
        public HttpResponseMessage GetLoadedDocumentationConsumerProtection(int consumerProtectionById, int operationId)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                List<LoadedDocumentSectionViewModel> response = lienRepo.GetLoadedDocumentationConsumerProtection(token.GetStaffId, operationId, consumerProtectionById, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-casa-lien")]
        public HttpResponseMessage AddCasaLien([FromBody] CasaLienViewModel model)
        {
            try
            {
                model.branchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                //var result = repo.AddCasaLien(model);
                var result = lienRepo.PlaceLien(model);

                if (result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = result, message = "Casa Lien has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Casa Lien could not be created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("remove-casa-lien")]
        public HttpResponseMessage RemoveCasaLien([FromBody] CasaLienViewModel model)
        {
            try
            {
                model.branchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var result = lienRepo.ReleaseLien(model);

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = result, message = "Casa Lien has been removed successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Casa Lien could not be removed" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("find-customer-casa-lien/accountNumberOrName/{accountNumberOrName}")]
        public HttpResponseMessage FindCustomerCasaLien(string accountNumberOrName)
        {
            try
            {
                var data = repo.FindCustomerCasaLien(accountNumberOrName, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("account-number-name/")]
        public HttpResponseMessage FindAccount(string accountNumberOrName)
        {
            try
            {
                var data = repo.FindAccount(accountNumberOrName, token.GetCompanyId);// token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        

       [HttpGet]
        [ClaimsAuthorization]
        [Route("group-account-number/")]
        public HttpResponseMessage GetGroupAccountNumberWithCustomerId(string accountNumberOrName,int customerId)
        {
            try
            {
                var data = repo.GetGroupAccountNumberWithCustomerId(accountNumberOrName, customerId, token.GetCompanyId);// token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("overdraft-account-number/")]
        public HttpResponseMessage GetOverdraftAccountNumberWithCustomerId(string accountNumberOrName, int customerId)
        {
            var data = repo.GetOverdraftAccountNumberWithCustomerId(accountNumberOrName, customerId, token.GetCompanyId);// token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-account/")]
        public HttpResponseMessage SearchForCustomerAccount(string searchQuery, int loanTypeId)
        {
            try
            {
                var data = repo.SearchForCustomerAccount(token.GetCompanyId, searchQuery, loanTypeId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer/search")]
        public HttpResponseMessage SearchCustomer(string q, string t)
        {
            try
            {
                var data = repo.SearchCustomer(int.Parse(t), token.GetCompanyId, q).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer/{customerId}/account-details")]
        public HttpResponseMessage SearchCustomerAccountDetails(int customerId)
        {
            try
            {
                var data = repo.GetCustomerAccountDetailsById(customerId);

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


        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-account-pull/")]
        public HttpResponseMessage AddCustomerAccounts(CasaViewModel model)
        {
            try
            {
                repo.AddCustomerAccounts(model?.customerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}