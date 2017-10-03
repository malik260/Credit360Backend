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

namespace FintrakBanking.Repositories.Finance

{
    public class FinanceTransactionRepository : IFinanceTransactionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanOperationsRepository creditOperations;

        public FinanceTransactionRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail, 
                                            ILoanOperationsRepository _creditOperations, FinTrakBankingContext _context)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            this.creditOperations = _creditOperations;
        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddCollateralSearchLien(CasaLienViewModel model)
        {

            var data =   new tbl_CASA_Lien
            {
                ProductAccountNumber = model.productAccountNumber,
                LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                SourceReferenceNumber = model.sourceReferenceNumber,
                BranchId = model.userBranchId,
                CompanyId = model.companyId,
                LienCreditAmount = creditOperations.GetCollateralSearchChargeAmount(model.stateId),
                LienDebitAmount = 0,
                LienTypeId = (short) LienTypeEnum.CollateralSearch,
                CreatedBy = model.createdBy,
                Description = "lien placed due to loan application collateral search", // model.description,
                DateCreated = generalSetup.GetApplicationDate()
                 
            };

            context.tbl_CASA_Lien.Add(data);

            // Audit Section ---------------------------            
         
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.LienAdded,
                    StaffId = model.createdBy,
                    BranchId = model.branchId,
                    Detail = $"Applied for lien with reference number: { model.sourceReferenceNumber}",
                    IPAddress = model.userIPAddress ,
                    Url = model.applicationUrl,
                    ApplicationDate = generalSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };
                this.auditTrail.AddAuditTrail(audit);
          
            //end of Audit section -------------------------------
           return  context.SaveChanges() != 0;
             
        }

        private void UpdateCASABalances(int casaAccountId, decimal debitAmount, decimal creditAmount)
        {
            var account = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == casaAccountId);

            if (debitAmount > 0)
            {
                account.LedgerBalance = account.LedgerBalance - debitAmount;
                account.AvailableBalance = account.AvailableBalance - debitAmount;
            }
            else
            {
                account.LedgerBalance = account.LedgerBalance + creditAmount;
                account.AvailableBalance = account.AvailableBalance +  creditAmount;
            }
        }

        public CasaBalanceViewModel GetCASABalance(int casaAccountId)
        {
            var account = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == casaAccountId);

            return new CasaBalanceViewModel {availableBalance = account.AvailableBalance, ledgerBalance = account.LedgerBalance };
        }

        public CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId)
        {
            CasaBalanceViewModel balance = new CasaBalanceViewModel();

            var trans = (from data in context.tbl_Finance_Transaction
                         where data.CasaAccountId == casaAccountId
                         select data.CreditAmount - data.DebitAmount).Sum();

            var account = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == casaAccountId);

            var lienBalance = GetLienBalance(account.ProductAccountNumber);

            balance.ledgerBalance = trans;
            balance.availableBalance = trans - lienBalance;

            return balance;
        }

        public decimal GetLienBalance(string productAccountNumber)
        {            
            var balance = (from data in context.tbl_CASA_Lien
                         where data.ProductAccountNumber == productAccountNumber
                         select data.LienCreditAmount - data.LienDebitAmount).Sum();

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

            List<tbl_Finance_Transaction> transactions = new List<tbl_Finance_Transaction>();

            var debitSum = (from a in inputTransactions
                            select a.transactionDetails.Sum(x => x.debitAmount));

            //transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = (from a in inputTransactions
                             select a.transactionDetails.Sum(x => x.creditAmount));
            //transaction.transactionDetails.Sum(x => x.creditAmount);

            if (debitSum != creditSum)
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

                    var glInfo = context.tbl_Chart_Of_Account.FirstOrDefault(x => x.GLAccountId == item.glAccountId);

                    GLClassEnum glClass = (GLClassEnum) glInfo.GLClassId;

                    if (glClass == GLClassEnum.CASA)
                    {
                        if (item.casaAccountId == null)
                            throw new Exception($"Specify the CASA Account Number in this transaction for GL Code {glInfo.AccountCode}");

                        UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                    }
                    else
                    { item.casaAccountId = null; }

                    tbl_Finance_Transaction trans = new tbl_Finance_Transaction();

                    trans.BatchCode = batchCode;
                    trans.OperationId = mainItem.operationId;
                    trans.Description = mainItem.description;
                    trans.ValueDate = mainItem.valueDate;
                    //trans.PostedDate = mainItem.transactionDate;
                    trans.CurrencyId = mainItem.currencyId;
                    trans.CurrencyRate = mainItem.currencyRate;
                    trans.PostedDateTime = DateTime.Now;
                    trans.IsApproved = mainItem.isApproved;
                    trans.PostedBy = mainItem.postedBy;
                    trans.ApprovedBy = mainItem.approvedBy;
                    trans.ApprovedDate = mainItem.approvedDate;
                    trans.ApprovedDateTime = mainItem.approvedDateTime;
                    trans.SourceApplicationId = mainItem.sourceApplicationId;
                    trans.CompanyId = mainItem.companyId;


                    trans.GLAccountId = item.glAccountId;
                    trans.SourceReferenceNumber = item.sourceReferenceNumber;
                    trans.CasaAccountId = item.casaAccountId;
                    trans.DebitAmount = item.debitAmount;
                    trans.CreditAmount = item.creditAmount;
                    trans.SourceBranchId = item.sourceBranchId;
                    trans.DestinationBranchId = item.destinationBranchId;

                    transactions.Add(trans);
                }
            }

            this.context.tbl_Finance_Transaction.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostCollateralSearch(CasaLienViewModel model)
        {
            var lienSearchAmount = this.context.tbl_CASA_Lien.FirstOrDefault(x => x.LienReferenceNumber == model.lienReferenceNumber).LienCreditAmount;


            var data = new tbl_CASA_Lien
            {


                ProductAccountNumber = model.productAccountNumber,
                LienReferenceNumber = model.lienReferenceNumber, //CommonHelpers.GenerateRandomDigitCode(10),
                BranchId = model.branchId,
                CompanyId = model.companyId,
                LienCreditAmount = 0,
                LienDebitAmount = lienSearchAmount,//creditOperations.GetCollateralSearchChargeAmount(model.stateId),
                LienTypeId = (short)LienTypeEnum.CollateralSearch,
                CreatedBy = model.createdBy,
                Description = "",
                DateCreated = generalSetup.GetApplicationDate()
            };

            context.tbl_CASA_Lien.Add(data);

            FinanceTransactionViewModel collateralTransaction = new FinanceTransactionViewModel();
            

            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.ProductAccountNumber == model.productAccountNumber && x.CompanyId == model.companyId);

            collateralTransaction.operationId = (int)OperationsEnum.CollateralSearch;
            collateralTransaction.description = "Customer Collateral Search Charge";
            collateralTransaction.valueDate = generalSetup.GetApplicationDate();
            collateralTransaction.transactionDate = collateralTransaction.valueDate;
            collateralTransaction.currencyId = casa.CurrencyId;
            collateralTransaction.currencyRate = GetExchangeRate(collateralTransaction.currencyId,   model.companyId);            
            collateralTransaction.isApproved = true;
            collateralTransaction.postedBy = model.createdBy;
            collateralTransaction.approvedBy = model.createdBy;
            collateralTransaction.approvedDate = collateralTransaction.transactionDate;
            collateralTransaction.approvedDateTime = DateTime.Now;
            collateralTransaction.sourceApplicationId = (short) SourceApplicationEnum.FinTrakBanking;
            collateralTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = casa.ProductAccountNumber;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = lienSearchAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BranchId;

            var chargeGL = this.context.tbl_Collateral_Type.FirstOrDefault(x => x.CollateralTypeId == (int) CollateralTypeEnum.Property).ChargeGLAccountId.Value;
            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = chargeGL;

            credit.sourceReferenceNumber = casa.ProductAccountNumber;
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

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LienAdded,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Applied for lien with reference number: {model.lienReferenceNumber}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return collateralTransaction;

        }

        public double GetExchangeRate(short currencyId,   int companyId)
        {
            var baseCurrency = this.context.tbl_Company.FirstOrDefault(x => x.CompanyId == companyId).CurrencyId;

            if (currencyId == baseCurrency)
            {
                return 1;
            }
            else
            {
                DateTime date = generalSetup.GetApplicationDate().Date;
                var rate = this.context.tbl_Currency_Rate.FirstOrDefault(x => x.BaseCurrencyId ==
                baseCurrency && x.CurrencyId == currencyId && x.Date == date).SellingRate;

                return rate;
            }           
            
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.CreditDailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Loan Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.currencyId, model.companyId);
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.InterestReceivablePayableGL.Value;
            debit.sourceReferenceNumber = product.ProductCode;
            debit.casaAccountId = null;
            debit.debitAmount = model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.InterestIncomeExpenseGL.Value; ;

            credit.sourceReferenceNumber = product.ProductCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Loan Daily Interest Accrual Posting: {product.ProductCode}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.CreditDailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Authorised Overdraft Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.currencyId, model.companyId);
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.InterestReceivablePayableGL.Value;
            debit.sourceReferenceNumber = product.ProductCode;
            debit.casaAccountId = null;
            debit.debitAmount = model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.InterestIncomeExpenseGL.Value; ;

            credit.sourceReferenceNumber = product.ProductCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Authorised Overdraft Daily Interest Accrual Posting: {product.ProductCode}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)
        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.CreditDailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Unauthorised Overdraft Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.currencyId, model.companyId);
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.InterestReceivablePayableGL.Value;
            debit.sourceReferenceNumber = product.ProductCode;
            debit.casaAccountId = null;
            debit.debitAmount = model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.InterestIncomeExpenseGL.Value; ;

            credit.sourceReferenceNumber = product.ProductCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Unauthorised Overdraft Daily Interest Accrual Posting: {product.ProductCode}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model)
        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.CreditDailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Past Due Daily Interest Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.currencyId, model.companyId);
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.InterestReceivablePayableGL.Value;
            debit.sourceReferenceNumber = product.ProductCode;
            debit.casaAccountId = null;
            debit.debitAmount = model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.InterestIncomeExpenseGL.Value; ;

            credit.sourceReferenceNumber = product.ProductCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Past Due Daily Interest Accrual Posting: {product.ProductCode}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model)

        {

            FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();

            dailyInterestAccrualTransaction.operationId = (int)OperationsEnum.CreditDailyInterestAccural;
            dailyInterestAccrualTransaction.description = "Past Due Daily Principal Accrual Posting";
            dailyInterestAccrualTransaction.valueDate = generalSetup.GetApplicationDate();
            dailyInterestAccrualTransaction.transactionDate = dailyInterestAccrualTransaction.valueDate;
            dailyInterestAccrualTransaction.currencyId = model.currencyId;
            dailyInterestAccrualTransaction.currencyRate = GetExchangeRate(dailyInterestAccrualTransaction.currencyId, model.companyId);
            dailyInterestAccrualTransaction.isApproved = true;
            dailyInterestAccrualTransaction.postedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedBy = (int)SystemStaff.System;
            dailyInterestAccrualTransaction.approvedDate = dailyInterestAccrualTransaction.transactionDate;
            dailyInterestAccrualTransaction.approvedDateTime = DateTime.Now;
            dailyInterestAccrualTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            dailyInterestAccrualTransaction.companyId = model.companyId;

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == model.productId);
            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = product.InterestReceivablePayableGL.Value;
            debit.sourceReferenceNumber = product.ProductCode;
            debit.casaAccountId = null;
            debit.debitAmount = model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = product.InterestIncomeExpenseGL.Value; ;

            credit.sourceReferenceNumber = product.ProductCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            dailyInterestAccrualTransaction.transactionDetails.Add(debit);
            dailyInterestAccrualTransaction.transactionDetails.Add(credit);


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(dailyInterestAccrualTransaction);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDailyPrincipalAccrual,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Past Due Daily Principal Accrual Posting: {product.ProductCode}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return dailyInterestAccrualTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.currencyId, model.companyId);
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BranchId;


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

            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.currencyId, model.companyId);
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BranchId;


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


                var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId);

                feeTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
                feeTransaction.description = "Fee charge";
                feeTransaction.valueDate = generalSetup.GetApplicationDate();
                feeTransaction.transactionDate = feeTransaction.valueDate;
                feeTransaction.currencyId = casa.CurrencyId;
                feeTransaction.currencyRate = GetExchangeRate(feeTransaction.currencyId, model.companyId);
                feeTransaction.isApproved = true;
                feeTransaction.postedBy = model.createdBy;
                feeTransaction.approvedBy = model.createdBy;
                feeTransaction.approvedDate = feeTransaction.transactionDate;
                feeTransaction.approvedDateTime = DateTime.Now;
                feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                feeTransaction.companyId = model.companyId;

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                debit.sourceReferenceNumber = model.loanReferenceNumber;
                debit.casaAccountId = casa.CasaAccountId;
                debit.debitAmount = (decimal)model.totalAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = model.branchId;
                debit.destinationBranchId = casa.BranchId;

            var feeGL = this.context.tbl_Charge_Fee.FirstOrDefault(x => x.ChargeFeeId == model.chargeFeeId);//.Select(x => x.GLAccountId).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL.GLAccountId;
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

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Interval Fees and Commission charge Posting : {model.loanReferenceNumber}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return feeTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanPrepaymentPosting (LoanPaymentScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == model.loanId && x.CompanyId == model.companyId);

            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == loan.CasaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.currencyId, model.companyId);
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = loan.LoanReferenceNumber;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BranchId;
            debit.destinationBranchId = casa.BranchId;


            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = loan.LoanReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loan.BranchId;
            credit.destinationBranchId = loan.BranchId;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public FinanceTransactionViewModel BuildChargeReversalPosting (LoanChargeFeeViewModel model)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();


            var loanData = this.context.tbl_Loan.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.ChargeReversal;
            loanTransaction.description = "Charge Reversal";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.currencyId, model.companyId);
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
                debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                debit.sourceReferenceNumber = loanData.LoanReferenceNumber;
                debit.casaAccountId = null;
                debit.debitAmount = Math.Abs(model.feeAmountDiff);
                debit.creditAmount = 0;
                debit.sourceBranchId = loanData.BranchId;
                debit.destinationBranchId = casa.BranchId;

                var feeGL = this.context.tbl_Charge_Fee.Where(x => x.ChargeFeeId == model.chargeFeeId).Select(x => x.GLAccountId).FirstOrDefault();
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;

                credit.sourceReferenceNumber = loanData.LoanReferenceNumber;
                credit.casaAccountId = casa.CasaAccountId;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(model.feeAmountDiff);
                credit.sourceBranchId = loanData.BranchId;
                credit.destinationBranchId = loanData.BranchId;

                loanTransaction.transactionDetails.Add(debit);
                loanTransaction.transactionDetails.Add(credit);
            }
            else
            {

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                debit.sourceReferenceNumber = loanData.LoanReferenceNumber;
                debit.casaAccountId = null;
                debit.debitAmount = 0;
                debit.creditAmount = Math.Abs(model.feeAmountDiff);
                debit.sourceBranchId = loanData.BranchId;
                debit.destinationBranchId = casa.BranchId;

                var feeGL = this.context.tbl_Charge_Fee.Where(x => x.ChargeFeeId == model.chargeFeeId).Select(x => x.GLAccountId).FirstOrDefault();
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;

                credit.sourceReferenceNumber = loanData.LoanReferenceNumber;
                credit.casaAccountId = casa.CasaAccountId;
                credit.debitAmount = Math.Abs(model.feeAmountDiff);
                credit.creditAmount = 0;
                credit.sourceBranchId = loanData.BranchId;
                credit.destinationBranchId = loanData.BranchId;

                loanTransaction.transactionDetails.Add(debit);
                loanTransaction.transactionDetails.Add(credit);
            }





            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public FinanceTransactionViewModel PostBuildLoanReversalPosting(LoanPaymentScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == model.loanId && x.CompanyId == model.companyId);

            ////var refNo  = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == model.loanId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanRepayment;
            loanTransaction.description = description; // "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = GetExchangeRate(loanTransaction.currencyId, model.companyId);
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = casa.LoanReferenceNumber;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BranchId;
            debit.destinationBranchId = casa.BranchId;


            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = casa.LoanReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BranchId;
            credit.destinationBranchId = casa.BranchId;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

    }
}
