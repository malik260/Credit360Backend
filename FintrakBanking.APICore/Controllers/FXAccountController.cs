using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class FXAccountController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IFXAccountCreationRepository repo;

        public FXAccountController(IFXAccountCreationRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-fx-code")]
        public HttpResponseMessage GetListOfFSCode()
        {
            try
            {
                var data = repo.GetListOfFSCode();

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
        [Route("get-gl-subhead-code")]
        public HttpResponseMessage GetAllGLSubHead(string schemeCode)
        {
            try
            {
                var data = repo.GetAllGLSubHead(schemeCode);

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
        [Route("foreign-currency-account-creation")]
        public HttpResponseMessage ForeignCurrencyAccountCreation([FromBody]CreateAccountViewModel entity)
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
                //entity.functionCode = "A";
                //entity.solId = "230";
                //entity.currencyCode = "USD";
                //    entity.customerCode = "230046318";
                //entity.schemeCode = "CA208";
                // entity.generalLedgerSubHeadCode = "30000";
                //entity.channel = "FINTRAK";
                //entity.sectorCode = "40900";
                //entity.subSectorCode = "40110";
                //entity.accountOccupationCode = "014";
                //entity.borrowerCategoryCode = "999";
                //entity.purposeOfAdavance = "999";
                //entity.natureOfAdavance = "001";
                //entity.modeOfAdavance = "999";
                //entity.typeOfAdavance = "002";
                //entity.freeCodeOne = "CT";
                //entity.freeCodeFour = "CPCF4";
                //entity.freeCodeFive = "0.05";
                //entity.freeCodeSix = "CPCF6";
                //entity.freeCodeSeven = "CPCF7";
                //entity.freeCodeEight = "CPCF8";
                //entity.freeCodeNine = "CPCF9";
                //entity.freeCodeTen = "CPC10";

                entity.solId = token.GetBranchId.ToString();
                var data = repo.ForeignCurrencyAccountCreation(entity, user);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The account has been created successfully. Account Number is {data}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this account" });
            }
            catch (TwoFactorAuthenticationException et)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = $" {et.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $" {e.Message}" });
            }
        }
    }
}
