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
            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            return new CasaBalanceViewModel {availableBalance = account.AVAILABLEBALANCE, ledgerBalance = account.LEDGERBALANCE };
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
        public string PostTransaction(List<FinanceTransactionViewModel> inputTransactions)
        {
           var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            //transaction.batchCode = batchCode;

            var transactionCount = (from a in inputTransactions
                                    select a.transactionDetails.Count());
            
            if(transactionCount.Sum() < 2) //transaction.transactionDetails.Count() < 2
                throw new Exception("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = (from a in inputTransactions
                            select a.transactionDetails.Sum(x => x.debitAmount));

            //transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = (from a in inputTransactions
                             select a.transactionDetails.Sum(x => x.creditAmount));
            //transaction.transactionDetails.Sum(x => x.creditAmount);
            //var sumDebit = debitSum.FirstOrDefault();


            if (debitSum.FirstOrDefault() != creditSum.FirstOrDefault())
                throw new Exception("Total Debit Amount should equal Total Credit Amount");

            foreach (var mainItem in inputTransactions)
            {
                foreach (var item in mainItem.transactionDetails)
                {
                    if (item.debitAmount != 0 && item.creditAmount != 0)
                        throw new Exception("Debit or Credit Amount should be 0");

                    if (item.debitAmount < 0)
                        throw new Exception("Debit Amount should NOT be less than 0");

                    if (item.creditAmount < 0)
                        throw new Exception("Credit Amount should NOT be less than 0");

                    var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                    GLClassEnum glClass = (GLClassEnum) glInfo.GLCLASSID;

                    if (glClass == GLClassEnum.CASA)
                    {
                        if (item.casaAccountId == null)
                            throw new Exception($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                        UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                    }
                    else
                    { item.casaAccountId = null; }

                    TBL_FINANCE_TRANSACTION trans = new TBL_FINANCE_TRANSACTION();

                    trans.BATCHCODE = batchCode;
                    trans.OPERATIONID = mainItem.operationId;
                    trans.DESCRIPTION = mainItem.description;
                    trans.VALUEDATE = mainItem.valueDate;
                    trans.POSTEDDATE = mainItem.transactionDate;
                    trans.CURRENCYID = mainItem.currencyId;
                    trans.CURRENCYRATE = mainItem.currencyRate;
                    trans.POSTEDDATETIME = DateTime.Now;
                    trans.ISAPPROVED = mainItem.isApproved;
                    trans.POSTEDBY = mainItem.postedBy;
                    trans.APPROVEDBY = mainItem.approvedBy;
                    trans.APPROVEDDATE = mainItem.approvedDate;
                    trans.APPROVEDDATETIME = mainItem.approvedDateTime;
                    trans.SOURCEAPPLICATIONID = mainItem.sourceApplicationId;
                    trans.COMPANYID = mainItem.companyId;


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

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostCollateralSearch(CasaLienViewModel model)
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

            FinanceTransactionViewModel collateralTransaction = new FinanceTransactionViewModel();
            

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId);

            collateralTransaction.operationId = (int)OperationsEnum.CollateralSearch;
            collateralTransaction.description = "Customer Collateral Search Charge";
            collateralTransaction.valueDate = generalSetup.GetApplicationDate();
            collateralTransaction.transactionDate = collateralTransaction.valueDate;
            collateralTransaction.currencyId = casa.CURRENCYID;
            collateralTransaction.currencyRate = GetExchangeRate(collateralTransaction.valueDate, collateralTransaction.currencyId,   model.companyId).sellingRate;            
            collateralTransaction.isApproved = true;
            collateralTransaction.postedBy = model.createdBy;
            collateralTransaction.approvedBy = model.createdBy;
            collateralTransaction.approvedDate = collateralTransaction.transactionDate;
            collateralTransaction.approvedDateTime = DateTime.Now;
            collateralTransaction.sourceApplicationId = (short) SourceApplicationEnum.FinTrakBanking;
            collateralTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = lienSearchAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;

            var chargeGL = this.context.TBL_COLLATERAL_TYPE.FirstOrDefault(x => x.COLLATERALTYPEID == (int)CollateralTypeEnum.Property).CHARGEGLACCOUNTID.Value;
            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = chargeGL;

            credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = lienSearchAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            collateralTransaction.transactionDetails.Add(debit);
            collateralTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(PostCollateralSearch(model));
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
            return collateralTransaction;

        }

        public CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId,   int companyId)
        {

            var baseCurrency = this.context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;
            
            //CurrencyExchangeRateViewModel rateInfo = new CurrencyExchangeRateViewModel();

            if (currencyId == baseCurrency)
            {
                return new CurrencyExchangeRateViewModel { baseCurrencyId = baseCurrency, currencyId = currencyId, buyingRate = 1, sellingRate = 1, date = date, isBaseCurrency = true};
            }
            else
            {
                //DateTime date = generalSetup.GetApplicationDate().Date;
                var rateInfo = (from x in this.context.TBL_CURRENCY_RATE
                            where x.CURRENCYID == currencyId && x.DATE == date.Date
                                select x).FirstOrDefault();

                if (rateInfo == null)
                    throw new Exception($"Exchange rate for {date} is not defined. Define the exchange rate and try again");

                return new CurrencyExchangeRateViewModel {
                    baseCurrencyId = rateInfo.BASECURRENCYID,
                    currencyId = rateInfo.CURRENCYID,
                    buyingRate = rateInfo.BUYINGRATE,
                    sellingRate = rateInfo.SELLINGRATE,
                    date = rateInfo.DATE,
                    isBaseCurrency = false 
                };
            }           
            
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.DailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Loan Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.valueDate, dailyInterestAccrualTransaction.currencyId, model.companyId).sellingRate;
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; 

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
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
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.DailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Authorised Overdraft Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.valueDate, dailyInterestAccrualTransaction.currencyId, model.companyId).sellingRate;
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; ;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
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
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)
        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.DailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Unauthorised Overdraft Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.valueDate, dailyInterestAccrualTransaction.currencyId, model.companyId).sellingRate;
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
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
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model)
        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.DailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Past Due Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.valueDate, dailyInterestAccrualTransaction.currencyId, model.companyId).sellingRate;
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; ;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
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
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.DailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Past Due Daily Principal Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.valueDate,dailyInterestAccrualTransaction.currencyId, model.companyId).sellingRate;
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value; ;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
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
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CURRENCYID;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.valueDate,loanTransaction.currencyId, model.companyId).sellingRate;
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = (int)SystemStaff.System; ;
            loanTransaction.approvedBy = (int)SystemStaff.System; ;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = model.loanRefNo;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CURRENCYID;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.valueDate, loanTransaction.currencyId, model.companyId).sellingRate;
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = model.loanRefNo;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanChargeFeesPosting(LoanViewModel model)

        {
                FinanceTransactionViewModel feeTransaction = new FinanceTransactionViewModel();


                var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);

                feeTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
                feeTransaction.description = "Fee charge";
                feeTransaction.valueDate = generalSetup.GetApplicationDate();
                feeTransaction.transactionDate = feeTransaction.valueDate;
                feeTransaction.currencyId = casa.CURRENCYID;
                feeTransaction.currencyRate = GetExchangeRate(feeTransaction.valueDate,feeTransaction.currencyId, model.companyId).sellingRate;
                feeTransaction.isApproved = true;
                feeTransaction.postedBy = model.createdBy;
                feeTransaction.approvedBy = model.createdBy;
                feeTransaction.approvedDate = feeTransaction.transactionDate;
                feeTransaction.approvedDateTime = DateTime.Now;
                feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                feeTransaction.companyId = model.companyId;

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = model.loanReferenceNumber;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = (decimal)model.totalAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = model.branchId;
                debit.destinationBranchId = casa.BRANCHID;

            var feeGL = this.context.TBL_CHARGE_FEE.FirstOrDefault(x => x.CHARGEFEEID == model.chargeFeeId);//.Select(x => x.GLAccountId).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL.GLACCOUNTID;
                credit.sourceReferenceNumber = model.loanReferenceNumber;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = (decimal)model.totalAmount;
                credit.sourceBranchId = model.branchId;
                credit.destinationBranchId = model.branchId;


                feeTransaction.transactionDetails.Add(debit);
                feeTransaction.transactionDetails.Add(credit);

            //    output.Add(feeTransaction);
            ////}

            //// Audit Section ---------------------------            

            //return output;
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(feeTransaction);
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
            return feeTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanPrepaymentPosting (LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CURRENCYID;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.valueDate, loanTransaction.currencyId, model.companyId).sellingRate;
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public FinanceTransactionViewModel BuildChargeReversalPosting (LoanChargeFeeViewModel model)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();


            var loanData = this.context.TBL_LOAN.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.Fee_chargeChange;
            loanTransaction.description = "Charge Reversal";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CURRENCYID;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.valueDate,loanTransaction.currencyId, model.companyId).sellingRate;
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            if (model.feeAmountDiff < 0)

            {

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = Math.Abs(model.feeAmountDiff);
                debit.creditAmount = 0;
                debit.sourceBranchId = loanData.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                var feeGL = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;

                credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                credit.casaAccountId = casa.CASAACCOUNTID;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(model.feeAmountDiff);
                credit.sourceBranchId = loanData.BRANCHID;
                credit.destinationBranchId = loanData.BRANCHID;

                loanTransaction.transactionDetails.Add(debit);
                loanTransaction.transactionDetails.Add(credit);
            }
            else
            {

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = 0;
                debit.creditAmount = Math.Abs(model.feeAmountDiff);
                debit.sourceBranchId = loanData.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                var feeGL = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;

                credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
                credit.casaAccountId = casa.CASAACCOUNTID;
                credit.debitAmount = Math.Abs(model.feeAmountDiff);
                credit.creditAmount = 0;
                credit.sourceBranchId = loanData.BRANCHID;
                credit.destinationBranchId = loanData.BRANCHID;

                loanTransaction.transactionDetails.Add(debit);
                loanTransaction.transactionDetails.Add(credit);
            }





            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            ////var refNo  = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == model.loanId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CURRENCYID;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.valueDate, loanTransaction.currencyId, model.companyId).sellingRate;
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model , decimal postedAmount, int creditGL, string description)
        {
            var loanData  = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();


            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            terminateAndRebookTransaction.operationId = (int)OperationsEnum.LoanTermination;
            terminateAndRebookTransaction.description = description;
            terminateAndRebookTransaction.valueDate = generalSetup.GetApplicationDate();
            terminateAndRebookTransaction.transactionDate = terminateAndRebookTransaction.valueDate;
            terminateAndRebookTransaction.currencyId = casa.CURRENCYID;
            terminateAndRebookTransaction.currencyRate = GetExchangeRate(terminateAndRebookTransaction.valueDate,terminateAndRebookTransaction.currencyId, model.companyId).sellingRate;
            terminateAndRebookTransaction.isApproved = true;
            terminateAndRebookTransaction.postedBy = model.createdBy;
            terminateAndRebookTransaction.approvedBy = model.createdBy;
            terminateAndRebookTransaction.approvedDate = terminateAndRebookTransaction.transactionDate;
            terminateAndRebookTransaction.approvedDateTime = DateTime.Now;
            terminateAndRebookTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            terminateAndRebookTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount; ;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            terminateAndRebookTransaction.transactionDetails.Add(debit);
            terminateAndRebookTransaction.transactionDetails.Add(credit);

            return terminateAndRebookTransaction;

        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel BuildCustomerApplicationChargeOrChargeReversalPosting (string postType, int loanId, GeneralEntity model, decimal postedAmount, int creditGL, string description)
        {
            var loanData = this.context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanId).FirstOrDefault();

            FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            terminateAndRebookTransaction.operationId = (int)OperationsEnum.LoanTermination;
            terminateAndRebookTransaction.description = description;
            terminateAndRebookTransaction.valueDate = generalSetup.GetApplicationDate();
            terminateAndRebookTransaction.transactionDate = terminateAndRebookTransaction.valueDate;
            terminateAndRebookTransaction.currencyId = casa.CURRENCYID;
            terminateAndRebookTransaction.currencyRate = GetExchangeRate(terminateAndRebookTransaction.valueDate, terminateAndRebookTransaction.currencyId, model.companyId).sellingRate;
            terminateAndRebookTransaction.isApproved = true;
            terminateAndRebookTransaction.postedBy = model.createdBy;
            terminateAndRebookTransaction.approvedBy = model.createdBy;
            terminateAndRebookTransaction.approvedDate = terminateAndRebookTransaction.transactionDate;
            terminateAndRebookTransaction.approvedDateTime = DateTime.Now;
            terminateAndRebookTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            terminateAndRebookTransaction.companyId = model.companyId;

            if(postType == "Post")
            {
                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = postedAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = casa.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = creditGL;

                credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = postedAmount; ;
                credit.sourceBranchId = casa.BRANCHID;
                credit.destinationBranchId = casa.BRANCHID;

                terminateAndRebookTransaction.transactionDetails.Add(debit);
                terminateAndRebookTransaction.transactionDetails.Add(credit);

                return terminateAndRebookTransaction;
            }
            else
            {
                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = postedAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = casa.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = creditGL;

                credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = postedAmount; ;
                credit.sourceBranchId = casa.BRANCHID;
                credit.destinationBranchId = casa.BRANCHID;

                terminateAndRebookTransaction.transactionDetails.Add(debit);
                terminateAndRebookTransaction.transactionDetails.Add(credit);

                return terminateAndRebookTransaction;
            }

        }

        public FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.InterestSuspension;
            dailyInterestAccrualTransaction.description = "Interest Suspension";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.valueDate,dailyInterestAccrualTransaction.currencyId, model.companyId).sellingRate;
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = staffId;
            dailyInterestAccrualTransaction.approvedBy = staffId;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = applicationDate;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            var InterestSuspensionGL = 8;////to be change when interestsuspenses is created
            credit.glAccountId = InterestSuspensionGL; ///product.InterestIncomeExpenseGL.Value;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
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
            return dailyInterestAccrualTransaction;

        }

    }
}
