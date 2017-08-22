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

namespace FintrakBanking.Repositories.Finance

{
    public class FinanceTransactionRepository : IFinanceTransactionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;

        public FinanceTransactionRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail,

        FinTrakBankingContext _context)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public CasaLienViewModel AddCollateralSearchLien(CasaLienViewModel model)
        {
            ////CasaLienViewModel model = new CasaLienViewModel();
            //var appRefNo = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).ApplicationReferenceNumber;
            //var company = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).CompanyId;
            ////var sector = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).BranchId;
            //var branch = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).BranchId;

            //model.productAccountNumber = model.productAccountNumber;
            //model.createdBy = company;
            //model.lienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            //model.sourceTypeId = 1;
            //model.lienCreditAmount = amount;
            //model.lienDebitAmount = 0;

            var data = new tbl_CASA_Lien
            {
                ProductAccountNumber = model.productAccountNumber,
                LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                BranchId = model.branchId,
                CompanyId = model.companyId,
                LienCreditAmount = model.lienCreditAmount,
                LienDebitAmount = 0,
                LienTypeId = model.lienTypeId,
                CreatedBy = model.createdBy,
                Description = model.description,
                DateCreated = generalSetup.GetApplicationDate()
                
            };

            context.tbl_CASA_Lien.Add(data);

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
                trans.PostedDateTime = transaction.postedDateTime;
                trans.IsApproved = transaction.isApproved;
                trans.PostedBy = transaction.postedBy;
                trans.ApprovedBy = transaction.approvedBy;
                trans.ApprovedDate = transaction.approvedDate;
                trans.ApprovedDateTime = transaction.approvedDateTime;
                trans.SourceApplicationId = transaction.sourceApplicationId;


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

            //CasaLienViewModel model = new CasaLienViewModel();
            //var appRefNo = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).ApplicationReferenceNumber;
            //var company = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).CompanyId;
            ////var sector = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).BranchId;
            //var branch = this.context.tbl_Loan_Application.FirstOrDefault(x => x.CustomerId == casaaccountId).BranchId;

            //model.productAccountNumber = appRefNo;
            ////model.createdBy = ;
            ////model.lienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            //model.sourceTypeId = 1;
            //model.lienCreditAmount = 0;
            //model.lienDebitAmount = amount;

            //var lienRefNo = context.tbl_CASA_Lien.Where(x => x.SourceTypeId == model.lienTypeId && x.ProductAccountNumber == model.productAccountNumber).Max(x => x.LienReferenceNumber);

            var lienRefNo = this.context.tbl_CASA_Lien.FirstOrDefault(x => x.LienTypeId == model.lienTypeId && x.ProductAccountNumber == model.productAccountNumber).LienReferenceNumber;

            var data = new tbl_CASA_Lien
            {

                ProductAccountNumber = model.productAccountNumber,
                LienReferenceNumber = lienRefNo, //CommonHelpers.GenerateRandomDigitCode(10),
                BranchId = model.branchId,
                CompanyId = model.companyId,
                LienCreditAmount = 0,
                LienDebitAmount = model.lienDebitAmount,
                LienTypeId = model.lienTypeId,
                CreatedBy = model.createdBy,
                Description = model.description,
                DateCreated = generalSetup.GetApplicationDate()
            };

            context.tbl_CASA_Lien.Add(data);

            FinanceTransactionViewModel collateralTransaction = new FinanceTransactionViewModel();
            collateralTransaction.operationId = (int) OperationsEnum.CollateralSearch;


            //trans.BatchCode = transaction.batchCode;
            //trans.OperationId = transaction.operationId;
            //trans.Description = transaction.description;
            //trans.ValueDate = transaction.valueDate;
            //trans.TransactionDate = transaction.transactionDate;
            //trans.CurrencyId = transaction.currencyId;
            //trans.CurrencyRate = transaction.currencyRate;
            //trans.PostedDateTime = transaction.postedDateTime;
            //trans.IsApproved = transaction.isApproved;
            //trans.PostedBy = transaction.postedBy;
            //trans.ApprovedBy = transaction.approvedBy;
            //trans.ApprovedDate = transaction.approvedDate;
            //trans.ApprovedDateTime = transaction.approvedDateTime;
            //trans.SourceApplicationId = transaction.sourceApplicationId;


            //trans.GLAccountId = item.glAccountId;
            //trans.SourceReferenceNumber = item.sourceReferenceNumber;
            //trans.CasaAccountId = item.casaAccountId;
            //trans.DebitAmount = item.debitAmount;
            //trans.CreditAmount = item.creditAmount;
            //trans.SourceBranchId = item.sourceBranchId;
            //trans.DestinationBranchId = item.destinationBranchId;
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

    }
}
