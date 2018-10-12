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
                addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
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
                addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                addStaging.DESCRIPTION = "Loan Daily Interest Accrual Posting";
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
                addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
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

                count++;
                if ((decimal)item.pastDueInterestAmount != 0)
                {
                    addStagingPastDueInterest.AMOUNT = (decimal) Math.Abs(item.pastDueInterestAmount);
                    addStagingPastDueInterest.FLOWTYPE = "BIF";
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
                    addStagingPastDueInterest.DESCRIPTION = "Past Due Interest Repayment";
                    addStagingPastDueInterest.DESTINATIONBRANCHID = item.branchId;
                    addStagingPastDueInterest.ISPOSTED = false;
                    addStagingPastDueInterest.OPERATIONID = (int)OperationsEnum.InterestLoanRepayment;
                    addStagingPastDueInterest.POSTEDBY = "SYSTEM";
                    addStagingPastDueInterest.POSTEDDATE = DateTime.Now.Date;;
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
                }


                count++;
                if ((decimal)item.pastDuePrincipalAmount != 0)
                {
                    addStagingPastDuePrincipal.AMOUNT = (decimal)Math.Abs(item.pastDuePrincipalAmount);
                    addStagingPastDuePrincipal.FLOWTYPE = "BPP";
                    addStagingPastDuePrincipal.FORCEDEBITACCOUNT = "N";
                    addStagingPastDuePrincipal.VALUEDATENUMBER = 1;
                    addStagingPastDuePrincipal.BATCHID = batchCode;
                    addStagingPastDuePrincipal.BATCHREFID = count;
                    addStagingPastDuePrincipal.SID = count;
                    addStagingPastDuePrincipal.COMPANYID = item.companyId;
                    addStagingPastDuePrincipal.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    addStagingPastDuePrincipal.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    addStagingPastDuePrincipal.CURRENCYRATE = financeTransaction.GetExchangeRate(applicationDate, item.currencyId, item.companyId).sellingRate;
                    addStagingPastDuePrincipal.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;//context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.PRINCIPALBALANCEGL.Value).ACCOUNTCODE;
                    addStagingPastDuePrincipal.DESCRIPTION = "Past Due Principal Repayment";
                    addStagingPastDuePrincipal.DESTINATIONBRANCHID = item.branchId;
                    addStagingPastDuePrincipal.ISPOSTED = false;
                    addStagingPastDuePrincipal.OPERATIONID = (int)OperationsEnum.PrincipalLoanRepayment;//change to periodPrincipalAmount
                    addStagingPastDuePrincipal.POSTEDBY = "SYSTEM";
                    addStagingPastDuePrincipal.POSTEDDATE = DateTime.Now.Date; 
                    addStagingPastDuePrincipal.SOURCEBRANCHID = item.branchId;
                    addStagingPastDuePrincipal.SOURCEREFERENCENUMBER = item.loanRefNo;
                    addStagingPastDuePrincipal.VALUEDATE = applicationDate;
                    addStagingPastDuePrincipal.TRANSACTIONTYPE = "BL";
                    addStagingPastDuePrincipal.BANKID = "01";
                    addStagingPastDuePrincipal.PRODUCTID = product.PRODUCTID;
                    addStagingPastDuePrincipal.CURRENCYID = item.currencyId;
                    addStagingPastDuePrincipal.CREDITGLACCOUNTID = product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPastDuePrincipal.DEBITGLACCOUNTID = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;// product.INTERESTRECEIVABLEPAYABLEGL.Value;
                    addStagingPastDuePrincipal.CREDITCASAACCOUNTID = null;
                    addStagingPastDuePrincipal.DEBITCASAACCOUNTID = casa.CASAACCOUNTID;
                    addStagingPastDuePrincipal.LOANID = item.loanId;
                    addStagingPastDuePrincipal.SYSTEMDATETIME = DateTime.Now;

                    context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStagingPastDuePrincipal);
                }

                count++;
                if ((decimal)item.periodInterestAmount != 0)
                {
                    addStagingInterest.AMOUNT = (decimal) Math.Abs(item.periodInterestAmount);
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
                }


                count++;
                if ((decimal)item.periodPrincipalAmount != 0)
                {
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
                            transactionDate= a.POSTEDDATE
                            
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
            return output;
        }

        public bool WriteBulkLoanRepaymentPostingForceDebitToStaging(List<LoanRepaymentViewModel> model, FinTrakBankingContext context, FinTrakBankingStagingContext stagingContext, IIntegrationWithFinacle finacle,
       IFinanceTransactionRepository financeTransaction, DateTime applicationDate)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            int count = 0;
            foreach (var item in model)
            {

                var addStagingInterest = new TBL_CUSTOM_TRANSACTION_BULK();

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

                count++;

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


                var addStagingPrincipal = new TBL_CUSTOM_TRANSACTION_BULK();

                count++;

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
