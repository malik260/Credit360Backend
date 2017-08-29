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
        public CasaLienViewModel AddCollateralSearchLien(CasaLienViewModel model)
        {

            var data = new tbl_CASA_Lien
            {
                ProductAccountNumber = model.productAccountNumber,
                LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                SourceReferenceNumber = model.sourceReferenceNumber,
                BranchId = model.branchId,
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
                Detail = $"Applied for lien with reference number: {data.LienReferenceNumber}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransaction(FinanceTransactionViewModel transaction)
        {
            transaction.batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            List<tbl_Finance_Transaction> transactions = new List<tbl_Finance_Transaction>();

            if(transaction.transactionDetails.Count() < 2)
                throw new Exception("Specify both debit and credit transactions");

            var debitSum = transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = transaction.transactionDetails.Sum(x => x.creditAmount);

            if(debitSum != creditSum)
                throw new Exception("Total Debit Amount should equal Total Credit Amount");

            foreach (var item in transaction.transactionDetails)
            {
                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new Exception("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new Exception("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new Exception("Credit Amount should NOT be less than 0");


                tbl_Finance_Transaction trans = new tbl_Finance_Transaction();

                trans.BatchCode = transaction.batchCode;
                trans.OperationId = transaction.operationId;
                trans.Description = transaction.description;
                trans.ValueDate = transaction.valueDate;
                trans.TransactionDate = transaction.transactionDate;
                trans.CurrencyId = transaction.currencyId;
                trans.CurrencyRate = transaction.currencyRate;
                trans.PostedDateTime = DateTime.Now;
                trans.IsApproved = transaction.isApproved;
                trans.PostedBy = transaction.postedBy;
                trans.ApprovedBy = transaction.approvedBy;
                trans.ApprovedDate = transaction.approvedDate;
                trans.ApprovedDateTime = transaction.approvedDateTime;
                trans.SourceApplicationId = transaction.sourceApplicationId;
                trans.CompanyId = transaction.companyId;


                trans.GLAccountId = item.glAccountId;
                trans.SourceReferenceNumber = item.sourceReferenceNumber;
                trans.CasaAccountId = item.casaAccountId;
                trans.DebitAmount = item.debitAmount;
                trans.CreditAmount = item.creditAmount;
                trans.SourceBranchId = item.sourceBranchId;
                trans.DestinationBranchId = item.destinationBranchId;

                transactions.Add(trans);
            }

            this.context.tbl_Finance_Transaction.AddRange(transactions);
            context.SaveChanges();

            return transaction.batchCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public CasaLienViewModel PostCollateralSearch(CasaLienViewModel model)
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

            PostTransaction(collateralTransaction);

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
            return model;

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
                var rate = this.context.tbl_Currency_Rate.FirstOrDefault(x => x.BaseCurrencyId ==
                baseCurrency && x.CurrencyId == currencyId && x.Date == generalSetup.GetApplicationDate()).SellingRate;

                return rate;
            }           
            
        }
    }
}
