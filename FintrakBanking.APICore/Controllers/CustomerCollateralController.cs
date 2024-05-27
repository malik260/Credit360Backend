using System;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using FintrakBanking.APICore.core;
using System.Net;
using FintrakBanking.Common.CustomException;
using System.Linq;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Interfaces.Credit;
using System.Threading;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Reports;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/credit")]
    public class CustomerCollateralController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        private ICustomerCollateralRepository repo;
        private ICollateralDocumentRepository document;
        private ICollateralTypeRepository type;
        private ICasaRepository casa;


        // private IGuaranteeCollateralRepository guaratee;

        public CustomerCollateralController(
            ICustomerCollateralRepository repo,
            ICollateralTypeRepository type,
            ICollateralDocumentRepository document,
            ICasaRepository _casa
            // IGuaranteeCollateralRepository guaratee
            )
        {
            this.repo = repo;
            this.type = type;
            this.document = document;
            this.casa = _casa;
            //  this.guaratee = guaratee;
        }

        #region
        [HttpGet, Route("collateral-document-release/{collateralId}")]
        public HttpResponseMessage GetCollateralReleaseDocumentByCollateral(int collateralId)
        {
            try
            {
                var data = document.GetCustomerCollateralReleaseDocument(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-collateral/supporting-document-upload")]
        public async Task<HttpResponseMessage> AddCollateralSupportingDocument() // DEPRECATED
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

                int uploadType;
                //if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                //}

                var entity = new CollateralViewModel
                {
                    collateralCode = provider.FormData["collateralCode"],
                    collateralReleaseId = Convert.ToInt32(provider.FormData["collateralReleaseId"]),
                    collateralCustomerId = Convert.ToInt32(provider.FormData["collateralCustomerId"]),
                    //SourceId = Convert.ToInt32( provider.FormData["sourceId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddReleaseDocument(entity, buffer);

                if (data == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-collateral/releaseId/{releaseId}")]
        public HttpResponseMessage GetCollateralReleaseDocumentByReleaseId(int releaseId)
        {
            try
            {
                var data = repo.GetCollateralReleaseDocument(releaseId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-collateral/{documentId}")]
        public HttpResponseMessage GetReleaseSupportingDocumentDocument(int documentId)
        {
            try
            {
                var data = repo.GetReleaseSupportingDocument(documentId);

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

        [HttpPost, Route("customer-collateral/release-collateral")]
        [ClaimsAuthorization]
        public HttpResponseMessage ReleaseCollateral([FromBody] CollateralViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = repo.ReleaseCollateral(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("customer-collateral/release-collateral-awaiting-approval")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCollateralReleaseAwaitingApproval()
        {
            try
            {
                var response = repo.GetCollateralReleaseAwaitingApproval(token.GetCompanyId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }
        [HttpPost, Route("customer-collateral/complete-job-request-release-collateral")]
        [ClaimsAuthorization]
        public HttpResponseMessage ReleaseCollateralJobRequest([FromBody] CollateralViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = repo.ReleaseCollateralJobRequest(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Release Sent For Approval successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }


        [HttpPost, Route("customer-collateral/go-for-approval-release-collateral")]
        [ClaimsAuthorization]
        public HttpResponseMessage ReleaseCollateralGoForApproval([FromBody] ApprovalViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.BranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = repo.ReleaseCollateralGoForApproval(entity);
                if (response.status == (int)ApprovalStatusEnum.Approved)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Release Approved successfully" });
                }
                if (response.status == (int)ApprovalStatusEnum.Processing)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Release Has Been Sent To The Next Approver (" + response.approvalLevel + ")" });
                }
                if (response.status == (int)ApprovalStatusEnum.Disapproved)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Release Disapproved successfull" });
                }
                if (response.status == (int)ApprovalStatusEnum.Referred)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Release Has Been Reffered Back successfull (" + response.approvalLevel + ")" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }



        [HttpGet, Route("customer-collateral/release-collateral-awaiting-job-request")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCollateralReleaseAwaitingJobRequest()
        {
            try
            {
                var response = repo.GetCollateralReleaseAwaitingJobRequest(token.GetCompanyId, token.GetBranchId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/release-collateral-awaiting-job-request/collateralId/{collateralId}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCollateralReleaseAwaitingJobRequest(int collateralId)
        {
            try
            {
                var response = repo.GetCollateralReleaseAwaitingJobRequest(token.GetCompanyId, token.GetBranchId).Where(a => a.collateralCustomerId == collateralId); ;
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/get-collateral-information/colateralcustomerId/{colateralcustomerId}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCollateralInformation(int colateralcustomerId)
        {
            try
            {
                var response = repo.GetCustomerCollateralInformation(colateralcustomerId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-property-collateral/customerId/{customerId}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCustomerPropertyCollaterals(int? customerId)
        {
            try
            {
                var response = repo.GetCustomerPropertyCollaterals(customerId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        //[HttpPost, Route("customer-collateral")]
        //public async Task<HttpResponseMessage> AddCollateral()
        //{
        //    CollateralViewModel incomingData = new CollateralViewModel();

        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
        //    }

        //    MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
        //    await Request.Content.ReadAsMultipartAsync(provider);

        //    var formData = provider.FormData["formData"];

        //    var errors = new List<string>();
        //    incomingData = JsonConvert.DeserializeObject<CollateralViewModel>(formData,
        //         new JsonSerializerSettings
        //         {
        //             NullValueHandling = NullValueHandling.Include,
        //             Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg)
        //             {
        //                 errors.Add(earg.ErrorContext.Member.ToString());
        //                 earg.ErrorContext.Handled = true;
        //             }
        //         });


        //    if (!provider.FileStreams.Any())
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
        //    }

        //    incomingData.userBranchId = (short)token.GetBranchId;
        //    incomingData.companyId = token.GetCompanyId;
        //    incomingData.createdBy = token.GetStaffId;
        //    incomingData.applicationUrl = HttpContext.Current.Request.Path;

        //    var file = provider.Contents.FirstOrDefault();
        //    var buffer = await file.ReadAsByteArrayAsync();
        //    var data = repo.AddCollateral(incomingData, buffer);

        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });


        //}

        [HttpPost, Route("customer-join-collateral")]
        [ClaimsAuthorization]
        public async Task<HttpResponseMessage> AddJoinCollateralInformation()
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

                var formData = provider.FormData["formData"];

                var errors = new List<string>();
                CollateralViewModel incomingData = JsonConvert.DeserializeObject<CollateralViewModel>(formData,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg)
                        {
                            errors.Add(earg.ErrorContext.Member.ToString());
                            earg.ErrorContext.Handled = true;
                        }
                    });


                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                incomingData.userBranchId = (short)token.GetBranchId;
                incomingData.companyId = token.GetCompanyId;
                incomingData.createdBy = token.GetStaffId;
                incomingData.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddGuaranteeJoinCollateral(incomingData, buffer);

                if (data.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "THERE WAS AN ERROR CREATING THIS RECORD, REFERENCE NUMBER EXIST" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpPut, Route("customer-collateral/{collateralId}")]
        [ClaimsAuthorization]
        public HttpResponseMessage UpdateCollateral([FromBody] CollateralViewModel entity, int collateralId)
        {
            //try
            //{
            entity.lastUpdatedBy = token.GetStaffId;
            entity.createdBy = token.GetStaffId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.companyId = token.GetCompanyId;

            var response = repo.UpdateCollateral(entity, collateralId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Updated successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            //}
            //catch (SecureException ex)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            //}
        }

        [HttpGet, Route("customer-collateral/customer/{id}/application/{applicationId}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCustomerCollateral(int id, int? applicationId)
        {
            try
            {
                var response = repo.GetCustomerCollateral(id, applicationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-facility/customer/{id}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCustomerFacility(int id)
        {
            try
            {
                var response = repo.GetCustomerFacility(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-cash-collateral/customer/{id}/application/{applicationId}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCustomerCashCollateral(int id, int? applicationId)
        {
            try
            {
                var response = repo.GetCustomerCashCollateral(id, applicationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-cash-collateral-applications/collateralCustomerId/{id}")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetCustomerCashCollateralApplications(int id)
        {
            try
            {
                var response = repo.GetCustomerCashCollateralApplications(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }


        [HttpGet, Route("collateral/application/{applicationId}/currencyId/{currencyId}")]
        public HttpResponseMessage GetProposedCustomerCollateral(int? applicationId, int currencyId)
        {
            try
            {
                var response = repo.GetProposedCustomerCollateral(applicationId, currencyId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("collateral/application/{customerId}/{getAll}")]
        public HttpResponseMessage GetProposedCustomerCollateralByCustomerId(int customerId, bool getAll)
        {
            try
            {
                var response = repo.GetProposedCustomerCollateralByCustomerId(customerId, getAll);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, ClaimsAuthorization, Route("collateral/failities/{collateralId}")]
        public HttpResponseMessage GetProposedFacilitiesToCollateralByCollateralId(int collateralId)
        {
            try
            {
                var response = repo.GetProposedFacilitiesToCollateralByCollateralId(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }


        [HttpGet, Route("collateral-lms/application/{customerId}/{getAll}")]
        public HttpResponseMessage GetProposedCustomerCollateralByCustomerIdLMS(int customerId, bool getAll)
        {
            try
            {
                var response = repo.GetProposedCustomerCollateralByCustomerIdLMS(customerId, getAll);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("collateral/{loanApplicationDetailId}")]
        public HttpResponseMessage GetProposedCustomerCollateralByLoanApplicationDetailId(int loanApplicationDetailId)
        {
            try
            {
                var response = repo.GetProposedCustomerCollateralByLoanApplicationDetailId(loanApplicationDetailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/searchParam/{searchParam}")]
        public HttpResponseMessage GetCustomerCollateralReport(string searchParam)
        {
            try
            {
                var response = repo.GetCustomerCollateralReport(searchParam, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("get-customer-fixed-deposit-collateral/searchParam/{searchParam}")]
        public HttpResponseMessage GetCustomerFixedDepositCollateral(string searchParam)
        {
            try
            {
                var response = repo.GetCustomerFixedDepositCollateral(searchParam, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("customer-collateral/customer")]
        public HttpResponseMessage GetCustomerCollateralRepo([FromBody] NewCollateralViewModel data)
        {
            try
            {
                var response = repo.GetCustomerCollateral(data.customerId, data.applicationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }




        [HttpGet, Route("temp-customer-collateral")]
        public HttpResponseMessage GetTempCustomerCollateral()
        {
            try
            {
                var response = repo.GetTempCustomerCollateralForApproval(token.GetCompanyId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("customer-collateral-by-collateralId")]
        public HttpResponseMessage GetCustomerCollateral([FromBody] int collateralId)
        {
            try
            {
                var response = repo.GetCustomerCollateralByCollateralId(token.GetCompanyId, collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-collateral-insurance-tracking")]
        public HttpResponseMessage saveCustomerCollateralInsuranceTracking([FromBody] CollateralInsuranceTrackingViewModel data)
        {
            try
            {
                bool response = repo.AddCollateralInsuranceTrackingForm(token.GetStaffId, data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("customer-collateral-insurance-tracking-update/{id}")]
        public HttpResponseMessage saveCustomerCollateralInsuranceTrackingUpdate(int id, [FromBody] CollateralInsuranceTrackingViewModel model)
        {
            try
            {
                bool response = repo.UpdateCollateralInsuranceTrackingForm(token.GetStaffId, id, model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral-insurance-details-confirmation/{id}")]
        public HttpResponseMessage getCustomerCollateralInsuranceDetailsConfirmation(int id)
        {
            try
            {
                bool response = repo.GetCustomerCollateralInsuranceDetailsConfirmation(token.GetStaffId, id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
        }

        [HttpGet, Route("delete-customer-collateral-insurance-details/{id}")]
        public HttpResponseMessage deleteCustomerCollateralInsuranceDetails(int id)
        {
            try
            {
                bool response = repo.DeleteCustomerCollateralInsuranceDetails(token.GetStaffId, id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
        }




        [HttpGet, Route("temp-item-policy")]
        public HttpResponseMessage GetItemPolicyCollateral()
        {
            try
            {
                var response = repo.GetTempCollateralInsurancePoliciesWaitingForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("insurance-policy-approval")]
        public HttpResponseMessage GetCollateralInsurancePoliciesWaitingForApproval()
        {
            try
            {
                var response = repo.GetCollateralInsurancePoliciesWaitingForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("temp-item-policy/{collateralId}")]
        public HttpResponseMessage GetItemPolicyCollateralList(int collateralId)
        {
            try
            {
                var response = repo.GetTempCollateralInsurancePolicy(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("item-policy/{collateralId}")]
        public HttpResponseMessage GetPolicyCollateralList(int collateralId)
        {
            try
            {
                var response = repo.GetCollateralInsurancePolicy(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("collateral-insurance-policy-list")]
        public HttpResponseMessage GetCollateralInsurancePolicyList([FromBody] InsurancePolicy model)
        {
            try
            {
                var response = repo.GetCollateralInsurancePolicyReport(model.startDate, model.expiryDate, model.valueCode, model?.businessUnitId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("insurance-policy-report/{trackingId}")]
        public HttpResponseMessage GetInsurancePolicyCollateralReport(int trackingId)
        {
            try
            {
                var response = repo.GetInsurancePolicyCollateralReport(trackingId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("temp/customer-collateral-approval")]
        public HttpResponseMessage PostCustomerCollateralApproval([FromBody] ApprovalViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = token.GetBranchId;

                var response = repo.GoForApproval(model);

                if (response == (int)ApprovalStatusEnum.Disapproved)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Disapproved Successfully" });
                }
                else if (response == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Approval has failed" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Approved Successfully" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("temp/policy-approval")]
        public HttpResponseMessage PostItemPolicyApproval([FromBody] ApprovalViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = token.GetBranchId;

                var response = repo.GoForPolicyApproval(model);
                if (response == (int)ApprovalStatusEnum.Disapproved)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Disapproved Successfully" });
                }
                else if (response == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Approval has failed" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Approved Successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("insurance-policy-approval")]
        public HttpResponseMessage GoForInsurancePolicyApproval([FromBody] ApprovalViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = token.GetBranchId;

                WorkflowResponse response = repo.GoForInsurancePolicyApproval(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/customer/{customerId}/collateral-type/{collateralTypeId}/thirdparty/{thirdpartyCustomerId}")]
        public HttpResponseMessage GetCollateralByCollateralTypeIdByCustomerId(int customerId, short collateralTypeId, short thirdpartyCustomerId = 0)
        {
            try
            {
                var response = repo.GetCollateralByCollateralTypeIdByCustomerId(token.GetCompanyId, collateralTypeId, customerId, thirdpartyCustomerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        //[HttpGet, Route("customer-collateral/type/collateral/{collateralId}/type/{typeId}")]
        //public HttpResponseMessage GetCollateralTypeByCollateralId(int collateralId, int typeId)
        //{
        //    try
        //    {
        //        var response = repo.GetCollateralTypeByCollateralId(collateralId, typeId);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
        //    }
        //}

        [HttpGet, Route("customer-collateral/{collateralId}/collateral/{typeId}/type")]
        public HttpResponseMessage GetCollateralTypeByCollateral(int collateralId, int typeId)
        {
            try
            {
                var response = repo.GetCollateralTypeByCollateralId(collateralId, typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }
        [HttpGet, Route("temp-customer-collateral/type/collateral/{collateralId}/type/{typeId}")]
        public HttpResponseMessage GetTempCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            try
            {
                var response = repo.GetTempCollateralTypeByCollateralId(collateralId, typeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("get-customer-collateral-by-customer-collateralId/{customerCollateralId}")]
        public HttpResponseMessage GetCustomerCollateralByCustomerCollateralId(int customerCollateralId)
        {
            try
            {
                var response = repo.GetCustomerCollateralByCustomerCollateralId(customerCollateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("insurance-policies/{collateralId}")]
        public HttpResponseMessage GetInsurancePolicies(int collateralId)
        {
            try
            {
                var response = repo.GetCollateralInsurancePolicies(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }
        [HttpGet, Route("insurance-policy/{collateralId}")]
        public HttpResponseMessage GetInsurancePolicy(int collateralId)
        {
            try
            {
                var response = repo.GetInsurancePolicy(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }
        [HttpPost, Route("add-insurance-policy")]
        public HttpResponseMessage AddNewInsurancePolicy(InsurancePolicy insurancePolicies)
        {
            try
            {
                insurancePolicies.userBranchId = (short)token.GetBranchId;
                insurancePolicies.companyId = token.GetCompanyId;
                insurancePolicies.createdBy = token.GetStaffId;
                insurancePolicies.applicationUrl = HttpContext.Current.Request.Path;

                var respo = repo.AddNewItemInsurancePolicy(insurancePolicies);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = repo });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }
        [HttpGet, Route("collateral-document/{collateralId}")]
        public HttpResponseMessage GetCollateralDocumentByCollateral(int collateralId)
        {
            try
            {
                var data = document.GetCustomerCollateralDocument(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet, Route("temp-collateral-document/{collateralId}")]
        public HttpResponseMessage GetTempCollateralDocumentByCollateral(int collateralId)
        {
            try
            {
                var data = document.GetTempAllCollateralDocument(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-visitation-file/{documentId}")]
        public HttpResponseMessage GetVisitationFile(int documentId)
        {
            try
            {
                var data = document.GetCollateralVisitationDocument(documentId); //CollateralVisitationDocumentViewModel

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
        [Route("temp-collateral-visitation-file/{documentId}")]
        public HttpResponseMessage GetTempVisitationFile(int documentId)
        {
            try
            {
                var data = document.GetTempCollateralVisitationDocument(documentId); //CollateralVisitationDocumentViewModel

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
        [Route("collateral-guarantee/{targetId}")]
        public HttpResponseMessage GetCollaterGuaranteeFile(int targetId)
        {
            try
            {
                var data = document.GetCollateralGuaranteeDocument(targetId); //CollateralVisitationDocumentViewModel

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
        [Route("loan-visitation/{collateralVisitationId}")]
        public HttpResponseMessage GetVisitationDocument(int collateralVisitationId)
        {
            try
            {
                var data = repo.GetPropertyVistation(collateralVisitationId);

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
        [Route("temp-collateral-visitation/{collateralVisitationId}")]
        public HttpResponseMessage GetTempVisitationDocument(int collateralVisitationId)
        {
            try
            {
                var data = repo.GetTempPropertyVistation(collateralVisitationId);

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
        [Route("visitation-document")]
        public async Task<HttpResponseMessage> AddVisitationDocument()
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

                int collateralCustomerId;
                if (!Int32.TryParse(provider.FormData["collateralCustomerId"], out collateralCustomerId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                var visitationDate = provider.FormData["lastVisitaionDate"];
                var nextVisitation = provider.FormData["nextVisitationDate"];

                var actualDate = visitationDate.Substring(0, 15);
                var nextDate = nextVisitation.Substring(0, 15);
                var dateVisited = DateTime.ParseExact(actualDate, "ddd MMM dd yyyy", CultureInfo.InvariantCulture);
                var nextVisitationDate = DateTime.ParseExact(nextDate, "ddd MMM dd yyyy", CultureInfo.InvariantCulture);

                var entity = new CollateralDocumentViewModel
                {
                    lastVisitaionDate = dateVisited,
                    nextVisitationDate = nextVisitationDate,
                    visitationRemark = provider.FormData["visitationRemark"],
                    collateralCustomerId = Convert.ToInt32(provider.FormData["collateralCustomerId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.collateralCustomerId = collateralCustomerId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = document.AddCollateralVisitation(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("temp-visitation-document")]
        public async Task<HttpResponseMessage> AddTempVisitationDocument()
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

                int collateralCustomerId;
                if (!Int32.TryParse(provider.FormData["collateralCustomerId"], out collateralCustomerId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                var visitationDate = provider.FormData["lastVisitaionDate"];

                var actualDate = visitationDate.Substring(0, 15);
                var dateVisited = DateTime.ParseExact(actualDate, "ddd MMM dd yyyy", CultureInfo.InvariantCulture);

                var entity = new CollateralDocumentViewModel
                {
                    lastVisitaionDate = dateVisited,
                    visitationRemark = provider.FormData["visitationRemark"],
                    collateralCustomerId = Convert.ToInt32(provider.FormData["collateralCustomerId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.collateralCustomerId = collateralCustomerId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = document.AddTempCollateralVisitation(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPost, Route("collateral-visitation")]
        public HttpResponseMessage AddCollateralVisitation([FromBody] CollateralDocumentViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = repo.AddPropertyVistation(entity);
                if (response > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost, Route("collateral-document")]
        public async Task<HttpResponseMessage> AddCollateralDocument()
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

                int collateralId;
                if (!Int32.TryParse(provider.FormData["collateralId"], out collateralId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }
                int documentTypeId;
                if (!Int32.TryParse(provider.FormData["documentTypeId"], out documentTypeId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }
                var entity = new CollateralDocumentViewModel
                {
                    documentTitle = provider.FormData["documentTitle"], // document code
                    documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                    collateralCode = provider.FormData["collateralCode"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.collateralId = collateralId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = document.AddCollateralDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPost, Route("temp-collateral-document")]
        public async Task<HttpResponseMessage> AddTempCollateralDocument()
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

                int collateralId;
                if (!Int32.TryParse(provider.FormData["collateralId"], out collateralId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                var entity = new CollateralDocumentViewModel
                {
                    documentTitle = provider.FormData["documentTitle"], // document code
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                    collateralCode = provider.FormData["collateralCode"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.collateralId = collateralId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = document.AddTempCollateralDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpGet, Route("customer-collateral/loan/{loanId}/productTypeId/{productTypeId}")]
        public HttpResponseMessage GetLoanCollateral(int loanId, int productTypeId)
        {
            try
            {
                var response = repo.GetLoanCollateral(loanId, productTypeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/active/{customerId}")]
        public HttpResponseMessage GetActiveCustomerCollateral(int customerId)
        {
            try
            {
                var response = repo.GetActiveCustomerCollateral(customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/release/{mappingId}")]
        public HttpResponseMessage ReleaseCollateral(int mappingId)
        {
            GeneralEntity userInfo = new GeneralEntity()
            {
                createdBy = token.GetStaffId,
                companyId = token.GetCompanyId,
                userBranchId = (short)token.GetBranchId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress,
            };
            var response = repo.ReleaseCollateral(mappingId, token.GetStaffId, userInfo);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
        }

        [HttpGet, Route("collateral-release/pending-approval")]
        public HttpResponseMessage GetPendingCustomerCollateralRelease()
        {
            try
            {
                var response = repo.GetPendingCustomerCollateralRelease(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("customer-collateral/release-approval")]
        public HttpResponseMessage ApproveCollateralRelease([FromBody] ApprovalViewModel entity)
        {
            try
            {
                GeneralEntity userInfo = new GeneralEntity()
                {
                    createdBy = token.GetStaffId,
                    companyId = token.GetCompanyId,
                    userBranchId = (short)token.GetBranchId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress,
                };
                var response = repo.ApproveCollateralRelease(entity, token.GetStaffId, userInfo);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("customer-collateral/assignment")]
        public HttpResponseMessage AssignCollateral([FromBody] ActiveCustomerCollateralViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = false; // repo.AssignCollateral(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/search/")]
        public HttpResponseMessage SearchStaff(string queryString)
        {
            try
            {
                var data = repo.SearchCollateral(queryString, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }

        }

        #endregion New

        #region Collateral 

        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-collateral")]
        public HttpResponseMessage AddCollateral([FromBody] CollateralViewModel entity)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();

            entity.createdBy = token.GetStaffId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            entity.companyId = token.GetCompanyId;

            var response = repo.AddCollateral(entity);
            if (response > 0 && entity.isRegistrationDoneViaLoanApplication != (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record Created successfully and sent for approval" });
            }
            if (response > 0 && entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record Created successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        }

        //[HttpPut]
        //[Route("customer-collateral/{collateralCustomerId}")]
        //public async Task<HttpResponseMessage> UpdateCustomCollateral(int collateralCustomerId, [FromBody] CollateralCustomerViewModel entity)
        //{

        //    try
        //    {
        //        entity.lastUpdatedBy = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.userBranchId = (short)token.GetBranchId;

        //        var response = await repo.UpdateCollateralCustomer(collateralCustomerId, entity);
        //        if (!response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}


        #endregion

        #region Collatera Types
        [Route("collateral-document-type/{id}")]
        public HttpResponseMessage GetCollateralDocumentType(int id)
        {
            try
            {
                var response = type.GetCollateralDocumentTypes(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("collateral-document-type")]
        public HttpResponseMessage AddCollateralDocumentType([FromBody] CollateralDocumentTypeViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = type.AddCollateralDocumentType(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-type")]
        public HttpResponseMessage GetCollateralType()
        {
            try
            {
                var response = type.GetCollateralTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-type/loan-application/{id}")]
        public HttpResponseMessage GetCollateralTypeByLoanApplicationId(int? id)
        {
            try
            {
                var response = type.CollateralTypesByLoanApplication(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-sub-type")]
        public HttpResponseMessage GetCollateralSubTypes()
        {
            try
            {
                var response = type.GetCollateralSubTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-sub-type/{id}")]
        public HttpResponseMessage GetCollateralSubTypes(int id)
        {
            try
            {
                var response = type.CollateralSubType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            //catch (SecureException ex)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            //}
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-sub-type/collateral-type/{collateralTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            try
            {
                var response = type.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("collateral-sub-type")]
        public HttpResponseMessage AddCollateralSubType([FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = type.AddCollateralSubTypes(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("collateral-type/{collateralTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralType(short collateralTypeId, [FromBody] CollateralTypeViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;
                entity.createdBy = token.GetStaffId;


                var response = await type.UpdateCollateralTypes(collateralTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Record created sussessfully", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public HttpResponseMessage UpdateCollateralSubType(short collateralSubTypeId, [FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;
                entity.createdBy = token.GetStaffId;

                var response = type.UpdateCollateralSubTypes(collateralSubTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Update successful", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion  End of Collateral Types

        #region Seniority Of Claims
        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-seniority-of-claims")]
        public HttpResponseMessage GetCollateralSeciorityOfClaims()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralSeniorityOfClaims();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Seniority Of Claims

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-valuer")]
        public HttpResponseMessage GetCollateralValuers()
        {
            try
            {
                var response = repo.GetCollateralValuer(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-perfection-status")]
        public HttpResponseMessage GetCollateralPerfectionStatus()
        {
            try
            {
                var response = repo.GetCollateralPerfectionStatus();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-valuer-type")]
        public HttpResponseMessage GetCollateralValuerType()
        {
            try
            {
                var response = repo.GetCollateralValuerType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-value-base-type/{collateralType}")]
        public HttpResponseMessage GetCollateralValueBaseType(short collateralType)
        {
            try
            {
                var response = repo.GetCollateralValueBaseType(collateralType);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost, Route("collateral-valuer")]
        public async Task<HttpResponseMessage> AddCollateralValuer([FromBody] CollateralValuersViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.AddCollateralValuer(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut, Route("collateral-valuer/{id}")]
        public async Task<HttpResponseMessage> UpdateCollateralValuer([FromBody] CollateralValuersViewModel entity, int id)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.UpdateCollateralValuer(entity, id);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        //[HttpGet, Route("unmapped-collateral-application/customer/{customerId}/loanapplication/{loanapplicationid}")]
        //public HttpResponseMessage GetAllUnmappedCustomerCollateral(int customerId, int loanApplicationId)
        //{
        //    try
        //    {
        //        var response = repo.GetAllUnmappedCustomerCollateral(customerId, loanApplicationId , token.GetCompanyId );
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
        //    }
        //}

        //[HttpGet, Route("mapped-collateral-application/customer/{customerId}/loanapplication/{loanapplicationid}")]
        //public HttpResponseMessage GetAllMappedCustomerCollateral(int customerId, int loanApplicationId)
        //{
        //    try
        //    {
        //        var response = repo.GetAllMappedCustomerCollateral(customerId, loanApplicationId, token.GetCompanyId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
        //    }
        //}

        [HttpPost, Route("application-collateral/map")]
        public HttpResponseMessage MapApplicationCollateral([FromBody] ApplicationCollateralMapping entity)
        {
            try
            {

                entity.staffId = token.GetStaffId;

                var response = repo.MapApplicationCollateral(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("application-collateral/mapped")]
        public HttpResponseMessage IsCollateralMapped([FromBody] ApplicationCollateralMapping entity)
        {
            try
            {

                //      entity.staffId = token.GetStaffId;

                var response = repo.IsCollateralMapped(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("application-collateral/unmap")]
        public HttpResponseMessage UnmapApplicationCollateral([FromBody] ApplicationCollateralMapping entity)
        {
            try
            {
                var response = repo.UnmapApplicationCollateral(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("collateral-information-view/{customercollateralId}")]
        public HttpResponseMessage GetCollateralInformationById(int customercollateralId)
        {
            try
            {
                var response = repo.GetCollateralInformationById(customercollateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("get-fixeddeposit-lien-amount")]
        public HttpResponseMessage GetLienAmountForFD([FromBody] string accountNumber)
        {
            try
            {
                var response = repo.GetAccountLeinAmountForFD(accountNumber);
                var lienData = repo.GetAccountLienDetail(accountNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, data = lienData });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-casa-lien-amount/{accountNumber}")]
        public HttpResponseMessage GetLienAmountForCASA(string accountNumber)
        {
            try
            {
                var response = repo.GetAccountLeinAmountForCASA(accountNumber);
                var lienData = repo.GetAccountLienDetail(accountNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, data = lienData });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-history/{collateralId}")]
        public HttpResponseMessage GetCollateralHistory(short collateralId)
        {
            var response = repo.getCollateralHistory(collateralId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Records not found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-history-usage/{collateralId}")]
        public HttpResponseMessage GetCollateralHistoryUsage(int collateralId)
        {
            var response = repo.getCollateralHistoryUsage(collateralId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Records not found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("stock-price")]
        public HttpResponseMessage GetStockPrice()
        {
            try
            {
                var response = repo.getStockPrice();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-stamp-to-cover-values/{customerId}")]
        public HttpResponseMessage GetCollateralStampToCoverValues(int customerId)
        {
            try
            {
                var response = repo.GetCollateralStampToCoverValues(customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-casa-balance/{accountNumber}")]
        public HttpResponseMessage GetCollateralStampToCoverValues(string accountNumber)
        {
            try
            {
                var response = repo.GetFixedDepositAccountDetail(accountNumber);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("reject-propose/collateral/{collateralCustomerId}/collateralCustomerId")]
        public HttpResponseMessage RejectCollateral(int collateralCustomerId)
        {
            try
            {
                var response = repo.RejectProposedCollateralForUsage(collateralCustomerId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("propose-collateral")]
        public HttpResponseMessage ProposeCollateral(CollateralCoverageViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var response = repo.ProposeCollateralForUsage(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("propose-collaterals-lms")]
        public HttpResponseMessage ProposeCollateralLms(CollateralCoverageViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var response = repo.ProposeCollateralForUsageLMS(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-usage")]
        public HttpResponseMessage CollateralUsage()
        {
            try
            {
                var response = repo.GetCollateralUsageStatus();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-type")]
        public HttpResponseMessage GetInsuranceType()
        {
            try
            {
                var response = repo.GetInsuranceType();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-company")]
        public HttpResponseMessage GetInsuranceCompany()
        {
            try
            {
                var response = repo.GetInsuranceCompany();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("check-insurance-policy")]
        public HttpResponseMessage checkInsurancePolicy([FromBody] InsurancePolicy model)
        {
            try
            {
                var response = repo.checkInsurancePolicy(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance-policy")]
        public HttpResponseMessage AddInsurancePolicy([FromBody] CollateralInsurancePolicyViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = repo.AddInsurancePolicy(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }

            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });

            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("policy-insurance-doc/{id}")]
        public HttpResponseMessage UpdateInsurancePolicy([FromUri] int id, [FromBody] CollateralInsurancePolicyViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                bool response = repo.UpdateInsurancePolicy(id, model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });

            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance-request-go-for-approval")]
        public HttpResponseMessage AddInsurancePolicy([FromBody] CollateralViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = repo.InsuranceRequestGoForApproval(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }

            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });

            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-insurance-policy-request/{id}")]
        public HttpResponseMessage UpdateInsurancePolicyRequest([FromBody] CollateralInsuranceRequestViewModel model, int id)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                var response = repo.UpdateInsurancePolicyRequest(model, id);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance-policy-request/{id}")]
        public HttpResponseMessage AddInsurancePolicyRequest([FromBody] CollateralInsuranceRequestViewModel model, int? id)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = repo.AddInsurancePolicyRequest(model, id);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this request, Contact the System Administrator" });
            }

            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });

            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-request-referenceNumber")]
        public HttpResponseMessage GetReferenceNumber()
        {
            try
            {
                var response = repo.GetReferenceNumber();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-insurance-requests")]
        public HttpResponseMessage GetInsuranceRequests()
        {
            try
            {
                var response = repo.GetInsuranceRequests(token.GetStaffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-insurance-last-comment/{operationId}/{targetId}")]
        public HttpResponseMessage GetLastComment(int operationId, int targetId)
        {
            try
            {
                var response = repo.GetLastComment(targetId, operationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-insurance-request/{insuranceRequestId}")]
        public HttpResponseMessage DeleteInsuranceRequest(int insuranceRequestId)
        {
            var response = repo.DeleteInsuranceRequest(insuranceRequestId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been removed successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error removing this record" });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-coverage/{collateralSubTypeId}/collateralSubTypeId")]
        public HttpResponseMessage GetCollateralCoverage(int collateralSubTypeId)
        {
            try
            {
                var response = repo.GetCollateralCoverage(collateralSubTypeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost, Route("collateral-coverage")]
        public HttpResponseMessage AddCollateralCoverage([FromBody] CollateralCoverageViewModel entity)
        {
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

            var response = repo.AddCollateralCoverage(entity);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpDelete, Route("delete-collateral-coverage/{collateralCoverageId}/collateralCoverageId")]
        public HttpResponseMessage DeleteCollateralCoverage(int collateralCoverageId)
        {
            var response = repo.DeleteCollateralCoverage(collateralCoverageId, token.GetStaffId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been deleted successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpDelete, Route("delete-valuation-valuer/{valuerId}")]
        public HttpResponseMessage DeleteAddedValuer(int valuerId)
        {
            var response = repo.DeleteAddedValuer(valuerId, token.GetStaffId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been deleted successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPost, Route("delete-proposed-collateral-coverage")]
        public HttpResponseMessage DeleteProposedCollateral(CollateralCoverageViewModel model)
        {
            model.createdBy = token.GetStaffId;
            var response = repo.DeleteProposedCollateral(model);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The collateral has been unproposed successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error unproposing this collateral" });
        }

        [HttpPost, Route("delete-duplicate-collateral")]
        public HttpResponseMessage DeleteDuplicatedCollateral(CollateralViewModel model)
        {
            model.createdBy = token.GetStaffId;
            model.deletedBy = token.GetStaffId;

            var response = repo.DeleteDuplicatedCollateral(model);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The collateral has been deleted successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "You can not delete a collateral that is not created by you" });
        }

        [HttpPost, Route("calculate-collateral-coverage")]
        public HttpResponseMessage CalculateCoverateOfCollateral([FromBody] CollateralCoverageViewModel entity)
        {
            try
            {
                var response = repo.CalculateCoverateOfCollateral(entity);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

        }

        [HttpPost, Route("calculate-collateral-coverage-lms")]
        public HttpResponseMessage CalculateCoverateOfCollateralLms([FromBody] CollateralCoverageViewModel entity)
        {
            try
            {
                var response = repo.CalculateCoverateOfCollateralLMS(entity);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance")]
        public HttpResponseMessage GetInsuranceCompanies()
        {
            IEnumerable<InsuranceCompanyViewModel> response = repo.GetInsuranceCompanies();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance/{id}")]
        public HttpResponseMessage GetInsuranceCompany(int id)
        {
            InsuranceCompanyViewModel response = repo.GetInsuranceCompany(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance")]
        public HttpResponseMessage AddInsuranceCompany([FromBody] InsuranceCompanyViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddInsuranceCompany(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }


        [HttpPut]
        [ClaimsAuthorization]
        [Route("insurance/{id}")]
        public HttpResponseMessage UpdateInsuranceCompany([FromBody] InsuranceCompanyViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateInsuranceCompany(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("insurance/{id}")]
        public HttpResponseMessage DeleteInsuranceCompany(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteInsuranceCompany(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-type-all")]
        public HttpResponseMessage GetInsuranceTypes()
        {
            IEnumerable<InsuranceTypeViewModel> response = repo.GetInsuranceTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-type-all")]
        public HttpResponseMessage GetCollateralTypes()
        {
            IEnumerable<CollateralTypeViewModel> response = repo.GetCollateralTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-sub-type-all/{collateralTypeId}")]
        public HttpResponseMessage GetCollateralSubType(int collateralTypeId)
        {
            IEnumerable<CollateralSubTypeViewModel> response = repo.GetCollateralSubTypes(collateralTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-status-all")]
        public HttpResponseMessage GetInsuranceStatus()
        {
            IEnumerable<InsuranceStatusViewModel> response = repo.GetInsuranceStatus();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-type-view-all")]
        public HttpResponseMessage GetInsuranceTypesViewAll()
        {
            IEnumerable<InsuranceTypeViewModel> response = repo.GetInsuranceTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-policy-type-all")]
        public HttpResponseMessage GetInsurancePolicyTypes()
        {
            IEnumerable<InsurancePolicyTypeViewModel> response = repo.GetInsurancePolicyTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-type/{id}")]
        public HttpResponseMessage GetInsuranceType(int id)
        {
            InsuranceTypeViewModel response = repo.GetInsuranceType(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance-type")]
        public HttpResponseMessage AddInsuranceType([FromBody] InsuranceTypeViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddInsuranceType(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance-policy-type")]
        public HttpResponseMessage AddInsurancePolicyType([FromBody] InsurancePolicyTypeViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddInsurancePolicyType(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }


        [HttpPut]
        [ClaimsAuthorization]
        [Route("insurance-type/{id}")]
        public HttpResponseMessage UpdateInsuranceType([FromBody] InsuranceTypeViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateInsuranceType(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("insurance-policy-type/{id}")]
        public HttpResponseMessage UpdateInsurancePolicyType([FromBody] InsurancePolicyTypeViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateInsurancePolicyType(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("insurance-type/{id}")]
        public HttpResponseMessage DeleteInsuranceType(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteInsuranceType(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("insurance-policy-type/{id}")]
        public HttpResponseMessage DeleteInsurancePolicyType(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteInsurancePolicyType(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("insurance-search/{searchString}")]
        public HttpResponseMessage GetInsuranceSearch(string searchString)
        {
            IEnumerable<InsurancePolicy> response = repo.Explore(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("policy-insurance-doc")]
        public HttpResponseMessage SaveInsurancePolicy([FromBody] InsurancePolicy model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddInsurancePolicyFile(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("policy-insurance-doc/{id}")]
        public HttpResponseMessage DeleteInsurancePolicy([FromUri] int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteInsurancePolicy(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        #region collateral-swap
        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-swap-search/{searchString}")]
        public HttpResponseMessage SearchCollateralSwap(string searchString)
        {
            IEnumerable<CollateralSwapViewModel> response = repo.SearchCollateralSwap(searchString);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-swap")]
        public HttpResponseMessage GetAllCollateralSwaps()
        {
            IEnumerable<CollateralSwapViewModel> response = repo.GetAllCollateralSwaps(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-swap-approval")]
        public HttpResponseMessage GetCollateralSwapsForApproval()
        {
            IEnumerable<CollateralSwapViewModel> response = repo.GetCollateralSwapsForApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-swap/{id}")]
        public HttpResponseMessage GetCollateralSwap(int id)
        {
            CollateralSwapViewModel response = repo.GetCollateralSwap(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("collateral-swap")]
        public HttpResponseMessage AddCollateralSwap([FromBody] CollateralSwapViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            try
            {
                var response = repo.AddCollateralSwap(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("collateral-swap/forward-for-approval")]
        public HttpResponseMessage CollateralSwapMemorandum([FromBody] CollateralSwapViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.CollateralSwapMemorandum(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "COLLATERAL SWAP") });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-next-level-for-collateral-swap/{collateralSwapId}")]
        public HttpResponseMessage GetNextLevelForCollateralSwap(int collateralSwapId)
        {
            var data = repo.GetNextLevelForCollateralSwap(collateralSwapId, token.GetStaffId, token.GetCompanyId);

            if (data > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, data, message = "NextLevelId fetching was successfull!" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "NextLevelId fetching was unsuccessful!" });

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("collateral-swap/{id}")]
        public HttpResponseMessage UpdateCollateralSwap([FromBody] CollateralSwapViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateCollateralSwap(model, id, user);

            if (!response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "An error occurred while updating the record", count = 0 });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("collateral-swap/{id}")]
        public HttpResponseMessage DeleteCollateralSwap(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteCollateralSwap(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("collateral-mapping/{id}")]
        public HttpResponseMessage GetCollateralMappingDetails(int id)
        {
            IEnumerable<LoanApplicationDetailViewModel> response = repo.GetCollateralMappingDetails(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-insurance/{CollateralId}")]
        public HttpResponseMessage GetAddedInsuranceById(int CollateralId)
        {
            var response = repo.GetAddedInsuranceById(CollateralId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }
        #endregion collateral-swap

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-facility-stamp-duty/{loanApplicationId}")]
        public HttpResponseMessage GetFacilityStampDuty(int loanApplicationId)
        {
            var response = repo.GetFacilityStampDuty(loanApplicationId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-facility-stamp-duty-id/{loanApplicationId}")]
        public HttpResponseMessage GetFacilityStampDutyId(int loanApplicationId)
        {
            var response = repo.GetFacilityStampDutyId(loanApplicationId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-facility-stamp-duty-by-id/{loanApplicationId}")]
        public HttpResponseMessage GetFacilityStampDutyById(int loanApplicationId)
        {
            var response = repo.GetFacilityStampDutyById(loanApplicationId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [Route("get-all-facility-stamp-duty")]
        public HttpResponseMessage GetAllFacilityStampDuty()
        {
            var response = repo.GetAllFacilityStampDuty();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [Route("get-all-facility-stamp-duty-fixed")]
        public HttpResponseMessage GetAllFacilityStampDutyFixed()
        {
            var response = repo.GetAllFacilityStampDutyFixed();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [Route("get-all-facility-stamp-duty-fixed-filtered")]
        public HttpResponseMessage GetAllFacilityStampDutyFixedFiltered(DateRange dateRange)
        {
            var response = repo.GetAllFacilityStampDutyFixedFiltered(dateRange);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("save-facility-stamp-sharing")]
        public HttpResponseMessage AddFacilityStampDutySharing([FromBody] FacilityStampDutyViewModel entity)
        {
            try
            {              
                var data = repo.AddFacilityStampDutySharing(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("get-all-facility-stamp-duty-report")]
        public HttpResponseMessage GetAllFacilityStampDutyReport(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();

            dateRange.companyId = token.GetCompanyId;
            var data = repo.GetAllFacilityStampDutyReport(dateRange);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("get-all-facility-stamp-duty-filtered")]
        public HttpResponseMessage GetAllFacilityStampDutyFiltered(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();

            dateRange.companyId = token.GetCompanyId;
            var data = repo.GetAllFacilityStampDutyFiltered(dateRange);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("stamp-setup")]
        public HttpResponseMessage AddStampSetup([FromBody] StampDutyConditionViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.AddStampSetup(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                //_errorLog.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"{"There was an error creating this record"} {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("stamp-setup")]
        public HttpResponseMessage GetStampSetup()
        {
            try
            {
                var data = repo.GetStampSetup();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No Record Found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {
                //_errorLog.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"{"Error"}: {ex.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("stamp-setup/{conditionId}")]
        public HttpResponseMessage UpdateTATSetup(short conditionId, [FromBody] StampDutyConditionViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.UpdateStampSetup(conditionId, entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                //_errorLog.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"{"There was an error updating this record"} {ex.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-stamp/{conditionId}")]
        public HttpResponseMessage DeleteStampSetup(int conditionId)
        {
            try
            {
                var user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host,
                    createdBy = token.GetStaffId
                };

                 repo.DeleteStampSetup(conditionId, user);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = conditionId, message = "Record has been deleted successfully"});
            }
            catch (SecureException ex)
            {
                //_errorLog.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }

}

