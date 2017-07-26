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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/casa")]
    public class CasaController : ApiControllerBase
    {
        private ICasaRepository repo;

        public CasaController(ICasaRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("{accountId}")]
        public HttpResponseMessage GetAccount(HttpRequestMessage request, int accountId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    CasaViewModel data = repo.GetAccount(accountId);
                    response = request.CreateResponse(HttpStatusCode.OK, data);
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetAccountByCustomerId(HttpRequestMessage request, int customerId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAccountByCustomerId(customerId);
                    if (data != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, data);
                    }
                    else
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, message = "No record found" }));
                    }
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("account-number-name/{accountNumberOrName}")]
        public HttpResponseMessage FindAccount(HttpRequestMessage request, string accountNumberOrName)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    //var token = new TokenDecryptionHelper(this.HttpContext);                

                    var data = repo.FindAccount(accountNumberOrName, 1);// token.GetCompanyId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                           Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("customer/search")]
        public HttpResponseMessage SearchCustomer(HttpRequestMessage request, string q, string t)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    // var token = new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.SearchCustomer(int.Parse(t), 1, q).ToList();// token.GetCompanyId,q);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });

        }
    }
} 