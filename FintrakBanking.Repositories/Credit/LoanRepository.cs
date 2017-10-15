using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
//using XLeratorDLL_financial;

namespace FintrakBanking.Repositories.Credit
{
    //using FinancialTypes = XLeratorDLL_financial.FinancialTypes;
    //using wct = XLeratorDLL_financial.XLeratorDLL_financial;

    public class LoanRepository : ILoanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        private IApprovalLevelStaffRepository level;
        private ICustomerRepository customers;
        private IWorkflow workflow;




        public LoanRepository(FinTrakBankingContext _context, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        ILoanCovenantRepository _loanCovenant,
                                        IFinanceTransactionRepository _financeTransaction, IApprovalLevelStaffRepository _level,
                                        ICustomerRepository _customers, IWorkflow _workflow)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.loanCovenant = _loanCovenant;
            this.financeTransaction = _financeTransaction;
            this.level = _level;
            this.customers = _customers;
            this.workflow = _workflow;
        }


        public IEnumerable<LookupViewModel> GetAllLoanTypes()
        {
            return (from data in context.tbl_Loan_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.LoanTypeId,
                        lookupName = data.LoanTypeName
                    });
        }



        private string GenerateLoanReferenceNumber(int customerId, int productId)
        {
            var customerCode = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == customerId).CustomerCode;
            var productCode = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productId).ProductCode;
            var data = ((this.context.tbl_Loan.Count(x => x.CustomerId == customerId && x.ProductId == productId)) + 1);
            return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
        }

        public string AddLoanBooking(LoanViewModel entity)
        {
            //...................CHECK IF THE LOAN RECORD IS A SCHEDULED LOAN..................//
            if (entity.productTypeId == (int)LoanProductTypeEnum.TermLoan || entity.productTypeId == (int)LoanProductTypeEnum.SelfLiquidating)
            {
                return this.AddTermLoan(entity);
            }

            // ...............CHECK IF THE LOAN RECORD IS A NON SCHEDULED LOAN....................//
            else if (entity.productTypeId != (int)LoanProductTypeEnum.RevolvingLoan)
            {
                var revolvingLoanInput = entity.revolvingLoanInput;

                revolvingLoanInput.loanApplicationDetailId = entity.loanApplicationDetailId;
                revolvingLoanInput.loanApplicationId = entity.loanApplicationId;
                revolvingLoanInput.companyId = entity.companyId;
                revolvingLoanInput.createdBy = entity.createdBy;
                revolvingLoanInput.branchId = entity.branchId;

                return addRevolvingLoan(revolvingLoanInput);
            }
            else if (entity.productTypeId != (int)LoanProductTypeEnum.ContingentLiability)
            {
                var contingentLoanInput = entity.contingentLoanInput;

                contingentLoanInput.loanApplicationDetailId = entity.loanApplicationDetailId;
                contingentLoanInput.loanApplicationId = entity.loanApplicationId;
                contingentLoanInput.companyId = entity.companyId;
                contingentLoanInput.createdBy = entity.createdBy;
                contingentLoanInput.branchId = entity.branchId;

                return addContingentLiability(contingentLoanInput);
            }
            {
                return "The Product type is Invalid";
            }
        }

        private string addRevolvingLoan(RevolvingLoanViewModel entity)
        {
            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);
            var data = new tbl_Loan_Revolving
            {
                RevolvingLoanId = entity.loanId,
                CustomerId = entity.customerId,
                ProductId = entity.productId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,
                CurrencyId = entity.currencyId,
                ExchangeRate = entity.exchangeRate,
                LoanApplicationDetailId = entity.loanApplicationDetailId,
                LoanReferenceNumber = entity.loanReferenceNumber,
                SubSectorId = entity.subSectorId,
                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMisCode,
                InterestRate = entity.interestRate,
                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,
                BookingDate = entity.bookingDate, 
                OverdraftLimit = entity.overdraftLimit,

                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                LoanStatusId = entity.loanStatusId,
                IsDisbursed = false,
                OperationId = (int)OperationsEnum.TermLoanBooking,
                CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                TrancheBatchCode = entity.trancheBatchCode,
                DischargeLetter = false,
                SuspendInterest = false,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,
                InternalPrudentialGuidelineStatusId = entity.internalPrudentialGuidelineStatusId,
                ExternalPrudentialGuidelineStatusId = entity.externalPrudentialGuidelineStatusId,
                NPLDate = entity.nplDate,
                CompanyId = entity.companyId

            };

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {

                    //...................Adding Contingent Loan Record.........................
                    var loan = context.tbl_Loan_Revolving.Add(data);

                    //...................Saving Loan Collaterals Mapping.......................
                    AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId);

                    //...................Saving Loan Gaurantors................................
                    AddLoanGuarantor(entity.loanGuarantor, (short)entity.productTypeId, entity.loanApplicationId);

                    //...................Adding Audit...............................
                    context.tbl_Audit.Add(audit);

                    //........Save Changes...............
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = loan.RevolvingLoanId,
                        comment = "Please approve this Loan",
                        amount = entity.approvedAmount,
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (LogApproval(approvalModel, (int)OperationsEnum.TermLoanBooking, true, (int)ApprovalStatusEnum.Pending))
                    {
                        trans.Commit();
                        //............save Loan Covenant..........
                        AddLoanCovenant(entity.loanCovenant, entity.loanApplicationDetailId, loan.RevolvingLoanId, (short)entity.productTypeId);
                        //............save Loan Fees..........
                        AddLoanFees(entity.loanChargeFee, loan.RevolvingLoanId, (short)entity.productTypeId);

                        context.SaveChanges();
                    }
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }

        }

        private string addContingentLiability(ContingentLoanViewModel entity)
        {
            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);
            var data = new tbl_Loan_Contingent
            {
                ContingentLoanId = entity.loanId,
                CustomerId = entity.customerId,
                ProductId = entity.productId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,
                CurrencyId = entity.currencyId,
                ExchangeRate = entity.exchangeRate,
                LoanApplicationDetailId = entity.loanApplicationDetailId,
                LoanReferenceNumber = entity.loanReferenceNumber,
                SubSectorId = entity.subSectorId,
                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMisCode,
                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,

                BookingDate = entity.bookingDate,

                ContingentAmount = entity.contingentAmount,
                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                LoanStatusId = entity.loanStatusId,
                IsDisbursed = false,
                OperationId = (int)OperationsEnum.TermLoanBooking,
                CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                TrancheBatchCode = entity.trancheBatchCode,
                DischargeLetter = false,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,


            };


            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    //...................Adding Contingent Loan Record.........................
                    var loan = context.tbl_Loan_Contingent.Add(data);

                    //...................Saving Loan Collaterals Mapping.......................
                    AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId);

                    //...................Saving Loan Gaurantors................................
                    AddLoanGuarantor(entity.loanGuarantor,  (short)entity.productTypeId, entity.loanApplicationId);

                    //...................Adding Audit...............................
                    context.tbl_Audit.Add(audit);

                    //........Save Changes...............
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = loan.ContingentLoanId,
                        comment = "Please approve this Loan",
                        amount = entity.approvedAmount,
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (LogApproval(approvalModel, (int)OperationsEnum.TermLoanBooking, true, (int)ApprovalStatusEnum.Pending))
                    {
                        //.....Commit transaction ............
                        trans.Commit();
                        //............save Loan Covenant..........
                        AddLoanCovenant(entity.loanCovenant, entity.loanApplicationDetailId, loan.ContingentLoanId, (short)entity.productTypeId);
                        //............save Loan Fees..........
                        AddLoanFees(entity.loanChargeFee, loan.ContingentLoanId, (short)entity.productTypeId);

                        context.SaveChanges();
                    }
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        private string AddTermLoan(LoanViewModel entity)
        {
            if (entity.loanScheduleInput.maturityDate <= entity.loanScheduleInput.effectiveDate)
                throw new Exception("Loan terminal date should be more than effective date");

            var principalAmount  = from a in context.tbl_Loan where a.LoanApplicationDetailId == entity.loanApplicationDetailId
                                   let sumPrincipalAmount = context.tbl_Loan.Where(x => x.LoanApplicationDetailId == entity.loanApplicationDetailId).Sum(x => x.PrincipalAmount)
                                   select sumPrincipalAmount;

            var totalPreviouslyBookedAmount = principalAmount.FirstOrDefault();

            var totalPrincipalAmount = totalPreviouslyBookedAmount + entity.principalAmount;

            if (totalPrincipalAmount > entity.customerAvailableAmount)
                throw new Exception("The loan amount cannot greater than the availiable amount");
   

            double? priceIndex = (from a in context.tbl_Product
                                  where a.ProductId == entity.productId
                                  select a.tbl_Product_Price_Index.PriceIndexRate).FirstOrDefault();

            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);

            if (entity.loanScheduleInput.scheduleMethodId == (short) LoanScheduleTypeEnum.BulletPayment)
            {
                entity.loanScheduleInput.principalFrequency = null;
                entity.loanScheduleInput.interestFrequency = null;
            }

            var data = new tbl_Loan
            {
                LoanApplicationDetailId = entity.loanApplicationDetailId,
                LoanReferenceNumber = loanReferenceNumber,
                LoanStatusId = (short)LoanStatusEnum.Inactive,
                IsDisbursed = false,
                PrincipalNumberOfInstallment = 0,
                InterestNumberOfInstallment = 0,
                //IsScheduledPrepayment = null,
                ScheduledPrepaymentAmount = entity.scheduledPrepaymentAmount,
                ScheduledPrepaymentFrequencyTypeId = null,
                ProductPriceIndexRate = (double)priceIndex,

                CustomerGroupId = (entity.customerGroupId != 0 ? entity.customerGroupId : null),

                LoanTypeId = entity.loanTypeId,
                SubSectorId = entity.subSectorId,
                CurrencyId = (short)entity.currencyId,

                DischargeLetter = false,
                SuspendInterest = false,
                
                CustomerId = entity.customerId,
                ProductId = (short)entity.productId,
                CompanyId = entity.companyId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,

                PrincipalFrequencyTypeId = entity.loanScheduleInput.principalFrequency,
                InterestFrequencyTypeId = entity.loanScheduleInput.interestFrequency,

                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMiscode,
                InterestRate = Convert.ToInt32(entity.interestRate),

                PrincipalInstallmentLeft = 0,
                InterestInstallmentLeft = 0,

                ScheduleTypeId = entity.loanScheduleInput.scheduleMethodId,

                PrincipalAmount = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),

                OperationId = (int)OperationsEnum.TermLoanBooking,

                EquityContribution = 0,
                OutstandingPrincipal = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),
                PrincipalAdditionCount = 0,
                PrincipalReductionCount = 0,
                FixedPrincipal = false,
                ProfileLoan = false,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,

                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,

                BookingDate = entity.bookingDate,
                CreatedBy = entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate(),
                EffectiveDate = entity.loanScheduleInput.effectiveDate,
                MaturityDate = entity.loanScheduleInput.maturityDate,
                FirstPrincipalPaymentDate = entity.loanScheduleInput.principalFirstpaymentDate,
                FirstInterestPaymentDate = entity.loanScheduleInput.interestFirstpaymentDate,
                AllowForceDebitRepayment = false,

            };

            if (entity.customerGroupId > 0)
            {
                data.CustomerGroupId = entity.customerGroupId;
            }


            ////Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            ////end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {

                    //...................Adding Contingent Loan Record.........................
                    var loan = context.tbl_Loan.Add(data);

                    //...................Saving Loan Collaterals Mapping.......................
                    AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId);

                    //...................Saving Loan Gaurantors................................
                    AddLoanGuarantor(entity.loanGuarantor, (short)entity.productTypeId, entity.loanApplicationId);

                    //...................Adding Audit...............................
                    var dataCount = context.SaveChanges();
                    
                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = loan.TermLoanId,
                        comment = "Please approve this Loan",
                        amount = entity.principalAmount,
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (LogApproval(approvalModel, (int)OperationsEnum.TermLoanBooking, true, (int)ApprovalStatusEnum.Pending))
                    {
                        //.....Commit transaction ............
                        trans.Commit();
                        
                        AddLoanCovenant(entity.loanCovenant, entity.loanApplicationId, loan.TermLoanId, (short)entity.productTypeId);
                        AddLoanFees(entity.loanChargeFee, loan.TermLoanId, (short)entity.productTypeId);

                        context.SaveChanges();
                    }
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool LogApproval(ForwardViewModel model, int operationId, bool externalInitialization, int ApprovalStatusId)
        {
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Comment = model.comment;
            workflow.ExternalInitialization = externalInitialization;
            workflow.StatusId = ApprovalStatusId;
            workflow.Amount = model.amount;

            workflow.LogActivity();

            return workflow.Saved;
        }

        private void DisburseLoan(LoanViewModel entity)
        {
            //PostLoanDisbursment(entity);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(BuildLoanDisbursmentPosting(entity));

            inputTransactions.AddRange(BuildLoanChargeFeesPosting(entity));

            financeTransaction.PostTransaction(inputTransactions);
        }

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public LoanViewModel PostLoanDisbursment(LoanViewModel model)
        //{      
        //    FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();


        //    var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

        //    loanTransaction.operationId = (int)OperationsEnum.LoanBooking;
        //    loanTransaction.description = "Loan Disbursment Amount";
        //    loanTransaction.valueDate = generalSetup.GetApplicationDate();
        //    loanTransaction.transactionDate = loanTransaction.valueDate;
        //    loanTransaction.currencyId = casa.CurrencyId;
        //    loanTransaction.currencyRate = financeTransaction.GetExchangeRate(loanTransaction.currencyId, loanTransaction.valueDate, model.companyId);
        //    loanTransaction.isApproved = true;
        //    loanTransaction.postedBy = model.createdBy;
        //    loanTransaction.approvedBy = model.createdBy;
        //    loanTransaction.approvedDate = loanTransaction.transactionDate;
        //    loanTransaction.approvedDateTime = DateTime.Now;
        //    loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    loanTransaction.companyId = model.companyId;

        //    FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
        //    debit.glAccountId = model.casaAccountId;  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    debit.sourceReferenceNumber = model.loanReferenceNumber;
        //    debit.casaAccountId = casa.CasaAccountId;
        //    debit.debitAmount = model.principalAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = model.branchId;
        //    debit.destinationBranchId = casa.BranchId;

        //    var repaymentGL = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
        //    credit.glAccountId = repaymentGL;

        //    credit.sourceReferenceNumber = model.loanReferenceNumber;
        //    credit.casaAccountId = casa.CasaAccountId;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = model.principalAmount;
        //    credit.sourceBranchId = model.branchId;
        //    credit.destinationBranchId = model.branchId;


        //    loanTransaction.transactionDetails.Add(debit);
        //    loanTransaction.transactionDetails.Add(credit);

        //    financeTransaction.PostTransaction(loanTransaction);

        //    // Audit Section ---------------------------            

        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.LoanApplication,
        //        StaffId = model.createdBy,
        //        BranchId = model.branchId,
        //        Detail = $"Applied for Loan Disbursment with reference number: {model.loanReferenceNumber}",
        //        IPAddress = model.userIPAddress,
        //        Url = model.applicationUrl,
        //        ApplicationDate = generalSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    context.SaveChanges();
        //    return model;

        //}

        //public LoanViewModel PostLoanChargeFee(LoanViewModel data)
        //{
        //    //foreach ( var model in data.loanChargeFee)
        //    //{
        //    //    //model.ledgerAccountId = data.
        //    //}
        //    //data.loanChargeFee
        //     LoanChargeFeeViewModel model = new LoanChargeFeeViewModel();
        //     FinanceTransactionViewModel feeTransaction = new FinanceTransactionViewModel();


        //    var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.ledgerAccountId && x.CompanyId == model.companyId);

        //    feeTransaction.operationId = (int)OperationsEnum.LoanBooking;
        //    feeTransaction.description = "Fee charge";
        //    feeTransaction.valueDate = generalSetup.GetApplicationDate();
        //    feeTransaction.transactionDate = feeTransaction.valueDate;
        //    feeTransaction.currencyId = casa.CurrencyId;
        //    feeTransaction.currencyRate = financeTransaction.GetExchangeRate(feeTransaction.currencyId, feeTransaction.valueDate, model.companyId);
        //    feeTransaction.isApproved = true;
        //    feeTransaction.postedBy = model.createdBy;
        //    feeTransaction.approvedBy = model.createdBy;
        //    feeTransaction.approvedDate = feeTransaction.transactionDate;
        //    feeTransaction.approvedDateTime = DateTime.Now;
        //    feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    feeTransaction.companyId = model.companyId;

        //    FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
        //    debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    debit.sourceReferenceNumber = data.loanReferenceNumber;
        //    debit.casaAccountId = casa.CasaAccountId;
        //    debit.debitAmount = (decimal)model.amount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = data.branchId;
        //    debit.destinationBranchId = casa.BranchId;

        //    var feeGL = model.ledgerAccountId; //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
        //    credit.glAccountId = feeGL;

        //    credit.sourceReferenceNumber = data.loanReferenceNumber;
        //    credit.casaAccountId = casa.CasaAccountId;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = (decimal)model.amount;
        //    credit.sourceBranchId = data.branchId;
        //    credit.destinationBranchId = data.branchId;


        //    feeTransaction.transactionDetails.Add(debit);
        //    feeTransaction.transactionDetails.Add(credit);

        //    financeTransaction.PostTransaction(feeTransaction);

        //    // Audit Section ---------------------------            

        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.LoanApplication,
        //        StaffId = model.createdBy,
        //        BranchId = data.branchId,
        //        Detail = $"Applied for fee charge with reference number: {data.loanReferenceNumber}",
        //        IPAddress = model.userIPAddress,
        //        Url = model.applicationUrl,
        //        ApplicationDate = generalSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    context.SaveChanges();
        //    return data;

        //}


        public IEnumerable<LoanViewModel> GetTermLoanBookingAwaitingApproval(int staffId, int companyId)
        { 

            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.TermLoanBooking);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.tbl_Loan
                        join coy in context.tbl_Company on ln.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on ln.BranchId equals br.BranchId
                        join atrail in context.tbl_Approval_Trail on ln.TermLoanId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending
                              && atrail.OperationId == (int)OperationsEnum.TermLoanBooking
                              && atrail.ToApprovalLevelId == staffApprovalLevelId
                              && atrail.ResponseStaffId == null
                        orderby ln.TermLoanId descending

                        select new LoanViewModel()
                        {
                            loanId = ln.TermLoanId,
                            operationId = (int)OperationsEnum.TermLoanBooking,
                            customerId = ln.CustomerId,
                            productId = ln.ProductId,
                            casaAccountId = ln.CasaAccountId,
                            loanApplicationDetailId = (int)ln.LoanApplicationDetailId,

                            branchId = ln.BranchId,
                            loanReferenceNumber = ln.LoanReferenceNumber,
                            applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,

                            //tenor = (ln.MaturityDate - ln.EffectiveDate).Days,
                            //principalFrequencyTypeId = ln.PrincipalFrequencyTypeId ?? 0,
                            pricipalFrequencyTypeName = ln.tbl_Frequency_Type.Description ?? null,
                           // interestFrequencyTypeId = ln.PrincipalFrequencyTypeId ?? 0,
                            interestFrequencyTypeName = ln.tbl_Frequency_Type.Description ?? null,

                            principalNumberOfInstallment = ln.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = ln.InterestNumberOfInstallment,
                            //relationshipOfficerId = ln.RelationshipOfficerId,
                            //relationshipManagerId = ln.RelationshipManagerId,
                            misCode = ln.MISCode,
                            teamMiscode = ln.TeamMISCode,
                            interestRate = ln.InterestRate,
                            effectiveDate = ln.EffectiveDate,
                            maturityDate = ln.MaturityDate,
                            bookingDate = ln.BookingDate,
                            principalAmount = ln.PrincipalAmount,
                            principalInstallmentLeft = ln.PrincipalInstallmentLeft,
                            interestInstallmentLeft = ln.InterestInstallmentLeft,
                            approvalStatusId = ln.ApprovalStatusId,
                            approvedBy = ln.ApprovedBy,
                            approverComment = ln.ApproverComment,
                            dateApproved = ln.DateApproved,
                            //loanStatusId = ln.LoanStatusId,
                            scheduleTypeId = ln.ScheduleTypeId,
                            isDisbursed = ln.IsDisbursed,
                            disbursedBy = ln.DisbursedBy,
                            disburserComment = ln.DisburserComment,
                            disburseDate = ln.DisburseDate,

                            approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.CustomerGroupId,
                           // operationId = ln.OperationId,
                            loanTypeId = ln.LoanTypeId,
                            equityContribution = ln.EquityContribution,
                            subSectorId = ln.SubSectorId,
                            subSectorName = ln.tbl_Sub_Sector.Name,
                            sectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,

                            firstPrincipalPaymentDate = ln.FirstInterestPaymentDate,
                            firstInterestPaymentDate = ln.FirstInterestPaymentDate,
                            outstandingPrincipal = ln.OutstandingPrincipal,
                            principalAdditionCount = ln.PrincipalAdditionCount,
                            principalReductionCount = ln.PrincipalReductionCount,
                            fixedPrincipal = ln.FixedPrincipal,
                            profileLoan = ln.ProfileLoan,
                            dischargeLetter = ln.DischargeLetter,
                            suspendInterest = ln.SuspendInterest,

                            scheduled = ln.IsScheduledPrepayment,
                            isScheduledPrepayment = ln.IsScheduledPrepayment,
                            scheduledPrepaymentAmount = ln.ScheduledPrepaymentAmount,
                            scheduledPrepaymentDate = ln.ScheduledPrepaymentDate,
                            //scheduledPrepaymentFrequencyTypeId = ln.ScheduledPrepaymentFrequencyTypeId,

                            customerSensitivityLevelId = ln.CustomerSensitivityLevelId,
                            customerSensitivityLevelName = ln.tbl_Customer_Sensitivity_Level.Description,
                            firstName = ln.tbl_Customer.FirstName,
                            middleName = ln.tbl_Customer.MiddleName,
                            lastName = ln.tbl_Customer.LastName,
                            customerCode = ln.tbl_Customer.CustomerCode,
                            productAccountNumber = ln.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = ln.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            loanTypeName = ln.tbl_Loan_Type.LoanTypeName,
                            customerName = ln.tbl_Customer.LastName + " " + ln.tbl_Customer.FirstName + " " + ln.tbl_Customer.MiddleName,
                            currencyId = ln.CurrencyId,

                            branchName = ln.tbl_Branch.BranchName,
                            relationshipOfficerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            relationshipManagerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,

                            productName = ln.tbl_Product.ProductName,

                            createdBy = ln.CreatedBy,
                            creatorName = ln.tbl_Staff.LastName + " " + ln.tbl_Staff.FirstName + " (" + ln.tbl_Staff.StaffCode + ")",
                            dateTimeCreated = ln.DateTimeCreated,
                            comment = "",
                            

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
                    
            return data;
        }

        public IEnumerable<RevolvingLoanViewModel> GetRevolvingLoanBookingAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.RevolvingLoanBooking);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.tbl_Loan_Revolving
                        join coy in context.tbl_Company on ln.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on ln.BranchId equals br.BranchId
                        join atrail in context.tbl_Approval_Trail on ln.RevolvingLoanId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending
                              && atrail.OperationId == (int)OperationsEnum.TermLoanBooking
                              && atrail.ToApprovalLevelId == staffApprovalLevelId
                              && atrail.ResponseStaffId == null
                        orderby ln.RevolvingLoanId descending

                        select new RevolvingLoanViewModel()
                        {
                            loanId = ln.RevolvingLoanId,
                            operationId = (int)OperationsEnum.RevolvingLoanBooking,
                            customerId = ln.CustomerId,
                            productId = ln.ProductId,
                            casaAccountId = ln.CasaAccountId,
                            loanApplicationDetailId = (int)ln.LoanApplicationDetailId,

                            branchId = ln.BranchId,
                            loanReferenceNumber = ln.LoanReferenceNumber,
                            applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,

                            relationshipOfficerId = ln.RelationshipOfficerId,
                            relationshipManagerId = ln.RelationshipManagerId,
                            misCode = ln.MISCode,
                            teamMiscode = ln.TeamMISCode,
                            interestRate = ln.InterestRate,
                            effectiveDate = ln.EffectiveDate,
                            maturityDate = ln.MaturityDate,
                            bookingDate = ln.BookingDate,
                           
                            approvalStatusId = ln.ApprovalStatusId,
                            approvedBy = ln.ApprovedBy,
                            approverComment = ln.ApproverComment,
                            dateApproved = ln.DateApproved,
                            //loanStatusId = ln.LoanStatusId,
                           
                            isDisbursed = ln.IsDisbursed,
                            disbursedBy = ln.DisbursedBy,
                            disburserComment = ln.DisburserComment,
                            disburseDate = ln.DisburseDate,

                            approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.CustomerGroupId,
                            // operationId = ln.OperationId,
                            loanTypeId = ln.LoanTypeId,
                            
                            subSectorId = ln.SubSectorId,
                            subSectorName = ln.tbl_Sub_Sector.Name,
                            //SectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,

                            dischargeLetter = ln.DischargeLetter,
                            suspendInterest = ln.SuspendInterest,

                            customerSensitivityLevelId = ln.CustomerSensitivityLevelId,
                            customerSensitivityLevelName = ln.tbl_Customer_Sensitivity_Level.Description,
                            firstName = ln.tbl_Customer.FirstName,
                            middleName = ln.tbl_Customer.MiddleName,
                            lastName = ln.tbl_Customer.LastName,
                            customerCode = ln.tbl_Customer.CustomerCode,
                            productAccountNumber = ln.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = ln.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            loanTypeName = ln.tbl_Loan_Type.LoanTypeName,
                            customerName = ln.tbl_Customer.LastName + " " + ln.tbl_Customer.FirstName + " " + ln.tbl_Customer.MiddleName,
                            currencyId = ln.CurrencyId,

                            branchName = ln.tbl_Branch.BranchName,
                            relationshipOfficerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            relationshipManagerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,

                            productName = ln.tbl_Product.ProductName,

                            createdBy = ln.CreatedBy,
                            creatorName = ln.tbl_Staff.LastName + " " + ln.tbl_Staff.FirstName + " (" + ln.tbl_Staff.StaffCode + ")",
                            dateTimeCreated = ln.DateTimeCreated,
                            comment = "",

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
            return data.ToList();
        }

        public IEnumerable<ContingentLoanViewModel> GetContingentLoanBookingAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.ContigentLoanBooking);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.tbl_Loan_Contingent
                        join coy in context.tbl_Company on ln.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on ln.BranchId equals br.BranchId
                        join atrail in context.tbl_Approval_Trail on ln.ContingentLoanId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending
                              && atrail.OperationId == (int)OperationsEnum.TermLoanBooking
                              && atrail.ToApprovalLevelId == staffApprovalLevelId
                              && atrail.ResponseStaffId == null
                        orderby ln.ContingentLoanId descending

                        select new ContingentLoanViewModel()
                        {
                            loanId = ln.ContingentLoanId,
                            operationId = (int)OperationsEnum.ContigentLoanBooking,
                            customerId = ln.CustomerId,
                            productId = ln.ProductId,
                            casaAccountId = ln.CasaAccountId,
                            loanApplicationDetailId = (int)ln.LoanApplicationDetailId,

                            branchId = ln.BranchId,
                            loanReferenceNumber = ln.LoanReferenceNumber,
                            applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,

                            relationshipOfficerId = ln.RelationshipOfficerId,
                            relationshipManagerId = ln.RelationshipManagerId,
                            misCode = ln.MISCode,
                            teamMiscode = ln.TeamMISCode,
                            effectiveDate = ln.EffectiveDate,
                            maturityDate = ln.MaturityDate,
                            bookingDate = ln.BookingDate,

                            approvalStatusId = ln.ApprovalStatusId,
                            approvedBy = ln.ApprovedBy,
                            approverComment = ln.ApproverComment,
                            dateApproved = ln.DateApproved,
                            //loanStatusId = ln.LoanStatusId,

                            isDisbursed = ln.IsDisbursed,
                            disbursedBy = ln.DisbursedBy,
                            disburserComment = ln.DisburserComment,
                            disburseDate = ln.DisburseDate,

                            approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.CustomerGroupId,
                            // operationId = ln.OperationId,
                            loanTypeId = ln.LoanTypeId,

                            subSectorId = ln.SubSectorId,
                            subSectorName = ln.tbl_Sub_Sector.Name,
                            //SectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,
                            dischargeLetter = ln.DischargeLetter,

                            customerSensitivityLevelId = ln.CustomerSensitivityLevelId,
                            customerSensitivityLevelName = ln.tbl_Customer_Sensitivity_Level.Description,
                            firstName = ln.tbl_Customer.FirstName,
                            middleName = ln.tbl_Customer.MiddleName,
                            lastName = ln.tbl_Customer.LastName,
                            customerCode = ln.tbl_Customer.CustomerCode,
                            productAccountNumber = ln.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = ln.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            loanTypeName = ln.tbl_Loan_Type.LoanTypeName,
                            customerName = ln.tbl_Customer.LastName + " " + ln.tbl_Customer.FirstName + " " + ln.tbl_Customer.MiddleName,
                            currencyId = ln.CurrencyId,

                            branchName = ln.tbl_Branch.BranchName,
                            relationshipOfficerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            relationshipManagerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,

                            productName = ln.tbl_Product.ProductName,

                            createdBy = ln.CreatedBy,
                            creatorName = ln.tbl_Staff.LastName + " " + ln.tbl_Staff.FirstName + " (" + ln.tbl_Staff.StaffCode + ")",
                            dateTimeCreated = ln.DateTimeCreated,
                            comment = "",

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
            return data;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            
            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.LogForApproval(entity);

                    var b = workflow.NextLevelId ?? 0;

                    if (b == 0 && workflow.NewState != (int)ApprovalState.Ended)
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    try
                    {
                        ApproveLoanBooking(entity.targetId, (short)workflow.StatusId, entity);
                        trans.Commit();
                        if (workflow.NewState != (int)ApprovalState.Ended) return false;
                        else return true;
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw new Exception("Approval failed. " + e.Message);
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }

        }

        private bool ApproveLoanBooking(int loanId, short approvalStatusId, ApprovalViewModel user)
        {
            var loanRecord = context.tbl_Loan.Find(loanId);
            var revolvingLoanRecord = context.tbl_Loan_Revolving.Find(loanId);
            var contingentLoanRecord = context.tbl_Loan_Contingent.Find(loanId);


            if (workflow.NewState != (int)ApprovalState.Ended)
            {
                if (user.operationId == (int)LoanProductTypeEnum.TermLoan || user.operationId == (int)LoanProductTypeEnum.SelfLiquidating)
                {
                    if (loanRecord.ApprovalStatusId != (int)ApprovalStatusEnum.Processing)
                        loanRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;
                }

                if (user.operationId == (int)LoanProductTypeEnum.ContingentLiability)
                {
                    if (contingentLoanRecord.ApprovalStatusId != (int)ApprovalStatusEnum.Processing)
                        contingentLoanRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;
                }

                if (user.operationId == (int)LoanProductTypeEnum.RevolvingLoan)
                {
                    if (revolvingLoanRecord.ApprovalStatusId != (int)ApprovalStatusEnum.Processing)
                        revolvingLoanRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;
                }

            }

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                //...................Updating Loan Application Status....................
                decimal totalBookedAmount = 0;
                
                //=======================================================================

                //...................Updating Loan Tables With Approved State Properties Values....................
                if (user.operationId == (int)LoanProductTypeEnum.RevolvingLoan)
                {
                    totalBookedAmount = (from a in context.tbl_Loan_Revolving.Where(x => x.LoanApplicationDetailId == loanRecord.LoanApplicationDetailId)
                                         select a).Sum(s => s.OverdraftLimit);

                    if(totalBookedAmount >= revolvingLoanRecord.tbl_Loan_Application_Detail.ApprovedAmount)
                    {
                        var loanApplicationRecord = context.tbl_Loan_Application.Find(loanRecord.tbl_Loan_Application_Detail.LoanApplicationId);
                        loanApplicationRecord.ApplicationStatusId = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                    }

                    revolvingLoanRecord.DateApproved = DateTime.Now;
                    revolvingLoanRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;
                }

                if (user.operationId == (int)LoanProductTypeEnum.ContingentLiability)
                {
                    totalBookedAmount = (from a in context.tbl_Loan_Contingent.Where(x => x.LoanApplicationDetailId == loanRecord.LoanApplicationDetailId)
                                         select a).Sum(s => s.ContingentAmount);

                    if (totalBookedAmount >= contingentLoanRecord.tbl_Loan_Application_Detail.ApprovedAmount)
                    {
                        var loanApplicationRecord = context.tbl_Loan_Application.Find(loanRecord.tbl_Loan_Application_Detail.LoanApplicationId);
                        loanApplicationRecord.ApplicationStatusId = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                    }

                    contingentLoanRecord.DateApproved = DateTime.Now;
                    contingentLoanRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;

                }
                //================================================================== 

                if (user.operationId == (int)LoanProductTypeEnum.TermLoan || user.operationId == (int)LoanProductTypeEnum.SelfLiquidating)
                {
                    loanRecord.DateApproved = DateTime.Now;
                    loanRecord.ApprovalStatusId = (int) ApprovalStatusEnum.Approved;

                    totalBookedAmount = (from a in context.tbl_Loan.Where(x => x.LoanApplicationDetailId == loanRecord.LoanApplicationDetailId)
                                         select a).Sum(s => s.PrincipalAmount);

                    if (totalBookedAmount >= loanRecord.tbl_Loan_Application_Detail.ApprovedAmount)
                    {
                        var loanApplicationRecord = context.tbl_Loan_Application.Find(loanRecord.tbl_Loan_Application_Detail.LoanApplicationId);
                        loanApplicationRecord.ApplicationStatusId = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                    }

                    //...................Build Schedule Model & Call Schedule Repo Add Function....................
                    var loanScheduleModel = BuildScheduleModel(loanId, user.createdBy);
                    this.loanSchedule.AddLoanSchedule(loanId, loanScheduleModel, user.createdBy);
                    //=================================================================================================

                    //...................Build Disbursement Model & Invoke Loan Disbursement....................
                    var loanDisbursementModel = BuildDisbursementModel(loanId, loanScheduleModel, user.createdBy);
                    DisburseLoan(loanDisbursementModel);

                    loanRecord.LoanStatusId = 1;
                    //==========================================================================================


                    // Audit Section ---------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LoanBookingApproved,
                        StaffId = user.staffId,
                        BranchId = (short)user.BranchId,
                        Detail = $"Approved Loan Booking with code ({loanRecord.LoanReferenceNumber})",
                        IPAddress = user.userIPAddress,
                        Url = user.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };

                    this.auditTrail.AddAuditTrail(audit);
                    // Audit Section ---------------------------
                }


            }

            return this.context.SaveChanges() > 0;
        }


        private LoanPaymentScheduleInputViewModel BuildScheduleModel(int targetId, int createdBy)
        {
            List<IrregularLoanScheduleInputViewModel> irregularPaymentScheduleList = new List<IrregularLoanScheduleInputViewModel>();
            var loanIrregularRecord = context.tbl_Loan_Schedule_Irregular_Input.Where(x => x.LoanId == targetId);
            foreach (var irregularLoan in loanIrregularRecord)
            {
                var irregularViewData = new IrregularLoanScheduleInputViewModel
                {
                    paymentAmount = (double)irregularLoan.PaymentAmount,
                    paymentDate = irregularLoan.PaymentDate
                };
                irregularPaymentScheduleList.Add(irregularViewData);
            };

            var loanScheduleData = context.tbl_Loan.Find(targetId);
            var loanFeeData = context.tbl_Loan_Fee.Where(x => x.LoanId == targetId && x.IsIntegralFee == true);
            double integraFeeAmount = 0;

            var applicationDate = generalSetup.GetApplicationDate();
            var maturityDate = applicationDate.AddDays(loanScheduleData.tbl_Loan_Application_Detail.ApprovedTenor);

            foreach (var record in loanFeeData)
            {
                integraFeeAmount = integraFeeAmount + (double)record.FeeAmount;
            }
            if(loanScheduleData.ScheduleTypeId == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                loanScheduleData.PrincipalFrequencyTypeId = null;
                loanScheduleData.InterestFrequencyTypeId = null;
                
            }
            var scheduleModel = new LoanPaymentScheduleInputViewModel
            {
                scheduleMethodId = loanScheduleData.ScheduleTypeId,

                principalAmount = (double)loanScheduleData.PrincipalAmount,
                effectiveDate = applicationDate,
                interestRate = loanScheduleData.InterestRate,
                principalFrequency = loanScheduleData.PrincipalFrequencyTypeId,
                interestFrequency = loanScheduleData.InterestFrequencyTypeId,
                tenor = (loanScheduleData.MaturityDate - loanScheduleData.EffectiveDate).Days,
                principalFirstpaymentDate = (DateTime)loanScheduleData.FirstPrincipalPaymentDate,
                interestFirstpaymentDate = (DateTime)loanScheduleData.FirstInterestPaymentDate,
                maturityDate = maturityDate,
                accurialBasis = loanScheduleData.ScheduleDayCountConventionId,
                integralFeeAmount = integraFeeAmount,
                firstDayType = loanScheduleData.ScheduleDayInterestTypeId,
                irregularPaymentSchedule = irregularPaymentScheduleList
            };

            return scheduleModel;
        }

        private LoanViewModel BuildDisbursementModel(int loanId, LoanPaymentScheduleInputViewModel loanInputModel, int staffId)
        {
            var covenantRecord = context.tbl_Loan_Covenant_Detail.Where(x => x.LoanId == loanId).ToList();
            List<LoanCovenantDetailViewModel> loanCovenantList = new List<LoanCovenantDetailViewModel>();

            foreach (var covenant in covenantRecord)
            {
                var loanCovenant = new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = covenant.LoanCovenantDetailId,
                    covenantDetail = covenant.CovenantDetail,
                    loanId = covenant.LoanId,
                    covenantTypeId = covenant.CovenantTypeId,
                    frequencyTypeId = covenant.FrequencyTypeId,
                    covenantAmount = covenant.CovenantAmount ?? 0,
                    covenantDate = covenant.CovenantDate,

                };
                loanCovenantList.Add(loanCovenant);
            };

            var feeRecord = context.tbl_Loan_Fee.Where(x => x.LoanId == loanId).ToList();
            List<LoanChargeFeeViewModel> loanChargeFeeList = new List<LoanChargeFeeViewModel>();
            foreach (var fee in feeRecord)
            {
                var loanfee = new LoanChargeFeeViewModel
                {
                    loanChargeFeeId = fee.LoanChargeFeeId,
                    chargeFeeId = fee.ChargeFeeId,
                    loanId = fee.LoanId,
                    productTypeId = fee.ProductTypeId,
                    feeRateValue = fee.FeeRateValue,
                    feeDependentAmount = fee.FeeRateValue,
                    feeAmount = fee.FeeAmount,
                    isIntegralFee = fee.IsIntegralFee,
                    recurring = fee.IsRecurring,
                };
                loanChargeFeeList.Add(loanfee);
            };


            var loanRecord = context.tbl_Loan.Find(loanId);
            if(loanRecord.ScheduleTypeId == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                loanRecord.PrincipalFrequencyTypeId = null;
                loanRecord.InterestFrequencyTypeId = null;
            }
        
            var loanModel = new LoanViewModel
            {
                loanId = loanRecord.TermLoanId,
                customerId = loanRecord.CustomerId,
                productId = loanRecord.ProductId,
                //productPriceIndexRate = (decimal)loanRecord.ProductPriceIndexRate,
                casaAccountId = loanRecord.CasaAccountId,
                loanApplicationDetailId = (int)loanRecord.LoanApplicationDetailId,

                branchId = loanRecord.BranchId,
                loanReferenceNumber = loanRecord.LoanReferenceNumber,
                applicationReferenceNumber = loanRecord.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,

                principalFrequencyTypeId = loanRecord.PrincipalFrequencyTypeId,
                interestFrequencyTypeId = loanRecord.InterestFrequencyTypeId,
                principalNumberOfInstallment = loanRecord.PrincipalNumberOfInstallment,
                interestNumberOfInstallment = loanRecord.InterestNumberOfInstallment,
                
               // relationshipOfficerId = loanRecord.RelationshipOfficerId,
               // relationshipManagerId = loanRecord.RelationshipManagerId,
               // misCode = loanRecord.MISCode,
               // teamMiscode = loanRecord.TeamMISCode,
                interestRate = loanRecord.InterestRate,
                effectiveDate = generalSetup.GetApplicationDate(),
                maturityDate = generalSetup.GetApplicationDate().AddDays(loanRecord.tbl_Loan_Application_Detail.ApprovedTenor),
                //bookingDate = loanRecord.BookingDate,
                principalAmount = loanRecord.PrincipalAmount,
                createdBy = staffId,
                //principalInstallmentLeft = loanRecord.PrincipalInstallmentLeft,
                //interestInstallmentLeft = loanRecord.InterestInstallmentLeft,
                loanStatusId = loanRecord.LoanStatusId,
                scheduleTypeId = loanRecord.ScheduleTypeId,
                operationId = (int)OperationsEnum.TermLoanBooking,
                customerGroupId = loanRecord.CustomerGroupId,
                loanTypeId = loanRecord.LoanTypeId,
                //equityContribution = loanRecord.EquityContribution,
                //subSectorId = loanRecord.SubSectorId,
                firstPrincipalPaymentDate = loanRecord.FirstPrincipalPaymentDate,
                firstInterestPaymentDate = loanRecord.FirstInterestPaymentDate,
                //outstandingPrincipal = loanRecord.OutstandingPrincipal,
                //principalAdditionCount = loanRecord.PrincipalAdditionCount,
                //principalReductionCount = loanRecord.PrincipalReductionCount,
                //fixedPrincipal = loanRecord.FixedPrincipal,
                //profileLoan = loanRecord.ProfileLoan,
                //dischargeLetter = loanRecord.DischargeLetter,
                //suspendInterest = loanRecord.SuspendInterest,
                //scheduled = true,
                //isScheduledPrepayment = (bool)loanRecord.IsScheduledPrepayment,
                //scheduledPrepaymentAmount = (decimal)loanRecord.ScheduledPrepaymentAmount,
                //scheduledPrepaymentDate = (DateTime)loanRecord.ScheduledPrepaymentDate,
                //scheduledPrepaymentFrequencyTypeId = (short)loanRecord.ScheduledPrepaymentFrequencyTypeId,
                //customerSensitivityLevelId = loanRecord.CustomerSensitivityLevelId,
                //customerCode = loanRecord.tbl_Customer.CustomerCode,
                companyId = loanRecord.CompanyId,
                currencyId = loanRecord.CurrencyId,
                accurialBasis = loanRecord.ScheduleDayCountConventionId,
                integralFeeAmount = (decimal)loanInputModel.integralFeeAmount,
                firstDayType = loanRecord.ScheduleDayInterestTypeId,

                //loanScheduleInput = loanInputModel,

                //loanCovenant = loanCovenantList,
                loanChargeFee = loanChargeFeeList,
            };

            return loanModel;
        }

        public FinanceTransactionViewModel BuildLoanDisbursmentPosting(LoanViewModel model)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
            loanTransaction.description = "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = financeTransaction.GetExchangeRate(loanTransaction.currencyId, model.companyId);
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = model.principalAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BranchId;

            var repaymentAccountGL = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = repaymentAccountGL;

            credit.sourceReferenceNumber = model.loanReferenceNumber;
            credit.casaAccountId = casa.CasaAccountId;
            credit.debitAmount = 0;
            credit.creditAmount = model.principalAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        public List<FinanceTransactionViewModel> BuildLoanChargeFeesPosting(LoanViewModel loanDetails)
        {
            //foreach ( var model in data.loanChargeFee)
            //{
            //    //model.ledgerAccountId = data.
            //}
            //data.loanChargeFee

            // LoanChargeFeeViewModel model = new LoanChargeFeeViewModel();

            List<FinanceTransactionViewModel> output = new List<FinanceTransactionViewModel>();

            foreach (var item in loanDetails.loanChargeFee)
            {
                FinanceTransactionViewModel feeTransaction = new FinanceTransactionViewModel();


                var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == loanDetails.casaAccountId);

                feeTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
                feeTransaction.description = "Fee charge";
                feeTransaction.valueDate = generalSetup.GetApplicationDate();
                feeTransaction.transactionDate = feeTransaction.valueDate;
                feeTransaction.currencyId = casa.CurrencyId;
                feeTransaction.currencyRate = financeTransaction.GetExchangeRate(feeTransaction.currencyId, loanDetails.companyId);
                feeTransaction.isApproved = true;
                feeTransaction.postedBy = loanDetails.createdBy;
                feeTransaction.approvedBy = loanDetails.createdBy;
                feeTransaction.approvedDate = feeTransaction.transactionDate;
                feeTransaction.approvedDateTime = DateTime.Now;
                feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                feeTransaction.companyId = loanDetails.companyId;

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                debit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                debit.casaAccountId = casa.CasaAccountId;
                debit.debitAmount = (decimal)item.feeAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = loanDetails.branchId;
                debit.destinationBranchId = casa.BranchId;

                var feeGL = this.context.tbl_Charge_Fee.Where(x => x.ChargeFeeId == item.chargeFeeId).Select(x => x.GLAccountId).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;
                credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = (decimal)item.feeAmount;
                credit.sourceBranchId = loanDetails.branchId;
                credit.destinationBranchId = loanDetails.branchId;


                feeTransaction.transactionDetails.Add(debit);
                feeTransaction.transactionDetails.Add(credit);

                output.Add(feeTransaction);
            }

            // Audit Section ---------------------------            

            return output;

        }

        private bool AddLoanCovenant(List<LoanCovenantDetailViewModel> covenantModel, int loanApplicationId, int loanId, short productTypeId)
        {
            foreach (LoanCovenantDetailViewModel entity in covenantModel)
            {
                var covenant = new tbl_Loan_Covenant_Detail
                {
                    CompanyId = entity.companyId,
                    CovenantAmount = entity.covenantAmount,
                    CovenantDate = entity.covenantDate,
                    CovenantDetail = entity.covenantDetail,
                    CovenantTypeId = entity.covenantTypeId,
                    CreatedBy = entity.createdBy,
                    DateTimeCreated = this.generalSetup.GetApplicationDate().Date,
                    FrequencyTypeId = entity.frequencyTypeId,
                    LoanId = loanId,
                    ProductTypeId = productTypeId
                };

                context.tbl_Loan_Covenant_Detail.Add(covenant);
            }
            var result = context.SaveChanges() > 0;
            return result;

        }

        public bool AddLoanGuarantor(List<LoanGuarantorViewModel> guarantorModel, short productTypeId, int loanApplicationId)
        {

            foreach (LoanGuarantorViewModel entity in guarantorModel)
            {
                var guarantor = new tbl_Loan_Guarantor
                {
                    ProductTypeId = productTypeId,
                    LoanApplicationId = loanApplicationId,
                    Firstname = entity.firstname,
                    Lastname = entity.lastname,
                    Middlename = entity.middlename,
                    Address = entity.address,
                    PhoneNumber1 = entity.phoneNumber1,
                    PhoneNumber2 = entity.phoneNumber2,
                    Relationship = entity.relationship,
                    RelationshipDuration = (short)entity.relationshipDuration,
                    BVN = entity.bvn,
                    EmailAddress = entity.emailAddress,
                    CreatedBy = 1,
                    DateTimeCreated = generalSetup.GetApplicationDate()
                };
                context.tbl_Loan_Guarantor.Add(guarantor);
            }

            //return context.SaveChanges() > 0
            return true;
        }

        private ICollection<tbl_Loan_Covenant_Detail> AddLoanCovenant(LoanCovenantDetailViewModel entity, short productTypeId)
        {
            ICollection<tbl_Loan_Covenant_Detail> covenant;

            covenant = new List<tbl_Loan_Covenant_Detail>();

            covenant.Add(new tbl_Loan_Covenant_Detail
            {
                CompanyId = entity.companyId,
                CovenantAmount = entity.covenantAmount,
                CovenantDetail = entity.covenantDetail,
                CovenantDate = entity.covenantDate,
                LoanId = entity.loanId,
                ProductTypeId = productTypeId,
                CovenantTypeId = entity.covenantTypeId,
                FrequencyTypeId = entity.frequencyTypeId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate(),
            });

            return covenant;
        }

        public bool AddLoanCollateralMapping(List<LoanCollateralMappingViewModel> collateralModel, int loanApplicationId)
        {

            foreach (LoanCollateralMappingViewModel entity in collateralModel)
            {
                var collateral = new tbl_Loan_Collateral_Mapping
                {
                    CollateralCustomerId = entity.collateralId,
                    LoanApplicationId = loanApplicationId,
                    IsReleased = false,
                };
                context.tbl_Loan_Collateral_Mapping.Add(collateral);
            }

            //return context.SaveChanges() > 0;
            return true;

        }

        private bool AddLoanFees(List<LoanChargeFeeViewModel> feeModel, int loanId, short productTypeId)
        {
            var feeAmount = 0;
            foreach (LoanChargeFeeViewModel ent in feeModel)
            {
                if (ent.feeTypeId == 1)
                {
                    feeAmount = 0;
                }
                var fee = new tbl_Loan_Fee
                {
                    ChargeFeeId = ent.chargeFeeId,
                    FeeAmount = feeAmount,
                    FeeDependentAmount = ent.feeDependentAmount,
                    FeeRateValue = ent.feeRateValue,
                    IsIntegralFee = ent.isIntegralFee,
                    LoanId = loanId,
                    ProductTypeId = productTypeId,
                    IsRecurring = false, //TODO : get from entity
                    RecurringPaymentDay = 28, //TODO: get from entity
                    CreatedBy = ent.createdBy,
                    DateTimeCreated = DateTime.Now.Date
                };
                context.tbl_Loan_Fee.Add(fee);
            }

            return context.SaveChanges() > 0;
        }

        private IQueryable<LoanViewModel> GetAllLoans()
        {
            var data = (from l in context.tbl_Loan
                        select new LoanViewModel
                        {
                            loanId = l.TermLoanId,
                            customerId = l.CustomerId,
                            customerName = l.tbl_Customer.FirstName + " " + l.tbl_Customer.LastName,
                            productId = l.ProductId,
                            companyId = l.CompanyId,
                            casaAccountId = l.CasaAccountId,
                            branchId = l.BranchId,
                            branchName = l.tbl_Branch.BranchName,
                            loanReferenceNumber = l.LoanReferenceNumber,
                            //tenor = (l.MaturityDate - l.EffectiveDate).Days, // returning error

                            principalFrequencyTypeId = (short)l.PrincipalFrequencyTypeId,
                            pricipalFrequencyTypeName = l.tbl_Frequency_Type.Description,
                            interestFrequencyTypeId = (short)l.InterestFrequencyTypeId,
                            interestFrequencyTypeName = l.tbl_Frequency_Type.Description,

                            principalNumberOfInstallment = l.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = l.InterestNumberOfInstallment,

                            relationshipOfficerId = l.RelationshipOfficerId,
                            relationshipOfficerName = l.tbl_Staff.FirstName + " " + l.tbl_Staff.MiddleName + " " + l.tbl_Staff.LastName,
                            relationshipManagerId = l.RelationshipManagerId,
                            relationshipManagerName = l.tbl_Staff.FirstName + " " + l.tbl_Staff.MiddleName + " " + l.tbl_Staff.LastName,
                            misCode = l.MISCode,
                            teamMiscode = l.TeamMISCode,
                            interestRate = l.InterestRate,
                            effectiveDate = l.EffectiveDate,
                            maturityDate = l.MaturityDate,
                            bookingDate = l.BookingDate,
                            principalAmount = l.PrincipalAmount,
                            principalInstallmentLeft = l.PrincipalInstallmentLeft,
                            interestInstallmentLeft = l.InterestInstallmentLeft,
                            approvalStatusId = l.ApprovalStatusId,
                            approvedBy = l.ApprovedBy,
                            approverComment = l.ApproverComment,
                            dateApproved = l.DateApproved,
                            loanStatusId = l.LoanStatusId,
                            scheduleTypeId = l.ScheduleTypeId,
                            isDisbursed = l.IsDisbursed,
                            disbursedBy = l.DisbursedBy,
                            disburserComment = l.DisburserComment,
                            disburseDate = l.DisburseDate,

                            approvedAmount = l.tbl_Loan_Application_Detail.ApprovedAmount,

                            //creditAppraisalCompleted = l.CreditAppraisalCompleted,
                            operationId = l.OperationId,
                            operationName = context.tbl_Operations.FirstOrDefault(x => x.OperationId == l.OperationId).OperationName,
                            productAccountNumber = l.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = l.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            subSectorName = l.tbl_Sub_Sector.Name,
                            sectorName = l.tbl_Sub_Sector.tbl_Sector.Name,
                            customerGroupId = l.CustomerGroupId,
                            loanTypeId = l.LoanTypeId,
                            loanTypeName = l.tbl_Loan_Type.LoanTypeName,
                            equityContribution = l.EquityContribution,
                            firstPrincipalPaymentDate = l.FirstPrincipalPaymentDate ?? DateTime.Now,
                            firstInterestPaymentDate = l.FirstInterestPaymentDate ?? DateTime.Now,
                            outstandingPrincipal = l.OutstandingPrincipal,
                            principalAdditionCount = l.PrincipalAdditionCount ?? 0,
                            principalReductionCount = l.PrincipalReductionCount ?? 0,
                            fixedPrincipal = l.FixedPrincipal,
                            profileLoan = l.ProfileLoan,
                            dischargeLetter = l.DischargeLetter,
                            suspendInterest = l.SuspendInterest,
                            customerSensitivityLevelId = l.CustomerSensitivityLevelId,
                            createdBy = l.CreatedBy,
                            dateTimeCreated = l.DateTimeCreated,
                            isCamsol = context.tbl_Loan_Camsol.Any(x => x.LoanId == l.TermLoanId)
                        });
            return data;
        }

        //public bool ValidateCamsol(int loanId)
        //{
        //    var check = context.tbl_Loan_Camsol.Where(x => x.LoanId == loanId);

        //    if (check.Any())
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public int CalculateTenorValue(DateTime maturityDate, DateTime effectiveDate)
        {
            var result = (maturityDate - effectiveDate).Days;

            return result;
        }

        public IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId)
        {
            var data = GetAllLoans().Where(l => l.customerId == customerId).GroupBy(x => x.loanId).Select(g => g.FirstOrDefault());
            return data;
               
        }

        public IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId)
        {
            var data = GetAllLoans().Where(l => l.customerGroupId == customerGroupId).GroupBy(x => x.loanId)
                .Select(g => g.FirstOrDefault());
            return data;
        }

        public IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId)
        {

            var loans = GetLoansByCompanyId(companyId).Where(c => c.approvalStatusId == (int)ApprovalStatusEnum.Approved && c.companyId == customerId)
                .Select(c => new LoanRepaymentScheduleViewModel()
                {
                    principalRepayment = (decimal)c.outstandingPrincipal,
                    interestAccrual = (decimal)c.outstandingPrincipal,
                    customerId = c.customerId,
                    principalAmount = c.principalAmount,
                    effectiveDate = c.effectiveDate,
                    interestRate = c.interestRate,
                    loanId = c.loanId,
                    productName = c.productAccountName,
                    tenor = c.tenor,
                    terminationDate = c.maturityDate
                }).AsQueryable();
            return loans;
        }

        public LoanViewModel GetLoan(int loanId)
        {
            return (from data in context.tbl_Loan
                    where data.TermLoanId == loanId
                    select new LoanViewModel()
                    {
                        loanId = data.TermLoanId,
                        customerId = data.CustomerId,
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        casaAccountId = data.CasaAccountId,
                        branchId = data.BranchId,
                        loanReferenceNumber = data.LoanReferenceNumber,
                        //tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        principalFrequencyTypeId = (short)data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = (short)data.InterestFrequencyTypeId,

                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        maturityDate = data.MaturityDate,
                        bookingDate = (DateTime)data.BookingDate,
                        principalAmount = data.PrincipalAmount,
                        principalInstallmentLeft = data.PrincipalInstallmentLeft,
                        interestInstallmentLeft = data.InterestInstallmentLeft,
                        approvalStatusId = data.ApprovalStatusId,
                        approvedBy = data.ApprovedBy,
                        approverComment = data.ApproverComment,
                        dateApproved = data.DateApproved,
                        loanStatusId = data.LoanStatusId,
                        scheduleTypeId = data.ScheduleTypeId,
                        isDisbursed = data.IsDisbursed,
                        disbursedBy = data.DisbursedBy,
                        disburserComment = data.DisburserComment,
                        disburseDate = data.DisburseDate,

                        approvedAmount = data.tbl_Loan_Application_Detail.ApprovedAmount,

                        operationId = data.OperationId,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        equityContribution = data.EquityContribution,
                        firstPrincipalPaymentDate = data.FirstPrincipalPaymentDate,
                        firstInterestPaymentDate = data.FirstInterestPaymentDate,
                        outstandingPrincipal = data.OutstandingPrincipal,
                        principalAdditionCount = data.PrincipalAdditionCount,
                        principalReductionCount = data.PrincipalReductionCount,
                        fixedPrincipal = data.FixedPrincipal,
                        profileLoan = data.ProfileLoan,
                        dischargeLetter = data.DischargeLetter,
                        suspendInterest = data.SuspendInterest,
                        customerSensitivityLevelId = data.CustomerSensitivityLevelId,
                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated
                    }).FirstOrDefault();
        }

        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId)
        {
            return (from data in context.tbl_Loan
                    where data.CompanyId == companyId && (data.LoanReferenceNumber == referenceNumberOrName ||
                      $"{data.tbl_Customer.FirstName} {data.tbl_Customer.MiddleName} {data.tbl_Customer.LastName} {data.tbl_Customer.CustomerCode} {data.tbl_CASA.ProductAccountNumber}".Contains(referenceNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                    select new LoanViewModel()
                    {
                        loanId = data.TermLoanId,
                        customerId = data.CustomerId,
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        casaAccountId = data.CasaAccountId,
                        branchId = data.BranchId,
                        loanReferenceNumber = data.LoanReferenceNumber,
                        //tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        //tenorModeId = data.TenorModeId,
                        principalFrequencyTypeId = (short)data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = (short)data.InterestFrequencyTypeId,

                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        maturityDate = data.MaturityDate,
                        bookingDate = (DateTime)data.BookingDate,
                        principalAmount = data.PrincipalAmount,
                        principalInstallmentLeft = data.PrincipalInstallmentLeft,
                        interestInstallmentLeft = data.InterestInstallmentLeft,
                        approvalStatusId = data.ApprovalStatusId,
                        approvedBy = data.ApprovedBy,
                        approverComment = data.ApproverComment,
                        dateApproved = data.DateApproved,
                        loanStatusId = data.LoanStatusId,
                        scheduleTypeId = data.ScheduleTypeId,
                        isDisbursed = data.IsDisbursed,
                        disbursedBy = data.DisbursedBy,
                        disburserComment = data.DisburserComment,
                        disburseDate = data.DisburseDate,

                        approvedAmount = data.tbl_Loan_Application_Detail.ApprovedAmount,

                        operationId = data.OperationId,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        equityContribution = data.EquityContribution,
                        firstPrincipalPaymentDate = data.FirstPrincipalPaymentDate,
                        outstandingPrincipal = data.OutstandingPrincipal,
                        principalAdditionCount = data.PrincipalAdditionCount,
                        principalReductionCount = data.PrincipalReductionCount,
                        fixedPrincipal = data.FixedPrincipal,
                        profileLoan = data.ProfileLoan,
                        dischargeLetter = data.DischargeLetter,
                        suspendInterest = data.SuspendInterest,
                        customerSensitivityLevelId = data.CustomerSensitivityLevelId,
                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated

                    });
        }

        public IQueryable<LoanViewModel> GetLoansByCompanyId(int companyId)
        {
            return (context.tbl_Loan //.Include("tbl_Customer").Include("tbl_CASA_AccountStatus")
                .Where(x => x.CompanyId == companyId)
                .Select(o => new LoanViewModel
                {
                    loanId = o.TermLoanId,
                    customerId = o.CustomerId,
                    productId = o.ProductId,
                    casaAccountId = o.CasaAccountId,
                    branchId = o.BranchId,
                    loanReferenceNumber = o.LoanReferenceNumber,
                    //tenor = (o.MaturityDate - o.EffectiveDate).Days,
                    principalFrequencyTypeId = (short)o.PrincipalFrequencyTypeId,
                    interestFrequencyTypeId = (short)o.InterestFrequencyTypeId,

                    principalNumberOfInstallment = o.PrincipalNumberOfInstallment,
                    interestNumberOfInstallment = o.InterestNumberOfInstallment,
                    relationshipOfficerId = o.RelationshipOfficerId,
                    relationshipManagerId = o.RelationshipManagerId,
                    misCode = o.MISCode,
                    teamMiscode = o.TeamMISCode,
                    interestRate = o.InterestRate,
                    effectiveDate = o.EffectiveDate,
                    maturityDate = o.MaturityDate,
                    bookingDate = (DateTime)o.BookingDate,
                    principalAmount = o.PrincipalAmount,
                    principalInstallmentLeft = o.PrincipalInstallmentLeft,
                    interestInstallmentLeft = o.InterestInstallmentLeft,
                    approvalStatusId = o.ApprovalStatusId,
                    firstName = o.tbl_Customer.FirstName,
                    middleName = o.tbl_Customer.MiddleName,
                    lastName = o.tbl_Customer.LastName,
                    customerCode = o.tbl_Customer.CustomerCode,
                    productAccountNumber = o.tbl_CASA.ProductAccountNumber,
                    productAccountName = o.tbl_CASA.ProductAccountName,
                }));
        }

        public IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel)
        {
            var loans = GetLoansByCompanyId(companyId);

            if (!String.IsNullOrEmpty(searchModel.loanReferenceNumber))
            {
                loans = loans.Where(x => x.loanReferenceNumber == searchModel.loanReferenceNumber);
            }

            if (!String.IsNullOrEmpty(searchModel.productAccountNumber))
            {
                loans = loans.Where(x => x.productAccountNumber == searchModel.productAccountNumber);
            }

            if (!String.IsNullOrEmpty(searchModel.customerName))
            {
                loans = loans.Where(x =>
                x.firstName.ToLower().Contains(searchModel.customerName.ToLower())
                || x.lastName.ToLower().Contains(searchModel.customerName.ToLower())
                || x.middleName.ToLower().Contains(searchModel.customerName.ToLower())
                || x.customerCode.ToLower().Contains(searchModel.customerName.ToLower())
                );
            }

            if (!String.IsNullOrEmpty(searchModel.loanName))
            {
                loans = loans.Where(x => x.productAccountName.ToLower().Contains(searchModel.loanName.ToLower()));
            }

            loans.OrderBy(x => x.productAccountNumber).ThenBy(x => x.productAccountName);

            return loans;
        }


        private int GetDaysInAYear(DayCountConventionEnum dayCountId)
        {
            if (dayCountId == DayCountConventionEnum.Actual_Actual)
            {
                var currentDate = DateTime.Now;
                var firstDate = new DateTime(currentDate.Year, 1, 1); //  DateTime.ParseExact(user, "MM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var lastdate = new DateTime(currentDate.Year, 12, 31);
                var difference = (lastdate - firstDate).TotalDays;

                return Convert.ToInt32(difference);
            }

            var value = context.tbl_Day_Count_Convention.FirstOrDefault(x => x.DayCountConventionId == (short)dayCountId).DaysInAYear;

            return value;
        }


        public List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId)
        {
            var data = (from a in context.tbl_Loan_Covenant_Detail
                        where a.LoanId == loanId && a.Deleted == false
                        select new LoanCovenantDetailViewModel
                        {
                            loanCovenantDetailId = a.LoanCovenantDetailId,
                            covenantDetail = a.CovenantDetail,
                            loanId = a.LoanId,
                            covenantTypeId = a.CovenantTypeId,
                            frequencyTypeId = a.FrequencyTypeId,
                            covenantAmount = a.CovenantAmount,
                            covenantDate = a.CovenantDate

                        }).ToList();
            return data;
        }

        public IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId)
        {
            var data = (from a in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee on a.ChargeFeeId equals p.ChargeFeeId
                        where p.ProductId == productId && a.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            chargeFeeId = a.ChargeFeeId,
                            chargeFeeName = a.ChargeFeeName,
                            feeTypeId = a.FeeTypeId,
                            feeTypeName = a.tbl_Fee_Type.FeeTypeName,
                            feeIntervalId = a.FeeIntervalId
                        }).ToList();
            return data;
        }

        public IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId)
        {
            var data = (from c in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee on c.ChargeFeeId equals p.ChargeFeeId
                        where p.ProductId == productId && c.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            productFeeId = p.ProductFeeId,
                            chargeFeeId = c.ChargeFeeId,
                            chargeFeeName = c.ChargeFeeName,
                            feeRateValue = p.RateValue,
                            feeDependentAmount = p.DependentAmount ?? 0,
                            feeAmount = c.Amount ?? 0,
                            feeIntervalId = c.FeeIntervalId,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            feeTypeId = c.FeeTypeId,
                            required = c.tbl_Fee_Type.ByAmountRequired,
                            recurring = (bool)c.Recurring,
                            //feeTypeName = c.FeeTypeName

                        }).ToList();
            return data;
        }

        public List<LoanChargeFeeViewModel> GetLoanChargeFee(int loanId)
        {
            var data = (from c in context.tbl_Loan_Fee
                        where c.LoanId == loanId //&& c.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            loanChargeFeeId = c.LoanChargeFeeId,
                            loanId = c.LoanId,
                            chargeFeeId = c.ChargeFeeId,
                            chargeFeeName = c.tbl_Charge_Fee.ChargeFeeName,
                            feeRateValue = c.FeeRateValue,
                            feeDependentAmount = c.FeeDependentAmount,
                            feeAmount = c.FeeAmount,
                            feeIntervalId = c.tbl_Charge_Fee.FeeIntervalId,
                            feeIntervalName = c.tbl_Charge_Fee.tbl_Fee_Interval.FeeIntervalName

                        }).ToList();
            return data;
        }


        public IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByChargeFeeId(int chargeFeeId)
        {
            var data = (from c in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee
                        on c.ChargeFeeId equals p.ChargeFeeId
                        where p.ChargeFeeId == chargeFeeId && p.Deleted == false
                        select new ProductFeeViewModel
                        {
                            productFeeId = p.ProductFeeId,
                            productId = p.ProductId,
                            productName = p.tbl_Product.ProductName,
                            feeId = c.ChargeFeeId,
                            feeName = c.ChargeFeeName,
                            feeTargetName = c.tbl_Fee_Target.FeeTargetName,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            rateValue = (decimal)p.RateValue,
                            dependentAmount = p.DependentAmount

                        }).ToList();
            return data;
        }

        public List<LoanGuarantorViewModel> GetLoanGuarantors(int loanApplicationId )
        {
            var data = (from c in context.tbl_Loan_Guarantor
                        where c.LoanApplicationId == loanApplicationId
                        select new LoanGuarantorViewModel
                        {
                            loanGuarantorId = c.LoanGuarantorId,
                            loanApplicationId = (int)c.LoanApplicationId,
                            firstname = c.Firstname,
                            lastname = c.Lastname,
                            middlename = c.Middlename,
                            address = c.Address,
                            phoneNumber1 = c.PhoneNumber1,
                            phoneNumber2 = c.PhoneNumber2,
                            relationship = c.Relationship,
                            relationshipDuration = (short)c.RelationshipDuration,
                            bvn = c.BVN,
                            emailAddress = c.EmailAddress,
                            fullName = c.Lastname + " " + c.Firstname + " " + c.Middlename

                        }).ToList();
            return data;
        }

        //public List<LoanCollateralMappingViewModel> GetLoanCollaterals(int loanId)
        //{
        //    var data = (from c in context.tbl_Loan_Collateral_Mapping
        //                where c.LoanId == loanId
        //                select new LoanCollateralMappingViewModel
        //                {
        //                    loanCollateralMappingId = c.LoanCollateralMappingId,
        //                    loanId = c.LoanId,
        //                    collateralCustomerId = c.CollateralCustomerId,
        //                    loanApplicationId = c.LoanApplicationId

        //                }).ToList();
        //    return data;
        //}

        public IQueryable<CustomerSearchItemViewModels> SearchCustomerCollateral(int companyId, string searchQuery)
        {
            return this.customers.CustomerSearchRealTime(companyId, searchQuery);
        }

        public IQueryable<CustomerViewModels> SearchForCustomerCollateral(int companyId, string searchQuery)
        {
            IQueryable<CustomerViewModels> allCustomers = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allCustomers = GetCustomers()
                    .Where(c => c.companyId == companyId)
                    .Where(x => x.firstName.Contains(searchQuery)
               || x.lastName.Contains(searchQuery)
               || x.middleName.Contains(searchQuery)
                );
            }

            return allCustomers;
        }

        IQueryable<CustomerViewModels> GetCustomers()
        {
            return from a in context.tbl_Customer
                   where a.Deleted == false
                   select

                   new CustomerViewModels
                   {
                       accountCreationComplete = a.AccountCreationComplete,
                       branchId = a.BranchId,
                       branchName = a.tbl_Branch.BranchName,
                       childDateOfBirth = a.ChildDateOfBirth.Value,
                       companyMainId = a.CompanyId,
                       createdBy = a.CreatedBy,
                       creationMailSent = a.CreationMailSent,
                       customerCode = a.CustomerCode,
                       customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                       customerTypeId = a.CustomerTypeId.Value,
                       dateOfBirth = a.DateOfBirth,
                       customerId = a.CustomerId,
                       emailAddress = a.EmailAddress,
                       firstChildName = a.FirstChildName,
                       firstName = a.FirstName,
                       gender = a.Gender,
                       lastName = a.LastName,
                       maidenName = a.MaidenName,
                       maritalStatus = a.MaritalStatus.Value,
                       title = a.Title,
                       middleName = a.MiddleName,
                       customerTypeName = context.tbl_Customer_Type.FirstOrDefault(c => c.CustomerTypeId == a.CustomerTypeId).Name,
                       misCode = a.MISCode,
                       misStaff = a.MISStaff,
                       nationality = a.Nationality,
                       occupation = a.Occupation,
                       placeOfBirth = a.PlaceOfBirth,
                       isInvestmentGrade  = a.IsInvestmentGrade ,
                        isRealatedParty = a.IsRealatedParty,
                       isPoliticallyExposed = a.IsPoliticallyExposed ,
                       relationshipOfficerId = a.RelationshipOfficerId.Value,
                       spouse = a.Spouse,
                       sectorId = a.tbl_Sub_Sector.tbl_Sector.SectorId,
                       sectorName = a.tbl_Sub_Sector.tbl_Sector.Name,
                       subSectorId = a.SubSectorId,
                       subSectorName = a.tbl_Sub_Sector.Name,
                       taxNumber = a.TaxNumber
                       ,
                       CustomerAddresses = context.tbl_Customer_Address.Where(x => x.CustomerId == a.CustomerId).Select(x => new CustomerAddressViewModels()
                       {
                           address = x.Address,
                           addressTypeId = x.AddressTypeId,
                           cityId = x.CityId,
                           customerId = x.CustomerId,
                           homeTown = x.HomeTown,
                           nearestLandmark = x.NearestLandmark,
                           electricMeterNumber = x.ElectricMeterNumber,
                           pobox = x.POBox,
                           stateId = x.StateId,
                           addressId = x.AddressId
                       }).ToList(),
                       CustomerBvn = context.tbl_Customer_BVN.Where(b => b.CustomerId == a.CustomerId).Select(b => new CustomerBvnViewModels()
                       {
                           bankVerificationNumber = b.BankVerificationNumber,
                           customerBvnid = b.CustomerBVNId,
                           firstname = b.Firstname,
                           isValidBvn = b.IsValidBVN,
                           isPoliticallyExposed = b.IsPoliticallyExposed,
                           surname = b.Surname
                       }).ToList(),
                       CustomerPhoneContact = context.tbl_Customer_PhoneContact.Where(c => c.CustomerId == a.CustomerId).Select(c => new CustomerPhoneContactViewModels
                       {
                           active = c.Active,
                           customerId = c.CustomerId,
                           phone = c.Phone,
                           phoneContactId = c.PhoneContactId,
                           phoneNumber = c.PhoneNumber
                       }).ToList(),
                       CustomerCompanyInfomation = context.tbl_Customer_CompanyInfomation.Where(d => d.CustomerId == a.CustomerId).Select(d => new CustomerCompanyInfomationViewModels()
                       {
                           annualTurnOver = d.AnnualTurnOver,
                           companyEmail = d.CompanyEmail,
                           companyId = d.CustomerId,
                           companyName = d.CompanyName,
                           companyWebsite = d.CompanyWebsite,
                           companyInfomationId = d.CompanyInfomationId,
                           corporateBusinessCategory = d.CorporateBusinessCategory,
                           createdBy = a.CreatedBy,
                           creditRating = d.CreditRating,
                           registeredOffice = d.RegisteredOffice,
                           previousCreditRating = d.PreviousCreditRating,
                           registrationNumber = d.RegistrationNumber,
                           paidUpCapital = d.PaidUpCapital,
                           authorizedCapital = d.AuthorisedCapital

                       }).ToList(),
                       CustomerIdentification = context.tbl_Customer_Identification.Where(e => e.CustomerId == a.CustomerId).Select(e => new CustomerIdentificationViewModels()
                       {
                           identificationId = e.IdentificationId,
                           identificationModeId = e.IdentificationModeId.Value,
                           identificationMode = context.tbl_Customer_IdentificationModeType.FirstOrDefault(r => r.IdentificationModeId == e.IdentificationModeId).IdentificationMode,
                           identificationNo = e.IdentificationNo,
                           issueAuthority = e.IssueAuthority,
                           issuePlace = e.IssuePlace
                       }).ToList(),
                       CustomerEmploymentHistory = context.tbl_Customer_EmploymentHistory.Where(s => s.CustomerId == a.CustomerId).Select(s => new CustomerEmploymentHistoryViewModels()
                       {
                           active = s.Active,
                           previousEmployer = s.PreviousEmployer,
                           customerId = s.CustomerId,
                           employDate = s.EmployDate,
                           placeOfWorkId = s.PlaceOfWorkId,
                           employerAddress = s.EmployerAddress,
                           employerCountryId = s.EmployerCountryId,
                           employerName = s.EmployerName,
                           officePhone = s.OfficePhone,
                           employerStateId = s.EmployerStateId
                       }).ToList(),
                       CustomerCompanyDirectors = context.tbl_Customer_Company_Director.Where(s => s.CustomerId == a.CustomerId && s.CompanyDirectorTypeId == (short)CompanyDirectorTypeEnum.BoardMember)
                       .Select(s => new CustomerCompanyDirectorsViewModels()
                       {
                           companyDirectorId = s.CompanyDirectorId,
                           surname = s.Surname,
                           firstname = s.Firstname,
                           numberOfShares = s.NumberOfShares,
                           isPoliticallyExposed = s.IsPoliticallyExposed,
                           bankVerificationNumber = s.CustomerBVN,
                           companyDirectorTypeId = s.CompanyDirectorTypeId,
                           companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                           customerId = s.CustomerId,
                           customerName = s.Firstname + " " + s.Surname,
                           address = s.Address,
                           phoneNumber = s.PhoneNumber,
                           email = s.EmailAddress
                       }).ToList(),
                       CustomerCompanyShareholder = context.tbl_Customer_Company_Director.Where(s => s.CustomerId == a.CustomerId && s.CompanyDirectorTypeId == (short)CompanyDirectorTypeEnum.Shareholder)
                       .Select(s => new CustomerCompanyShareholderViewModels()
                       {
                           companyDirectorId = s.CompanyDirectorId,
                           surname = s.Surname,
                           firstname = s.Firstname,
                           numberOfShares = s.NumberOfShares,
                           isPoliticallyExposed = s.IsPoliticallyExposed,
                           bankVerificationNumber = s.CustomerBVN,
                           companyDirectorTypeId = s.CompanyDirectorTypeId,
                           companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                           customerId = s.CustomerId,
                           customerName = s.Firstname + " " + s.Surname,
                           address = s.Address,
                           phoneNumber = s.PhoneNumber,
                           email = s.EmailAddress
                       }).ToList(),
                       CustomerCollateral = context.tbl_Collateral_Customer.Where(cc => cc.CustomerId == a.CustomerId)
                       .Select(x => new CollateralViewModel()
                       {
                           collateralId = x.CollateralCustomerId,
                           collateralTypeId = x.CollateralTypeId,
                           collateralSubTypeId = x.CollateralSubTypeId,
                           customerId = x.CustomerId,
                           currencyId = x.CurrencyId,
                           currency = x.tbl_Currency.CurrencyName,
                           collateralTypeName = x.tbl_Collateral_Type.CollateralTypeName,
                           collateralCode = x.CollateralCode,
                           camRefNumber = x.CamRefNumber,
                           allowSharing = x.AllowSharing,
                           isLocationBased = x.IsLocationBased,
                           valuationCycle = x.ValuationCycle,
                           haircut = x.HairCut,
                           approvalStatus = x.ApprovalStatus,
                       }).ToList(),
                   };

        }

        public IEnumerable<LoanApplicationCollateralViewModel> GetAppraisalMemorandumCollateralChanges(int loanApplicationId)
        {
            var data = (from lac in context.tbl_Loan_Application_Collateral
                        where lac.LoanApplicationId == loanApplicationId && lac.Deleted == false
                        select new LoanApplicationCollateralViewModel()
                        {
                            customerCollateralId = lac.CustomerCollateralId,
                            latitude = lac.Latitude,
                            longitude = lac.Longitude,
                            nearestBusStop = lac.NearestBusStop,
                            nearestLandmark = lac.NearestLandmark,
                            locationAddress = lac.LocationAddress,
                            documentTitle = lac.DocumentTitle,
                            otherInformations = lac.OtherInformations
                        });
            return data;
        }

        public AppraisalMemorandumLoanDetailViewModel GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId)
        {
            return (from data in context.tbl_Credit_Appraisal_Memorandum_Loan_Detail
                    where data.AppraisalMemorandumId == appraisalMemorandumId
                    orderby data.AppraisalMemorandumLoanDetailId descending
                    select new AppraisalMemorandumLoanDetailViewModel()
                    {
                        interestRate = data.InterestRate,
                        principalAmount = data.PrincipalAmount,
                        tenor = data.Tenor,
                    }).FirstOrDefault();
        }

        public IEnumerable<CamProcessedLoanViewModel> GetAppraisalMemorandumProcessedLoanApplications(int companyId)
        {
            var data = (from d in context.tbl_Loan_Application_Detail
                        join m in context.tbl_Loan_Application on d.LoanApplicationId equals m.LoanApplicationId
                        join cust in context.tbl_Customer on d.CustomerId equals cust.CustomerId
                        where m.CompanyId == companyId && d.Deleted == false && m.ApplicationStatusId == (int)LoanApplicationStatusEnum.AvailmentCompleted
                        select new CamProcessedLoanViewModel
                        {
                            approvalStatusId = m.ApprovalStatusId,
                            loanApplicationId = m.LoanApplicationId,
                            loanApplicationDetailId = d.LoanApplicationDetailId,
                            applicationReferenceNumber = m.ApplicationReferenceNumber,
                            casaAccountId = m.CasaAccountId,
                            customerId = m.CustomerId ?? 0,
                            customerCode = cust.CustomerCode,
                            customerName = m.CustomerId.HasValue ? m.tbl_Customer.FirstName + " " + m.tbl_Customer.MiddleName + " " + m.tbl_Customer.LastName : "",

                            customerGroupId = m.CustomerGroupId,
                            customerGroupName = m.CustomerGroupId.HasValue ? m.tbl_Customer_Group.GroupName : "",
                            customerGroupCode = m.tbl_Customer_Group.GroupCode,
                            customerSensitivityLevelId = m.tbl_Customer.CustomerSensitivityLevelId,

                            customerAccounts = (from k in context.tbl_CASA
                                                where k.Deleted == false
                                                && k.CustomerId == d.CustomerId  
                                                select (
                                 new CasaViewModel
                                 {
                                     productAccountNumber = k.ProductAccountNumber,
                                     productAccountName = k.ProductAccountName,
                                     casaAccountId = k.CasaAccountId,
                                     currencyId = k.CurrencyId,
                                     productId = k.ProductId,
                                     customerId = k.CustomerId,
                                     isCurrentAccount = k.IsCurrentAccount,
                                     tenor = (int)k.Tenor,
                                 })).ToList(),
                            loanInformation = m.LoanInformation,
                            companyId = m.CompanyId,
                            branchId = m.BranchId,
                            branchName = m.tbl_Branch.BranchName,
                            subSectorId = d.SubSectorId,
                            subSectorName = d.tbl_Sub_Sector.Name,
                            sectorName = d.tbl_Sub_Sector.tbl_Sector.Name,
                            applicationTenor = m.ApplicationTenor,
                            effectiveDate = (DateTime)m.EffectiveDate,
                            expiryDate = (DateTime)m.ExpiryDate,
                            relationshipOfficerId = m.RelationshipOfficerId,
                            relationshipOfficerName = m.tbl_Staff.FirstName + " " + m.tbl_Staff.MiddleName + " " + m.tbl_Staff.LastName,
                            relationshipManagerId = m.RelationshipManagerId,
                            relationshipManagerName = m.tbl_Staff.FirstName + " " + m.tbl_Staff.MiddleName + " " + m.tbl_Staff.LastName,

                            currencyId = d.CurrencyId,
                            currencyCode = d.tbl_Currency.CurrencyCode,
                            loanTypeId = m.LoanTypeId,
                            loanTypeName = m.tbl_Loan_Type.LoanTypeName,
                            camReference = m.tbl_Credit_Appraisal_Memorandum.FirstOrDefault().CAMRef,
                            productId = d.ApprovedProductId,
                            productTypeId = d.tbl_Product.ProductTypeId,
                            productTypeName = d.tbl_Product.tbl_Product_Type.ProductTypeName,
                            productName = d.tbl_Product.ProductName,

                            misCode = m.MISCode,
                            teamMisCode = m.TeamMISCode,

                            interestRate = d.ApprovedInterestRate,
                            isRelatedParty = m.IsRelatedParty,
                            isPoliticallyExposed = m.IsPoliticallyExposed,
                            submittedForAppraisal = m.SubmittedForAppraisal,
                            approvedAmount = d.ApprovedAmount,
                            groupApprovedAmount = m.ApprovedAmount,

                            customerAvailableAmount = (d.tbl_Product.ProductTypeId == (short)LoanProductTypeEnum.TermLoan
                                                      || d.tbl_Product.ProductTypeId == (short)LoanProductTypeEnum.SelfLiquidating)
                             ? (d.ApprovedAmount - d.tbl_Loan.Where(tl => tl.LoanApplicationDetailId == d.LoanApplicationDetailId).Sum(s => s.PrincipalAmount)) :
                            (
                                (d.tbl_Product.ProductTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                                        ? (d.ApprovedAmount - d.tbl_Loan_Revolving
                                            .Where(tl => tl.LoanApplicationDetailId == d.LoanApplicationDetailId).Sum(s => s.OverdraftLimit)) :
                                (d.tbl_Product.ProductTypeId == (short)LoanProductTypeEnum.ContingentLiability
                                        ? (d.ApprovedAmount - d.tbl_Loan_Contingent
                                            .Where(tl => tl.LoanApplicationDetailId == d.LoanApplicationDetailId).Sum(s => s.ContingentAmount)) :
                                            0)
                            ),
                            approvedTenor = d.ApprovedTenor,
                            createdBy = m.CreatedBy,
                            applicationDate = m.ApplicationDate,
                            dateTimeCreated = d.DateTimeCreated,

                            loanPreliminaryEvaluationId = m.LoanPreliminaryEvaluationId,
                            exchangeRate = d.ExchangeRate,
                        }).ToList();

            // data = (from a in data where ((a.customerAvailableAmount > 0) || (a.customerAvailableAmount == null)) select a).ToList();
           
            foreach (var item in data)
            {
                if(item.customerAvailableAmount != 0)
                {
                    if (!item.customerAvailableAmount.HasValue)
                        item.customerAvailableAmount = item.approvedAmount;
                }
                
            }


            return data.ToList();
        }


        private IEnumerable<LoanViewModel> BookedLoan(int companyId)
        {
            return GetAllLoans().Where(x => x.companyId == companyId).OrderByDescending(x => x.loanId);
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId)
        {
            var loans = BookedLoan(companyId);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                //loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x => x.loanReferenceNumber == loanReferenceNumber);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                // loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x => x.customerCode == customerCode);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                //loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByProductId(int productId)
        {
            var data = (from c in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee
                        on c.ChargeFeeId equals p.ChargeFeeId
                        where p.ProductId == productId && p.Deleted == false
                        select new ProductFeeViewModel
                        {
                            productFeeId = p.ProductFeeId,
                            productId = p.ProductId,
                            productName = p.tbl_Product.ProductName,
                            feeId = c.ChargeFeeId,
                            feeName = c.ChargeFeeName,
                            feeTargetName = c.tbl_Fee_Target.FeeTargetName,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            rateValue = (decimal)p.RateValue,
                            dependentAmount = p.DependentAmount

                        }).ToList();
            return data;
        }

        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(int customerId, int companyId)
        {

            try
            {
                var data = (from a in context.tbl_Loan
                            where
                              a.CustomerId == customerId && a.CompanyId == companyId && a.ApprovalStatusId == (int)LoanStatusEnum.Active
                            select new CurrentCustomerExposure
                            {
                                facilityType = a.tbl_Product.ProductName,

                                existingLimit = a.PrincipalAmount,

                                proposedLimit = a.OutstandingInterest,
                                PastDueObligationsInterest = ((System.Decimal?)(
                              a.AllowForceDebitRepayment == false ? (System.Decimal?)
                                (from c in context.tbl_Loan_Force_Debit
                                 where c.LoanId == a.TermLoanId && c.TransactionTypeId == (byte)LoanTransactionTypeEnum.Interest
                                 select new
                                 {
                                     DebitRepayment = (c.DebitAmount - c.CreditAmount)
                                 }).Sum(p => p.DebitRepayment) :
                              a.AllowForceDebitRepayment == false ? (System.Decimal?)
                                (from c in context.tbl_Loan_Force_Debit
                                 where c.LoanId == a.TermLoanId &&
                                    c.TransactionTypeId == (byte)LoanTransactionTypeEnum.Interest
                                 select new
                                 {
                                     DebitRepayment = (c.DebitAmount - c.CreditAmount)
                                 }).Sum(p => p.DebitRepayment) : null) ?? (System.Decimal?)0 ?? 0),
                                PastDueObligationsPrincipal = ((System.Decimal?)(
                              a.AllowForceDebitRepayment == false ?
                                (from c in context.tbl_Loan_Force_Debit
                                 where c.LoanId == a.TermLoanId && c.TransactionTypeId == (byte)LoanTransactionTypeEnum.Principal
                                 select new
                                 {
                                     DebitRepayment = (c.DebitAmount - c.CreditAmount)
                                 }).Sum(p => p.DebitRepayment) :
                              a.AllowForceDebitRepayment == false ? (System.Decimal?)
                                (from c in context.tbl_Loan_Force_Debit
                                 where c.LoanId == a.TermLoanId && c.TransactionTypeId == (byte)LoanTransactionTypeEnum.Principal
                                 select new
                                 {
                                     DebitRepayment = (c.DebitAmount - c.CreditAmount)
                                 }).Sum(p => p.DebitRepayment) : null) ?? (System.Decimal?)0 ?? 0),
                                reviewDate = DateTime.Now
                            });

                return data.ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public IQueryable<LoanViewModel> SearchForLoan(string searchQuery)
        {
            try
            {
                IQueryable<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    
                    allFilteredLoan = (from a in context.tbl_Loan
                                       join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                                       join c in context.tbl_CASA on a.CasaAccountId equals c.CasaAccountId
                                       where (a.LoanReferenceNumber.Contains(searchQuery) ||
                                       b.CustomerCode.ToLower().Contains(searchQuery) ||
                                       b.FirstName.ToLower().Contains(searchQuery) ||
                                       b.LastName.ToLower().Contains(searchQuery) ||
                                       c.ProductAccountNumber.ToLower().Contains(searchQuery))
                                       select new LoanViewModel
                                       {
                                           loanId = a.TermLoanId,
                                           customerId = a.CustomerId,
                                           customerName = a.tbl_Customer.FirstName + " " + a.tbl_Customer.LastName,
                                           customerCode = a.tbl_Customer.CustomerCode,
                                           productId = a.ProductId,
                                           companyId = a.CompanyId,
                                           casaAccountId = a.CasaAccountId,
                                           branchId = a.BranchId,
                                           branchName = a.tbl_Branch.BranchName,
                                           loanReferenceNumber = a.LoanReferenceNumber,
                                           applicationReferenceNumber = a.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber ?? "N/A",
                                           principalFrequencyTypeId = a.PrincipalFrequencyTypeId != null ? (short)a.PrincipalFrequencyTypeId : (short)0,
                                           pricipalFrequencyTypeName = a.tbl_Frequency_Type.Mode,
                                           interestFrequencyTypeId = a.InterestFrequencyTypeId != null ? (short)a.InterestFrequencyTypeId : (short)0,
                                           interestFrequencyTypeName = a.tbl_Frequency_Type1.Mode,
                                           productTypeId = a.tbl_Product.ProductTypeId,
                                           productName = a.tbl_Product.ProductName, 
                                           productTypeName = a.tbl_Product.tbl_Product_Type.ProductTypeName,
                                           principalNumberOfInstallment = a.PrincipalNumberOfInstallment,
                                           interestNumberOfInstallment = a.InterestNumberOfInstallment,

                                           relationshipOfficerId = a.RelationshipOfficerId,
                                           relationshipOfficerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                                           relationshipManagerId = a.RelationshipManagerId,
                                           relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                                       
                                           misCode = a.MISCode,
                                           teamMiscode = a.TeamMISCode,
                                           interestRate = a.InterestRate,
                                           effectiveDate = a.EffectiveDate,
                                           maturityDate = a.MaturityDate,
                                           bookingDate = a.BookingDate,
                                           principalAmount = a.PrincipalAmount,
                                           principalInstallmentLeft = a.PrincipalInstallmentLeft,
                                           interestInstallmentLeft = a.InterestInstallmentLeft,
                                           approvalStatusId = a.ApprovalStatusId,
                                           approvedBy = a.ApprovedBy,
                                           approverComment = a.ApproverComment,
                                           dateApproved = a.DateApproved,
                                           loanStatusId = a.LoanStatusId,
                                           scheduleTypeId = a.ScheduleTypeId,
                                           scheduleTypeName = a.tbl_Loan_Schedule_Type.ScheduleTypeName,
                                           isDisbursed = a.IsDisbursed,
                                           disbursedBy = a.DisbursedBy,
                                           disburserComment = a.DisburserComment,
                                           disburseDate = a.DisburseDate,
                                           //approvedAmount = a.ApprovedAmount,
                                           operationId = a.OperationId,
                                           operationName = context.tbl_Operations.FirstOrDefault(x => x.OperationId == a.OperationId).OperationName,
                                           subSectorName = a.tbl_Sub_Sector.Name,
                                           sectorName = a.tbl_Sub_Sector.tbl_Sector.Name,
                                           productAccountNumber = a.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                                           productAccountName = a.tbl_Product.tbl_Chart_Of_Account.AccountName,
                                           customerGroupId = a.CustomerGroupId,
                                           loanTypeId = a.LoanTypeId,
                                           loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                                           equityContribution = a.EquityContribution,
                                           firstPrincipalPaymentDate = a.FirstPrincipalPaymentDate,
                                           firstInterestPaymentDate = a.FirstInterestPaymentDate,
                                           outstandingPrincipal = a.OutstandingPrincipal,
                                           outstandingInterest = a.OutstandingInterest,
                                           principalAdditionCount = a.PrincipalAdditionCount ?? 0,
                                           principalReductionCount = a.PrincipalReductionCount ?? 0,
                                           fixedPrincipal = a.FixedPrincipal,
                                           profileLoan = a.ProfileLoan,
                                           dischargeLetter = a.DischargeLetter,
                                           suspendInterest = a.SuspendInterest,
                                           customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                                           createdBy = a.CreatedBy,
                                           dateTimeCreated = a.DateTimeCreated,
                                           isCamsol = context.tbl_Loan_Camsol.Where(x => x.LoanId == a.TermLoanId).Any(),
                                           exchangeRate = a.ExchangeRate,
                                           currencyId = a.CurrencyId,
                                           currency = a.tbl_Currency.CurrencyName
                                       }).Take(10).AsQueryable();
                }
                return allFilteredLoan;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> GetLoanScheduleByLoanId(int loanId)
        {
            var loanSchedule = (from sch in context.tbl_Loan_Schedule_Periodic
                                where sch.LoanId == loanId
                                select new LoanPaymentSchedulePeriodicViewModel
                                {
                                    loanId = sch.LoanId,
                                    paymentNumber = sch.PaymentNumber,
                                    paymentDate = sch.PaymentDate,
                                    startPrincipalAmount = (double)sch.StartPrincipalAmount,
                                    periodPaymentAmount = (double)sch.PeriodPaymentAmount,
                                    periodInterestAmount = (double)sch.PeriodInterestAmount,
                                    periodPrincipalAmount = (double)sch.PeriodPrincipalAmount,
                                    endPrincipalAmount = (double)sch.EndPrincipalAmount,
                                    interestRate = sch.InterestRate,
                                    amortisedStartPrincipalAmount = (double)sch.AmortisedStartPrincipalAmount,
                                    amortisedPeriodPaymentAmount = (double)sch.AmortisedPeriodPaymentAmount,
                                    amortisedPeriodInterestAmount = (double)sch.AmortisedPeriodInterestAmount,
                                    amortisedPeriodPrincipalAmount = (double)sch.AmortisedPeriodPrincipalAmount,
                                    amortisedEndPrincipalAmount = (double)sch.AmortisedEndPrincipalAmount,
                                    effectiveInterestRate = sch.EffectiveInterestRate
                                }).ToList();
            return loanSchedule;
        }
    }


}

