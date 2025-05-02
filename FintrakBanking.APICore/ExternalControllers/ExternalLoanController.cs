using FintrakBanking.APICore.core;
using FintrakBanking.APICore.Filters;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.External;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.External.Customer;
using FintrakBanking.ViewModels.External.Loan;
using Microsoft.AspNetCore.Http;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Utility.WebApi.OutputCache.V2.TimeAttributes;
//using Microsoft.AspNetCore.Mvc;

namespace FintrakBanking.APICore.ExternalControllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [MiddlewareAuthorizeAttribute]
    [RoutePrefix("api/v1/third-party/loan")]
    public class ExternalLoanController : ApiController
    {

        private IProductRepository repoProduct;
        private ILoanRepositoryExternal repoLoan;
        private ILoanApplicationRepository loanApplicationRepository;
        private ILoanPrincipalRepository loanPrincipalRepository;
        private IGeneralSetupRepository generalSetupRepository;
        private ILoanScheduleRepository scheduleRepo;

        public ExternalLoanController(IProductRepository _repoProduct, ILoanRepositoryExternal _repoLoan, 
            ILoanApplicationRepository _loanApplicationRepository, ILoanPrincipalRepository _loanPrincipalRepository,
            IGeneralSetupRepository _generalSetupRepository, ILoanScheduleRepository _scheduleRepo
)
        {
            this.repoProduct = _repoProduct;
            this.repoLoan = _repoLoan;
            this.loanApplicationRepository = _loanApplicationRepository;
            this.loanPrincipalRepository = _loanPrincipalRepository;
            this.generalSetupRepository = _generalSetupRepository;     
            this.scheduleRepo = _scheduleRepo;
        }

        [HttpGet]
        [Route("loan-schedule/")]
        public HttpResponseMessage GetLoanSchedule(string applicationRefNo)
        {
            try
            {
                var data = repoLoan.LoanSchedule(applicationRefNo);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, count = data.Count(), result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]        
        [Route("get-loans/")]
        public HttpResponseMessage GetNHFLoans(string nhfNo)
        {
            try
            {
                var data =  repoLoan.GetNHFLoans(nhfNo);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, count = data.Count(), result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("loan-affordability-check")]
        public HttpResponseMessage LoanAffordabilityCheck(AffordabilityViewModel model)
        {
            try
            {
                var data =  repoLoan.AffordabilityChecks(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet] 
        //[Route("products")]
        //public async Task<HttpResponseMessage> GetAllProduct()
        //{
        //    try
        //    {
        //        var data = await repoProduct.GetAllExternalProductAsync(); 
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet]
        [Route("products")]
        public HttpResponseMessage GetAllProduct()
        {
            try
            {
                var data = repoProduct.GetAllExternalProduct();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [CacheOutputUntilToday(23, 55)]
        [Route("crms-funding-source")] 
        public async Task<HttpResponseMessage> GetAllCRMSFundingSource()
        {
            try
            {
                var data = loanApplicationRepository.GetAllCRMSFundingSource();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [CacheOutputUntilToday(23, 55)]
        [Route("crms-repayment-source")]
        public async Task<HttpResponseMessage> GetAllCRMSRepaymentSource()
        {
            try
            {
                var data = loanApplicationRepository.GetAllCRMSRepaymentSource();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [Route("loan-principals")]
        [CacheOutputUntilToday(23, 55)]
        [HttpGet] 
        public HttpResponseMessage GetAllLoanPrincipal()
        {
            try
            {
                var data = loanPrincipalRepository.GetLoanPrincipal();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }


        [Route("sectors")]
        [CacheOutputUntilToday(23, 55)]
        [HttpGet]
        public HttpResponseMessage GetAllSectors()
        {
            try
            {
                var data = generalSetupRepository.GetAllSectors();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("sub-sectors-by-sector/{sectorId}")] 
        [HttpGet]
        public HttpResponseMessage SubSectorBySector(int sectorId)
        {
            try
            {
                var data = generalSetupRepository.GetSubSectorsBySector(sectorId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        [Route("loan-application")]
        public async Task<HttpResponseMessage> LoanApplication(LoanApplicationForCreation loan)
        {
            try
            {
                var data = repoLoan.AddLoanApplication(loan);
                if(data == null)
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error applying for this facility, kindly contact admin." });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("loan-application-multiple")]
        public async Task<HttpResponseMessage> AddBatchedLoanApplication(LoanApplicationForCreation loan)
        {
            try
            {
                var data = repoLoan.AddBatchedLoanApplication(loan);
                if (data == null)
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error applying for this facility, kindly contact admin." });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //[Route("loan-eligibility")]
        //public async Task<HttpResponseMessage> LoanEligibility(LoanEligibilityForInquiry loanEligibility)
        //{
        //    try
        //    {
        //        var data = await repoLoan.LoanEligibility(loanEligibility);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}


        [HttpGet]
        [Route("existing-loan-applications/{customerCode}")]
        public async Task<HttpResponseMessage> GetLoanApplicationByCustomer(string customerCode)
        {
            try
            {
                var data = await repoLoan.GetLoanApplicationByCustomer(customerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("existing-loan-application-detail/{applicationReferenceNo}")]
        public async Task<HttpResponseMessage> GetLoanApplicationByRefNo(string applicationReferenceNo)
        {
            try
            {
                var data = await repoLoan.GetLoanApplicationByRefNo(applicationReferenceNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("existing-loan-application-detail1/{applicationReferenceNo}")]
        public async Task<HttpResponseMessage> GetLoanApplicationByRefNo1(string applicationReferenceNo)
        {
            try
            {
                var data = repoLoan.GetLoanApplicationByRefNo1(applicationReferenceNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("is-offerletter-ready/{applicationReferenceNo}")]
        public async Task<HttpResponseMessage> IsOfferLetterReady(string applicationReferenceNo)
        {
            try
            {
                var data = await repoLoan.GetLoanApplicationByRefNo(applicationReferenceNo);
                if(data != null)
                {
                    if (data.isOfferLetterAvailable)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            result = data.isOfferLetterAvailable,
                            message = "The offer letter is ready for download."
                        }); 
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            result = data.isOfferLetterAvailable,
                            message = "The offer letter is not yet ready for download."
                        });
                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true, 
                    message = "This data does not exist."
                });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        [Route("download-offerletter/{applicationReferenceNo}")]
        public async Task<HttpResponseMessage> DownloadOfferLetter(string applicationReferenceNo)
        {
            try
            {
                var data = await repoLoan.DownloadOfferLetter(applicationReferenceNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("loan-document-upload")]
        public async Task<HttpResponseMessage> LoanDocumentUpload()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                //int uploadType;
                //if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                //}

                var entity = new LoanDocumentViewModel();
                //customerId = Convert.ToInt32(provider.FormData["customerId"]),
                entity.loanReferenceNumber = provider.FormData["loanReferenceNumber"];
                entity.documentTitle = provider.FormData["documentTitle"];
                //entity.documentTypeId = (short)uploadType; 
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                // var file = provider.Contents.LastOrDefault();
                var file = provider.FileStreams.FirstOrDefault();
                byte[] buffer;
                //var buffer = await file.ReadAsByteArrayAsync();
                using (var ms = new MemoryStream())
                {
                    await file.Value.CopyToAsync(ms);
                    buffer = ms.ToArray();
                }

                var data = await repoLoan.LaonDocumentUpload(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been uploaded successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this record" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error : {ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }


        [HttpPost]
        [Route("loan-document-upload-multiple")]
        public async Task<HttpResponseMessage> LoanDocumentUploadMultiple()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                //int uploadType;
                //if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                //}
                var file = provider.FileStreams.ToList();
                List<LoanDocumentViewModel> uploads= new List<LoanDocumentViewModel>();
                for (int i = 0; i < provider.FileStreams.Count; i++)
                {

                    var entity = new LoanDocumentViewModel();
                    //customerId = Convert.ToInt32(provider.FormData["customerId"]),
                    entity.loanReferenceNumber = provider.FormData["loanReferenceNumber"];
                    entity.documentTitle = provider.FormData["documentTitle"+i];
                    //entity.documentTypeId = (short)uploadType; 
                    entity.fileName = provider.FormData["fileName"+i];
                    entity.fileExtension = provider.FormData["fileExtension"+i];
                    // var file = provider.Contents.LastOrDefault();
                   
                    byte[] buffer;
                    //var buffer = await file.ReadAsByteArrayAsync();
                    using (var ms = new MemoryStream())
                    {
                        await file[i].Value.CopyToAsync(ms);
                        buffer = ms.ToArray();
                    }

                    entity.fileData = buffer;

                    uploads.Add(entity);
                }
                

                var data = await repoLoan.LoanDocumentUpload(uploads);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The documents were uploaded successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading documents" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error : {ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }


        [HttpGet]
        [Route("uploaded-loan-documents/{loanReferenceNumber}")]
        public async Task<HttpResponseMessage> GetLoanDocumentUploadByRefNo(string loanReferenceNumber)
        {
            try
            {
                var data = await repoLoan.GetLoanDocumentUploadByRefNo(loanReferenceNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("check-outstanding-loan/")]
        public  async Task<HttpResponseMessage> CheckOutstandingLoan(string nhfNo)
        {
            try
            {
                var res = await repoLoan.CheckOutstandingLoan(nhfNo);
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, status = true, message = $"No outstanding loan found for NHF Account: {nhfNo}"});
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, status = true, message = ex.Message });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("get-disbursed-loans/")]
        public HttpResponseMessage GetDisbursedLoans(int companyId)
        {
            try
            {
                var data = repoLoan.GetDisbursedLoans(companyId);
                if (data.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, count = data.Count(), result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

       
        [HttpPost]
        [Route("Refinance-Loans")]
        public HttpResponseMessage LoanRefinancing(RefinanceViewModel Model)
        {
            try
            {
                var data = repoLoan.RefinanceLoan(Model);
                if (data == null)
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error applying for this facility, kindly contact admin." });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost]
        [Route("Post-Customer-uus")]
        public HttpResponseMessage PostCustomerUus(List<CustomerUusViewModel> Model)
        {
            try
            {
                

                var data = repoLoan.PostCustomersUItems(Model);
                if (data == null)
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error applying for this facility, kindly contact admin." });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-obligor-uus")]
        public async Task<HttpResponseMessage> GetObligorUUS()
        {
            try
            {
                var data = await repoLoan.GetUUSForObligor();
                if (data.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found", result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, count = data.Count(), result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-applied-loans-refinance/")]
        public async Task<HttpResponseMessage> GetLoanForRefinance1(string  RefinanceNumber)
        {
            try
            {
                var data = await repoLoan.GetLoanForRefinance1(RefinanceNumber);
                if (data.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found", result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, count = data.Count(), result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("periodic-schedule-nmrc")]
        public HttpResponseMessage GeneratePeriodicLoanScheduleNMRC([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            var data = scheduleRepo.GeneratePeriodicLoanScheduleNMRC(loanInput);

            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

    }
}
