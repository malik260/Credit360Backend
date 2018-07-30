using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/finance")]
    public class FinanceTransactionController : ApiControllerBase
    {
        private IFinanceTransactionRepository repo;
        private ILoanOperationsRepository repoLoan;
        private IGeneralSetupRepository setupRepo;      

        TokenDecryptionHelper token = new TokenDecryptionHelper();

         

        public FinanceTransactionController(
            IFinanceTransactionRepository _repo,
            ILoanOperationsRepository _repoLoan, IGeneralSetupRepository _setupRepo
            )
        {
            this.repo = _repo;
            this.repoLoan = _repoLoan;
            this.setupRepo = _setupRepo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getexchangerate/{currencyId}/{date}")]
        public HttpResponseMessage GetExchangeRate(DateTime ? date, short currencyId)
        { 
                try
                {
                   DateTime inputDate;

                if (date.HasValue)
                    inputDate = date.Value;
                else
                    inputDate = setupRepo.GetApplicationDate();

                    var data = repo.GetExchangeRate(inputDate, currencyId, token.GetCompanyId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                }
             
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("Posttransaction")]
        public HttpResponseMessage PostTransaction( [FromBody] List<FinanceTransactionViewModel> transaction)
        {
            try
            {
                var data = repo.PostTransaction(transaction);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Transaction Posted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("addcollateralsearchlien")]
        public HttpResponseMessage AddCollateralSearchLien([FromBody] CasaLienViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;
                model.userBranchId = (short)token.GetBranchId;


                var response = repoLoan.AddCollateralSearchLien(model);
                if (response == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


        // [HttpPost] [ClaimsAuthorization]
        //[Route("Posttransaction")]
        //public HttpResponseMessage PostCollateralSearch( [FromBody] CasaLienViewModel model)
        //{
        //    try
        //    {
        //        var data = repo.PostCollateralSearch(model);
        //        if (data != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Transaction Posted successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
        //    }
        //}


    }
} 