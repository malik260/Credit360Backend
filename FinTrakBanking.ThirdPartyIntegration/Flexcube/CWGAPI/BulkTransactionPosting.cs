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
using System.Diagnostics;
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
            values.Add(new ItemValue { valueCode = "FFF", valueName = "Interest Accrual Posting" });

            return values;
        }

        public List<ItemValue> GetTransactionTypes()
        {
            List<ItemValue> values = new List<ItemValue>();
            values.Add(new ItemValue { valueCode = "BL", valueName = "Batch Lien" });
            values.Add(new ItemValue { valueCode = "BP", valueName = "Batch Posting" });
            values.Add(new ItemValue { valueCode = "NT", valueName = "New transaction" });
            return values;
        }


        //public bool WriteBulkContingentLiabilityTerminationAtMaturityToStaging(List<FinanceTransactionViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
        //                                     IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        //{
        //    var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
        //    int count = 0;
        //    foreach (var item in model)
        //    {
        //        item.date = applicationDate;

        //        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

        //        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

        //        count++;

        //        addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
        //        addStaging.FLOWTYPE = "FFF";
        //        addStaging.FORCEDEBITACCOUNT = "Y";
        //        addStaging.VALUEDATENUMBER = 1;
        //        addStaging.BATCHID = batchCode;
        //        addStaging.BATCHREFID = count;
        //        addStaging.SID = count;
        //        addStaging.COMPANYID = item.companyId;
        //        addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
        //        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
        //        addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
        //        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
        //        addStaging.DESCRIPTION = "Loan Daily Interest Accrual Posting";
        //        addStaging.DESTINATIONBRANCHID = item.branchId;
        //        addStaging.ISPOSTED = false;
        //        addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
        //        addStaging.POSTEDBY = "SYSTEM";
        //        addStaging.POSTEDDATE = item.date;
        //        addStaging.SOURCEBRANCHID = item.branchId;
        //        addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
        //        addStaging.VALUEDATE = item.date;
        //        addStaging.TRANSACTIONTYPE = "BP";
        //        addStaging.BANKID = "01";
        //        addStaging.PRODUCTID = product.PRODUCTID;
        //        addStaging.CURRENCYID = item.currencyId;
        //        addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
        //        addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
        //        addStaging.CREDITCASAACCOUNTID = null;
        //        addStaging.DEBITCASAACCOUNTID = null;
        //        addStaging.LOANID = null;
        //        addStaging.SYSTEMDATETIME = item.date;
        //        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
        //        context.SaveChanges();

        //    }
        //    return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        //}

        public bool WriteBulkContingentLiabilityTerminationAtMaturityToStaging(List<TBL_LOAN_CONTINGENT> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                               IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {
                //item.date = applicationDate;

                var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.PRODUCTID);

                count++;

                addStaging.AMOUNT = (decimal)item.CONTINGENTAMOUNT;
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.COMPANYID;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.CURRENCYID, item.BRANCHID);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.CURRENCYID).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.CURRENCYID, item.COMPANYID).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL2.Value, item.CURRENCYID, item.BRANCHID);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Contingent Liability Amount at Maturity Reversal";
                addStaging.DESTINATIONBRANCHID = item.BRANCHID;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.ContingentLiabilityTermination;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.BRANCHID;
                addStaging.SOURCEREFERENCENUMBER = item.LOANREFERENCENUMBER;
                addStaging.VALUEDATE = applicationDate;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.CURRENCYID;
                addStaging.CREDITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                addStaging.DEBITGLACCOUNTID = product.PRINCIPALBALANCEGL2.Value;
                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = item.CONTINGENTLOANID;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);

                context.SaveChanges();

            }

            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }


        public bool WriteBulkDailyCompleteWriteOffInterestAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
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
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = 1; // financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Complete Write-Off Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.CompleteWriteOff;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = item.referenceNumber; // groupedQ.Key.PRODUCTCODE + '/' + groupedQ.Key.CURRENCYCODE + '/' + groupedQ.Key.BRANCHCODE + '/' + groupedQ.Key.COMPANYID.ToString();//product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;
                addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

        public bool WriteBulkDailyTermLoanInterestAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                              IFinanceTransactionRepository financeTransaction, DateTime applicationDate, out string transactionReferenceNo, int companyId, int staffId)
        {


            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (DailyInterestAccrualViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.referenceNumber;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual;
                        eod_operation_Detail.EODDATE = applicationDate;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }



            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {

                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();

                    try
                    {


                        transactionReferenceNo = item.referenceNumber;

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        item.date = applicationDate;

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                        var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;

                        count++;

                        ////if (item.referenceNumber == "0063/NGN/406/1/1")
                        ////{
                        ////    var me = "";
                        ////}


                        addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "N";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, companyCurrencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        /*addStaging.CURRENCYRATE = 1; */// financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                        addStaging.CURRENCYRATE = item.exchangeRate;
                        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                        addStaging.DESCRIPTION = "Loan Daily Interest Accrual Posting";
                        addStaging.DESTINATIONBRANCHID = item.branchId;
                        addStaging.ISPOSTED = false;
                        addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                        addStaging.POSTEDBY = "SYSTEM";
                        addStaging.POSTEDDATE = DateTime.Now.Date;
                        addStaging.SOURCEBRANCHID = item.branchId;
                        addStaging.SOURCEREFERENCENUMBER = item.referenceNumber; // groupedQ.Key.PRODUCTCODE + '/' + groupedQ.Key.CURRENCYCODE + '/' + groupedQ.Key.BRANCHCODE + '/' + groupedQ.Key.COMPANYID.ToString();//product.PRODUCTCODE;
                        addStaging.VALUEDATE = item.date;
                        addStaging.TRANSACTIONTYPE = "BP";
                        addStaging.BANKID = "01";
                        addStaging.PRODUCTID = product.PRODUCTID;
                        addStaging.CURRENCYID = item.currencyId;
                        addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                        addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                        addStaging.CREDITCASAACCOUNTID = null;
                        addStaging.DEBITCASAACCOUNTID = null;
                        addStaging.LOANID = null;
                        addStaging.SYSTEMDATETIME = DateTime.Now;
                        if (item.currencyId != companyCurrencyId)
                            addStaging.CURRENCYRATECODE = "TTB";

                        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.referenceNumber} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }


                }


            }

            return true;

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
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = 1; // financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Loan Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = item.referenceNumber; // groupedQ.Key.PRODUCTCODE + '/' + groupedQ.Key.CURRENCYCODE + '/' + groupedQ.Key.BRANCHCODE + '/' + groupedQ.Key.COMPANYID.ToString();//product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;
                addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }


        public bool WriteBulkDailyWriteOffInterestAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                             IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {
                item.date = applicationDate;

                var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                var accounts = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (int)OtherOperationEnum.InterestOffBalansheetCompleteWriteOffAccount).FirstOrDefault();
                
                count++;

                addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(accounts.GLACCOUNTID2.Value, item.currencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = 1; // financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(accounts.GLACCOUNTID, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Write-off Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyWriteoffInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = item.referenceNumber; // groupedQ.Key.PRODUCTCODE + '/' + groupedQ.Key.CURRENCYCODE + '/' + groupedQ.Key.BRANCHCODE + '/' + groupedQ.Key.COMPANYID.ToString();//product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = item.productId;
                addStaging.CURRENCYID = item.currencyId;
                addStaging.CREDITGLACCOUNTID = accounts.GLACCOUNTID2.Value;
                addStaging.DEBITGLACCOUNTID = accounts.GLACCOUNTID;
                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }


        public bool WriteBulkDailyFeeAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                            IFinanceTransactionRepository financeTransaction, DateTime applicationDate, int staffId)
        {


            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == applicationDate).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (DailyInterestAccrualViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyFeeAccrual).FirstOrDefault();

                    if (checkExistence != null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.referenceNumber;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessDailyFeeAccrual;
                        eod_operation_Detail.EODDATE = applicationDate;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);
                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }

            int count = 0;
            var operationLogDetail = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == applicationDate && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyFeeAccrual).Select(d=>d.REFERENCENUMBER).ToList();
            List<DailyInterestAccrualViewModel> modelList = new List<DailyInterestAccrualViewModel>();
            foreach(var i in model)
            {
                string referenceNumber = i.referenceNumber;
                if (operationLogDetail.Contains(referenceNumber)) { modelList.Add(i); }
            }

            
            foreach (var item in modelList)
            {
               if(modelList.IndexOf(item) >= 574)
                {
                    var b = modelList.IndexOf(item);
                }

                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyFeeAccrual).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyFeeAccrual).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();
                    FinTrakBankingContext newContext = new FinTrakBankingContext();


                    try
                    {

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);


                        item.date = applicationDate;

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                        var chardedFeeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == item.chargedFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Primary && x.REQUIREAMORTISATION == true).FirstOrDefault();
                        count++;

                        addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "N";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;

                        if (chardedFeeDetails != null)
                        {
                            //addStaging.CREDITACCOUNT = context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            //addStaging.DEBITACCOUNT =  context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;

                            var glAccountId2 = chardedFeeDetails?.GLACCOUNTID2 ?? 0;
                            var glAccountId1 = chardedFeeDetails?.GLACCOUNTID1 ?? 0;

                            addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(glAccountId2, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(glAccountId2, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;


                            addStaging.DESCRIPTION = "Fee Daily Accrual Posting";
                            addStaging.DESTINATIONBRANCHID = item.branchId;
                            addStaging.ISPOSTED = false;
                            addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                            addStaging.POSTEDBY = "SYSTEM";
                            addStaging.POSTEDDATE = DateTime.Now.Date;
                            addStaging.SOURCEBRANCHID = item.branchId;
                            addStaging.SOURCEREFERENCENUMBER = item.referenceNumber;
                            addStaging.VALUEDATE = item.date;
                            addStaging.TRANSACTIONTYPE = "BP";
                            addStaging.BANKID = "01";
                            addStaging.PRODUCTID = product.PRODUCTID;
                            addStaging.CURRENCYID = item.currencyId;


                            //addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                            //addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;

                            addStaging.CREDITGLACCOUNTID = chardedFeeDetails.GLACCOUNTID2.Value;
                            addStaging.DEBITGLACCOUNTID = chardedFeeDetails.GLACCOUNTID1.Value;


                            addStaging.CREDITCASAACCOUNTID = null;
                            addStaging.DEBITCASAACCOUNTID = null;
                            addStaging.LOANID = null;
                            addStaging.SYSTEMDATETIME = DateTime.Now;


                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                            context.SaveChanges();

                            WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);
                        }

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";

                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        // Get stack trace for the exception with source file information
                        var st = new StackTrace(ex, true);
                        // Get the top stack frame
                        var frame = st.GetFrame(0);
                        // Get the line number from the stack frame
                        var line = frame.GetFileLineNumber();

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.referenceNumber} Exception - {ex.Message} exception -  {ex.InnerException}. @ Line {line}" ;
                        context.SaveChanges();
                    }

                }

            }

            //REMOVE THIS CODE SNIPPET WHEN EOD LOGIC IS FULLY RESOLVED.
            var pending = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c =>  c.EODDATE == applicationDate && c.EODSTATUSID == (int)EodOperationStatusEnum.Processing && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyFeeAccrual);
            if(pending.Count() > 0)
            {
                foreach (var i in pending)
                {
                    i.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                    i.ERRORINFORMATION = "No error. No fee defined.";
                }
                context.SaveChanges();
            }
           

            return true;
        }




        public bool WriteBulkDailyFeeAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                            IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {
                item.date = applicationDate;

                var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                var chardedFeeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == item.chargedFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Primary && x.REQUIREAMORTISATION == true).FirstOrDefault();

                count++;

                addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;


                //addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;

                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(chardedFeeDetails.GLACCOUNTID2.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(chardedFeeDetails.GLACCOUNTID1.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;


                addStaging.DESCRIPTION = "Fee Daily Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;


                //addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                //addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;

                addStaging.CREDITGLACCOUNTID = chardedFeeDetails.GLACCOUNTID2.Value;
                addStaging.DEBITGLACCOUNTID = chardedFeeDetails.GLACCOUNTID1.Value;


                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

        public bool WriteBulkDailyTaxAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                    IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {
                item.date = applicationDate;

                var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                var chardedFeeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == item.chargedFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax && x.REQUIREAMORTISATION == true).FirstOrDefault();


                count++;

                addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;

                //addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;


                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(chardedFeeDetails.GLACCOUNTID2.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(chardedFeeDetails.GLACCOUNTID1.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;


                addStaging.DESCRIPTION = "Tax Daily Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;


                //addStaging.CREDITGLACCOUNTID = product.INTERESTINCOMEEXPENSEGL.Value;
                //addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;

                addStaging.CREDITGLACCOUNTID = chardedFeeDetails.GLACCOUNTID2.Value;
                addStaging.DEBITGLACCOUNTID = chardedFeeDetails.GLACCOUNTID1.Value;


                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

        public bool WriteBulkDailyAuthorisedOverdraftInterestAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
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
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "Y";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Authorised Overdraft Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
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
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

        public bool WriteBulkDailyUnauthorisedOverdraftInterestAccuralToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
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
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "Y";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Unauthorised Overdraft Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
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
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }


        public bool WriteBulkDailyPastDueInterestAccrualToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                         IFinanceTransactionRepository financeTransaction, DateTime applicationDate, string description, int companyId, int staffId, out string transactionReferenceNo)
        {

            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (DailyInterestAccrualViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.referenceNumber;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual;
                        eod_operation_Detail.EODDATE = applicationDate;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }


            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {


                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();


                    try
                    {


                        transactionReferenceNo = item.referenceNumber;

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        item.date = applicationDate;

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                        var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;

                        count++;

                        addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "N";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.CURRENCYRATE = item.exchangeRate;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.PENALCHARGEGL.Value, companyCurrencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        //addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                        addStaging.DESCRIPTION = description;
                        addStaging.DESTINATIONBRANCHID = item.branchId;
                        addStaging.ISPOSTED = false;
                        addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                        addStaging.POSTEDBY = "SYSTEM";
                        addStaging.POSTEDDATE = DateTime.Now.Date;
                        addStaging.SOURCEBRANCHID = item.branchId;
                        addStaging.SOURCEREFERENCENUMBER = item.referenceNumber;//product.PRODUCTCODE;
                        addStaging.VALUEDATE = item.date;
                        addStaging.TRANSACTIONTYPE = "BP";
                        addStaging.BANKID = "01";
                        addStaging.PRODUCTID = product.PRODUCTID;
                        addStaging.CURRENCYID = item.currencyId;
                        addStaging.CREDITGLACCOUNTID = product.PENALCHARGEGL.Value;
                        addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                        addStaging.CREDITCASAACCOUNTID = null;
                        addStaging.DEBITCASAACCOUNTID = null;
                        addStaging.LOANID = null;
                        addStaging.SYSTEMDATETIME = DateTime.Now;


                        if (item.currencyId != companyCurrencyId)
                            addStaging.CURRENCYRATECODE = "TTB";


                        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.referenceNumber} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }


                }

            }
            return true;

        }


        public bool WriteBulkDailyPastDueInterestAccrualToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                         IFinanceTransactionRepository financeTransaction, DateTime applicationDate, string description)
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
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.PENALCHARGEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = description;
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = item.referenceNumber;//product.PRODUCTCODE;
                addStaging.VALUEDATE = item.date;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;
                addStaging.CREDITGLACCOUNTID = product.PENALCHARGEGL.Value;
                addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                addStaging.CREDITCASAACCOUNTID = null;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }


        public bool WriteBulkDailyPastDuePrincipalAccrualToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                   IFinanceTransactionRepository financeTransaction, DateTime applicationDate, string description, int companyId, int staffId, out string transactionReferenceNo)
        {

            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (DailyInterestAccrualViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.referenceNumber;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual;
                        eod_operation_Detail.EODDATE = applicationDate;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }


            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {


                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();


                    try
                    {


                        transactionReferenceNo = item.referenceNumber;

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        item.date = applicationDate;

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                        var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;

                        count++;

                        addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "N";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.PENALCHARGEGL.Value, companyCurrencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        addStaging.CURRENCYRATE = item.exchangeRate;
                        //addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                        addStaging.DESCRIPTION = description;
                        addStaging.DESTINATIONBRANCHID = item.branchId;
                        addStaging.ISPOSTED = false;
                        addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                        addStaging.POSTEDBY = "SYSTEM";
                        addStaging.POSTEDDATE = DateTime.Now.Date;
                        addStaging.SOURCEBRANCHID = item.branchId;
                        addStaging.SOURCEREFERENCENUMBER = item.referenceNumber;//product.PRODUCTCODE;
                        addStaging.VALUEDATE = item.date;
                        addStaging.TRANSACTIONTYPE = "BP";
                        addStaging.BANKID = "01";
                        addStaging.PRODUCTID = product.PRODUCTID;
                        addStaging.CURRENCYID = item.currencyId;
                        addStaging.CREDITGLACCOUNTID = product.PENALCHARGEGL.Value;
                        addStaging.DEBITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                        addStaging.CREDITCASAACCOUNTID = null;
                        addStaging.DEBITCASAACCOUNTID = null;
                        addStaging.LOANID = null;
                        addStaging.SYSTEMDATETIME = DateTime.Now;


                        if (item.currencyId != companyCurrencyId)
                            addStaging.CURRENCYRATECODE = "TTB";


                        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.referenceNumber} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }


                }

            }
            return true;

        }

        public bool WriteBulkDailyPastDuePrincipalAccrualToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
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
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "N";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Past Due Daily Interest Accrual Posting";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
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
                addStaging.LOANID = null;
                addStaging.SYSTEMDATETIME = DateTime.Now;


                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }


            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }


        public bool WriteBulkLoanRepaymentPostingPastDueToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
             IFinanceTransactionRepository financeTransaction, DateTime applicationDate, int companyId, int staffId, out string transactionReferenceNo)
        {

            DateTime dateChange = applicationDate.AddDays(-1);

            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (LoanRepaymentViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.loanRefNo;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue;
                        eod_operation_Detail.EODDATE = dateChange;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }



            transactionReferenceNo = "";
            int count = 0;

            foreach (var item in model)
            {

                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();


                    try
                    {

                        transactionReferenceNo = item.loanRefNo;

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        var addStagingInterest = new TBL_CUSTOM_TRANSACTION_BULK();
                        var addStagingPrincipal = new TBL_CUSTOM_TRANSACTION_BULK();
                        var addStagingPastDueInterest = new TBL_CUSTOM_TRANSACTION_BULK();
                        var addStagingPastDuePrincipal = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                        //var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);          

                        TBL_CASA casa;

                        if (product.PRODUCTCLASSID != (short)ProductClassEnum.InvoiceDiscountingFacility)
                        {
                            //casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);
                            casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2 && x.COMPANYID == item.companyId);
                        }
                        else
                        {
                            casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2.Value && x.COMPANYID == item.companyId);
                        }


                        if (item.interestOnPastDueInterest > 0)
                        {
                            count++;

                            addStagingPastDueInterest.AMOUNT = (decimal)Math.Abs(item.interestOnPastDueInterest);
                            addStagingPastDueInterest.FLOWTYPE = "BPF";
                            addStagingPastDueInterest.FORCEDEBITACCOUNT = "N";
                            addStagingPastDueInterest.VALUEDATENUMBER = 1;
                            addStagingPastDueInterest.BATCHID = batchCode;
                            addStagingPastDueInterest.BATCHREFID = count;
                            addStagingPastDueInterest.SID = count;
                            addStagingPastDueInterest.COMPANYID = item.companyId;
                            addStagingPastDueInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStagingPastDueInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                            //addStagingPastDueInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                            addStagingPastDueInterest.CURRENCYRATE = item.exchangeRate;
                            addStagingPastDueInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                            addStagingPastDueInterest.DESCRIPTION = "Interest on Past Due Interest";
                            addStagingPastDueInterest.DESTINATIONBRANCHID = item.branchId;
                            addStagingPastDueInterest.ISPOSTED = false;
                            addStagingPastDueInterest.OPERATIONID = (int)OperationsEnum.InterestOnPastDueInterest;
                            addStagingPastDueInterest.POSTEDBY = "SYSTEM";
                            addStagingPastDueInterest.POSTEDDATE = DateTime.Now.Date; ;
                            addStagingPastDueInterest.SOURCEBRANCHID = item.branchId;
                            addStagingPastDueInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                            addStagingPastDueInterest.VALUEDATE = applicationDate;
                            addStagingPastDueInterest.TRANSACTIONTYPE = "BL";
                            addStagingPastDueInterest.BANKID = "01";
                            addStagingPastDueInterest.PRODUCTID = product.PRODUCTID;
                            addStagingPastDueInterest.CURRENCYID = item.currencyId;
                            addStagingPastDueInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingPastDueInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingPastDueInterest.CREDITCASAACCOUNTID = null;
                            addStagingPastDueInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                            addStagingPastDueInterest.LOANID = item.loanId;
                            addStagingPastDueInterest.SYSTEMDATETIME = DateTime.Now;

                            var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                            if (item.currencyId != companyCurrencyId)
                                addStagingPastDueInterest.CURRENCYRATECODE = "TTB";

                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDueInterest);

                            var pastDueInterestDaily = (from a in context.TBL_DAILY_ACCRUAL
                                                        where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                                        && a.CATEGORYID == (short)DailyAccrualCategory.PastDueInterest && a.REPAYMENTPOSTEDSTATUS == false
                                                        && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                                        select a);


                            foreach (var itemDaily in pastDueInterestDaily)
                            {
                                itemDaily.REPAYMENTPOSTEDSTATUS = true;
                                itemDaily.DEMANDDATE = applicationDate;
                            }
                        }


                        if (item.interestOnPastDuePrincipal > 0)
                        {
                            count++;

                            addStagingPastDuePrincipal.AMOUNT = (decimal)Math.Abs(item.interestOnPastDuePrincipal);
                            addStagingPastDuePrincipal.FLOWTYPE = "BPF";
                            addStagingPastDuePrincipal.FORCEDEBITACCOUNT = "N";
                            addStagingPastDuePrincipal.VALUEDATENUMBER = 1;
                            addStagingPastDuePrincipal.BATCHID = batchCode;
                            addStagingPastDuePrincipal.BATCHREFID = count;
                            addStagingPastDuePrincipal.SID = count;
                            addStagingPastDuePrincipal.COMPANYID = item.companyId;
                            addStagingPastDuePrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStagingPastDuePrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                            //addStagingPastDuePrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                            addStagingPastDueInterest.CURRENCYRATE = item.exchangeRate;
                            addStagingPastDuePrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                            addStagingPastDuePrincipal.DESCRIPTION = "Interest on Past Due Principal";
                            addStagingPastDuePrincipal.DESTINATIONBRANCHID = item.branchId;
                            addStagingPastDuePrincipal.ISPOSTED = false;
                            addStagingPastDuePrincipal.OPERATIONID = (int)OperationsEnum.InterestOnPastDuePrincipal;
                            addStagingPastDuePrincipal.POSTEDBY = "SYSTEM";
                            addStagingPastDuePrincipal.POSTEDDATE = DateTime.Now.Date; ;
                            addStagingPastDuePrincipal.SOURCEBRANCHID = item.branchId;
                            addStagingPastDuePrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                            addStagingPastDuePrincipal.VALUEDATE = applicationDate;
                            addStagingPastDuePrincipal.TRANSACTIONTYPE = "BL";
                            addStagingPastDuePrincipal.BANKID = "01";
                            addStagingPastDuePrincipal.PRODUCTID = product.PRODUCTID;
                            addStagingPastDuePrincipal.CURRENCYID = item.currencyId;
                            addStagingPastDuePrincipal.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingPastDuePrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingPastDuePrincipal.CREDITCASAACCOUNTID = null;
                            addStagingPastDuePrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                            addStagingPastDuePrincipal.LOANID = item.loanId;
                            addStagingPastDuePrincipal.SYSTEMDATETIME = DateTime.Now;

                            var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                            if (item.currencyId != companyCurrencyId)
                                addStagingPastDuePrincipal.CURRENCYRATECODE = "TTB";


                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDuePrincipal);

                            var pastDuePrincipalDaily = (from a in context.TBL_DAILY_ACCRUAL
                                                         where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                                         && a.CATEGORYID == (short)DailyAccrualCategory.PastDuePrincipal && a.REPAYMENTPOSTEDSTATUS == false
                                                         && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                                         select a);

                            foreach (var itemDaily in pastDuePrincipalDaily)
                            {
                                itemDaily.REPAYMENTPOSTEDSTATUS = true;
                                itemDaily.DEMANDDATE = applicationDate;
                            }
                        }



                        if ((decimal)item.periodInterestAmount > 0)
                        {
                            count++;

                            addStagingInterest.AMOUNT = (decimal)Math.Abs(item.periodInterestAmount);
                            addStagingInterest.FLOWTYPE = "BIF";
                            addStagingInterest.FORCEDEBITACCOUNT = "N";
                            addStagingInterest.VALUEDATENUMBER = 1;
                            addStagingInterest.BATCHID = batchCode;
                            addStagingInterest.BATCHREFID = count;
                            addStagingInterest.SID = count;
                            addStagingInterest.COMPANYID = item.companyId;
                            addStagingInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStagingInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                            //addStagingInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                            addStagingPastDueInterest.CURRENCYRATE = item.exchangeRate;
                            addStagingInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                            addStagingInterest.DESCRIPTION = "Interest Repayment";
                            addStagingInterest.DESTINATIONBRANCHID = item.branchId;
                            addStagingInterest.ISPOSTED = false;
                            addStagingInterest.OPERATIONID = (int)OperationsEnum.InterestLoanRepayment;
                            addStagingInterest.POSTEDBY = "SYSTEM";
                            addStagingInterest.POSTEDDATE = DateTime.Now.Date;
                            addStagingInterest.SOURCEBRANCHID = item.branchId;
                            addStagingInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                            addStagingInterest.VALUEDATE = applicationDate;
                            addStagingInterest.TRANSACTIONTYPE = "BL";
                            addStagingInterest.BANKID = "01";
                            addStagingInterest.PRODUCTID = product.PRODUCTID;
                            addStagingInterest.CURRENCYID = item.currencyId;
                            addStagingInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingInterest.CREDITCASAACCOUNTID = null;
                            addStagingInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                            addStagingInterest.LOANID = item.loanId;
                            addStagingInterest.SYSTEMDATETIME = DateTime.Now;

                            var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                            if (item.currencyId != companyCurrencyId)
                                addStagingInterest.CURRENCYRATECODE = "TTB";

                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingInterest);

                            var interestDaily = (from a in context.TBL_DAILY_ACCRUAL
                                                 where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                                 && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan && a.REPAYMENTPOSTEDSTATUS == false
                                                 && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                                 select a);

                            foreach (var itemDaily in interestDaily)
                            {
                                itemDaily.REPAYMENTPOSTEDSTATUS = true;
                                itemDaily.DEMANDDATE = applicationDate;
                            }
                        }


                        if ((decimal)item.periodPrincipalAmount > 0)
                        {
                            count++;

                            addStagingPrincipal.AMOUNT = (decimal)Math.Abs(item.periodPrincipalAmount);
                            addStagingPrincipal.FLOWTYPE = "BPP";
                            addStagingPrincipal.FORCEDEBITACCOUNT = "N";
                            addStagingPrincipal.VALUEDATENUMBER = 1;
                            addStagingPrincipal.BATCHID = batchCode;
                            addStagingPrincipal.BATCHREFID = count;
                            addStagingPrincipal.SID = count;
                            addStagingPrincipal.COMPANYID = item.companyId;
                            addStagingPrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStagingPrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                            //addStagingPrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                            addStagingPastDueInterest.CURRENCYRATE = item.exchangeRate;
                            addStagingPrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                            addStagingPrincipal.DESCRIPTION = "Principal Repayment";
                            addStagingPrincipal.DESTINATIONBRANCHID = item.branchId;
                            addStagingPrincipal.ISPOSTED = false;
                            addStagingPrincipal.OPERATIONID = (int)OperationsEnum.PrincipalLoanRepayment;//change to periodPrincipalAmount
                            addStagingPrincipal.POSTEDBY = "SYSTEM";
                            addStagingPrincipal.POSTEDDATE = DateTime.Now.Date;
                            addStagingPrincipal.SOURCEBRANCHID = item.branchId;
                            addStagingPrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                            addStagingPrincipal.VALUEDATE = applicationDate;
                            addStagingPrincipal.TRANSACTIONTYPE = "BL";
                            addStagingPrincipal.BANKID = "01";
                            addStagingPrincipal.PRODUCTID = product.PRODUCTID;
                            addStagingPrincipal.CURRENCYID = item.currencyId;
                            addStagingPrincipal.CREDITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                            addStagingPrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;// product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingPrincipal.CREDITCASAACCOUNTID = null;
                            addStagingPrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                            addStagingPrincipal.LOANID = item.loanId;
                            addStagingPrincipal.SYSTEMDATETIME = DateTime.Now;

                            var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                            if (item.currencyId != companyCurrencyId)
                                addStagingPrincipal.CURRENCYRATECODE = "TTB";


                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPrincipal);
                        }

                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BL", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.loanRefNo} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }



                }





            }

            return true;
        }


        public bool WriteBulkLoanRepaymentPostingPastDueToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
             IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;

            foreach (var item in model)
            {
                var addStagingInterest = new TBL_CUSTOM_TRANSACTION_BULK();
                var addStagingPrincipal = new TBL_CUSTOM_TRANSACTION_BULK();
                var addStagingPastDueInterest = new TBL_CUSTOM_TRANSACTION_BULK();
                var addStagingPastDuePrincipal = new TBL_CUSTOM_TRANSACTION_BULK();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                //var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);          

                TBL_CASA casa;

                if (product.PRODUCTCLASSID != (short)ProductClassEnum.InvoiceDiscountingFacility)
                {
                    casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);
                }
                else
                {
                    casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2.Value && x.COMPANYID == item.companyId);
                }


                if (item.interestOnPastDueInterest > 0)
                {
                    count++;

                    addStagingPastDueInterest.AMOUNT = (decimal)Math.Abs(item.interestOnPastDueInterest);
                    addStagingPastDueInterest.FLOWTYPE = "BPF";
                    addStagingPastDueInterest.FORCEDEBITACCOUNT = "N";
                    addStagingPastDueInterest.VALUEDATENUMBER = 1;
                    addStagingPastDueInterest.BATCHID = batchCode;
                    addStagingPastDueInterest.BATCHREFID = count;
                    addStagingPastDueInterest.SID = count;
                    addStagingPastDueInterest.COMPANYID = item.companyId;
                    addStagingPastDueInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingPastDueInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingPastDueInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingPastDueInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingPastDueInterest.DESCRIPTION = "Interest on Past Due Interest";
                    addStagingPastDueInterest.DESTINATIONBRANCHID = item.branchId;
                    addStagingPastDueInterest.ISPOSTED = false;
                    addStagingPastDueInterest.OPERATIONID = (int)OperationsEnum.InterestOnPastDueInterest;
                    addStagingPastDueInterest.POSTEDBY = "SYSTEM";
                    addStagingPastDueInterest.POSTEDDATE = DateTime.Now.Date; ;
                    addStagingPastDueInterest.SOURCEBRANCHID = item.branchId;
                    addStagingPastDueInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingPastDueInterest.VALUEDATE = applicationDate;
                    addStagingPastDueInterest.TRANSACTIONTYPE = "BL";
                    addStagingPastDueInterest.BANKID = "01";
                    addStagingPastDueInterest.PRODUCTID = product.PRODUCTID;
                    addStagingPastDueInterest.CURRENCYID = item.currencyId;
                    addStagingPastDueInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPastDueInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPastDueInterest.CREDITCASAACCOUNTID = null;
                    addStagingPastDueInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingPastDueInterest.LOANID = item.loanId;
                    addStagingPastDueInterest.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDueInterest);

                    var pastDueInterestDaily = (from a in context.TBL_DAILY_ACCRUAL
                                                where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                                && a.CATEGORYID == (short)DailyAccrualCategory.PastDueInterest && a.REPAYMENTPOSTEDSTATUS == false
                                                && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                                select a);


                    foreach (var itemDaily in pastDueInterestDaily)
                    {
                        itemDaily.REPAYMENTPOSTEDSTATUS = true;
                        itemDaily.DEMANDDATE = applicationDate;
                    }
                }


                if (item.interestOnPastDuePrincipal > 0)
                {
                    count++;

                    addStagingPastDuePrincipal.AMOUNT = (decimal)Math.Abs(item.interestOnPastDuePrincipal);
                    addStagingPastDuePrincipal.FLOWTYPE = "BPF";
                    addStagingPastDuePrincipal.FORCEDEBITACCOUNT = "N";
                    addStagingPastDuePrincipal.VALUEDATENUMBER = 1;
                    addStagingPastDuePrincipal.BATCHID = batchCode;
                    addStagingPastDuePrincipal.BATCHREFID = count;
                    addStagingPastDuePrincipal.SID = count;
                    addStagingPastDuePrincipal.COMPANYID = item.companyId;
                    addStagingPastDuePrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingPastDuePrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingPastDuePrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingPastDuePrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingPastDuePrincipal.DESCRIPTION = "Interest on Past Due Principal";
                    addStagingPastDuePrincipal.DESTINATIONBRANCHID = item.branchId;
                    addStagingPastDuePrincipal.ISPOSTED = false;
                    addStagingPastDuePrincipal.OPERATIONID = (int)OperationsEnum.InterestOnPastDuePrincipal;
                    addStagingPastDuePrincipal.POSTEDBY = "SYSTEM";
                    addStagingPastDuePrincipal.POSTEDDATE = DateTime.Now.Date; ;
                    addStagingPastDuePrincipal.SOURCEBRANCHID = item.branchId;
                    addStagingPastDuePrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingPastDuePrincipal.VALUEDATE = applicationDate;
                    addStagingPastDuePrincipal.TRANSACTIONTYPE = "BL";
                    addStagingPastDuePrincipal.BANKID = "01";
                    addStagingPastDuePrincipal.PRODUCTID = product.PRODUCTID;
                    addStagingPastDuePrincipal.CURRENCYID = item.currencyId;
                    addStagingPastDuePrincipal.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPastDuePrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPastDuePrincipal.CREDITCASAACCOUNTID = null;
                    addStagingPastDuePrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingPastDuePrincipal.LOANID = item.loanId;
                    addStagingPastDuePrincipal.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDuePrincipal);

                    var pastDuePrincipalDaily = (from a in context.TBL_DAILY_ACCRUAL
                                                 where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                                 && a.CATEGORYID == (short)DailyAccrualCategory.PastDuePrincipal && a.REPAYMENTPOSTEDSTATUS == false
                                                 && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                                 select a);

                    foreach (var itemDaily in pastDuePrincipalDaily)
                    {
                        itemDaily.REPAYMENTPOSTEDSTATUS = true;
                        itemDaily.DEMANDDATE = applicationDate;
                    }
                }

                //count++;
                //if ((decimal)item.pastDueInterestAmount > 0)
                //{
                //    addStagingPastDueInterest.AMOUNT = (decimal) Math.Abs(item.pastDueInterestAmount);
                //    addStagingPastDueInterest.FLOWTYPE = "BIF";
                //    addStagingPastDueInterest.FORCEDEBITACCOUNT = "N";
                //    addStagingPastDueInterest.VALUEDATENUMBER = 1;
                //    addStagingPastDueInterest.BATCHID = batchCode;
                //    addStagingPastDueInterest.BATCHREFID = count;
                //    addStagingPastDueInterest.SID = count;
                //    addStagingPastDueInterest.COMPANYID = item.companyId;
                //    addStagingPastDueInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStagingPastDueInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStagingPastDueInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                //    addStagingPastDueInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                //    addStagingPastDueInterest.DESCRIPTION = "Past Due Interest Repayment";
                //    addStagingPastDueInterest.DESTINATIONBRANCHID = item.branchId;
                //    addStagingPastDueInterest.ISPOSTED = false;
                //    addStagingPastDueInterest.OPERATIONID = (int)OperationsEnum.InterestPastDueLoanRepayment;
                //    addStagingPastDueInterest.POSTEDBY = "SYSTEM";
                //    addStagingPastDueInterest.POSTEDDATE = DateTime.Now.Date;;
                //    addStagingPastDueInterest.SOURCEBRANCHID = item.branchId;
                //    addStagingPastDueInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                //    addStagingPastDueInterest.VALUEDATE = applicationDate;
                //    addStagingPastDueInterest.TRANSACTIONTYPE = "BL";
                //    addStagingPastDueInterest.BANKID = "01";
                //    addStagingPastDueInterest.PRODUCTID = product.PRODUCTID;
                //    addStagingPastDueInterest.CURRENCYID = item.currencyId;
                //    addStagingPastDueInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                //    addStagingPastDueInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                //    addStagingPastDueInterest.CREDITCASAACCOUNTID = null;
                //    addStagingPastDueInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                //    addStagingPastDueInterest.LOANID = item.loanId;
                //    addStagingPastDueInterest.SYSTEMDATETIME = DateTime.Now;

                //    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDueInterest);
                //}


                //count++;
                //if ((decimal)item.pastDuePrincipalAmount > 0)
                //{
                //    addStagingPastDuePrincipal.AMOUNT = (decimal)Math.Abs(item.pastDuePrincipalAmount);
                //    addStagingPastDuePrincipal.FLOWTYPE = "BPP";
                //    addStagingPastDuePrincipal.FORCEDEBITACCOUNT = "N";
                //    addStagingPastDuePrincipal.VALUEDATENUMBER = 1;
                //    addStagingPastDuePrincipal.BATCHID = batchCode;
                //    addStagingPastDuePrincipal.BATCHREFID = count;
                //    addStagingPastDuePrincipal.SID = count;
                //    addStagingPastDuePrincipal.COMPANYID = item.companyId;
                //    addStagingPastDuePrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStagingPastDuePrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStagingPastDuePrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                //    addStagingPastDuePrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                //    addStagingPastDuePrincipal.DESCRIPTION = "Past Due Principal Repayment";
                //    addStagingPastDuePrincipal.DESTINATIONBRANCHID = item.branchId;
                //    addStagingPastDuePrincipal.ISPOSTED = false;
                //    addStagingPastDuePrincipal.OPERATIONID = (int)OperationsEnum.PrincipalPastDueLoanRepayment;//change to periodPrincipalAmount
                //    addStagingPastDuePrincipal.POSTEDBY = "SYSTEM";
                //    addStagingPastDuePrincipal.POSTEDDATE = DateTime.Now.Date; 
                //    addStagingPastDuePrincipal.SOURCEBRANCHID = item.branchId;
                //    addStagingPastDuePrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                //    addStagingPastDuePrincipal.VALUEDATE = applicationDate;
                //    addStagingPastDuePrincipal.TRANSACTIONTYPE = "BL";
                //    addStagingPastDuePrincipal.BANKID = "01";
                //    addStagingPastDuePrincipal.PRODUCTID = product.PRODUCTID;
                //    addStagingPastDuePrincipal.CURRENCYID = item.currencyId;
                //    addStagingPastDuePrincipal.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                //    addStagingPastDuePrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;// product.INTERESTRECEIVABLEPAYABLEGL.Value;
                //    addStagingPastDuePrincipal.CREDITCASAACCOUNTID = null;
                //    addStagingPastDuePrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                //    addStagingPastDuePrincipal.LOANID = item.loanId;
                //    addStagingPastDuePrincipal.SYSTEMDATETIME = DateTime.Now;

                //    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDuePrincipal);
                //}


                if ((decimal)item.periodInterestAmount > 0)
                {
                    count++;

                    addStagingInterest.AMOUNT = (decimal)Math.Abs(item.periodInterestAmount);
                    addStagingInterest.FLOWTYPE = "BIF";
                    addStagingInterest.FORCEDEBITACCOUNT = "N";
                    addStagingInterest.VALUEDATENUMBER = 1;
                    addStagingInterest.BATCHID = batchCode;
                    addStagingInterest.BATCHREFID = count;
                    addStagingInterest.SID = count;
                    addStagingInterest.COMPANYID = item.companyId;
                    addStagingInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingInterest.DESCRIPTION = "Interest Repayment";
                    addStagingInterest.DESTINATIONBRANCHID = item.branchId;
                    addStagingInterest.ISPOSTED = false;
                    addStagingInterest.OPERATIONID = (int)OperationsEnum.InterestLoanRepayment;
                    addStagingInterest.POSTEDBY = "SYSTEM";
                    addStagingInterest.POSTEDDATE = DateTime.Now.Date;
                    addStagingInterest.SOURCEBRANCHID = item.branchId;
                    addStagingInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingInterest.VALUEDATE = applicationDate;
                    addStagingInterest.TRANSACTIONTYPE = "BL";
                    addStagingInterest.BANKID = "01";
                    addStagingInterest.PRODUCTID = product.PRODUCTID;
                    addStagingInterest.CURRENCYID = item.currencyId;
                    addStagingInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingInterest.CREDITCASAACCOUNTID = null;
                    addStagingInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingInterest.LOANID = item.loanId;
                    addStagingInterest.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingInterest);

                    var interestDaily = (from a in context.TBL_DAILY_ACCRUAL
                                         where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                         && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan && a.REPAYMENTPOSTEDSTATUS == false
                                         && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                         select a);

                    foreach (var itemDaily in interestDaily)
                    {
                        itemDaily.REPAYMENTPOSTEDSTATUS = true;
                        itemDaily.DEMANDDATE = applicationDate;
                    }
                }


                if ((decimal)item.periodPrincipalAmount > 0)
                {
                    count++;

                    addStagingPrincipal.AMOUNT = (decimal)Math.Abs(item.periodPrincipalAmount);
                    addStagingPrincipal.FLOWTYPE = "BPP";
                    addStagingPrincipal.FORCEDEBITACCOUNT = "N";
                    addStagingPrincipal.VALUEDATENUMBER = 1;
                    addStagingPrincipal.BATCHID = batchCode;
                    addStagingPrincipal.BATCHREFID = count;
                    addStagingPrincipal.SID = count;
                    addStagingPrincipal.COMPANYID = item.companyId;
                    addStagingPrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingPrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingPrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingPrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingPrincipal.DESCRIPTION = "Principal Repayment";
                    addStagingPrincipal.DESTINATIONBRANCHID = item.branchId;
                    addStagingPrincipal.ISPOSTED = false;
                    addStagingPrincipal.OPERATIONID = (int)OperationsEnum.PrincipalLoanRepayment;//change to periodPrincipalAmount
                    addStagingPrincipal.POSTEDBY = "SYSTEM";
                    addStagingPrincipal.POSTEDDATE = DateTime.Now.Date;
                    addStagingPrincipal.SOURCEBRANCHID = item.branchId;
                    addStagingPrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingPrincipal.VALUEDATE = applicationDate;
                    addStagingPrincipal.TRANSACTIONTYPE = "BL";
                    addStagingPrincipal.BANKID = "01";
                    addStagingPrincipal.PRODUCTID = product.PRODUCTID;
                    addStagingPrincipal.CURRENCYID = item.currencyId;
                    addStagingPrincipal.CREDITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                    addStagingPrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;// product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPrincipal.CREDITCASAACCOUNTID = null;
                    addStagingPrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingPrincipal.LOANID = item.loanId;
                    addStagingPrincipal.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPrincipal);
                }




                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BL", batchCode);
        }


        public bool WriteBulkProcessLoanDisbursmentRollOverToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
  IFinanceTransactionRepository financeTransaction, DateTime applicationDate, int companyId, out string transactionReferenceNo)
        {

            DateTime dateChange = applicationDate.AddDays(-1);

            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover && c.COMPANYID == companyId).FirstOrDefault();

            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (LoanRepaymentViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.loanRefNo;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover;
                        eod_operation_Detail.EODDATE = dateChange;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }


            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {


                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;

                    context.SaveChanges();


                    try
                    {

                        transactionReferenceNo = item.loanRefNo + " - to staging";

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        if (item.maturityInstructionTypeId == (int)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal)
                        {
                            item.totalAmount = item.periodInterestAmount + item.periodPrincipalAmount;
                        }
                        else
                        {
                            item.totalAmount = item.periodPrincipalAmount;
                        }

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                        var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);

                        count++;

                        addStaging.AMOUNT = (decimal)item.totalAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "Y";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CREDITACCOUNT = casa.PRODUCTACCOUNTNUMBER;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        //addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                        addStaging.CURRENCYRATE = item.exchangeRate;
                        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);
                        addStaging.DESCRIPTION = "Roll Over";
                        addStaging.DESTINATIONBRANCHID = item.branchId;
                        addStaging.ISPOSTED = false;
                        addStaging.OPERATIONID = (int)OperationsEnum.CommercialLoanRollOver;///change to periodInterestAmount
                        addStaging.POSTEDBY = "SYSTEM";
                        addStaging.POSTEDDATE = DateTime.Now.Date;
                        addStaging.SOURCEBRANCHID = item.branchId;
                        addStaging.SOURCEREFERENCENUMBER = item.loanRefNo;
                        addStaging.VALUEDATE = applicationDate;
                        addStaging.TRANSACTIONTYPE = "BP";
                        addStaging.BANKID = "01";
                        addStaging.PRODUCTID = product.PRODUCTID;
                        addStaging.CURRENCYID = item.currencyId;
                        addStaging.CREDITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                        addStaging.DEBITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                        addStaging.CREDITCASAACCOUNTID = casa.CASAACCOUNTID;
                        addStaging.DEBITCASAACCOUNTID = null;
                        addStaging.LOANID = item.loanId;
                        addStaging.SYSTEMDATETIME = DateTime.Now;

                        var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                        if (item.currencyId != companyCurrencyId)
                            addStaging.CURRENCYRATECODE = "TTB";

                        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.loanRefNo} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }



                }


            }

            return true;

        }

        public bool WriteBulkProcessLoanDisbursmentRollOverToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
    IFinanceTransactionRepository financeTransaction, DateTime applicationDate, int companyId, int staffId, out string transactionReferenceNo)
        {

            DateTime dateChange = applicationDate.AddDays(-1);

            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover && c.COMPANYID == companyId).FirstOrDefault();

            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (LoanRepaymentViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.loanRefNo;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover;
                        eod_operation_Detail.EODDATE = dateChange;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }


            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {


                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();


                    try
                    {

                        transactionReferenceNo = item.loanRefNo + " - to staging";

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        if (item.maturityInstructionTypeId == (int)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal)
                        {
                            item.totalAmount = item.periodInterestAmount + item.periodPrincipalAmount;
                        }
                        else
                        {
                            item.totalAmount = item.periodPrincipalAmount;
                        }

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                        //var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);
                        var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2 && x.COMPANYID == item.companyId);

                        count++;

                        addStaging.AMOUNT = (decimal)item.totalAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "Y";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CREDITACCOUNT = casa.PRODUCTACCOUNTNUMBER;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        //addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                        addStaging.CURRENCYRATE = item.exchangeRate;
                        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);
                        addStaging.DESCRIPTION = "Roll Over";
                        addStaging.DESTINATIONBRANCHID = item.branchId;
                        addStaging.ISPOSTED = false;
                        addStaging.OPERATIONID = (int)OperationsEnum.CommercialLoanRollOver;///change to periodInterestAmount
                        addStaging.POSTEDBY = "SYSTEM";
                        addStaging.POSTEDDATE = DateTime.Now.Date;
                        addStaging.SOURCEBRANCHID = item.branchId;
                        addStaging.SOURCEREFERENCENUMBER = item.loanRefNo;
                        addStaging.VALUEDATE = applicationDate;
                        addStaging.TRANSACTIONTYPE = "BP";
                        addStaging.BANKID = "01";
                        addStaging.PRODUCTID = product.PRODUCTID;
                        addStaging.CURRENCYID = item.currencyId;
                        addStaging.CREDITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                        addStaging.DEBITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                        addStaging.CREDITCASAACCOUNTID = casa.CASAACCOUNTID;
                        addStaging.DEBITCASAACCOUNTID = null;
                        addStaging.LOANID = item.loanId;
                        addStaging.SYSTEMDATETIME = DateTime.Now;

                        var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                        if (item.currencyId != companyCurrencyId)
                            addStaging.CURRENCYRATECODE = "TTB";

                        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.loanRefNo} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }



                }


            }

            return true;

        }

        public bool WriteBulkDailyWrittenOffFacilityAccrualToStaging(List<DailyInterestAccrualViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
                                           IFinanceTransactionRepository financeTransaction, DateTime applicationDate, int companyId, int staffId, out string transactionReferenceNo)
        {


            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.DailyWrittenOffFacilityAccrual && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (DailyInterestAccrualViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.baseReferenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.DailyWrittenOffFacilityAccrual).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.referenceNumber;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.DailyWrittenOffFacilityAccrual;
                        eod_operation_Detail.EODDATE = applicationDate;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }


            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {




                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.DailyWrittenOffFacilityAccrual).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.referenceNumber && c.EODDATE == applicationDate && c.EODOPERATIONID == (int)EodOperationEnum.DailyWrittenOffFacilityAccrual).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();



                    try
                    {

                        transactionReferenceNo = item.referenceNumber;

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        item.date = applicationDate;

                        var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                        //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);                

                        var accounts = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (int)OtherOperationEnum.InterestOffBalansheetCompleteWriteOffAccount).FirstOrDefault();

                        count++;

                        addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                        addStaging.FLOWTYPE = "FFF";
                        addStaging.FORCEDEBITACCOUNT = "N";
                        addStaging.VALUEDATENUMBER = 1;
                        addStaging.BATCHID = batchCode;
                        addStaging.BATCHREFID = count;
                        addStaging.SID = count;
                        addStaging.COMPANYID = item.companyId;
                        addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(accounts.GLACCOUNTID2.Value, item.currencyId, item.branchId);//context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE; // GetGLAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId)  product.INTERESTINCOMEEXPENSEGL.Value;
                        addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                        //addStaging.CURRENCYRATE = 1; // financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                        addStaging.CURRENCYRATE = item.exchangeRate;
                        addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(accounts.GLACCOUNTID, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                        addStaging.DESCRIPTION = "Write-off Daily Interest Accrual Posting";
                        addStaging.DESTINATIONBRANCHID = item.branchId;
                        addStaging.ISPOSTED = false;
                        addStaging.OPERATIONID = (int)OperationsEnum.DailyWriteoffInterestAccural;
                        addStaging.POSTEDBY = "SYSTEM";
                        addStaging.POSTEDDATE = DateTime.Now.Date;
                        addStaging.SOURCEBRANCHID = item.branchId;
                        addStaging.SOURCEREFERENCENUMBER = item.referenceNumber; // groupedQ.Key.PRODUCTCODE + '/' + groupedQ.Key.CURRENCYCODE + '/' + groupedQ.Key.BRANCHCODE + '/' + groupedQ.Key.COMPANYID.ToString();//product.PRODUCTCODE;
                        addStaging.VALUEDATE = item.date;
                        addStaging.TRANSACTIONTYPE = "BP";
                        addStaging.BANKID = "01";
                        addStaging.PRODUCTID = item.productId;
                        addStaging.CURRENCYID = item.currencyId;
                        addStaging.CREDITGLACCOUNTID = accounts.GLACCOUNTID2.Value;
                        addStaging.DEBITGLACCOUNTID = accounts.GLACCOUNTID;
                        addStaging.CREDITCASAACCOUNTID = null;
                        addStaging.DEBITCASAACCOUNTID = null;
                        addStaging.LOANID = null;
                        addStaging.SYSTEMDATETIME = DateTime.Now;

                        var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                        if (item.currencyId != companyCurrencyId)
                            addStaging.CURRENCYRATECODE = "TTB";


                        context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();

                    }
                    catch (Exception ex)
                    {

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.referenceNumber} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                        context.SaveChanges();
                    }


                }



            }

            return true;

        }


        private bool WriteBulkPostingToStagingSub(FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, DateTime applicationDate, string TransactionType, string batchCode)
        {
            bool output = false;
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
                            forceDebitAccount = a.FORCEDEBITACCOUNT,
                            valueDate = a.VALUEDATE,
                            transactionDate = a.POSTEDDATE

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
                addStaging.TOD_FLG = item.forceDebitAccount;
                addStaging.LOAN_ACCT = item.sourceReferenceNumber;
                addStaging.STATUS = "NEW";
                addStaging.RCRE_DATE = applicationDate;
                addStaging.PSTD_FLG = "N";
                addStaging.PSTD_DATE = applicationDate;
                addStaging.DEL_FLG = "N";
                addStaging.FAIL_FLG = "N";
                addStaging.FINTRAK_FLG = "N";

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
                main.Add(addMain);

            }

            stagingContext.FINTRAK_TRAN_PROC_MAIN.AddRange(main);

            var result = stagingContext.SaveChanges() > 0;
            if (result)
            {
                output = true;
            }
            else
            {

            }
            return output;
        }


        public bool WriteBulkLoanRepaymentPostingForceDebitToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
       IFinanceTransactionRepository financeTransaction, DateTime applicationDate, int companyId, int staffId, out string transactionReferenceNo)
        {

            DateTime dateChange = applicationDate.AddDays(-1);

            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == dateChange && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (model.Count() != 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (LoanRepaymentViewModel loan in model)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.loanRefNo;
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit;
                        eod_operation_Detail.EODDATE = dateChange;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }

            transactionReferenceNo = "";

            int count = 0;
            foreach (var item in model)
            {

                var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit).FirstOrDefault();

                if (checkExistence != null)
                {

                    var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == item.loanRefNo && c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit).FirstOrDefault();

                    eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                    eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                    context.SaveChanges();

                    try
                    {

                        transactionReferenceNo = item.loanRefNo;

                        var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                        //var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);

                        TBL_CASA casa;

                        //var interest = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == item.loanId && x.PAYMENTDATE == DbFunctions.TruncateTime(applicationDate));
                        //var interestAmount = interest.Sum(x => x.DAILYPRINCIPALAMOUNT);



                        //item.periodInterestAmount = interestAmount; ///TODO will not work for CP since its unscheduled 


                        if (product.PRODUCTCLASSID != (short)ProductClassEnum.InvoiceDiscountingFacility)
                        {
                            //casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);
                            casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2 && x.COMPANYID == item.companyId);
                        }
                        else
                        {
                            casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2.Value && x.COMPANYID == item.companyId);
                        }

                        if (item.periodInterestAmount > 0)
                        {
                            count++;

                            var addStagingInterest = new TBL_CUSTOM_TRANSACTION_BULK();
                            addStagingInterest.AMOUNT = (decimal)item.periodInterestAmount;
                            addStagingInterest.FLOWTYPE = "BIF";
                            addStagingInterest.FORCEDEBITACCOUNT = "Y";
                            addStagingInterest.VALUEDATENUMBER = 1;
                            addStagingInterest.BATCHID = batchCode;
                            addStagingInterest.BATCHREFID = count;
                            addStagingInterest.SID = count;
                            addStagingInterest.COMPANYID = item.companyId;
                            addStagingInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStagingInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                            //addStagingInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                            addStagingInterest.CURRENCYRATE = item.exchangeRate;
                            addStagingInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                            addStagingInterest.DESCRIPTION = "Interest Repayment";
                            addStagingInterest.DESTINATIONBRANCHID = item.branchId;
                            addStagingInterest.ISPOSTED = false;
                            addStagingInterest.OPERATIONID = (int)OperationsEnum.InterestLoanRepayment;///change to periodInterestAmount
                            addStagingInterest.POSTEDBY = "SYSTEM";
                            addStagingInterest.POSTEDDATE = DateTime.Now.Date;
                            addStagingInterest.SOURCEBRANCHID = item.branchId;
                            addStagingInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                            addStagingInterest.VALUEDATE = applicationDate;
                            addStagingInterest.TRANSACTIONTYPE = "BP";
                            addStagingInterest.BANKID = "01";
                            addStagingInterest.PRODUCTID = product.PRODUCTID;
                            addStagingInterest.CURRENCYID = item.currencyId;
                            addStagingInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingInterest.CREDITCASAACCOUNTID = null;
                            addStagingInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                            addStagingInterest.LOANID = item.loanId;
                            addStagingInterest.SYSTEMDATETIME = DateTime.Now;

                            var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                            if (item.currencyId != companyCurrencyId)
                                addStagingInterest.CURRENCYRATECODE = "TTB";

                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingInterest);

                            var interestDaily = (from a in context.TBL_DAILY_ACCRUAL
                                                 where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                                 && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan && a.REPAYMENTPOSTEDSTATUS == false
                                                 && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                                 select a);

                            foreach (var itemDaily in interestDaily)
                            {
                                itemDaily.REPAYMENTPOSTEDSTATUS = true;
                                itemDaily.DEMANDDATE = applicationDate;
                            }

                        }


                        if (item.periodPrincipalAmount > 0)
                        {
                            count++;

                            var addStagingPrincipal = new TBL_CUSTOM_TRANSACTION_BULK();

                            addStagingPrincipal.AMOUNT = (decimal)item.periodPrincipalAmount;
                            addStagingPrincipal.FLOWTYPE = "BPP";
                            addStagingPrincipal.FORCEDEBITACCOUNT = "Y";
                            addStagingPrincipal.VALUEDATENUMBER = 1;
                            addStagingPrincipal.BATCHID = batchCode;
                            addStagingPrincipal.BATCHREFID = count;
                            addStagingPrincipal.SID = count;
                            addStagingPrincipal.COMPANYID = item.companyId;
                            addStagingPrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                            addStagingPrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                            addStagingPrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                            addStagingPrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                            addStagingPrincipal.DESCRIPTION = "Principal Repayment";
                            addStagingPrincipal.DESTINATIONBRANCHID = item.branchId;
                            addStagingPrincipal.ISPOSTED = false;
                            addStagingPrincipal.OPERATIONID = (int)OperationsEnum.PrincipalLoanRepayment;//change to periodPrincipalAmount
                            addStagingPrincipal.POSTEDBY = "SYSTEM";
                            addStagingPrincipal.POSTEDDATE = DateTime.Now.Date;
                            addStagingPrincipal.SOURCEBRANCHID = item.branchId;
                            addStagingPrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                            addStagingPrincipal.VALUEDATE = applicationDate;
                            addStagingPrincipal.TRANSACTIONTYPE = "BP";
                            addStagingPrincipal.BANKID = "01";
                            addStagingPrincipal.PRODUCTID = product.PRODUCTID;
                            addStagingPrincipal.CURRENCYID = item.currencyId;
                            addStagingPrincipal.CREDITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                            addStagingPrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;// product.INTERESTRECEIVABLEPAYABLEGL.Value;
                            addStagingPrincipal.CREDITCASAACCOUNTID = null;
                            addStagingPrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                            addStagingPrincipal.LOANID = item.loanId;
                            addStagingPrincipal.SYSTEMDATETIME = DateTime.Now;
                            var companyCurrencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == item.companyId).CURRENCYID;
                            if (item.currencyId != companyCurrencyId)
                                addStagingPrincipal.CURRENCYRATECODE = "TTB";

                            context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPrincipal);
                        }

                        context.SaveChanges();

                        WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                        context.SaveChanges();


                    }
                    catch (Exception ex)
                    {
                        // Get stack trace for the exception with source file information
                        var st = new StackTrace(ex, true);
                        // Get the top stack frame
                        var frame = st.GetFrame(0);
                        // Get the line number from the stack frame
                        var line = frame.GetFileLineNumber();

                        eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                        eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {item.loanRefNo} Exception - {ex.Message}  - inner exception -  {ex.InnerException}. @Line: {line}";
                        context.SaveChanges();
                    }


                }
            }

            return true;

        }


        public bool WriteBulkLoanRepaymentPostingForceDebitToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
       IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                //var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);

                TBL_CASA casa;

                //var interest = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == item.loanId && x.PAYMENTDATE == DbFunctions.TruncateTime(applicationDate));
                //var interestAmount = interest.Sum(x => x.DAILYPRINCIPALAMOUNT);



                //item.periodInterestAmount = interestAmount; ///TODO will not work for CP since its unscheduled 


                if (product.PRODUCTCLASSID != (short)ProductClassEnum.InvoiceDiscountingFacility)
                {
                    casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);
                }
                else
                {
                    casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId2.Value && x.COMPANYID == item.companyId);
                }

                if (item.periodInterestAmount > 0)
                {
                    count++;

                    var addStagingInterest = new TBL_CUSTOM_TRANSACTION_BULK();
                    addStagingInterest.AMOUNT = (decimal)item.periodInterestAmount;
                    addStagingInterest.FLOWTYPE = "BIF";
                    addStagingInterest.FORCEDEBITACCOUNT = "Y";
                    addStagingInterest.VALUEDATENUMBER = 1;
                    addStagingInterest.BATCHID = batchCode;
                    addStagingInterest.BATCHREFID = count;
                    addStagingInterest.SID = count;
                    addStagingInterest.COMPANYID = item.companyId;
                    addStagingInterest.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingInterest.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingInterest.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingInterest.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingInterest.DESCRIPTION = "Interest Repayment";
                    addStagingInterest.DESTINATIONBRANCHID = item.branchId;
                    addStagingInterest.ISPOSTED = false;
                    addStagingInterest.OPERATIONID = (int)OperationsEnum.InterestLoanRepayment;///change to periodInterestAmount
                    addStagingInterest.POSTEDBY = "SYSTEM";
                    addStagingInterest.POSTEDDATE = DateTime.Now.Date;
                    addStagingInterest.SOURCEBRANCHID = item.branchId;
                    addStagingInterest.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingInterest.VALUEDATE = applicationDate;
                    addStagingInterest.TRANSACTIONTYPE = "BP";
                    addStagingInterest.BANKID = "01";
                    addStagingInterest.PRODUCTID = product.PRODUCTID;
                    addStagingInterest.CURRENCYID = item.currencyId;
                    addStagingInterest.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingInterest.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingInterest.CREDITCASAACCOUNTID = null;
                    addStagingInterest.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingInterest.LOANID = item.loanId;
                    addStagingInterest.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingInterest);

                    var interestDaily = (from a in context.TBL_DAILY_ACCRUAL
                                         where a.REFERENCENUMBER == item.loanRefNo && a.COMPANYID == item.companyId
                                         && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan && a.REPAYMENTPOSTEDSTATUS == false
                                         && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                         select a);

                    foreach (var itemDaily in interestDaily)
                    {
                        itemDaily.REPAYMENTPOSTEDSTATUS = true;
                        itemDaily.DEMANDDATE = applicationDate;
                    }

                }


                if (item.periodPrincipalAmount > 0)
                {
                    count++;

                    var addStagingPrincipal = new TBL_CUSTOM_TRANSACTION_BULK();

                    addStagingPrincipal.AMOUNT = (decimal)item.periodPrincipalAmount;
                    addStagingPrincipal.FLOWTYPE = "BPP";
                    addStagingPrincipal.FORCEDEBITACCOUNT = "Y";
                    addStagingPrincipal.VALUEDATENUMBER = 1;
                    addStagingPrincipal.BATCHID = batchCode;
                    addStagingPrincipal.BATCHREFID = count;
                    addStagingPrincipal.SID = count;
                    addStagingPrincipal.COMPANYID = item.companyId;
                    addStagingPrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingPrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingPrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingPrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingPrincipal.DESCRIPTION = "Principal Repayment";
                    addStagingPrincipal.DESTINATIONBRANCHID = item.branchId;
                    addStagingPrincipal.ISPOSTED = false;
                    addStagingPrincipal.OPERATIONID = (int)OperationsEnum.PrincipalLoanRepayment;//change to periodPrincipalAmount
                    addStagingPrincipal.POSTEDBY = "SYSTEM";
                    addStagingPrincipal.POSTEDDATE = DateTime.Now.Date;
                    addStagingPrincipal.SOURCEBRANCHID = item.branchId;
                    addStagingPrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingPrincipal.VALUEDATE = applicationDate;
                    addStagingPrincipal.TRANSACTIONTYPE = "BP";
                    addStagingPrincipal.BANKID = "01";
                    addStagingPrincipal.PRODUCTID = product.PRODUCTID;
                    addStagingPrincipal.CURRENCYID = item.currencyId;
                    addStagingPrincipal.CREDITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                    addStagingPrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;// product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPrincipal.CREDITCASAACCOUNTID = null;
                    addStagingPrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingPrincipal.LOANID = item.loanId;
                    addStagingPrincipal.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPrincipal);
                }

                context.SaveChanges();

            }

            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

        public bool WriteBulkProcessLoanDisbursmentRollOverToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
      IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {
                if (item.maturityInstructionTypeId == (int)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal)
                {
                    item.totalAmount = item.periodInterestAmount + item.periodPrincipalAmount;
                }
                else
                {
                    item.totalAmount = item.periodPrincipalAmount;
                }

                var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);

                count++;

                addStaging.AMOUNT = (decimal)item.totalAmount;
                addStaging.FLOWTYPE = "FFF";
                addStaging.FORCEDEBITACCOUNT = "Y";
                addStaging.VALUEDATENUMBER = 1;
                addStaging.BATCHID = batchCode;
                addStaging.BATCHREFID = count;
                addStaging.SID = count;
                addStaging.COMPANYID = item.companyId;
                addStaging.CREDITACCOUNT = casa.PRODUCTACCOUNTNUMBER;
                addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.PRINCIPALBALANCEGL.Value, item.currencyId, item.branchId);
                addStaging.DESCRIPTION = "Roll Over";
                addStaging.DESTINATIONBRANCHID = item.branchId;
                addStaging.ISPOSTED = false;
                addStaging.OPERATIONID = (int)OperationsEnum.CommercialLoanRollOver;///change to periodInterestAmount
                addStaging.POSTEDBY = "SYSTEM";
                addStaging.POSTEDDATE = DateTime.Now.Date;
                addStaging.SOURCEBRANCHID = item.branchId;
                addStaging.SOURCEREFERENCENUMBER = item.loanRefNo;
                addStaging.VALUEDATE = applicationDate;
                addStaging.TRANSACTIONTYPE = "BP";
                addStaging.BANKID = "01";
                addStaging.PRODUCTID = product.PRODUCTID;
                addStaging.CURRENCYID = item.currencyId;
                addStaging.CREDITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                addStaging.DEBITGLACCOUNTID = product.PRINCIPALBALANCEGL.Value;
                addStaging.CREDITCASAACCOUNTID = casa.CASAACCOUNTID;
                addStaging.DEBITCASAACCOUNTID = null;
                addStaging.LOANID = item.loanId;
                addStaging.SYSTEMDATETIME = DateTime.Now;

                context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                context.SaveChanges();

            }
            return WriteBulkPostingToStagingSub(context, stagingContext, applicationDate, "BP", batchCode);

        }

    }
}
