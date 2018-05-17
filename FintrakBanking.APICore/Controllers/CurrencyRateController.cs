using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/admin")]
    public class CurrencyRateController : ApiControllerBase
    {
        private ICurrencyRateRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CurrencyRateController(ICurrencyRateRepository _repo)
        {
            this.repo = _repo;
        }



      [HttpGet] [ClaimsAuthorization]  [Route("currency")]
        public HttpResponseMessage GetCurrency()
        {
            try
            { 
                var data = repo.GetCurrency();
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("currency-ratecode")]
        public HttpResponseMessage GetCurrencyRaceCode()
        {
            try
            {
                var data = repo.GetAllCurrencyRateCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("base-currency")]
        public HttpResponseMessage GetBaseCurrency()
        {
            try
            {
                var data = repo.GetBaseCurrency(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


      [HttpGet] [ClaimsAuthorization]  [Route("currency-rate")]
        public HttpResponseMessage GetCurrencyRate()
        {
            try
            {
                var data = repo.GetCurrencyRate();
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("currency-exchange-rate/{currencyId}")]
        public HttpResponseMessage GetCurrentCurrencyExchangeRate(short currencyId)
        {
            try
            {
                var data = repo.GetCurrentCurrencyExchangeRate(currencyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("currency-rate/{currencyId}")]
        public HttpResponseMessage GetCurrencyRateById(short currencyId)
        {
            try
            {
                var data = repo.GetCurrencyRateById(currencyId);
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost][Route("currency-rate")]
        public HttpResponseMessage AddFSRatioCaption( [FromBody] CurrencyRateViewModel model)
        {
            try
            {
                    model.createdBy = token.GetStaffId;
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                var data = repo.AddCurrencyRate(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpPut][Route("currency-rate/{currencyId}")]
        public HttpResponseMessage UpdateFSRatioCaption(short currencyId, [FromBody] CurrencyRateViewModel model)
        {
            try
            {
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;

                var data = repo.UpdateCurrencyRate(currencyId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data, message = "The record has been updated successfully" });

                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
    }
}