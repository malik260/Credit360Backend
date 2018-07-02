using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI
{
    public class BulkTransactionPosting
    {
        public List<ItemValue> GetFlowTypes()
        {
            List<ItemValue> values = new List<ItemValue>();
            values.Add(new ItemValue { valueCode = "BLF", valueName = "Late penal fee" });
            values.Add(new ItemValue { valueCode = "BPF", valueName = "Penal Fee" });
            values.Add(new ItemValue { valueCode = "BRF", valueName = "Running Fee" });
            values.Add(new ItemValue { valueCode = "BVF", valueName = "Vat Fee" });
            values.Add(new ItemValue { valueCode = "BOF", valueName = "Other Fee" });
            values.Add(new ItemValue { valueCode = "BAF", valueName = "Repayment Fee" });
            values.Add(new ItemValue { valueCode = "BIF", valueName = "Interest payment" });
            values.Add(new ItemValue { valueCode = "BPP", valueName = "Principal Payment" });

            return values;
        }

        public bool WriteBulkDailyTermLoanInterestAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                              IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {
                item.date = applicationDate;

                var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                count++;

                addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                addStaging.FLOWTYPE = "fff";
                addStaging.FORCEDEBITACCOUNT = "Y";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Loan Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = item.date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;
                addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;


                addStaging.SYSTEMDATETIME = item.date;
                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();
               
            }


            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

        private bool WriteBulkPostingToStagingSub(FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, DateTime applicationDate, string TransactionType, string batchCode)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            //    try
            //    {
            var data = (from a in context.TBL_CUSTOM_TRANSACTION_BULK
                        where a.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && a.BATCHID == batchCode
                        select new FinanceTransactionStagingViewModel()
                        {
                            batchId = a.BATCHID,
                            batchRefId = a.BATCHREFID,
                            transType = a.TRANSACTIONTYPE,
                            flowType = a.FLOWTYPE,
                            amount = a.AMOUNT,
                            debitGlAccount = a.DEBITACCOUNT,
                            creditGlAccount = a.CREDITACCOUNT,
                            currencyCode = a.CURRENCYCODE,
                            currencyRate = a.CURRENCYRATE,
                            currencyRateCode = a.CURRENCYRATECODE,
                            description = a.DESCRIPTION,
                            amountCollected = 0,
                            bankId = a.BANKID,
                            branchId = (short)a.DESTINATIONBRANCHID,
                            sourceReferenceNumber = a.SOURCEREFERENCENUMBER,

                            //sid = a.SID,


                        }).ToList();

            List<FINTRAK_TRAN_PROC_DETAILS> staging = new List<FINTRAK_TRAN_PROC_DETAILS>();


            foreach (var item in data)
            {
                FINTRAK_TRAN_PROC_DETAILS addStaging = new FINTRAK_TRAN_PROC_DETAILS();

                addStaging.BATCH_ID = item.batchId;
                addStaging.BATCH_REF_ID = item.batchRefId;
                addStaging.TRAN_TYPE = item.transType;
                addStaging.FLOW_TYPE = item.flowType;
                addStaging.AMT = item.amount;
                addStaging.CR_ACCT = item.creditGlAccount;
                addStaging.DR_ACCT = item.debitGlAccount;
                addStaging.RATE_CODE = item.currencyRateCode;
                addStaging.REF_CRNCY_CODE = item.currencyCode;
                addStaging.RATE = (decimal)item.currencyRate;
                addStaging.NARRATION = item.description;
                addStaging.AMT_COLLECTED = item.amountCollected;
                addStaging.BANK_ID = item.bankId;
                addStaging.TOD_FLG = "N";
                addStaging.LOAN_ACCT = item.sourceReferenceNumber;
                addStaging.STATUS = "NEW";
                addStaging.RCRE_DATE = applicationDate;
                addStaging.PSTD_FLG = "N";
                addStaging.PSTD_DATE = applicationDate;
                addStaging.DEL_FLG = "N";
                addStaging.FAIL_FLG = "N";

                staging.Add(addStaging);

            }

            stagingContext.FINTRAK_TRAN_PROC_DETAILS.AddRange(staging);
            stagingContext.SaveChanges();

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.WriteToStagingTable,
            //    STAFFID = (int)SystemStaff.System,//model.createdBy,
            //    BRANCHID = data.FirstOrDefault().branchId,
            //    DETAIL = $"Write to Staging: {data.FirstOrDefault().sourceReferenceNumber}",
            //    IPADDRESS = data.FirstOrDefault().userIPAddress,
            //    URL = data.FirstOrDefault().applicationUrl,
            //    APPLICATIONDATE = applicationDate,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);


            var model = (from a in context.TBL_CUSTOM_TRANSACTION_BULK
                         where a.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && a.BATCHID == batchCode
                         group a by new { a.BATCHID } into groupedQ
                         select new FinanceTransactionStagingViewModel()
                         {
                             batchId = groupedQ.Key.BATCHID,
                             amount = groupedQ.Sum(i => i.AMOUNT),
                         }).ToList();

            List<FINTRAK_TRAN_PROC_MAIN> main = new List<FINTRAK_TRAN_PROC_MAIN>();
            var recordCount = context.TBL_CUSTOM_TRANSACTION_BULK.Where(x => x.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && x.BATCHID == batchCode).Count();
            foreach (var item in model)
            {
                FINTRAK_TRAN_PROC_MAIN addMain = new FINTRAK_TRAN_PROC_MAIN();

                addMain.BATCH_ID = item.batchId;
                addMain.RCRE_DATE = applicationDate;
                addMain.TRAN_TYPE = TransactionType;
                addMain.RCRE_USER = "SYSTEM";
                addMain.TOTAL_AMT = item.amount;
                addMain.STATUS = "NEW";
                addMain.REC_COUNT = recordCount;
                addMain.BANK_ID = "01";
                addMain.IS_SELECTED = "N";
                addMain.PSTD_DATE = applicationDate;
                addMain.PSTD_FLG = "N";
                addMain.DEL_FLG = "N";

                //addMain.SID = 1;


                main.Add(addMain);

            }

            stagingContext.FINTRAK_TRAN_PROC_MAIN.AddRange(main);

            var result = stagingContext.SaveChanges() > 0;
            if (result)
            {
                //trans.Commit();
                output = true;
            }
            //output = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        trans.Rollback();
            //        output = false;

            //    }
            //} 
            return output;
        }

    }
}
