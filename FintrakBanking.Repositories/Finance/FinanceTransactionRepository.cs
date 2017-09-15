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
using FintrakBanking.Interfaces.CreditOperations;

namespace FintrakBanking.Repositories.Finance

{
    public class FinanceTransactionRepository : IFinanceTransactionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ICreditOperationsRepository creditOperations;

        public FinanceTransactionRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail, 
                                            ICreditOperationsRepository _creditOperations, FinTrakBankingContext _context)
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
    }
}
