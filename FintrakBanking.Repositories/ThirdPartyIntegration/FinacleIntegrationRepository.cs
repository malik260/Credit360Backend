using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.Repositories.ThirdPartyIntegration
{
    public class FinacleIntegrationRepository : IFinacleIntegrationRepository
    {
        private FinTrakBankingStagingContext _stgCon;
        private FinTrakBankingContext _context;
        public FinacleIntegrationRepository(FinTrakBankingStagingContext stgCon, FinTrakBankingContext context)
        {
            _stgCon = stgCon;
            _context = context;
        }

        #region Batch Posting Report
        public List<BatchPostingViewModel> GetBatchPostingDetail(DateTime startDate, DateTime endDate, string searchItem)
        {
            if (searchItem != null)
                searchItem = searchItem.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_DETAILS
                    where DbFunctions.TruncateTime(x.RCRE_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.RCRE_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && (x.CR_ACCT.ToLower() == searchItem || x.DR_ACCT.ToLower() == searchItem || x.BATCH_ID.ToLower() == searchItem
                                                    || x.FLOW_TYPE.ToLower() == searchItem || searchItem.StartsWith(x.NARRATION.ToLower())
                                                    || x.TRAN_ID.ToLower() == searchItem || x.TRAN_TYPE.ToLower() == searchItem || searchItem == null || searchItem == "")
                    orderby x.PSTD_DATE descending
                    select new BatchPostingViewModel
                    {
                        sid = x.SID,
                        batchId = x.BATCH_ID,
                        batchRefId = x.BATCH_REF_ID,
                        trancType = x.TRAN_TYPE,
                        flowType = x.FLOW_TYPE,
                        amt = x.AMT,
                        drAccount = x.DR_ACCT,
                        crAccount = x.CR_ACCT,
                        currencyCode = x.REF_CRNCY_CODE,
                        rateCode = x.RATE_CODE,
                        rate = x.RATE,
                        naration = x.NARRATION,
                        status = x.STATUS,
                        tranactionId = x.TRAN_ID,
                        postedFlag = x.PSTD_FLG,
                        postedDate = x.PSTD_DATE,
                        rcreDate = x.RCRE_DATE,
                        postedUserId = x.PSTD_USR_ID,
                        failedFlag = x.FAIL_FLG,
                        deleteFlag = x.DEL_FLG,
                        failureReasonCode = x.FAILURE_REASON_CODE,
                        failureReason = x.FAILURE_REASON,
                        amountCollected = x.AMT_COLLECTED,
                        lienAmount = x.LIEN_AMT,
                        lienFlg = x.LIEN_FLG,
                        TodFlg = x.TOD_FLG,
                        valueDateNumber = x.VALUE_DATE_NUM,
                        loanAccount = x.LOAN_ACCT,
                        fintrakFlag = x.FINTRAK_FLG,
                        bankId = x.BANK_ID


                    }).ToList();
        }

        public List<BatchPostingViewModel> GetBatchPostingMain(DateTime startDate, DateTime endDate, string searchItem)
        {
            if (searchItem != null)
                searchItem = searchItem.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_MAIN
                    where DbFunctions.TruncateTime(x.RCRE_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.RCRE_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && (x.BATCH_ID.ToLower() == searchItem || searchItem == null || searchItem == "")
                    orderby x.PSTD_DATE descending
                    select new BatchPostingViewModel
                    {
                        sid = x.SID,
                        batchId = x.BATCH_ID,
                        trancType = x.TRAN_TYPE,
                        rcreDate = x.RCRE_DATE,
                        rcreUser = x.RCRE_USER,
                        totalAmount = x.TOTAL_AMT,
                        recCount = x.REC_COUNT,
                        status = x.STATUS,
                        postedDate = x.PSTD_DATE,
                        postedUserId = x.PSTD_USR_ID,
                        deleteFlag = x.DEL_FLG,
                        isSelected = x.IS_SELECTED,
                        bankId = x.BANK_ID,
                        totalAmountCollected = x.TOTAL_AMT_COLLECTED,
                        postedFlag = x.PSTD_FLG

                    }).ToList();
        }

        public List<BatchPostingViewModel> GetBatchPostingDetailSearch(DateTime startDate, DateTime endDate, string status)
        {
            if (status != null)
                status = status.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_DETAILS
                    where DbFunctions.TruncateTime(x.RCRE_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.RCRE_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && x.STATUS.ToLower() == status
                    orderby x.PSTD_DATE descending
                    select new BatchPostingViewModel
                    {
                        sid = x.SID,
                        batchId = x.BATCH_ID,
                        batchRefId = x.BATCH_REF_ID,
                        trancType = x.TRAN_TYPE,
                        flowType = x.FLOW_TYPE,
                        amt = x.AMT,
                        drAccount = x.DR_ACCT,
                        crAccount = x.CR_ACCT,
                        currencyCode = x.REF_CRNCY_CODE,
                        rateCode = x.RATE_CODE,
                        rate = x.RATE,
                        naration = x.NARRATION,
                        status = x.STATUS,
                        tranactionId = x.TRAN_ID,
                        postedFlag = x.PSTD_FLG,
                        postedDate = x.PSTD_DATE,
                        rcreDate = x.RCRE_DATE,
                        postedUserId = x.PSTD_USR_ID,
                        failedFlag = x.FAIL_FLG,
                        deleteFlag = x.DEL_FLG,
                        failureReasonCode = x.FAILURE_REASON_CODE,
                        failureReason = x.FAILURE_REASON,
                        amountCollected = x.AMT_COLLECTED,
                        lienAmount = x.LIEN_AMT,
                        lienFlg = x.LIEN_FLG,
                        TodFlg = x.TOD_FLG,
                        valueDateNumber = x.VALUE_DATE_NUM,
                        loanAccount = x.LOAN_ACCT,
                        fintrakFlag = x.FINTRAK_FLG,
                        bankId = x.BANK_ID


                    }).ToList();
        }

        private List<DailyAccrualDetailViewModel> GetDailyAccrualDetails(DateTime date, string loanAcct)
        {
            string[] searchValues = loanAcct.Split('/');
            if (searchValues.Count() < 5 )
                throw new Exception("Invalid Loan Account");

            string productCode = searchValues[0] == null ? "" : searchValues[0];
            string currencyCode = searchValues[1] == null ? "" : searchValues[1];
            string branchCode = searchValues[2] == null ? "" : searchValues[2];
            int companyId = searchValues[3] == null ? 0 : Convert.ToInt32(searchValues[3]);
            int categoryId = searchValues[4] == null ? 0 : Convert.ToInt32(searchValues[4]);

            var branchId = _context.TBL_BRANCH.Where(o => o.BRANCHCODE == branchCode).Select(o => o.BRANCHID).FirstOrDefault();
            var currencyId = _context.TBL_CURRENCY.Where(o => o.CURRENCYCODE == currencyCode).Select(o => o.CURRENCYID).FirstOrDefault();
            var productId = _context.TBL_PRODUCT.Where(o => o.PRODUCTCODE == productCode).Select(o => o.PRODUCTID).FirstOrDefault();


            var record = (from a in _context.TBL_DAILY_ACCRUAL
                          where a.BRANCHID == branchId
                          && a.PRODUCTID == productId
                          && a.COMPANYID == companyId
                          && a.CURRENCYID == currencyId
                          && a.CATEGORYID == categoryId
                          && a.DATE == date
                          select new DailyAccrualDetailViewModel
                          {
                              REFERENCENUMBER = a.REFERENCENUMBER,
                              BASEREFERENCENUMBER = a.BASEREFERENCENUMBER,
                              CATEGORY = _context.TBL_DAILY_ACCRUAL_CATEGORY.Where(o => o.CATEGORYID == a.CATEGORYID).Select(o => o.CATEGORYNAME).FirstOrDefault(),
                              TRANSACTIONTYPE = _context.TBL_LOAN_TRANSACTION_TYPE.Where(o => o.TRANSACTIONTYPEID == a.TRANSACTIONTYPEID).Select(o => o.TRANSACTIONTYPENAME).FirstOrDefault(),
                              PRODUCT = _context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                              BRANCH = _context.TBL_BRANCH.Where(o => o.BRANCHID == a.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                              CURRENCY = _context.TBL_CURRENCY.Where(o => o.CURRENCYID == a.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                              EXCHANGERATE = a.EXCHANGERATE,
                              MAINAMOUNT = a.MAINAMOUNT,
                              INTERESTRATE = a.INTERESTRATE,
                              DAILYACCURALAMOUNT = a.DAILYACCURALAMOUNT,
                              REPAYMENTPOSTEDSTATUS = a.REPAYMENTPOSTEDSTATUS == true ? "TRUE" : "FALSE",
                              SYSTEMDATETIME = a.SYSTEMDATETIME,
                              DATE = a.DATE,
                              CHARGEFEE = _context.TBL_CHARGE_FEE.Where(o => o.CHARGEFEEID == a.CHARGEFEEID).Select(o => o.CHARGEFEENAME).FirstOrDefault(),
                              DEMANDDATE = a.DEMANDDATE,
                              DAILYACCURALAMOUNT2 = a.DAILYACCURALAMOUNT2
                          }).ToList();

            return record;
        }

        public CRMSRecord GenerateExcell(DateTime date, string loanAcct)
        {

            Byte[] fileBytes = null;
            CRMSRecord outPut = new CRMSRecord();

            var data = GetDailyAccrualDetails(date, loanAcct);

            if (data != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Daily Accrual Detail");



                    using (var range = ws.Cells[1, 1, 1, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                        range.Style.Font.Color.SetColor(Color.White);
                    }
                    // ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    //ws.Cells[1, 1, 1, count].AutoFilter = true;

                    ws.Cells[1, 1].Value = "REFERENCENUMBER";
                    ws.Cells[1, 2].Value = "BASEREFERENCENUMBER";
                    ws.Cells[1, 3].Value = "CATEGORY";
                    ws.Cells[1, 4].Value = "TRANSACTIONTYPE";
                    ws.Cells[1, 5].Value = "PRODUCT";
                    ws.Cells[1, 6].Value = "BRANCH";
                    ws.Cells[1, 7].Value = "CURRENCY";
                    ws.Cells[1, 8].Value = "EXCHANGERATE";
                    ws.Cells[1, 9].Value = "MAINAMOUNT";
                    ws.Cells[1, 10].Value = "INTERESTRATE";
                    ws.Cells[1, 11].Value = "DAYCOUNTCONVENTION";
                    ws.Cells[1, 12].Value = "DAILYACCURALAMOUNT";
                    ws.Cells[1, 13].Value = "REPAYMENTPOSTEDSTATUS";
                    ws.Cells[1, 14].Value = "SYSTEMDATETIME";
                    ws.Cells[1, 15].Value = "DATE";
                    ws.Cells[1, 16].Value = "CHARGEFEE";
                    ws.Cells[1, 17].Value = "DEMANDDATE";
                    ws.Cells[1, 18].Value = "DAILYACCURALAMOUNT2";


                    for (int i = 2; i <= data.Count + 1; i++)
                    {

                        var record = data[i - 2];


                        ws.Cells[i, 1].Value = record.REFERENCENUMBER;
                        ws.Cells[i, 2].Value = record.BASEREFERENCENUMBER;
                        ws.Cells[i, 3].Value = record.CATEGORY;
                        ws.Cells[i, 4].Value = record.TRANSACTIONTYPE;
                        ws.Cells[i, 5].Value = record.PRODUCT;
                        ws.Cells[i, 6].Value = record.BRANCH;
                        ws.Cells[i, 7].Value = record.CURRENCY;
                        ws.Cells[i, 8].Value = record.EXCHANGERATE;
                        ws.Cells[i, 9].Value = record.MAINAMOUNT;
                        ws.Cells[i, 10].Value = record.INTERESTRATE; //temor
                        ws.Cells[i, 11].Value = record.DAYCOUNTCONVENTION;//EXPIRY_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 12].Value = record.DAILYACCURALAMOUNT;
                        ws.Cells[i, 13].Value = record.REPAYMENTPOSTEDSTATUS;
                        ws.Cells[i, 14].Value = record.SYSTEMDATETIME;
                        ws.Cells[i, 15].Value = record.DATE;
                        ws.Cells[i, 16].Value = record.CHARGEFEE;
                        ws.Cells[i, 17].Value = record.DEMANDDATE;
                        ws.Cells[i, 18].Value = record.DAILYACCURALAMOUNT2;

                    }
                    fileBytes = pck.GetAsByteArray();

                    outPut.reportData = fileBytes;
                    outPut.templateTypeName = "Daily Accrual Details";
                }


            }

            return outPut;
        }


        public CRMSRecord GetEODErrorLogDetail(FinanceEndofdayViewModel model)
        {
            var loanInput = (from a in _context.TBL_EOD_OPERATION_LOG_DETAIL
                             join b in _context.TBL_EOD_OPERATION_LOG on a.EODOPERATIONID equals b.EODOPERATIONID
                             join c in _context.TBL_EOD_STATUS on a.EODSTATUSID equals c.EODSTATUSID
                             join d in _context.TBL_EOD_OPERATION on b.EODOPERATIONID equals d.EODOPERATIONID
                             where a.EODDATE == b.EODDATE && a.EODOPERATIONLOGID == b.EODOPERATIONLOGID && a.EODDATE == model.eodDate && a.EODOPERATIONID == model.eodOperationId && b.COMPANYID == model.companyId
                             select new EODErrorLogDetailsViewModel
                             {
                                 eodOperationLogDetailId = a.EODOPERATIONLOGDETAILID,
                                 eodOperationLogId = b.EODOPERATIONLOGID,
                                 eodOperationName = d.EODOPERATIONNAME,
                                 startDateTime = (DateTime)a.STARTDATETIME,
                                 endDateTime = (DateTime)a.ENDDATETIME,
                                 eodStatusName = c.EODSTATUSNAME,
                                 errorInformation = a.ERRORINFORMATION,
                                 referenceNumber = a.REFERENCENUMBER,
                                 eodDate = (DateTime)a.EODDATE,
                                 eodUserName = _context.TBL_STAFF.Where(x => x.STAFFID == a.EODUSERID).Select(x => x.LASTNAME + " " + x.FIRSTNAME + " " + x.MIDDLENAME + "  (" + x.STAFFCODE + ")").FirstOrDefault(),

                             }).OrderBy(x => x.eodOperationLogDetailId).ToList();


            Byte[] fileBytes = null;
            CRMSRecord excel = new CRMSRecord();
            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Batch Posting Data");


                    ws.Cells[1, 1].Value = "EODOPERATIONLOGDETAILID";
                    ws.Cells[1, 2].Value = "EODOPERATIONLOGID";
                    ws.Cells[1, 8].Value = "EODOPERATIONNAME";
                    ws.Cells[1, 3].Value = "STARTDATETIME";
                    ws.Cells[1, 4].Value = "ENDDATETIME";
                    ws.Cells[1, 5].Value = "EODSTATUSNAME";
                    ws.Cells[1, 6].Value = "ERRORINFORMATION";
                    ws.Cells[1, 7].Value = "REFERENCENUMBER";
                    ws.Cells[1, 9].Value = "EODDATE";
                    ws.Cells[1, 10].Value = "EODUSERCODE";


                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];

                        ws.Cells[i, 1].Value = record.eodOperationLogDetailId;
                        ws.Cells[i, 2].Value = record.eodOperationLogId;
                        ws.Cells[i, 8].Value = record.eodOperationName;
                        ws.Cells[i, 3].Value = record.startDateTime;
                        ws.Cells[i, 4].Value = record.endDateTime;
                        ws.Cells[i, 5].Value = record.eodStatusName;
                        ws.Cells[i, 6].Value = record.errorInformation;
                        ws.Cells[i, 7].Value = record.referenceNumber;
                        ws.Cells[i, 9].Value = record.eodDate;
                        ws.Cells[i, 10].Value = record.eodUserName;

                    }
                    fileBytes = pck.GetAsByteArray();
                    excel.reportData = fileBytes;
                    excel.templateTypeName = "EOD Error Log Details";
                }
            }

            return excel;
        }



        #endregion

    }
}
