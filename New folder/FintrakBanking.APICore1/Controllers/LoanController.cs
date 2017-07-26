using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.Interfaces.Customer;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using OfficeOpenXml;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FintrakBanking.Entities.SPModels;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Globalization;


namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/loan")]
    public class LoanController : BaseController
    {
        private ILoanRepository repo;
        private IHostingEnvironment _hostingEnvironment;
        public LoanController(ILoanRepository _repo,
                                IHostingEnvironment hostingEnvironment)
        {
            this.repo = _repo;
            this._hostingEnvironment = hostingEnvironment;
        }


        #region Loan 

        //[HttpGet("loan-payment-schedule")]
        //public IActionResult GenerateLoanPaymentSchedule([FromBody] LoanPaymentScheduleInput entity)
        //{
        //    try
        //    {
        //        var response = LoanPaymentSchedule.GenerateLoanPaymentSchedule(input);
        //        if (!response.Any())
        //        {
        //            return Ok(new { success = false, message = "No record found" });
        //        }

        //        return Ok(new { success = true, result = response, count = response.Count() });
        //    }
        //    catch (Exception e)
        //    {
        //        return Ok(new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

        [HttpGet("loan-types")]
        public IActionResult GetAllLoanTypes()
        {
            try
            {
                var response = repo.GetAllLoanTypes();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("loan-schedule-category")]
        public IActionResult GetAllLoanScheduleCategory()
        {
            try
            {
                var response = repo.GetAllLoanScheduleCategory();
                //if (!response.Any())
                //{
                //    return Ok(new { success = false, message = "No record found" });
                //}

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("loan-schedule-types")]
        public IActionResult GetAllLoanScheduleType()
        {
            try
            {
                var response = repo.GetAllLoanScheduleType();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet("loan-schedule-types/category/{categoryId}")]
        public IActionResult GetLoanScheduleTypeByCategory(short categoryId)
        {
            try
            {
                var response = repo.GetLoanScheduleTypeByCategory(categoryId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost("loan-booking")]
        public IActionResult AddLoanBooking([FromBody] LoanViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;


                var response = repo.AddLoanBooking(entity);
                if (response != "")
                {
                    return Ok(new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet("number-of-installments/tenor-mode/{tenorModeId}/frequency-type/{frequencyTypeId}/tenor/{tenor}")]
        public IActionResult GetNumberOfInstallments(short tenorModeId, short frequencyTypeId, int tenor)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.CalculateNumberOfInstallments((TenorModeEnum)tenorModeId, frequencyTypeId, tenor);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("{loanId}")]
        public IActionResult GetLoan(int loanId)
        {
            try
            {
                //var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetLoan(loanId);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("find/{searchCriteria}")]
        public IActionResult FindLoan(string searchCriteria)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.FindLoan(searchCriteria, token.GetCompanyId);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost("loan-search")]
        public IActionResult SearchLoan([FromBody] LoanSearchViewModel searchModel)
        {
            try
            {
                var tokenHelper = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.LoanSearch(tokenHelper.GetCompanyId, searchModel);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("first-pay-date/effective-date/{effectiveDate}/frequency-type/{frequencyTypeId}")]
        public IActionResult GetFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            try
            {
                //var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.CalculateFirstPayDate(effectiveDate, frequencyTypeId);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost("schedule")]
        public IActionResult GenerateLoanSchedule([FromBody] LoanPaymentScheduleInput input)
        {
            try
            {

                var response = repo.GenerateLoanSchedule(input);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response.Where(x => x.paymentNumber > 0).ToList() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost("schedule/export")]
        public IActionResult ExportScheduleToExcel([FromBody] PaymentScheduleExcelViewModel model)
        {
            try
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string sFileName = $"schedule{DateTime.Now.Ticks.ToString()}.xlsx";
                string _path = "docs\\" + sFileName;
                string URL = Path.Combine(sWebRootFolder, _path); //
                string downloadUrl = string.Format("{0}://{1}/{2}/{3}", Request.Scheme, Request.Host, "docs", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, _path));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, _path));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    var workSheetName = $"Schedule_{DateTime.Now.Ticks.ToString()}";
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(workSheetName);
                    //First add the headers

                    worksheet.Cells[1, 1].Value = "LOAN REPAYMENT SCHEDULE";
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[3, 1, 5, 4].Style.Font.Size = 12;

                    var numberformat = "#,##0";
                    var dataCellStyleName = "TableNumber";
                    var numStyle = package.Workbook.Styles.CreateNamedStyle(dataCellStyleName);
                    numStyle.Style.Numberformat.Format = numberformat;

                    var dateFormat = "dd/MM/yyyy";
                    var dataCellDateStyleName = "TableDate";
                    var dtStyle = package.Workbook.Styles.CreateNamedStyle(dataCellDateStyleName);
                    dtStyle.Style.Numberformat.Format = dateFormat;

                    worksheet.Cells[3, 1].Value = "Principal Amount";
                    worksheet.Cells[3, 2].Value = model.principalAmount;
                    worksheet.Cells[3, 2].Style.Numberformat.Format = numberformat;

                    worksheet.Cells[3, 3].Value = "Interest Rate";
                    worksheet.Cells[3, 4].Value = model.interestRate;
                    worksheet.Cells[3, 4].Style.Numberformat.Format = numberformat;

                    worksheet.Cells[4, 1].Value = "Payment Mode";
                    worksheet.Cells[4, 2].Value = model.tenorMode;
                    worksheet.Cells[4, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[4, 3].Value = "No of Repayment";
                    worksheet.Cells[4, 4].Value = model.numberOfPayments;
                    worksheet.Cells[4, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[5, 1].Value = "Loan Date";
                    worksheet.Cells[5, 2].Value = model.loanDate;
                    worksheet.Cells[5, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[5, 2].Style.Numberformat.Format = dateFormat;

                    worksheet.Cells[5, 3].Value = "First Repyment Date";
                    worksheet.Cells[5, 4].Value = model.firstPaymentDate;
                    worksheet.Cells[5, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[5, 4].Style.Numberformat.Format = dateFormat;

                    worksheet.Cells[7, 1].Value = "Periodic Payment Amount";
                    worksheet.Cells[7, 2].Value = "Periodic Interest Amount";
                    worksheet.Cells[7, 3].Value = "Periodic Principal Amount";
                    worksheet.Cells[7, 4].Value = "Payment Date";
                    worksheet.Cells[7, 5].Value = "Deferred Interest Amount";


                    int rowNum = 8;
                    //int colNum = 1;
                    foreach (var item in model.scheduleList)
                    {
                        worksheet.Cells[rowNum, 1].Value = item.periodicPaymentAmount;
                        worksheet.Cells[rowNum, 2].Value = item.periodInterestAmount;
                        worksheet.Cells[rowNum, 3].Value = item.periodPrincipalAmount;
                        worksheet.Cells[rowNum, 4].Value = item.paymentDate;
                        worksheet.Cells[rowNum, 5].Value = item.deferredInterestAmount;

                        //Format Money
                        worksheet.Cells[rowNum, 1].Style.Numberformat.Format = numberformat;
                        worksheet.Cells[rowNum, 2].Style.Numberformat.Format = numberformat;
                        worksheet.Cells[rowNum, 3].Style.Numberformat.Format = numberformat;
                        worksheet.Cells[rowNum, 5].Style.Numberformat.Format = numberformat;

                        rowNum++;
                    }

                    package.Save(); //Save the workbook.
                    //var bytes = System.IO.File.ReadAllBytes(URL);
                    //var stream = File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
                    //return stream;
                }

                return Ok(new { result = downloadUrl });
                //var fs = new FileStream(URL, FileMode.Open);
                //Byte[] fileByte = fs.WriteByte();


                //return Ok(new { result = URL });
            }
            catch (Exception ex)
            {

                throw;
            }


        }






        #endregion


    }
}