using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Repositories.Finance;
using FintrakBanking.ViewModels.CASA;
using System;
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

        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CasaController(ICasaRepository _repo)
        {
            this.repo = _repo;
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
    }
}