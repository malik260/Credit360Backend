using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
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
    [RoutePrefix("api/v1/credit")]
    public class CustomerCreditBureauController : ApiControllerBase
    {
        private ICustomerCreditBureauRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        //private ICreditBureauProcess creditBureau;
        //private IErrorLogRepository errorLogger;

        public CustomerCreditBureauController(ICustomerCreditBureauRepository _repo) // ICreditBureauProcess _creditBureau) //IErrorLogRepository _errorLogger
        {
            this.repo = _repo;
            //creditBureau = _creditBureau;
           // errorLogger = _errorLogger;
        }

        #region CREDIT BUREAU REPORT
        [HttpGet]
        [Route("credit-bureau-customer-details/{customerId}")]
        public HttpResponseMessage GetCreditBureauCustomerDetailsByCustomerId(int customerId)
        {
            try
            {
                var test = repo;
                var data = repo.GetCreditBureauCustomerDetailsByCustomerId(customerId);
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
        [Route("credit-bureau-information")]
        public HttpResponseMessage GetCreditBureauInformation()
        {
            try
            {
                var test = repo;
                var data = repo.GetCreditBureauInformation();

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("credit-bureau-report-log/{customerId}/director/{companyDirectorId}")]
        public HttpResponseMessage GetCustomerCreditBureauReportLog(int customerId, int? companyDirectorId)
        {
            try
            {
                var data = repo.GetCustomerCreditBureauReportLog(customerId, companyDirectorId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("loan/customer/credit-bureau-charge")]
        public HttpResponseMessage AddCustomerCreditBureauCharge(LoanCreditBereauViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.staffId = token.GetStaffId;

                var result = repo.AddCustomerCreditBureauCharge(model);

                if (result > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, date = result, message = model.creditBureauName + " Credit Bureau Report Successfully Saved" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = model.creditBureauName + " report upload failed" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("credit-bureau-customer-report-status/{status}")]
        public HttpResponseMessage UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBereauViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.UpdateCreditBureauCustomerReportStatus(status, entity);
                if (data)
                {
                    if (status) return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, result = data, message = entity.creditBureauName + " report is positive" });
                    else return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = entity.creditBureauName + " report is negative" });
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

        [HttpPut]
        [Route("multiple-credit-bureau-customer-report-status/{status}")]
        public HttpResponseMessage UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBereauViewModel> model)
        {
            try
            {
                foreach (var entity in model)
                {
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = (short)token.GetCompanyId;
                    //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                }


                var data = repo.UpdateMultipleCreditBureauCustomerReportStatus(status, model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "All Result Validate Okay" });
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
        #endregion

        #region Integration
        //[HttpPost]
        //[Route("credit-bureau-search")]
        //public HttpResponseMessage GetCustomerCreditMatch(List<CreditBureauSearchViewModel> searchInfoList)
        //{
        //    try
        //    {
        //        foreach (var model in searchInfoList)
        //        {
        //            model.applicationUrl = HttpContext.Current.Request.Path;
        //            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //            model.createdBy = token.GetStaffId;
        //            model.companyId = token.GetCompanyId;
        //            model.staffId = token.GetStaffId;
        //        }

        //        var result = repo.GetCustomerCreditMatch(searchInfoList);

        //        if (result != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                                    new { success = true, date = result, message = " Search Completed" });
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, message = " Search failed" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}
        #endregion

    }
}