using FintrakBanking.APICore.JWTAuth;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.APICore.Controllers //D:\Projects\FintrakBanking\FintrakBankingAPIFW\FintrakBankingAPI462\FintrakBanking.APICore\Controllers\LoanController.cs
{
    [RoutePrefix("api/v1/loan")]
    public class LoanController : ApiControllerBase
    {
        private ILoanRepository repo;
        private ICustomerCollateralRepository repoCollateral;
        private ILoanScheduleRepository scheduleRepo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //private IHostingEnvironment _hostingEnvironment;
        //private IHostingEnvironment _hostingEnvironment;
        //TokenDecryptionHelper token = new TokenDecryptionHelper();
        public LoanController(ILoanRepository _repo,
                              ICustomerCollateralRepository _repoCollateral,
                               ILoanScheduleRepository _scheduleRepo)
        {
            this.repo = _repo;
            this.repoCollateral = _repoCollateral;
            this.scheduleRepo = _scheduleRepo;

            //this._hostingEnvironment = hostingEnvironment;
        }


        #region Loan 

        //[HttpGet][Route("loan-payment-schedule")]
        //public HttpResponseMessage GenerateLoanPaymentSchedule([FromBody] LoanPaymentScheduleInput entity)
        //{
        //    try
        //    {
        //        var data = LoanPaymentSchedule.GenerateLoanPaymentSchedule(input);
        //        if (!data.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

        [HttpGet]
        [Route("runningloans/customer/{id}")]
        public HttpResponseMessage GetAllLoanTypes(int id)
        {
            try
            {

                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.RunningLoans(id, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("loan-types")]
        public HttpResponseMessage GetAllLoanTypes()
        {
            try
            {
                var data = repo.GetAllLoanTypes();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-category")]
        public HttpResponseMessage GetAllLoanScheduleCategory()
        {
            try
            {
                var data = scheduleRepo.GetAllLoanScheduleCategory();
                //if (!data.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                //}

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-types")]
        public HttpResponseMessage GetAllLoanScheduleType()
        {
            try
            {
                var data = scheduleRepo.GetAllLoanScheduleType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-schedule-types/{productTypeId}")]
        public HttpResponseMessage GetAllLoanScheduleType(short? productTypeId)
        {
            try
            {
                var data = scheduleRepo.GetAllLoanScheduleType(productTypeId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [Route("loan-schedule-types/category/{categoryId}")]
        public HttpResponseMessage GetLoanScheduleTypeByCategory(short categoryId)
        {
            try
            {
                var data = scheduleRepo.GetLoanScheduleTypeByCategory(categoryId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost]
        [Route("loan-booking")]
        public async Task<HttpResponseMessage> AddLoanBooking([FromBody] LoanViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;


                var data = await repo.AddLoanBooking(entity);
                if (data != "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The Loan booking was successful and and is waiting for approval.\r\n Loan Reference Number: " + data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet]
        [Route("number-of-installments/tenor-mode/{tenorModeId}/frequency-type/{frequencyTypeId}/tenor/{tenor}")]
        public HttpResponseMessage GetNumberOfInstallments(short tenorModeId, short frequencyTypeId, int tenor)
        {
            try
            {

                var data = scheduleRepo.CalculateNumberOfInstallments((TenorModeEnum)tenorModeId, frequencyTypeId, tenor);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("{loanId}")]
        public HttpResponseMessage GetLoan(int loanId)
        {
            try
            {
                var data = repo.GetLoan(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-booking/awaiting-approval")]
        public HttpResponseMessage GetLoanBookingAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetLoanBookingAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data.Any() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost]
        [Route("loan-booking/approval")]
        public async Task<HttpResponseMessage> ApproveLoanBooking(ApprovalViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = (short)token.GetBranchId;
                model.staffId = token.GetStaffId;

                var data = await repo.GoForApproval(model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, message = "Loan Booking has been approved successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetCustomerLoans(int customerId)
        {
            try
            {
                var data = repo.GetLoanByCustomer(customerId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-group/{customerGroupId}")]
        public HttpResponseMessage GetCustomerGroupLoans(int customerGroupId)
        {
            try
            {
                var data = repo.GetLoanByCustomerGroup(customerGroupId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        result = data.ToList(),
                        message = "No record found"
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    result = data.ToList(),
                    count = data.Count()
                });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = $"Error: {e.Message}"
                });
            }
        }

        [HttpGet]
        [Route("find/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {

                var data = repo.FindLoan(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan-search")]
        public HttpResponseMessage SearchLoan([FromBody] LoanSearchViewModel searchModel)
        {
            try
            {
                var data = repo.LoanSearch(token.GetCompanyId, searchModel);
                //if (!data.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                //}

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("first-pay-date/effective-date/{effectiveDate}/frequency-type/{frequencyTypeId}")]
        public HttpResponseMessage GetFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            try
            {

                var data = scheduleRepo.CalculateFirstPayDate(effectiveDate, frequencyTypeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost]
        [Route("periodic-schedule")]
        public HttpResponseMessage GeneratePeriodicLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            try
            {
                var data = scheduleRepo.GeneratePeriodicLoanSchedule(loanInput);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("daily-schedule")]
        public HttpResponseMessage GenerateDailyLoanSchedule([FromBody] LoanPaymentScheduleInputViewModel loanInput)
        {
            try
            {
                var data = scheduleRepo.GenerateDailyLoanSchedule(loanInput);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [Route("detail")]
        public HttpResponseMessage GetBookedLoanDetails()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetails(token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("details/customer/{customerCode}")]
        public HttpResponseMessage GetBookedLoanDetails(string customerCode)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetailsByCustomerCode(customerCode, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("details/reference-number/{loanReferenceNumber}")]
        public HttpResponseMessage GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetBookedLoanDetailsByLoanReferenceNumber(loanReferenceNumber, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpPost][Route("schedule/export")]
        //public HttpResponseMessage ExportScheduleToExcel([FromBody] PaymentScheduleExcelViewModel model)
        //{
        //    try
        //    {
        //        string sWebRootFolder = _hostingEnvironment.ContentRootPath;
        //        string sFileName = $"schedule{DateTime.Now.Ticks.ToString()}.xlsx";
        //        string _path = "docs\\" + sFileName;
        //        string URL = Path.Combine(sWebRootFolder, _path); //
        //        string downloadUrl = string.Format("{0}://{1}/{2}/{3}", Request.Scheme, Request.Host, "docs", sFileName);
        //        FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, _path));
        //        if (file.Exists)
        //        {
        //            file.Delete();
        //            file = new FileInfo(Path.Combine(sWebRootFolder, _path));
        //        }
        //        using (ExcelPackage package = new ExcelPackage(file))
        //        {
        //            // add a new worksheet to the empty workbook
        //            var workSheetName = $"Schedule_{DateTime.Now.Ticks.ToString()}";
        //            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(workSheetName);
        //            //First add the headers

        //            worksheet.Cells[1, 1].Value = "LOAN REPAYMENT SCHEDULE";
        //            worksheet.Cells[1, 1].Style.Font.Bold = true;
        //            worksheet.Cells[1, 1].Style.Font.Size = 16;
        //            worksheet.Cells[3, 1, 5, 4].Style.Font.Size = 12;

        //            var numberformat = "#,##0";
        //            var dataCellStyleName = "TableNumber";
        //            var numStyle = package.Workbook.Styles.CreateNamedStyle(dataCellStyleName);
        //            numStyle.Style.Numberformat.Format = numberformat;

        //            var dateFormat = "dd/MM/yyyy";
        //            var dataCellDateStyleName = "TableDate";
        //            var dtStyle = package.Workbook.Styles.CreateNamedStyle(dataCellDateStyleName);
        //            dtStyle.Style.Numberformat.Format = dateFormat;

        //            worksheet.Cells[3, 1].Value = "Principal Amount";
        //            worksheet.Cells[3, 2].Value = model.principalAmount;
        //            worksheet.Cells[3, 2].Style.Numberformat.Format = numberformat;

        //            worksheet.Cells[3, 3].Value = "Interest Rate";
        //            worksheet.Cells[3, 4].Value = model.interestRate;
        //            worksheet.Cells[3, 4].Style.Numberformat.Format = numberformat;

        //            worksheet.Cells[4, 1].Value = "Payment Mode";
        //            worksheet.Cells[4, 2].Value = model.tenorMode;
        //            worksheet.Cells[4, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        //            worksheet.Cells[4, 3].Value = "No of Repayment";
        //            worksheet.Cells[4, 4].Value = model.numberOfPayments;
        //            worksheet.Cells[4, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        //            worksheet.Cells[5, 1].Value = "Loan Date";
        //            worksheet.Cells[5, 2].Value = model.loanDate;
        //            worksheet.Cells[5, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        //            worksheet.Cells[5, 2].Style.Numberformat.Format = dateFormat;

        //            worksheet.Cells[5, 3].Value = "First Repyment Date";
        //            worksheet.Cells[5, 4].Value = model.firstPaymentDate;
        //            worksheet.Cells[5, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        //            worksheet.Cells[5, 4].Style.Numberformat.Format = dateFormat;

        //            worksheet.Cells[7, 1].Value = "Periodic Payment Amount";
        //            worksheet.Cells[7, 2].Value = "Periodic Interest Amount";
        //            worksheet.Cells[7, 3].Value = "Periodic Principal Amount";
        //            worksheet.Cells[7, 4].Value = "Payment Date";
        //            worksheet.Cells[7, 5].Value = "Deferred Interest Amount";


        //            int rowNum = 8;
        //            //int colNum = 1;
        //            foreach (var item in model.scheduleList)
        //            {
        //                worksheet.Cells[rowNum, 1].Value = item.periodicPaymentAmount;
        //                worksheet.Cells[rowNum, 2].Value = item.periodInterestAmount;
        //                worksheet.Cells[rowNum, 3].Value = item.periodPrincipalAmount;
        //                worksheet.Cells[rowNum, 4].Value = item.paymentDate;
        //                worksheet.Cells[rowNum, 5].Value = item.deferredInterestAmount;

        //                //Format Money
        //                worksheet.Cells[rowNum, 1].Style.Numberformat.Format = numberformat;
        //                worksheet.Cells[rowNum, 2].Style.Numberformat.Format = numberformat;
        //                worksheet.Cells[rowNum, 3].Style.Numberformat.Format = numberformat;
        //                worksheet.Cells[rowNum, 5].Style.Numberformat.Format = numberformat;

        //                rowNum++;
        //            }

        //            package.Save(); //Save the workbook.
        //            //var bytes = System.IO.File.ReadAllBytes(URL);
        //            //var stream = File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        //            //return stream;
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { result = downloadUrl });
        //        //var fs = new FileStream(URL, FileMode.Open);
        //        //Byte[] fileByte = fs.WriteByte();


        //        //return Request.CreateResponse(HttpStatusCode.OK, new { result = URL });
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }


        //}
        #endregion

        #region (Loan Application Date) Pre - Loan booking
        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum")]
        public HttpResponseMessage GetCamProcessedLoanApplications()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetCamProcessedLoanApplications(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/collateral/customer/{customerId}")]
        public HttpResponseMessage GetCollateralCustomer(int customerId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repoCollateral.GetCustomerCollateral(customerId, token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/charge-fee/{chargeFeeId}/product/{productId}")]
        public HttpResponseMessage GetLoanProductChargeFee(int chargeFeeId, int productId)
        {
            try
            {
                var response = repo.GetLoanProductChargeFee(chargeFeeId, productId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-product-fees/{productId}")]
        public HttpResponseMessage GetProductFees(int productId)
        {
            try
            {
                var response = repo.GetProductFees(productId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

    }
}