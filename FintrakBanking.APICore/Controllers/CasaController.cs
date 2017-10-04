using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General; 
using System.Linq;
using FintrakBanking.ViewModels.CASA;
using System;
using System.Collections.Generic;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;


namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/casa")]
    public class CasaController : ApiControllerBase
    {
        private ICasaRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CasaController(ICasaRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("{accountId}")]
        public HttpResponseMessage GetAccount(  int accountId)
        { 
                try
                {
                    CasaViewModel data = repo.GetAccount(accountId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                }
             
        }

       
        [Route("customer-accounts/{customerId}")]
        public HttpResponseMessage GetAllCustomerAccountByCustomerId(int customerId)
        {
            try
            {
                var data = repo.GetAllCustomerAccountByCustomerId(customerId, token.GetCompanyId);
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }



        [HttpGet]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetAccountByCustomerId(  int customerId)
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
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet]
        [Route("account-number-name/{accountNumberOrName}")]
        public HttpResponseMessage FindAccount(  string accountNumberOrName)
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
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet]
        [Route("customer-account/")]
        public HttpResponseMessage SearchForCustomerAccount(string searchQuery)
        {
            try
            {
                var data = repo.SearchForCustomerAccount(token.GetCompanyId, searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("customer/search")]
        public HttpResponseMessage SearchCustomer(  string q, string t)
        { 
                try
                {

                    var data = repo.SearchCustomer(int.Parse(t), token.GetCompanyId, q).ToList();
                    if (data == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data.ToList() });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                } 

        }

        [Route("customer-accounts/{customerId}")]
        public HttpResponseMessage GetAllCustomerAccountByCustomerId(int customerId)
        {
            try
            {
                var data = repo.GetAllCustomerAccountByCustomerId(customerId, token.GetCompanyId);
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }
    }
} 