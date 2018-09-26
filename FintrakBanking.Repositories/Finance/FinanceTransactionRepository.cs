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
using FintrakBanking.ViewModels.Credit;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Credit;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using FintrakBanking.Common.CustomException;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;

namespace FintrakBanking.Repositories.Finance

{
    public class FinanceTransactionRepository : IFinanceTransactionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        //private ILoanOperationsRepository creditOperations;
        private CustomerDetails customerInfo;
        private IIntegrationWithFinacle integration;
        private ITwoFactorAuthIntegrationService twoFactoeAuth;
        bool USE_THIRD_PARTY_INTEGRATION;
        bool USE_TWO_FACTOR_AUTHENTICATION;
        public FinanceTransactionRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail, IIntegrationWithFinacle _integration,
                                            //ILoanOperationsRepository _creditOperations, 
                                            ITwoFactorAuthIntegrationService _twoFactoeAuth,
                                            FinTrakBankingContext _context, CustomerDetails customerInfo)
        {
            this.context = _context;
            this.customerInfo = customerInfo;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            this.integration = _integration;
            this.twoFactoeAuth = _twoFactoeAuth;
            //this.creditOperations = _creditOperations;
            var global = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (global != null)
            {
                USE_THIRD_PARTY_INTEGRATION = global.USE_THIRD_PARTY_INTEGRATION;
                USE_TWO_FACTOR_AUTHENTICATION = global.USE_TWO_FACTOR_AUTHENTICATION;
            }
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
                account.AVAILABLEBALANCE = account.AVAILABLEBALANCE + creditAmount;
            }
        }

        public CasaBalanceViewModel GetCASABalance(int casaAccountId)
        {

            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);
            var data = new CasaBalanceViewModel();
            if (USE_THIRD_PARTY_INTEGRATION)
            {
                data = integration.GetCustomerAccountBalance(account.PRODUCTACCOUNTNUMBER);
                //Task.Run(async () => { data = await customerInfo .GetCustomerAccountBalance(account.PRODUCTACCOUNTNUMBER); }).GetAwaiter().GetResult();
                return data; //  new CasaBalanceViewModel { availableBalance = data.availableBalance, ledgerBalance = 0,accountStatusId = data.accountStatusId);
            }
            else
            {
                return new CasaBalanceViewModel { availableBalance = account.AVAILABLEBALANCE, ledgerBalance = account.LEDGERBALANCE, accountStatusId = (CASAAccountStatusEnum)account.ACCOUNTSTATUSID, currencyId = account.CURRENCYID, accountNo = account.PRODUCTACCOUNTNUMBER, accountName = account.PRODUCTACCOUNTNAME, hasBalance = true };
            }

        }

        public string GetCustomerAccountType(string accountNumber)
        {
            if (USE_THIRD_PARTY_INTEGRATION)
            {
                var type = integration.GetCustomerAccountBalance(accountNumber);
                return type.productType;
            }
            return null;
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
                           select data.LIENAMOUNT).Sum();

            return balance;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransactionBulk(List<FinanceTransactionViewModel> inputTransactions)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            //transaction.batchCode = batchCode;

            var transactionCount = (inputTransactions.Count());

            if (transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new ConditionNotMetException("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);

            //transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            //transaction.transactionDetails.Sum(x => x.creditAmount);
            //var sumDebit = debitSum.FirstOrDefault();

            if (debitSum != creditSum)
                throw new ConditionNotMetException("Total Debit Amount should equal Total Credit Amount");



            foreach (var item in inputTransactions)
            {
                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new ConditionNotMetException("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new ConditionNotMetException("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new ConditionNotMetException("Credit Amount should NOT be less than 0");

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {
                    if (item.casaAccountId == null)
                        throw new ConditionNotMetException($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                    if (referenceCount <= 0)
                        throw new ConditionNotMetException($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
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
            var result = context.TBL_CUSTOM_FIANCE_TRANSACTION.Where(x => x.CONSUMED == false && x.DATETIMECONSUMED == null && x.BATCHCODE.Contains(batchCode)).ToList();
            result.ForEach(a => a.CONSUMED = true);
            result.ForEach(a => a.DATETIMECONSUMED = DateTime.Now);

            context.SaveChanges();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransaction(List<FinanceTransactionViewModel> inputTransactions, bool isBulkPosting = false, TwoFactorAutheticationViewModel twoFADetails = null)
        //public string PostTransaction(List<FinanceTransactionViewModel> inputTransactions, bool isBulkPosting = false)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            var transactionCount = (inputTransactions.Count());

            if (transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new ConditionNotMetException("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            if (debitSum != creditSum)
                throw new ConditionNotMetException("Total Debit Amount should equal Total Credit Amount");

            foreach (var item in inputTransactions)
            {
                item.batchCode = batchCode;

                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new ConditionNotMetException("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new ConditionNotMetException("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new ConditionNotMetException("Credit Amount should NOT be less than 0");

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {
                    if (item.casaAccountId == null)
                        throw new ConditionNotMetException($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    //UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);


                    var casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == item.casaAccountId).FirstOrDefault();

                    if (casa == null)
                        throw new ConditionNotMetException($"CASA account number {item.sourceReferenceNumber} does not exist in the CASA table for this transaction for GL Code {glInfo.ACCOUNTCODE}");

                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                    if (referenceCount <= 0)
                        throw new ConditionNotMetException($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                }
                //else
                //{
                //    item.casaAccountId = null;
                //}

            }

            //api call
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                if (authenticated.authenticated == false)
                    throw new TwoFactorAuthenticationException(authenticated.message);
            }

            if (USE_THIRD_PARTY_INTEGRATION && isBulkPosting == false)
            {
                bool data;

                data = integration.PostTransactions(inputTransactions);

                if (data)
                {
                    PostTransactionSub(batchCode, inputTransactions, transactions);
                    UpdateCustomTransactions(batchCode);
                }
                else
                {
                    throw new SecureException($"Transaction Failed.");
                }
            }
            else
                PostTransactionSub(batchCode, inputTransactions, transactions);

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            var result = context.SaveChanges() > 0;

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
                trans.BATCHCODE2 = item.batchId;

                transactions.Add(trans);

            }
        }


        //[OperationBehavior(TransactionScopeRequired = true)]
        //public List<FinanceTransactionViewModel> PostCollateralSearch(CasaLienViewModel model)
        //{
        //    var lienSearchAmount = this.context.TBL_CASA_LIEN.FirstOrDefault(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).LIENAMOUNT;


        //    //var data = new TBL_CASA_LIEN
        //    //{
        //    //    PRODUCTACCOUNTNUMBER = model.productAccountNumber,
        //    //    LIENREFERENCENUMBER = model.lienReferenceNumber, //CommonHelpers.GenerateRandomDigitCode(10),
        //    //    BRANCHID = model.branchId,
        //    //    COMPANYID = model.companyId,
        //    //    LIENCREDITAMOUNT = 0,
        //    //    LIENDEBITAMOUNT = lienSearchAmount,//creditOperations.GetCollateralSearchChargeAmount(model.stateId),
        //    //    LIENTYPEID = (short)LienTypeEnum.CollateralSearch,
        //    //    CREATEDBY = model.createdBy,
        //    //    DESCRIPTION = "",
        //    //    DATECREATED = generalSetup.GetApplicationDate()
        //    //};

        //    //context.TBL_CASA_LIEN.Add(data);                       

        //    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId);

        //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
        //    debit.operationId = (int)OperationsEnum.CollateralSearch;
        //    debit.description = "Customer Collateral Search Charge";
        //    debit.valueDate = generalSetup.GetApplicationDate();
        //    debit.transactionDate = debit.valueDate;
        //    debit.currencyId = casa.CURRENCYID;
        //    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //    debit.isApproved = true;
        //    debit.postedBy = model.createdBy;
        //    debit.approvedBy = model.createdBy;
        //    debit.approvedDate = debit.transactionDate;
        //    debit.approvedDateTime = DateTime.Now;
        //    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    debit.companyId = model.companyId;

        //    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
        //    debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
        //    debit.casaAccountId = casa.CASAACCOUNTID;
        //    debit.debitAmount = lienSearchAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = model.branchId;
        //    debit.destinationBranchId = casa.BRANCHID;


        //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
        //    credit.operationId = (int)OperationsEnum.CollateralSearch;
        //    credit.description = "Customer Collateral Search Charge";
        //    credit.valueDate = debit.valueDate;
        //    credit.transactionDate = debit.valueDate;
        //    credit.currencyId = casa.CURRENCYID;
        //    credit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //    credit.isApproved = true;
        //    credit.postedBy = model.createdBy;
        //    credit.approvedBy = model.createdBy;
        //    credit.approvedDate = debit.transactionDate;
        //    credit.approvedDateTime = DateTime.Now;
        //    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    credit.companyId = model.companyId;

        //    var chargeGL = this.context.TBL_COLLATERAL_TYPE.FirstOrDefault(x => x.COLLATERALTYPEID == (int)CollateralTypeEnum.Property).CHARGEGLACCOUNTID.Value;
        //    credit.glAccountId = chargeGL;

        //    credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
        //    credit.casaAccountId = null;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = lienSearchAmount;
        //    credit.sourceBranchId = model.branchId;
        //    credit.destinationBranchId = model.branchId;


        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
        //    inputTransactions.Add(debit);
        //    inputTransactions.Add(credit);
        //    PostTransaction(inputTransactions);

        //    // Audit Section ---------------------------            

        //    var audit = new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LienAdded,
        //        STAFFID = model.createdBy,
        //        BRANCHID = model.branchId,
        //        DETAIL = $"Applied for lien with reference number: {model.lienReferenceNumber}",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = generalSetup.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    context.SaveChanges();
        //    return null;

        //}

        public CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId)
        {
            var baseCurrency = this.context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;


            if (USE_THIRD_PARTY_INTEGRATION)
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

                    // integration.
                    var rate = integration.GetExchangeRate(fromCurrencyCode, toCurrencyCode, rateCode);

                    return rate;
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
                    var rateInfo = (from x in this.context.TBL_CURRENCY_EXCHANGERATE
                                    where x.CURRENCYID == currencyId && x.DATE == date.Date && x.RATECODEID == 1
                                    select x).FirstOrDefault();

                    if (rateInfo == null)
                        throw new ConditionNotMetException($"Exchange rate for {date} is not defined. Define the exchange rate and try again");

                    return new CurrencyExchangeRateViewModel
                    {
                        baseCurrencyId = rateInfo.BASECURRENCYID,
                        currencyId = rateInfo.CURRENCYID,
                        buyingRate = rateInfo.EXCHANGERATE,
                        sellingRate = rateInfo.EXCHANGERATE,
                        date = rateInfo.DATE,
                        isBaseCurrency = false
                    };
                }
            }



        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model)

        {

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Loan Daily Interest Accrual Posting";
            debit.valueDate = model.date; //generalSetup.GetApplicationDate();
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
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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

            var batchPost = PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = (int)SystemStaff.System,//model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Loan Daily Interest Accrual Posting: {model.referenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            if (batchPost != null)
            {
                var result = context.SaveChanges() > 0;
                return result;
            }
            return false;

        }

        public FinanceTransactionViewModel PostDailyFeeAccrual(DailyInterestAccrualViewModel model)

        {
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Daily Amortized Fee Posting";
            debit.valueDate = model.date; //generalSetup.GetApplicationDate();
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
            credit.description = "Daily Amortized Fee Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
                DETAIL = $"Daily Amortized Fee Posting: {model.referenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
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
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
                DETAIL = $"Authorised Overdraft Daily Interest Accrual Posting: {model.referenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
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
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
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
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
            //    STAFFID = model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"Past Due Daily Interest Accrual Posting: {model.referenceNumber}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);

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
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            //context.SaveChanges();
            // Audit Section ---------------------------            

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanDailyPrincipalAccrual,
            //    STAFFID = model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"Past Due Daily Principal Accrual Posting: {model.referenceNumber}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            // FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
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
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
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

        public FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
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
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
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

        public bool PostBuildLoanChargeFeesPosting(LoanViewModel model)

        {
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            //var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId orderby details.POSTINGGROUP select details ).ToList();

            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            foreach (var item in postingGroups)
            {
                var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

                foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                {
                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                    decimal debitAmount = 0;
                    if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        debitAmount = (decimal)model.totalAmount * (decimal)(debits.VALUE / 100.0);
                    else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        debitAmount = (decimal)debits.VALUE;

                    debit.operationId = (int)OperationsEnum.TermLoanBooking;//// to be change 
                    debit.description = $"Fee charge on {debits.DESCRIPTION}";
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
                    debit.debitAmount = debitAmount;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = model.branchId;
                    debit.destinationBranchId = casa.BRANCHID;

                    inputTransactions.Add(debit);
                }

                foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                {
                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                    decimal creditAmount = 0;
                    if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        creditAmount = (decimal)model.totalAmount * (decimal)(credits.VALUE / 100.0);
                    else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        creditAmount = (decimal)credits.VALUE;

                    credit.operationId = (int)OperationsEnum.TermLoanBooking;
                    credit.description = $"Fee charge on {credits.DESCRIPTION}";
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
                    credit.glAccountId = (int)credits.GLACCOUNTID1;
                    credit.sourceReferenceNumber = model.loanReferenceNumber;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = creditAmount;
                    credit.sourceBranchId = model.branchId;
                    credit.destinationBranchId = model.branchId;

                    inputTransactions.Add(credit);

                }
            }


            var batchPost = PostTransaction(inputTransactions);

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
            //context.SaveChanges();
            //return null;

            if (batchPost != null)
            {
                var result = context.SaveChanges() > 0;
                return result;
            }
            return false;

        }

        public FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
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
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
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
            PostTransaction(inputTransactions, false, twoFactorAuth);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description, int operationId)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            foreach (var item in postingGroups)
            {
                var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

                foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                {
                    decimal debitAmount = 0;
                    if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        debitAmount = (decimal)postedAmount * (decimal)(debits.VALUE / 100.0);
                    else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        debitAmount = (decimal)debits.VALUE;

                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

                    debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
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
                    debit.debitAmount = debitAmount;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = loan.BRANCHID;
                    debit.destinationBranchId = casa.BRANCHID;

                    inputTransactions.Add(debit);
                }

                foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                {
                    decimal creditAmount = 0;
                    if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        creditAmount = (decimal)postedAmount * (decimal)(credits.VALUE / 100.0);
                    else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        creditAmount = (decimal)credits.VALUE;

                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                    credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
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
                    credit.glAccountId = credits.GLACCOUNTID1.Value;
                    credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = creditAmount;
                    credit.sourceBranchId = loan.BRANCHID;
                    credit.destinationBranchId = loan.BRANCHID;

                    inputTransactions.Add(credit);
                }
            }
            PostTransaction(inputTransactions);

            return null;


        }

        //public List<FinanceTransactionViewModel> PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description, int operationId)
        //{
        //    FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

        //    //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

        //    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

        //    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

        //    var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

        //    foreach (var item in postingGroups)
        //    {
        //        var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

        //        foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
        //        {
        //            decimal debitAmount = 0;
        //            if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
        //                debitAmount = (decimal)postedAmount * (decimal)(debits.VALUE / 100.0);
        //            else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
        //                debitAmount = (decimal)debits.VALUE;

        //            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

        //            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
        //            debit.description = description; // "Loan Disbursment Amount";
        //            debit.valueDate = generalSetup.GetApplicationDate();
        //            debit.transactionDate = debit.valueDate;
        //            debit.currencyId = casa.CURRENCYID;
        //            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //            debit.isApproved = true;
        //            debit.postedBy = model.createdBy;
        //            debit.approvedBy = model.createdBy;
        //            debit.approvedDate = debit.transactionDate;
        //            debit.approvedDateTime = DateTime.Now;
        //            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //            debit.companyId = model.companyId;
        //            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
        //            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
        //            debit.casaAccountId = casa.CASAACCOUNTID;
        //            debit.debitAmount = debitAmount;
        //            debit.creditAmount = 0;
        //            debit.sourceBranchId = loan.BRANCHID;
        //            debit.destinationBranchId = casa.BRANCHID;

        //            inputTransactions.Add(debit);
        //        }

        //        foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
        //        {
        //            decimal creditAmount = 0;
        //            if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
        //                creditAmount = (decimal)postedAmount * (decimal)(credits.VALUE / 100.0);
        //            else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
        //                creditAmount = (decimal)credits.VALUE;

        //            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
        //            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
        //            credit.description = description; // "Loan Disbursment Amount";
        //            credit.valueDate = generalSetup.GetApplicationDate();
        //            credit.transactionDate = credit.valueDate;
        //            credit.currencyId = casa.CURRENCYID;
        //            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
        //            credit.isApproved = true;
        //            credit.postedBy = model.createdBy;
        //            credit.approvedBy = model.createdBy;
        //            credit.approvedDate = credit.transactionDate;
        //            credit.approvedDateTime = DateTime.Now;
        //            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //            credit.companyId = model.companyId;
        //            credit.glAccountId = credits.GLACCOUNTID1.Value;
        //            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
        //            credit.casaAccountId = null;
        //            credit.debitAmount = 0;
        //            credit.creditAmount = creditAmount;
        //            credit.sourceBranchId = loan.BRANCHID;
        //            credit.destinationBranchId = loan.BRANCHID;

        //            inputTransactions.Add(credit);
        //        }
        //    }

        //    //PostTransaction(inputTransactions);

        //    return inputTransactions;


        //}


        public List<FinanceTransactionViewModel> BuildContingentChargeFeePosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();

            foreach (var item in postingGroups)
            {
                var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

                foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                {
                    decimal debitAmount = 0;
                    if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        debitAmount = (decimal)postedAmount * (decimal)(debits.VALUE / 100.0);
                    else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        debitAmount = (decimal)debits.VALUE;

                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

                    //debit.batchCode = batchCode;
                    debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                    debit.description = description; // "Loan Disbursment Amount";
                    debit.valueDate = applicationDate;
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
                    debit.sourceReferenceNumber = sourceReferenceNumber;
                    debit.casaAccountId = casa.CASAACCOUNTID;
                    debit.debitAmount = debitAmount;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = loan.BRANCHID;
                    debit.destinationBranchId = casa.BRANCHID;

                    inputTransactions.Add(debit);
                }

                foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                {
                    decimal creditAmount = 0;
                    if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        creditAmount = (decimal)postedAmount * (decimal)(credits.VALUE / 100.0);
                    else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        creditAmount = (decimal)credits.VALUE;

                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

                    //credit.batchCode = batchCode;
                    credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                    credit.description = description; // "Loan Disbursment Amount";
                    credit.valueDate = applicationDate;
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
                    credit.glAccountId = credits.GLACCOUNTID1.Value;
                    credit.sourceReferenceNumber = sourceReferenceNumber;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = creditAmount;
                    credit.sourceBranchId = loan.BRANCHID;
                    credit.destinationBranchId = loan.BRANCHID;

                    inputTransactions.Add(credit);
                }
            }

            //PostTransaction(inputTransactions);

            return inputTransactions;

        }

        public List<FinanceTransactionViewModel> BuildContingentPrincipalPosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID);

            //var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = sourceReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = loan.CONTINGENTAMOUNT;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loan.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.PRINCIPALBALANCEGL2.Value;
            credit.sourceReferenceNumber = sourceReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = loan.CONTINGENTAMOUNT;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }

        public List<FinanceTransactionViewModel> BuildContingentPrincipalPostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId)
        {
            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.PRINCIPALBALANCEGL2.Value;
            debit.sourceReferenceNumber = sourceReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = loan.CONTINGENTAMOUNT;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loan.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = sourceReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = loan.CONTINGENTAMOUNT;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }

        public List<FinanceTransactionViewModel> BuildContingentUnEarnedFeePostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId)
        {

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var casaProduct = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID);

            var feeGL = this.context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Primary);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = feeGL.GLACCOUNTID1.Value;
            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
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
            credit.glAccountId = casaProduct.PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }


        public FinanceTransactionViewModel BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth)
        {
            //*FinanceTransactionViewModel*/ loanTransaction = new FinanceTransactionViewModel();


            var loanData = this.context.TBL_LOAN.FirstOrDefault(x => x.CASAACCOUNTID == x.TBL_CASA.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            if (model.feeAmountDiff > 0)

            {

                FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                debit.operationId = (int)OperationsEnum.Fee_chargeChange;
                debit.description = "Charge Reversal";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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

                var feeGL = 1;//this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();
                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.Fee_chargeChange;
                credit.description = "Charge Reversal";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
                PostTransaction(inputTransactions, false, twoFactorAuth);
            }
            else
            {

                FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                debit.operationId = (int)OperationsEnum.Fee_chargeChange;
                debit.description = "Charge Addition";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = Math.Abs(model.feeAmountDiff);
                debit.creditAmount = 0;
                debit.sourceBranchId = loanData.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                var feeGL = 1;//this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();
                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)OperationsEnum.Fee_chargeChange;
                credit.description = "Charge Addition";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(model.feeAmountDiff);
                credit.sourceBranchId = loanData.BRANCHID;
                credit.destinationBranchId = loanData.BRANCHID;

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                inputTransactions.Add(debit);
                inputTransactions.Add(credit);
                PostTransaction(inputTransactions, false, twoFactorAuth);
            }
            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanNegativeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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

        public FinanceTransactionViewModel PostBuildLoanPositiveReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
        public FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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


            debit.glAccountId = creditGL;
            debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = 0;
            debit.creditAmount = postedAmount;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value; ;

            credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = postedAmount;
            credit.creditAmount = 0;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            return null;

        }


       
        public List<FinanceTransactionViewModel> BuildRecapitalisationAccuredInterestReceivablePosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanRecapitilization;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loanData.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;


            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanData.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;
            debit.valueDate = generalSetup.GetApplicationDate();


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            credit.operationId = (int)OperationsEnum.LoanRecapitilization;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loanData.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;
            credit.valueDate = generalSetup.GetApplicationDate();


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            //PostTransaction(inputTransactions);

            return inputTransactions;

        }


        public FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId)

        {
            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.InterestSuspension;
            debit.description = "Interest Suspension";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
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
            debit.sourceReferenceNumber = model.referenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.InterestSuspension;
            credit.description = "Interest Suspension";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
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

            credit.sourceReferenceNumber = model.referenceNumber;
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
                DETAIL = $"Interest Suspension Posting: {model.referenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string TempPostTransaction(List<FinanceTransactionViewModel> inputTransactions)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            var transactionCount = (inputTransactions.Count());

            if (transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new ConditionNotMetException("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            if (debitSum != creditSum)
                throw new ConditionNotMetException("Total Debit Amount should equal Total Credit Amount");

            foreach (var item in inputTransactions)
            {

                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new ConditionNotMetException("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new ConditionNotMetException("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new ConditionNotMetException("Credit Amount should NOT be less than 0");

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {
                    if (item.casaAccountId == null)
                        throw new ConditionNotMetException($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    //UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);


                    var casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == item.casaAccountId).FirstOrDefault();

                    if (casa == null)
                        throw new ConditionNotMetException($"CASA account number {item.sourceReferenceNumber} does not exist in the CASA table for this transaction for GL Code {glInfo.ACCOUNTCODE}");

                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                    if (referenceCount <= 0)
                        throw new ConditionNotMetException($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                }
                //else
                //{
                //    item.casaAccountId = null;
                //}

            }

            //api call
            //var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            //if (setup.USE_THIRD_PARTY_INTEGRATION)
            //{
            //    TransactionPosting tran = new TransactionPosting(context);
            //    bool data = false;

            //    Task.Run(async () => { data = await tran.APITransactionPosting(inputTransactions); }).GetAwaiter().GetResult();

            //    if (data == true)
            //    {
            //        TempPostTransactionSub(batchCode, inputTransactions, transactions);
            //        UpdateCustomTransactions(batchCode);
            //    }
            //    else
            //    {
            //        //display message
            //        throw new SecureException($"Transaction Failed.");
            //    }

            //}
            //else
            TempPostTransactionSub(batchCode, inputTransactions, transactions);

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void TempPostTransactionSub(string batchCode, List<FinanceTransactionViewModel> inputTransactions, List<TBL_FINANCE_TRANSACTION> transactions)
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

        public bool BulkIntegrationPosting(FinanceTransactionStagingViewModel model)

        {
            model.branchId = 100;
            model.staffId = 1;
            //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = model.operationId;
            debit.description = model.description;
            debit.valueDate = model.valueDate; //generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = (short)model.currencyId;
            debit.currencyRate = model.currencyRate;//GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = model.debitGlAccountId;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = model.sourceReferenceNumber;//product.PRODUCTCODE;
            debit.casaAccountId = model.debitCasaAccountId;
            debit.debitAmount = model.actualAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;
            debit.batchId = model.batchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = model.operationId;
            credit.description = model.description;
            credit.valueDate = model.valueDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = (short)model.currencyId;
            credit.currencyRate = model.currencyRate;//GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = model.creditGlAccountId;//product.INTERESTINCOMEEXPENSEGL.Value;

            credit.sourceReferenceNumber = model.sourceReferenceNumber;
            credit.casaAccountId = model.creditCasaAccountId;
            credit.debitAmount = 0;
            credit.creditAmount = model.actualAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;
            credit.batchId = model.batchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            var batchPost = PostTransaction(inputTransactions, true);

            // Audit Section ---------------------------            

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.BulkIntegrationPosting,
            //    STAFFID = (int)SystemStaff.HQ,//model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"{ model.description}: {model.sourceReferenceNumber}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = model.valueDate,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            if (batchPost != null)
            {
                var result = true;
                return result;
            }
            return false;

        }


    }
}