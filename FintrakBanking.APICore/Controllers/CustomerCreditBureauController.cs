using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class CustomerCreditBureauController : ApiControllerBase
    {
        private IIntegrationWithFinacle flexcube;
        private ICustomerCreditBureauRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        //private ICreditBureauProcess creditBureau;
        //private IErrorLogRepository errorLogger;

        public CustomerCreditBureauController(ICustomerCreditBureauRepository _repo, IIntegrationWithFinacle _flexcube) // ICreditBureauProcess _creditBureau) //IErrorLogRepository _errorLogger
        {
            this.repo = _repo;
            flexcube = _flexcube;
            //creditBureau = _creditBureau;
            // errorLogger = _errorLogger;
        }

        #region CREDIT BUREAU REPORT
      [HttpGet] [ClaimsAuthorization]  
        [Route("credit-bureau-customer-details/{customerId}")]
        public HttpResponseMessage GetCreditBureauCustomerDetailsByCustomerId(int customerId)
        {
            try
            {
                var test = repo;
                var data = repo.GetCreditBureauCustomerDetailsByCustomerId(customerId, true);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("credit-bureau-customer-account")]
        public HttpResponseMessage AddCustomerAccounts(IEnumerable<CustomerViewModels> models)
        {
            try
            {
                repo.AddCustomerAccounts(models);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true});
            }
            catch(SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("thirdparty-service-charge")]
        public HttpResponseMessage GetLoanThirdPartyServiceChargeStatusDetails()
        {
            try
            {
                var test = repo;
                var data = repo.GetLoanThirdPartyServiceChargeStatusDetails(token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-credit-bureau-document/{customerCreditBureauId}")]
        public HttpResponseMessage GetCreditBureauDocument(int customerCreditBureauId)
        {
            try
            {
                var data = repo.GetCreditBureauDocument(customerCreditBureauId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("crc-products")]
        public HttpResponseMessage GetCRCBureauFacilities()
        {
            try
            {
                var test = repo;
                var data = repo.GetCRCBureauFacilities();

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("crms-credit-check")]
        public HttpResponseMessage GetCreditCheck([FromBody] CreditCheckViewModel model)
        {
            try
            {
                var data = flexcube.GetCreditCheck(model);

                if (!data.posted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, responseMessage = data.responseMessage });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.responseMessage, credits = data.responseObject.Credits.ToList(), summary = data.responseObject.Summary });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan/customer/credit-bureau-charge")]
        public HttpResponseMessage AddCustomerCreditBureauCharge(LoanCreditBureauViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.staffId = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;

                var result =  repo.AddCustomerCreditBureauCharge(model);

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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("credit-bureau-customer-report-status/{status}")]
        public HttpResponseMessage UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBureauViewModel entity)
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

       [HttpPut] [ClaimsAuthorization]
        [Route("multiple-credit-bureau-customer-report-status/{status}")]
        public HttpResponseMessage UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBureauViewModel> model)
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

         [HttpPost] [ClaimsAuthorization]
        [Route("customer-credit-bureau-report-file-upload")]
        public async Task<HttpResponseMessage> AddCreditBureauReportDocument()
        {
            try
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

                //int uploadType;
                //if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                //}

                var entity = new LoanDocumentViewModel();
                //entity.customerCreditBureauId = Convert.ToInt32(provider.FormData["customerCreditBureauId"]);
                entity.documentTitle = provider.FormData["documentTitle"];
                //entity.documentTypeId = (short)uploadType;
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];

                var loanCreditBureauModel = new LoanCreditBureauViewModel();

                var companyDirectorId = 0;
                if(provider.FormData["companyDirectorId"] != null && provider.FormData["companyDirectorId"] != "null") companyDirectorId = Convert.ToInt32(provider.FormData["companyDirectorId"]);
                loanCreditBureauModel.creditBureauId = (short) Convert.ToInt32(provider.FormData["creditBureauId"]);  
                loanCreditBureauModel.customerId = Convert.ToInt32(provider.FormData["customerId"]); 
                loanCreditBureauModel.chargeAmount = Convert.ToDecimal(provider.FormData["chargeAmount"]);
                loanCreditBureauModel.isComplete = Convert.ToBoolean(provider.FormData["isComplete"]);
                loanCreditBureauModel.isReportOkay = Convert.ToBoolean(provider.FormData["isReportOkay"]);
                loanCreditBureauModel.usedIntegration = Convert.ToBoolean(provider.FormData["usedIntegration"]);
                loanCreditBureauModel.companyDirectorId = companyDirectorId;
                
                
                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                loanCreditBureauModel.userBranchId = (short)token.GetBranchId;
                loanCreditBureauModel.companyId = token.GetCompanyId;
                loanCreditBureauModel.createdBy = token.GetStaffId;
                loanCreditBureauModel.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var result = repo.AddCustomerCreditBureauUpload(loanCreditBureauModel, entity, buffer);

                if (result > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                           new { success = true, data = result, message = loanCreditBureauModel.creditBureauName + " Credit Bureau Report Successfully Saved" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = loanCreditBureauModel.creditBureauName + " report upload failed" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record. " +ex.Message });
            }
        }

        #endregion

        #region Integration

         [HttpPost] [ClaimsAuthorization]
        [Route("download-credit-bureau-search-result-in-pdf")]
        public HttpResponseMessage GetFullSearchResultInPDF([FromBody] SearchInput searchInput)
        {
            try
            {
                searchInput.applicationUrl = HttpContext.Current.Request.Path;
                searchInput.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                searchInput.createdBy = token.GetStaffId;
                searchInput.companyId = token.GetCompanyId;
                searchInput.staffId = token.GetStaffId;
                searchInput.userBranchId = (short)token.GetBranchId;

                var result = repo.GetXDSFullSearchResultInPDF(searchInput);

                if (!result.errorOccured)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,  new { success = true, data = result, message = " download Completed" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = " download failed" });
                }
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: An unhandles error occured"+ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("download-crc-credit-bureau-search-result-in-pdf")]
        public HttpResponseMessage GetCRCFullCreditMergeReport([FromBody] MultiHitRequestViewModel searchInput)
        {
            try
            {
                searchInput.applicationUrl = HttpContext.Current.Request.Path;
                searchInput.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                searchInput.createdBy = token.GetStaffId;
                searchInput.companyId = token.GetCompanyId;
                searchInput.staffId = token.GetStaffId;
                searchInput.userBranchId = (short)token.GetBranchId;

                var result = repo.GetCRCFullCreditMergeReport(searchInput);

                if (result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, data = result, message = " download Completed" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = " download failed" });
                }
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (TimeoutException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: An unhandles error occured "+ex.Message });
            }
        }


        [HttpPost] [ClaimsAuthorization]
        [Route("xds-credit-bureau-search")]
        public HttpResponseMessage GetCustomerXDSCreditMatch([FromBody] CreditBureauSearchViewModel searchInfoList)
        {
            try
            {
                searchInfoList.applicationUrl = HttpContext.Current.Request.Path;
                searchInfoList.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                searchInfoList.createdBy = token.GetStaffId;
                searchInfoList.companyId = token.GetCompanyId;
                searchInfoList.staffId = token.GetStaffId;
                searchInfoList.userBranchId = (short)token.GetBranchId;

                var result = repo.GetCustomerXDSCreditMatch(searchInfoList);

                if (result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, data = result, message = " Search Completed" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = " Search failed" });
                }
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $" {ex.Message}" });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (TimeoutException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: An unhandled error occured." });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("crc-credit-bureau-search")]
        public HttpResponseMessage GetCustomerCRCCreditMatch([FromBody] CRCRequestViewModel searchInfo)
        {
            try
            {
                searchInfo.applicationUrl = HttpContext.Current.Request.Path;
                searchInfo.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                searchInfo.createdBy = token.GetStaffId;
                searchInfo.companyId = token.GetCompanyId;
                searchInfo.staffId = token.GetStaffId;
                searchInfo.userBranchId = (short)token.GetBranchId;

                var result = repo.GetCustomerCRCCreditMatch(searchInfo);

                if (result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, data = result, message = " Search Completed" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = " Search failed" });
                }
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $" {ex.Message}" });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (TimeoutException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

       
        #endregion

    }
}