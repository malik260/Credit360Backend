using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using System.Linq;
using System;
using System.ServiceModel;
using System.Collections.Generic;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System.Data.Entity;
using FintrakBanking.ViewModels;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle;

namespace FintrakBanking.Repositories.Finance

{
    public class FinanceTransactionRepository : IFinanceTransactionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        //private ILoanOperationsRepository creditOperations;

        public FinanceTransactionRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail, 
                                            //ILoanOperationsRepository _creditOperations, 
                                            FinTrakBankingContext _context)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            //this.creditOperations = _creditOperations;
        }

        private void UpdateCASABalances(int casaAccountId, decimal debitAmount, decimal creditAmount)
        {
            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            if (debitAmount > 0)
            {
                account.LEDGERBALANCE = account.LEDGERBALANCE - debitAmount;
                account.AVAILABLEBALANCE = account.AVAILABLEBALANCE - debitAmount;
            }
            else
            {
                account.LEDGERBALANCE = account.LEDGERBALANCE + creditAmount;
                account.AVAILABLEBALANCE = account.AVAILABLEBALANCE +  creditAmount;
            }
        }

        public CasaBalanceViewModel GetCASABalance(int casaAccountId)
        {
            CustomerDetails cust = new CustomerDetails(context);
            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);
            var data = new CasaBalanceViewModel();
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                Task.Run(async () => { data = await cust.GetCustomerAccountBalance(account.PRODUCTACCOUNTNUMBER); }).GetAwaiter().GetResult();
                return data; //  new CasaBalanceViewModel { availableBalance = data.availableBalance, ledgerBalance = 0,accountStatusId = data.accountStatusId);
            }

            else
            {
                return new CasaBalanceViewModel { availableBalance = account.AVAILABLEBALANCE, ledgerBalance = account.LEDGERBALANCE, accountStatusId = (CASAAccountStatusEnum)account.ACCOUNTSTATUSID,currencyId = account.CURRENCYID,accountNo = account.PRODUCTACCOUNTNUMBER,accountName= account.PRODUCTACCOUNTNAME };
            }

        }

        public CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId)
        {
            CasaBalanceViewModel balance = new CasaBalanceViewModel();

            var trans = (from data in context.TBL_FINANCE_TRANSACTION
                         where data.CASAACCOUNTID == casaAccountId
                         select data.CREDITAMOUNT - data.DEBITAMOUNT).Sum();

            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            var lienBalance = GetLienBalance(account.PRODUCTACCOUNTNUMBER);

            balance.ledgerBalance = trans;
            balance.availableBalance = trans - lienBalance;

            return balance;
        }

        public decimal GetLienBalance(string productAccountNumber)
        {            
            var balance = (from data in context.TBL_CASA_LIEN
                         where data.PRODUCTACCOUNTNUMBER == productAccountNumber
                         select data.LIENCREDITAMOUNT - data.LIENDEBITAMOUNT).Sum();

            return balance;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransactionBulk(List<FinanceTransactionViewModel> inputTransactions)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            //transaction.batchCode = batchCode;

            var transactionCount = (inputTransactions.Count()); 

            if (transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new Exception("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);

            //transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);  
            //transaction.transactionDetails.Sum(x => x.creditAmount);
            //var sumDebit = debitSum.FirstOrDefault();


            if (debitSum != creditSum)
                throw new Exception("Total Debit Amount should equal Total Credit Amount");



            foreach (var item in inputTransactions)
            {
                
                    if (item.debitAmount != 0 && item.creditAmount != 0)
                        throw new Exception("Debit or Credit Amount should be 0");

                    if (item.debitAmount < 0)
                        throw new Exception("Debit Amount should NOT be less than 0");

                    if (item.creditAmount < 0)
                        throw new Exception("Credit Amount should NOT be less than 0");

                    var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                    GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                    if (glClass == GLClassEnum.CASA)
                    {
                        if (item.casaAccountId == null)
                            throw new Exception($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                        UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                    }
                    else if (glClass == GLClassEnum.LoanSchedule)
                    {
                        item.casaAccountId = null;
                        int referenceCount = 0;

                        referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                        if (referenceCount <= 0)
                            throw new Exception($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                    }
                    else
                    { item.casaAccountId = null; }

                    TBL_FINANCE_TRANSACTION trans = new TBL_FINANCE_TRANSACTION();

                    trans.BATCHCODE = batchCode;
                    //trans.OPERATIONID = mainItem.operationId;
                    trans.OPERATIONID = (int)item.operationId;
                    trans.DESCRIPTION = item.description;
                    trans.VALUEDATE = item.valueDate;
                    trans.POSTEDDATE = item.transactionDate;
                    trans.CURRENCYID = item.currencyId;
                    trans.CURRENCYRATE = item.currencyRate;
                    trans.POSTEDDATETIME = DateTime.Now;
                    trans.ISAPPROVED = item.isApproved;
                    trans.POSTEDBY = item.postedBy;
                    trans.APPROVEDBY = item.approvedBy;
                    trans.APPROVEDDATE = item.approvedDate;
                    trans.APPROVEDDATETIME = item.approvedDateTime;
                    trans.SOURCEAPPLICATIONID = item.sourceApplicationId;
                    trans.COMPANYID = item.companyId;


                    trans.GLACCOUNTID = item.glAccountId;
                    trans.SOURCEREFERENCENUMBER = item.sourceReferenceNumber;
                    trans.CASAACCOUNTID = item.casaAccountId;
                    trans.DEBITAMOUNT = item.debitAmount;
                    trans.CREDITAMOUNT = item.creditAmount;
                    trans.SOURCEBRANCHID = item.sourceBranchId;
                    trans.DESTINATIONBRANCHID = item.destinationBranchId;

                    transactions.Add(trans);
                
            }

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        public void UpdateCustomTransactions(string batchCode)
        {

            //var result = (from p in context.TBL_CUSTOM_FIANCE_TRANSACTION
            //              where p.BATCHCODE == batchCode && p.CONSUMED == false
            //                select new CustomFinanceTransactionViewModel()
            //                  {

            //                  });
            var result = context.TBL_CUSTOM_FIANCE_TRANSACTION.Where(x => x.CONSUMED == false && x.DATETIMECONSUMED == null &&  x.BATCHCODE.Contains(batchCode)).ToList();
            result.ForEach(a => a.CONSUMED = true);
            result.ForEach(a => a.DATETIMECONSUMED = DateTime.Now);

            context.SaveChanges();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransaction(List<FinanceTransactionViewModel> inputTransactions)
        {
           var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            //transaction.batchCode = batchCode;

            var transactionCount = (inputTransactions.Count());
            
            if(transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new Exception("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount); 

            //transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            //transaction.transactionDetails.Sum(x => x.creditAmount);
            //var sumDebit = debitSum.FirstOrDefault();


            if (debitSum != creditSum)
                throw new Exception("Total Debit Amount should equal Total Credit Amount");


            foreach (var item in inputTransactions)
            {
  
                    if (item.debitAmount != 0 && item.creditAmount != 0)
                        throw new Exception("Debit or Credit Amount should be 0");

                    if (item.debitAmount < 0)
                        throw new Exception("Debit Amount should NOT be less than 0");

                    if (item.creditAmount < 0)
                        throw new Exception("Credit Amount should NOT be less than 0");

                    var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                    GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                    if (glClass == GLClassEnum.CASA)
                    {
                        if (item.casaAccountId == null)
                            throw new Exception($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                        //UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                        

                        var casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == item.casaAccountId).FirstOrDefault();

                        if (casa == null)
                            throw new Exception($"CASA account number {item.sourceReferenceNumber} does not exist in the CASA table for this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    }
                    else if (glClass == GLClassEnum.LoanSchedule)
                    {
                        item.casaAccountId = null;
                        int referenceCount = 0;

                        referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                        if (referenceCount <= 0)
                            throw new Exception($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                    }
                    //else
                    //{
                    //    item.casaAccountId = null;
                    //}
                
            }

            //api call
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                TransactionPosting tran = new TransactionPosting(context);
                bool data = false;

                Task.Run(async () => { data = await tran.APITransactionPosting(inputTransactions); }).GetAwaiter().GetResult();

                if(data == true)
                {
                    PostTransactionSub(batchCode, inputTransactions, transactions);
                    UpdateCustomTransactions(batchCode);
                }
                else
                {
                    //display message
                    throw new Exception($"Transaction Failed.");
                }
                    
            }
            else
               PostTransactionSub(batchCode, inputTransactions, transactions);

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void PostTransactionSub(string batchCode, List<FinanceTransactionViewModel> inputTransactions, List<TBL_FINANCE_TRANSACTION> transactions)
        {
            foreach (var item in inputTransactions)
            {
 
                    var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                    GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                    if (glClass == GLClassEnum.CASA)
                    {

                        UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                    }
                    else if (glClass == GLClassEnum.LoanSchedule)
                    {
                        item.casaAccountId = null;
                        int referenceCount = 0;

                        referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);
                    }
                    else
                    { item.casaAccountId = null; }

                    TBL_FINANCE_TRANSACTION trans = new TBL_FINANCE_TRANSACTION();

                    trans.BATCHCODE = batchCode;
                    //trans.OPERATIONID = mainItem.operationId;
                    trans.OPERATIONID = (int)item.operationId;
                    trans.DESCRIPTION = item.description;
                    trans.VALUEDATE = item.valueDate;
                    trans.POSTEDDATE = item.transactionDate;
                    trans.CURRENCYID = item.currencyId;
                    trans.CURRENCYRATE = item.currencyRate;
                    trans.POSTEDDATETIME = DateTime.Now;
                    trans.ISAPPROVED = item.isApproved;
                    trans.POSTEDBY = item.postedBy;
                    trans.APPROVEDBY = item.approvedBy;
                    trans.APPROVEDDATE = item.approvedDate;
                    trans.APPROVEDDATETIME = item.approvedDateTime;
                    trans.SOURCEAPPLICATIONID = item.sourceApplicationId;
                    trans.COMPANYID = item.companyId;


                    trans.GLACCOUNTID = item.glAccountId;
                    trans.SOURCEREFERENCENUMBER = item.sourceReferenceNumber;
                    trans.CASAACCOUNTID = item.casaAccountId;
                    trans.DEBITAMOUNT = item.debitAmount;
                    trans.CREDITAMOUNT = item.creditAmount;
                    trans.SOURCEBRANCHID = item.sourceBranchId;
                    trans.DESTINATIONBRANCHID = item.destinationBranchId;

                    transactions.Add(trans);
                
            }
        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public List<FinanceTransactionViewModel> PostCollateralSearch(CasaLienViewModel model)
        {
            var lienSearchAmount = this.context.TBL_CASA_LIEN.FirstOrDefault(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).LIENCREDITAMOUNT;


            var data = new TBL_CASA_LIEN
            {


                PRODUCTACCOUNTNUMBER = model.productAccountNumber,
                LIENREFERENCENUMBER = model.lienReferenceNumber, //CommonHelpers.GenerateRandomDigitCode(10),
                BRANCHID = model.branchId,
                COMPANYID = model.companyId,
                LIENCREDITAMOUNT = 0,
                LIENDEBITAMOUNT = lienSearchAmount,//creditOperations.GetCollateralSearchChargeAmount(model.stateId),
                LIENTYPEID = (short)LienTypeEnum.CollateralSearch,
                CREATEDBY = model.createdBy,
                DESCRIPTION = "",
                DATECREATED = generalSetup.GetApplicationDate()
            };

            context.TBL_CASA_LIEN.Add(data);                       

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CollateralSearch;
            debit.description = "Customer Collateral Search Charge";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = lienSearchAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;

            
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CollateralSearch;
            credit.description = "Customer Collateral Search Charge";
            credit.valueDate = debit.valueDate;
            credit.transactionDate = debit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = debit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;

            var chargeGL = this.context.TBL_COLLATERAL_TYPE.FirstOrDefault(x => x.COLLATERALTYPEID == (int)CollateralTypeEnum.Property).CHARGEGLACCOUNTID.Value;
            credit.glAccountId = chargeGL;

            credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = lienSearchAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LienAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Applied for lien with reference number: {model.lienReferenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId,   int companyId)
        {
            var baseCurrency = this.context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;
            TransactionPosting transPosting = new TransactionPosting(context);
            var data = new CurrencyExchangeRateViewModel();
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {

                if (currencyId == baseCurrency)
                {
                    return new CurrencyExchangeRateViewModel { baseCurrencyId = baseCurrency, currencyId = currencyId, buyingRate = 1, sellingRate = 1, date = date, isBaseCurrency = true };
                }
                else
                {
                    var fromCurrencyCode = this.context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == baseCurrency).CURRENCYCODE;
                    var toCurrencyCode = this.context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == currencyId).CURRENCYCODE;
                    var rateCode = "TTB";
                    Task.Run(async () => { data = await transPosting.GetExchangeRate(fromCurrencyCode, toCurrencyCode, rateCode); }).GetAwaiter().GetResult();
                    return new CurrencyExchangeRateViewModel
                    {
                        baseCurrencyId = baseCurrency,
                        currencyId = data.currencyId,
                        buyingRate = data.buyingRate,
                        sellingRate = data.sellingRate,
                        date = data.date,
                        isBaseCurrency = false
                    };
                }
 
                //return data;
            }
            else
            {

                //var baseCurrency = this.context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;

                //CurrencyExchangeRateViewModel rateInfo = new CurrencyExchangeRateViewModel();

                if (currencyId == baseCurrency)
                {
                    return new CurrencyExchangeRateViewModel { baseCurrencyId = baseCurrency, currencyId = currencyId, buyingRate = 1, sellingRate = 1, date = date, isBaseCurrency = true };
                }
                else
                {
                    //DateTime date = generalSetup.GetApplicationDate().Date;
                    var rateInfo = (from x in this.context.TBL_CURRENCY_RATE
                                    where x.CURRENCYID == currencyId && x.DATE == date.Date
                                    select x).FirstOrDefault();

                    if (rateInfo == null)
                        throw new Exception($"Exchange rate for {date} is not defined. Define the exchange rate and try again");

                    return new CurrencyExchangeRateViewModel
                    {
                        baseCurrencyId = rateInfo.BASECURRENCYID,
                        currencyId = rateInfo.CURRENCYID,
                        buyingRate = rateInfo.BUYINGRATE,
                        sellingRate = rateInfo.SELLINGRATE,
                        date = rateInfo.DATE,
                        isBaseCurrency = false
                    };
                }
            }


            
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model)

        {
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Loan Daily Interest Accrual Posting";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Loan Daily Interest Accrual Posting";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; 

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Loan Daily Interest Accrual Posting: {product.PRODUCTCODE}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)

        {

            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Authorised Overdraft Daily Interest Accrual Posting";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
           
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Authorised Overdraft Daily Interest Accrual Posting";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; ;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Authorised Overdraft Daily Interest Accrual Posting: {product.PRODUCTCODE}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)
        {

            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Unauthorised Overdraft Daily Interest Accrual Posting";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Unauthorised Overdraft Daily Interest Accrual Posting";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Unauthorised Overdraft Daily Interest Accrual Posting: {product.PRODUCTCODE}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model)
        {

            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Past Due Daily Interest Accrual Posting";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
           
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Past Due Daily Interest Accrual Posting";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; ;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Past Due Daily Interest Accrual Posting: {product.PRODUCTCODE}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model)

        {
            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Past Due Daily Principal Accrual Posting";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
          
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Past Due Daily Principal Accrual Posting";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; ;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyPrincipalAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Past Due Daily Principal Accrual Posting: {product.PRODUCTCODE}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description)
        {
           // FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System; ;
            debit.approvedBy = (int)SystemStaff.System; ;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;            
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System; ;
            credit.approvedBy = (int)SystemStaff.System; ;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = model.loanRefNo;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            return null;

        }

        public FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description)
        {
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = model.loanRefNo;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            


            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanChargeFeesPosting(LoanViewModel model)

        {
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            //FinanceTransactionViewModel feeTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

                debit.operationId = (int)OperationsEnum.TermLoanBooking;
                debit.description = "Fee charge";
                debit.valueDate = generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;

                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = model.loanReferenceNumber;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = (decimal)model.totalAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = model.branchId;
                debit.destinationBranchId = casa.BRANCHID;

            var feeGL = this.context.TBL_CHARGE_FEE.FirstOrDefault(x => x.CHARGEFEEID == model.chargeFeeId);//.Select(x => x.GLAccountId).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.TermLoanBooking;
                credit.description = "Fee charge";
                credit.valueDate = generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = feeGL.GLACCOUNTID;
                credit.sourceReferenceNumber = model.loanReferenceNumber;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = (decimal)model.totalAmount;
                credit.sourceBranchId = model.branchId;
                credit.destinationBranchId = model.branchId;
            //// Audit Section ---------------------------            

            //return output;
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Interval Fees and Commission charge Posting : {model.loanReferenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanPrepaymentPosting (LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;        
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }

        public FinanceTransactionViewModel BuildChargeReversalPosting (LoanChargeFeeViewModel model)
        {
            //*FinanceTransactionViewModel*/ loanTransaction = new FinanceTransactionViewModel();


            var loanData = this.context.TBL_LOAN.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            if (model.feeAmountDiff < 0)

            {

                FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                debit.operationId = (int)OperationsEnum.Fee_chargeChange;
                debit.description = "Charge Reversal";
                debit.valueDate = generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = Math.Abs(model.feeAmountDiff);
                debit.creditAmount = 0;
                debit.sourceBranchId = loanData.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                var feeGL = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();
                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.Fee_chargeChange;
                credit.description = "Charge Reversal";
                credit.valueDate = generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = feeGL;
                credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                credit.casaAccountId = casa.CASAACCOUNTID;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(model.feeAmountDiff);
                credit.sourceBranchId = loanData.BRANCHID;
                credit.destinationBranchId = loanData.BRANCHID;


                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                inputTransactions.Add(debit);
                inputTransactions.Add(credit);
                PostTransaction(inputTransactions);
            }
            else
            {

                FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                debit.operationId = (int)OperationsEnum.Fee_chargeChange;
                debit.description = "Charge Reversal";
                debit.valueDate = generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = 0;
                debit.creditAmount = Math.Abs(model.feeAmountDiff);
                debit.sourceBranchId = loanData.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                var feeGL = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();
                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.Fee_chargeChange;
                credit.description = "Charge Reversal";
                credit.valueDate = generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = feeGL;

                credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                credit.casaAccountId = casa.CASAACCOUNTID;
                credit.debitAmount = Math.Abs(model.feeAmountDiff);
                credit.creditAmount = 0;
                credit.sourceBranchId = loanData.BRANCHID;
                credit.destinationBranchId = loanData.BRANCHID;

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                inputTransactions.Add(debit);
                inputTransactions.Add(credit);
                PostTransaction(inputTransactions);
            }
            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);
            debit.operationId = (int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;         
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model , decimal postedAmount, int creditGL, string description)
        {
            var loanData  = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount; ;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            return null;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel BuildCustomerApplicationChargeOrChargeReversalPosting (string postType, int loanId, GeneralEntity model, decimal postedAmount, int creditGL, string description)
        {
            var loanData = this.context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanId).FirstOrDefault();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);
            if(postType == "Post")
            {
                FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                debit.operationId = (int)OperationsEnum.LoanTermination;
                debit.description = description;
                debit.valueDate = generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = postedAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = casa.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.LoanTermination;
                credit.description = description;
                credit.valueDate = generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = creditGL;

                credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = postedAmount; ;
                credit.sourceBranchId = casa.BRANCHID;
                credit.destinationBranchId = casa.BRANCHID;

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                inputTransactions.Add(debit);
                inputTransactions.Add(credit);
                PostTransaction(inputTransactions);

                return null;
            }
            else
            {
                FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                debit.operationId = (int)OperationsEnum.LoanTermination;
                debit.description = description;
                debit.valueDate = generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = postedAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = casa.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.LoanTermination;
                credit.description = description;
                credit.valueDate = generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = creditGL;

                credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = postedAmount; ;
                credit.sourceBranchId = casa.BRANCHID;
                credit.destinationBranchId = casa.BRANCHID;

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                inputTransactions.Add(debit);
                inputTransactions.Add(credit);
                PostTransaction(inputTransactions);

                return null;
            }

        }

        public FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId)

        {
            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.InterestSuspension;
            debit.description = "Interest Suspension";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = staffId;
            debit.approvedBy = staffId;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = applicationDate;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);       
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.InterestSuspension;
            credit.description = "Interest Suspension";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = staffId;
            credit.approvedBy = staffId;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = applicationDate;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            var InterestSuspensionGL = 8;////to be change when interestsuspenses is created
            credit.glAccountId = InterestSuspensionGL; ///product.InterestIncomeExpenseGL.Value;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Interest Suspension Posting: {product.PRODUCTCODE}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

    }
}
