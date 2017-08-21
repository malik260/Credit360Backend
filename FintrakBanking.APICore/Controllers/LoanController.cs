using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using System.Collections.Generic;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/loan")]
    public class LoanController : ApiControllerBase
    {
        private ILoanRepository repo;
        private ILoanScheduleRepository scheduleRepo;
        //private IHostingEnvironment _hostingEnvironment;
        public LoanController(ILoanRepository _repo, ILoanScheduleRepository _scheduleRepo
            //, IHostingEnvironment hostingEnvironment
            )
        {
            this.repo = _repo;
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

        [HttpGet][Route("loan-types")]
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

        [HttpGet][Route("loan-schedule-category")]
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

        [HttpGet][Route("loan-schedule-types")]
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


        [HttpGet][Route("loan-schedule-types/category/{categoryId}")]
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


        [HttpPost][Route("loan-booking")]
        public HttpResponseMessage AddLoanBooking([FromBody] LoanViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;


                var data = repo.AddLoanBooking(entity);
                if (data != "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet][Route("number-of-installments/tenor-mode/{tenorModeId}/frequency-type/{frequencyTypeId}/tenor/{tenor}")]
        public HttpResponseMessage GetNumberOfInstallments(short tenorModeId, short frequencyTypeId, int tenor)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = scheduleRepo.CalculateNumberOfInstallments((TenorModeEnum)tenorModeId, frequencyTypeId, tenor);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet][Route("{loanId}")]
        public HttpResponseMessage GetLoan(int loanId)
        {
            try
            {
                //TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetLoan(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet][Route("find/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.FindLoan(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost][Route("loan-search")]
        public HttpResponseMessage SearchLoan([FromBody] LoanSearchViewModel searchModel)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
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

        [HttpGet][Route("first-pay-date/effective-date/{effectiveDate}/frequency-type/{frequencyTypeId}")]
        public HttpResponseMessage GetFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            try
            {
                //TokenDecryptionHelper token = new TokenDecryptionHelper();

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


    }
}