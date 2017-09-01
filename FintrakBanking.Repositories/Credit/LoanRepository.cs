using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.General;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    using FinancialTypes = XLeratorDLL_financial.FinancialTypes;
    using wct = XLeratorDLL_financial.XLeratorDLL_financial;

    public class LoanRepository : ILoanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;


        public LoanRepository(FinTrakBankingContext _context,IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        ILoanCovenantRepository _loanCovenant, IFinanceTransactionRepository _financeTransaction)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.loanCovenant = _loanCovenant;
            this.financeTransaction = _financeTransaction;
        }

        //public List<LoanPaymentScheduleOutput> GenerateLoanPaymentSchedule(LoanPaymentScheduleInput input)
        //{
        //    return LoanPaymentSchedule.GenerateLoanPaymentSchedule(input);
        //}

        public IEnumerable<LookupViewModel> GetAllLoanTypes()
        {
            return (from data in context.tbl_Loan_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.LoanTypeId,
                        lookupName = data.LoanTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllLoanScheduleCategory()
        {
            return (from data in context.tbl_Loan_Schedule_Category
                    select new LookupViewModel()
                    {
                        lookupId = data.ScheduleCategoryId,
                        lookupName = data.ScheduleCategoryName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllLoanScheduleType()
        {
            return (from data in context.tbl_Loan_Schedule_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.ScheduleTypeId,
                        lookupName = data.ScheduleTypeName,
                        lookupTypeId = data.ScheduleCategoryId,
                        lookupTypeName = data.tbl_Loan_Schedule_Category.ScheduleCategoryName
                    });
        }


        public IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId)
        {
            return (from data in context.tbl_Loan_Schedule_Type
                    where data.ScheduleCategoryId == categoryId
                    select new LookupViewModel()
                    {
                        lookupId = data.ScheduleTypeId,
                        lookupName = data.ScheduleTypeName,
                        lookupTypeId = data.ScheduleCategoryId,
                        lookupTypeName = data.tbl_Loan_Schedule_Category.ScheduleCategoryName
                    });
        }


        private string GenerateLoanReferenceNumber(int customerId, int productId)
        {
            var customerCode = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == customerId).CustomerCode;
            var productCode = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productId).ProductCode;
            var data = ((this.context.tbl_Loan.Count(x => x.CustomerId == customerId && x.ProductId == productId)) + 1);
            return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
        }

        //private string GenerateLoanTypeBatchCode()
        //{
        //    var data = this.context.tbl_Loan_Type_Batch.Count();
        //    int counter = data + 1;

        //    var numberCode = string.Format("{0}", counter.ToString().PadLeft(9, '0'));

        //    return numberCode;
        //}

        //private int GetLoanTypeBatchId(LoanTypeEnum loanTypeId, int customerId, int customerGroupId, decimal groupAmount)
        //{
        //    if (loanTypeId == LoanTypeEnum.Single)
        //    { return 1; }
        //    else if (loanTypeId == LoanTypeEnum.Batch)
        //    {
        //        int? loanInfo = (from data in context.tbl_Loan
        //                         where data.CustomerId == customerId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
        //                         select data.LoanTypeBatchId).FirstOrDefault();

        //        if (loanInfo.HasValue)
        //            return loanInfo.Value;
        //        else
        //        {
        //            var data = new tbl_Loan_Type_Batch()
        //            {
        //                LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
        //                CustomerId = customerId,
        //                LoanTypeId = (short)loanTypeId,
        //                DateCreated = generalSetup.GetApplicationDate()
        //            };

        //            this.context.tbl_Loan_Type_Batch.Add(data);

        //            context.SaveChanges();

        //            return data.LoanTypeBatchId;
        //        }
        //    }
        //    else if (loanTypeId == LoanTypeEnum.CustomerGroup)
        //    {
        //        int? loanInfo = (from data in context.tbl_Loan
        //                         where data.CustomerGroupId == customerGroupId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
        //                         select data.LoanTypeBatchId).FirstOrDefault();

        //        if (loanInfo.HasValue)
        //            return loanInfo.Value;
        //        else
        //        {
        //            var data = new tbl_Loan_Type_Batch()
        //            {
        //                LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
        //                CustomerGroupId = customerGroupId,
        //                GroupAmount = groupAmount,
        //                LoanTypeId = (short)loanTypeId,
        //                DateCreated = generalSetup.GetApplicationDate()
        //            };

        //            this.context.tbl_Loan_Type_Batch.Add(data);

        //            context.SaveChanges();

        //            return data.LoanTypeBatchId;
        //        }
        //    }

        //    return -1;

        //}

        public int CalculateNumberOfInstallments(TenorModeEnum tenorModeId, short frequencyTypeId, int tenor)
        {
            double totalTenor = 0;

            if (tenorModeId == TenorModeEnum.Days)
                totalTenor = tenor / 365; //365 days in a year
            else if (tenorModeId == TenorModeEnum.Months)
                totalTenor = tenor / 12; //12 = months in a year
            else if (tenorModeId == TenorModeEnum.Years)
                totalTenor = tenor;

            if (frequencyTypeId == 10 || frequencyTypeId == 11) // 10 = end of period and 11 = now
                return 1;

            var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == frequencyTypeId).Value;

            var installments = totalTenor * frequencyValue;

            return (int)installments;
        }

        public int CalculateNumberOfInstallments(DateTime firstPaymentDate, DateTime maturityDate, FrequencyTypeEnum frequencyType)
        {
            var startdate = LocalDateTime.FromDateTime(firstPaymentDate);
            var endDate = LocalDateTime.FromDateTime(maturityDate);

            //Period period = Period.Between(startdate, endDate, PeriodUnits.Months);

            double numberOfpayments = 0;

            if (frequencyType == FrequencyTypeEnum.Daily)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Days).Days;
            else if (frequencyType == FrequencyTypeEnum.Monthly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months;
            else if (frequencyType == FrequencyTypeEnum.Quarterly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 3.0;
            else if (frequencyType == FrequencyTypeEnum.SixTimesYearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 6.0;
            else if (frequencyType == FrequencyTypeEnum.ThriceYearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 4.0;
            else if (frequencyType == FrequencyTypeEnum.TwiceMonthly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months * 2.0;
            else if (frequencyType == FrequencyTypeEnum.TwiceYearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 6.0;
            else if (frequencyType == FrequencyTypeEnum.Weekly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Weeks).Weeks;
            else if (frequencyType == FrequencyTypeEnum.Yearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Years).Years;


            ////if (tenorModeId == TenorModeEnum.Days)
            //totalTenor = tenor / daysInAYear; //365 days in a year

            //var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == (short) frequencyType).Value;

            //var installments = totalTenor * frequencyValue;

            return Convert.ToInt32(numberOfpayments + 1);
        }

        public DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == frequencyTypeId).Value;

            DateTime output = effectiveDate.AddMonths(12 / (int)frequencyValue);
            return output;
        }

            public string AddLoanBooking(LoanViewModel entity)
        {
            if (entity.maturityDate <= entity.effectiveDate)
                throw new Exception("Loan terminal date should be more than effective date");

            
            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);
            
                var data = new tbl_Loan
            {
                LoanReferenceNumber = loanReferenceNumber,
                LoanStatusId = (short)LoanStatusEnum.Inactive,
                IsDisbursed = false,
                PrincipalNumberOfInstallment = 0, // loanSchedule.CalculateNumberOfInstallments((TenorModeEnum)entity.tenorModeId, entity.principalFrequencyTypeId, entity.tenor),
                InterestNumberOfInstallment = 0, // loanSchedule.CalculateNumberOfInstallments((TenorModeEnum)entity.tenorModeId, entity.interestFrequencyTypeId, entity.tenor),
                FirstPrincipalPaymentDate = entity.firstPrincipalPaymentDate, 
                FirstInterestPaymentDate = entity.firstInterestPaymentDate,
                IsScheduledPrepayment = entity.isScheduledPrepayment,
                ScheduledPrepaymentAmount = entity.scheduledPrepaymentAmount,
                ScheduledPrepaymentDate = entity.scheduledPrepaymentDate,
                ScheduledPrepaymentFrequencyTypeId = entity.scheduledPrepaymentFrequencyTypeId,

                CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                //LoanTypeBatchId = GetLoanTypeBatchId((LoanTypeEnum)entity.loanTypeId, entity.customerId, entity.customerGroupId ?? -1, entity.groupAmount ?? 0),

                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,

                DischargeLetter = false,
                SuspendInterest = false,

                CustomerId = entity.customerId,
                ProductId = (short)entity.productId,
                CompanyId = entity.companyId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,
                Tenor = entity.tenor,

                PrincipalFrequencyTypeId = entity.principalFrequencyTypeId,
                InterestFrequencyTypeId = entity.interestFrequencyTypeId,
                //FeeFrequencyTypeId = entity.feeFrequencyTypeId,
                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMiscode,
                InterestRate = entity.interestRate,
                PrincipalAmount = entity.principalAmount,
                PrincipalInstallmentLeft = entity.principalInstallmentLeft,
                InterestInstallmentLeft = entity.interestInstallmentLeft,
                ApprovalStatusId = entity.approvalStatusId,
                ApprovedBy = entity.approvedBy,
                ApproverComment = entity.approverComment,
                DateApproved = entity.dateApproved,
                ScheduleTypeId = entity.scheduleTypeId,
                DisbursedBy = entity.disbursedBy,
                DisburserComment = entity.disburserComment,
                DisburseDate = entity.disburseDate,
                ApprovedAmount = (decimal)entity.approvedAmount, 
                OperationId = entity.operationId,
                TrancheBatchCode = entity.trancheBatchCode,

                EquityContribution = (decimal)entity.equityContribution,
                OutstandingPrincipal = entity.outstandingPrincipal,
                PrincipalAdditionCount = entity.principalAdditionCount,
                PrincipalReductionCount = entity.principalReductionCount,
                FixedPrincipal = entity.fixedPrincipal,
                ProfileLoan = entity.profileLoan,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,
                DateCreated = generalSetup.GetApplicationDate(),
                CreatedBy = entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate(),
                tbl_Loan_Covenant_Detail = AddLoanCovenantDetail(entity.loanCovenant),
                tbl_Loan_Guarantor = AddLoanGuarantor(entity.loanGuarantor),
                tbl_Loan_Collateral_Mapping = AddLoanCollateralMapping(entity.loanCollateral),
                tbl_Loan_Fee = AddLoanFees(entity.loanChargeFee)

            };

            context.tbl_Loan.Add(data);
         
                //PostLoanDisbursment(entity);
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(BuildLoanDisbursmentPosting(entity));

                inputTransactions.AddRange(BuildLoanChargeFeesPosting(entity));

                financeTransaction.PostTransaction(inputTransactions);

            // Audit Section ---------------------------            
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanApplication,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {entity.loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var dataCount = context.SaveChanges();

            if (dataCount > 0)
                return loanReferenceNumber;
            else
            
                return "";
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

        public FinanceTransactionViewModel BuildLoanDisbursmentPosting(LoanViewModel model)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();


            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.LoanBooking;
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
            debit.glAccountId = model.casaAccountId;  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = model.principalAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BranchId;

            var repaymentGL = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = repaymentGL;

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

                feeTransaction.operationId = (int)OperationsEnum.LoanBooking;
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
                debit.debitAmount = (decimal)item.amount;
                debit.creditAmount = 0;
                debit.sourceBranchId = loanDetails.branchId;
                debit.destinationBranchId = casa.BranchId;

                var feeGL = this.context.tbl_Charge_Fee.Where(x => x.ChargeFeeId == item.chargeFeeId).Select(x => x.GLAccountId).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;
                credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                credit.casaAccountId = casa.CasaAccountId;
                credit.debitAmount = 0;
                credit.creditAmount = (decimal)item.amount;
                credit.sourceBranchId = loanDetails.branchId;
                credit.destinationBranchId = loanDetails.branchId;


                feeTransaction.transactionDetails.Add(debit);
                feeTransaction.transactionDetails.Add(credit);

                output.Add(feeTransaction);
            }

            // Audit Section ---------------------------            

            return output;

        }

        public ICollection<tbl_Loan_Covenant_Detail> AddLoanCovenantDetail(List<LoanCovenantDetailViewModel> covenantModel)
        {
            ICollection<tbl_Loan_Covenant_Detail> covenant;

            if (covenantModel.Count  < 1)
                return null;

            covenant = new List<tbl_Loan_Covenant_Detail>();
            foreach(LoanCovenantDetailViewModel entity in covenantModel)
            covenant.Add(new tbl_Loan_Covenant_Detail
            {
                CompanyId = entity.companyId,
                CovenantAmount = entity.covenantAmount,
                CovenantDate = entity.covenantDate,
                CovenantDetail = entity.covenantDetail,
                CovenantTypeId = entity.covenantTypeId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = this.generalSetup.GetApplicationDate().Date,
                FrequencyTypeId = entity.frequencyTypeId,
                LoanId = entity.loanId

            });

            return covenant;
        }

        private ICollection<tbl_Loan_Covenant_Detail> AddLoanCovenant(LoanCovenantDetailViewModel entity)
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
                CovenantTypeId = entity.covenantTypeId,
                FrequencyTypeId = entity.frequencyTypeId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate()
            });

            return covenant;
        }

        public ICollection<tbl_Loan_Guarantor> AddLoanGuarantor(List<LoanGuarantorViewModel> guarantorModel)
        {
            ICollection<tbl_Loan_Guarantor> guarantor;

            if (guarantorModel.Count < 1)
                return null;

            guarantor = new List<tbl_Loan_Guarantor>();
            foreach (LoanGuarantorViewModel entity in guarantorModel)
                guarantor.Add(new tbl_Loan_Guarantor
                {
                    LoanId = entity.loanId,
                    Firstname = entity.firstname,
                    Lastname = entity.lastname,
                    Middlename = entity.middlename,
                    Address = entity.address,
                    PhoneNumber1 = entity.phoneNumber1,
                    PhoneNumber2 = entity.phoneNumber2,
                    Relationship = entity.relationship,
                    RelationshipDuration = (short)entity.relationshipDuration,
                    BVN = entity.bvn,
                    EmailAddress = entity.emailAddress

                });

            return guarantor;
        }

        public ICollection<tbl_Loan_Collateral_Mapping> AddLoanCollateralMapping(List<LoanCollateralMappingViewModel> collateralModel)
        {
            ICollection<tbl_Loan_Collateral_Mapping> collateral;

            if (collateralModel.Count < 1)
                return null;

            collateral = new List<tbl_Loan_Collateral_Mapping>();
            foreach (LoanCollateralMappingViewModel entity in collateralModel)
                collateral.Add(new tbl_Loan_Collateral_Mapping
                {
                    LoanId = entity.loanId,
                    CollateralCustomerId =entity.collateralCustomerId,
                    LoanApplicationId = entity.loanApplicationId,
                });

            return collateral;
        }

        private ICollection<tbl_Loan_Fee> AddLoanFees(List<LoanChargeFeeViewModel> feeModel)
        {
            ICollection<tbl_Loan_Fee> fee;
            var feeAmount = 0;
            
            fee = new List<tbl_Loan_Fee>();

            foreach (LoanChargeFeeViewModel entity in feeModel)
            {
                if (entity.feeTypeId == 1)
                {
                    feeAmount = 0;
                }
                fee.Add(new tbl_Loan_Fee
                {
                    ChargeFeeId = entity.chargeFeeId,
                    FeeAmount = feeAmount,
                    FeeDependentAmount = entity.feeDependentAmount,
                    FeeRateValue = entity.feeRateValue,
                    IsIntegralFee = entity.isIntegralFee,
                    LoanId = entity.loanId
                });
            }
                
            return fee;
        }

        private IQueryable<LoanViewModel> GetAllLoans()
        {
            var data = (from l in context.tbl_Loan
                        select new LoanViewModel()
                        {
                            loanId = l.LoanId,
                            customerId = l.CustomerId,
                            customerName = l.tbl_Customer.FirstName + " " + l.tbl_Customer.LastName,
                            productId = l.ProductId,
                            companyId = l.CompanyId,
                            casaAccountId = l.CasaAccountId,
                            branchId = l.BranchId,
                            branchName = l.tbl_Branch.BranchName,
                            loanReferenceNumber = l.LoanReferenceNumber,
                            tenor = l.Tenor,
                            principalFrequencyTypeId = (short)l.PrincipalFrequencyTypeId,
                            interestFrequencyTypeId = (short)l.InterestFrequencyTypeId,
                            //feeFrequencyTypeId = l.FeeFrequencyTypeId,
                            principalNumberOfInstallment = l.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = l.InterestNumberOfInstallment,
                            relationshipOfficerId = l.RelationshipOfficerId,
                            relationshipManagerId = l.RelationshipManagerId,
                            misCode = l.MISCode,
                            teamMiscode = l.TeamMISCode,
                            interestRate = l.InterestRate,
                            effectiveDate = l.EffectiveDate,
                            maturityDate = l.MaturityDate,
                            dateCreated = l.DateCreated,
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
                            approvedAmount = l.ApprovedAmount,
                            //creditAppraisalCompleted = l.CreditAppraisalCompleted,
                            operationId = l.OperationId,
                            //hasLien = l.HasLien,
                            //hasOfferLetter = l.HasOfferLetter,
                            customerGroupId = l.CustomerGroupId,
                            loanTypeId = l.LoanTypeId,
                            loanTypeName = l.tbl_Loan_Type.LoanTypeName,
                            trancheBatchCode = l.TrancheBatchCode ?? "Empty",
                            equityContribution = l.EquityContribution,
                            firstPrincipalPaymentDate = l.FirstPrincipalPaymentDate ?? DateTime.Now,
                            firstInterestPaymentDate = l.FirstInterestPaymentDate ?? DateTime.Now,
                            outstandingPrincipal = l.OutstandingPrincipal ,
                            principalAdditionCount = l.PrincipalAdditionCount ?? 0,
                            principalReductionCount = l.PrincipalReductionCount ?? 0,
                            fixedPrincipal = l.FixedPrincipal,
                            profileLoan = l.ProfileLoan,
                            dischargeLetter = l.DischargeLetter,
                            suspendInterest = l.SuspendInterest,
                            customerSensitivityLevelId = l.CustomerSensitivityLevelId,
                            createdBy = l.CreatedBy,
                            dateTimeCreated = l.DateTimeCreated,
                            isCamsol = context.tbl_Loan_Camsol.Where(x => x.LoanId == l.LoanId).Any()
                        });
            return data;
        }

        public bool validateCamsol(int loanId)
        {
            var check = context.tbl_Loan_Camsol.Where(x => x.LoanId == loanId);

            if (check.Any())
            {
                return true;
            }

            return false;
        }

        public IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId)
        {
            return GetAllLoans().Where(l => l.customerId == customerId);
        }

        public IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId)
        {
            return GetAllLoans().Where(l => l.customerGroupId == customerGroupId);
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
                    where data.LoanId == loanId
                    select new LoanViewModel()
                    {
                        loanId = data.LoanId,
                        customerId = data.CustomerId,
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        casaAccountId = data.CasaAccountId,
                        branchId = data.BranchId,
                        loanReferenceNumber = data.LoanReferenceNumber,
                        tenor = data.Tenor,
                        principalFrequencyTypeId = (short)data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = (short)data.InterestFrequencyTypeId,
                        //feeFrequencyTypeId = data.FeeFrequencyTypeId,
                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        maturityDate = data.MaturityDate,
                        dateCreated = data.DateCreated,
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
                        approvedAmount = data.ApprovedAmount,
                        operationId = data.OperationId,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        trancheBatchCode = data.TrancheBatchCode,
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
                        loanId = data.LoanId,
                        customerId = data.CustomerId,
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        casaAccountId = data.CasaAccountId,
                        branchId = data.BranchId,
                        loanReferenceNumber = data.LoanReferenceNumber,
                        tenor = data.Tenor,
                        //tenorModeId = data.TenorModeId,
                        principalFrequencyTypeId = (short)data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = (short)data.InterestFrequencyTypeId,
                        //feeFrequencyTypeId = data.FeeFrequencyTypeId,
                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        maturityDate = data.MaturityDate,
                        dateCreated = data.DateCreated,
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
                        approvedAmount = data.ApprovedAmount,
                        operationId = data.OperationId,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        trancheBatchCode = data.TrancheBatchCode,
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
                    loanId = o.LoanId,
                    customerId = o.CustomerId,
                    productId = o.ProductId,
                    casaAccountId = o.CasaAccountId,
                    branchId = o.BranchId,
                    loanReferenceNumber = o.LoanReferenceNumber,
                    tenor = o.Tenor,
                    //tenorModeId = o.TenorModeId,
                    principalFrequencyTypeId = (short)o.PrincipalFrequencyTypeId,
                    interestFrequencyTypeId = (short)o.InterestFrequencyTypeId,
                    //feeFrequencyTypeId = o.FeeFrequencyTypeId,
                    principalNumberOfInstallment = o.PrincipalNumberOfInstallment,
                    interestNumberOfInstallment = o.InterestNumberOfInstallment,
                    relationshipOfficerId = o.RelationshipOfficerId,
                    relationshipManagerId = o.RelationshipManagerId,
                    misCode = o.MISCode,
                    teamMiscode = o.TeamMISCode,
                    interestRate = o.InterestRate,
                    effectiveDate = o.EffectiveDate,
                    maturityDate = o.MaturityDate,
                    dateCreated = o.DateCreated,
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


        public IQueryable<LoanPaymentScheduleViewModel> GenerateLoanSchedule(LoanPaymentScheduleInput input)
        {
            return null;
            //context
            //    .tblSPLoanSchedule
            //    .FromSql("sp_GenerateLoanPaymentSchedule @p0,@p1,@p2,@p3,@p4,@p5,@p6", 
            //    input.principalAmount,input.loanDate,input.interestRate,input.firstPaymentDate,
            //    input.numberOfPayments,input.numberOfPaymentsInAYear,input.daysInAYear);
             
        }

        public List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            //LoanPaymentScheduleExtendedInputViewModel loanDetails = (LoanPaymentScheduleInputViewModel) loanInput;

            //loanDetails.maturityDate = loanInput.interestFirstpaymentDate.AddDays(loanInput.tenor);
            //loanDetails.daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);
            //loanDetails.numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanDetails.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);
            //loanDetails.numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value; 

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();
            LoanScheduleTypeEnum scheduleMethod = (LoanScheduleTypeEnum) loanInput.scheduleMethodId;

            if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                output = GenerateIrregularLoanPeriodicScheduleWithAmortisedCost(loanInput).ToList();
            else if (scheduleMethod == LoanScheduleTypeEnum.Annuity)
            {
                if (loanInput.interestFirstpaymentDate == loanInput.principalFirstpaymentDate && loanInput.interestFrequency == loanInput.principalFrequency)
                    output = GenerateNormalAnnuityPeriodicLoanSchedule(loanInput);
                else
                    output = GenerateMoratoriumAnnuityPeriodicLoanSchedule(loanInput);
            }

            return output;
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

            var value = context.tbl_Day_Count.FirstOrDefault(x => x.DayCountId == (short) dayCountId).DaysInAYear;

            return value;
        }

        private double CalculateIRR(LoanPaymentScheduleInputViewModel loanInput, List<LoanPaymentSchedulePeriodicViewModel>  cashflow, int numberOfPayments, int numberOfPaymentsInAYear,
                                    int daysInAYear, Double FV, string interestRule, DateTime maturityDate)
        {
            double output = 0;


            if (loanInput.integralFeeAmount == 0)
                output = loanInput.interestRate / 100;
            else if (numberOfPayments < 2 && loanInput.interestRate > 0)
            {
                //var amounts = paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
                //var dates = paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.paymentDate).ToList();

                List<double> cashflowAmounts = new List<double>();
                List<int> paymentNumbers = new List<int>();

                cashflowAmounts.Add((loanInput.principalAmount - loanInput.integralFeeAmount) * -1);
                paymentNumbers.Add(0);

                var counter = 0;
                foreach (var payment in cashflow)
                {
                    if (counter > 0)
                    {
                        cashflowAmounts.Add(payment.periodPaymentAmount);
                        paymentNumbers.Add(payment.paymentNumber);
                    }

                    counter += 1;
                }

                output = wct.IRR(cashflowAmounts, paymentNumbers, wct.NULL_DOUBLE);
            }
            else if (numberOfPayments > 1 && loanInput.interestRate > 0)
            {
                var pmt = wct.LPMT(loanInput.principalAmount, loanInput.effectiveDate, loanInput.interestRate/100.0, loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear,
                                   daysInAYear, FV, interestRule);

                output = wct.LRATE(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, pmt, loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, 
                                      daysInAYear, 0, interestRule, wct.NULL_DOUBLE);
           
              //  SELECT @IRR = wct.LRATE(
              // @la - @fa--PV
              //, @sd--Loan Date
              //,[wct].[LPMT](@la, @sd, cast(cast(@rt as float) / cast(100 as float) as float), @fpd, @np, @rfqy, @NDY, NULL, 'U')--PMT
              //, @fpd--FirstPayment Date
              //, @np--Number ofPaymens
              //, @rfqy--Payments peryear
              //, @NDY--Days in Year
              //, 0--FV
              //, 'U'--InterestRule
              //, NULL--Guess
              //)

            }
            else if (loanInput.interestRate <= 0)
                output = 0;

            if(loanInput.interestFirstpaymentDate == maturityDate)
            {
                Period period = Period.Between(LocalDateTime.FromDateTime(loanInput.interestFirstpaymentDate), LocalDateTime.FromDateTime(maturityDate));

                output = output * (365 / period.Days);
            }

            return output * 100;
        }


        /// <summary>
        /// USE FOR NORMAL ANNUITY SCHEDULE AS SHOWN WHERE INTEREST AND PRINCIPAL DROPS THE SAME DAY
        /// </summary>
        /// <param name="loanInput"></param>
        /// <returns></returns>         
        private List<LoanPaymentSchedulePeriodicViewModel> GenerateNormalAnnuityPeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();            

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum) loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum) loanInput.interestFrequency);
            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value; 
            
            Double FV;
            FinancialTypes.InterestRuleType IntRule;
            FinancialTypes.AMORTSCHED_table result;      

            FV = wct.NULL_DOUBLE;
            IntRule = FinancialTypes.InterestRuleType.US;           

            //result = wct.AMORTSCHED(PV, LoanDate, rate, FirstPayDate, NumPmts, Pmtpyr, DaysInYr, FV, IntRule);

            result = wct.AMORTSCHED(loanInput.principalAmount, loanInput.effectiveDate, (loanInput.interestRate/100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);
            

            int counter = 0;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);                
                payment.paymentDate = Convert.ToDateTime(row["date_pmt"]);
                payment.startPrincipalAmount = Convert.ToDouble(row["amt_prin_init"]);
                payment.periodPaymentAmount = Convert.ToDouble(row["amt_pmt"]);
                payment.periodInterestAmount = Convert.ToDouble(row["amt_int_pay"]);
                payment.periodPrincipalAmount = Convert.ToDouble(row["amt_prin_pay"]);
                payment.endPrincipalAmount = Convert.ToDouble(row["amt_prin_end"]);
                payment.interestRate =  loanInput.interestRate / 100.0;

                output.Add(payment);

                counter += 1;
            }

            var maturityDate = output.Max(x => x.paymentDate); //loanInput.maturityDate

            var internalRateOfReturn = CalculateIRR(loanInput, output, numberOfPayments, numberOfPaymentsInAYear,
                                             daysInAYear, FV, "U", maturityDate);

            FinancialTypes.AMORTSCHED_table amortisedResult;
            amortisedResult = wct.AMORTSCHED(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, (internalRateOfReturn / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            counter = 0;
            foreach (var payment in output)
            {
                payment.amortisedStartPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_init"]);
                payment.amortisedPeriodPaymentAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_pmt"]);
                payment.amortisedPeriodInterestAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_int_pay"]);
                payment.amortisedPeriodPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_pay"]);
                payment.amortisedEndPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_end"]);
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return output;

        }

        /// <summary>
        /// USE FOR ANNUITY SCHEDULE WHERE THERE IS A MORATORIUM 
        /// </summary>
        /// <param name="loanInput"></param>
        /// <returns></returns>         
        private List<LoanPaymentSchedulePeriodicViewModel> GenerateMoratoriumAnnuityPeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value;

            var principalPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.principalFrequency).Value;

            int principalPaymentMultiple = Convert.ToInt32(12 / principalPaymentsInAYear);

            var principalFirstPaymentNumber = numberOfPayments - numberOfPrincipalPayments;

            Double FV = 0;
            
            FinancialTypes.UNEQUALLOANPAYMENTS_table result;
                        

            //result = wct.AMORTSCHED(loanInput.principalAmount, loanInput.effectiveDate, (loanInput.interestRate / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            result = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount, (loanInput.interestRate / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);


            int counter = 0;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);
                payment.paymentDate = Convert.ToDateTime(row["date_pmt"]);
                payment.startPrincipalAmount = Convert.ToDouble(row["amt_prin_init"]);
                payment.periodPaymentAmount = Convert.ToDouble(row["amt_pmt"]);
                payment.periodInterestAmount = Convert.ToDouble(row["amt_int_pay"]);
                payment.periodPrincipalAmount = Convert.ToDouble(row["amt_prin_pay"]);
                payment.endPrincipalAmount = Convert.ToDouble(row["amt_prin_end"]);
                payment.interestRate = loanInput.interestRate / 100.0;

                output.Add(payment);

                counter += 1;
            }

            var maturityDate = output.Max(x => x.paymentDate); //loanInput.maturityDate

            var internalRateOfReturn = CalculateIRR(loanInput, output, numberOfPayments, numberOfPaymentsInAYear,
                                             daysInAYear, FV, "U", maturityDate);

            FinancialTypes.UNEQUALLOANPAYMENTS_table amortisedResult;

            amortisedResult = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount - loanInput.integralFeeAmount, (internalRateOfReturn / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);

            counter = 0;
            foreach (var payment in output)
            {
                payment.amortisedStartPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_init"]);
                payment.amortisedPeriodPaymentAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_pmt"]);
                payment.amortisedPeriodInterestAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_int_pay"]);
                payment.amortisedPeriodPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_pay"]);
                payment.amortisedEndPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_end"]);
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return output;

        }

        public List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = GeneratePeriodicLoanSchedule(loanInput);

            List<LoanPaymentScheduleDailyViewModel> output = new List<LoanPaymentScheduleDailyViewModel>();

            int counter = 1;
            foreach (var item in periodicSchedule)
            {
  
                //output.Add(payment);

                counter += 1;
            }

            return output;

        }

        private IEnumerable<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanScheduleRange(LoanPaymentSchedulePeriodicViewModel periodicSchedule, DateTime nextPaymentDate, int lastRowCount)
        {
            List<LoanPaymentScheduleDailyViewModel> output = new List<LoanPaymentScheduleDailyViewModel>();

            var dateDifferenceCount = (periodicSchedule.paymentDate - nextPaymentDate).TotalDays;

            for (int counter = 1; counter <= dateDifferenceCount; counter++)
            {

            }

            return output;

        }


        private List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularLoanPeriodicScheduleWithAmortisedCost(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateIrregularLoanPeriodicSchedule(loanInput);

            var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
            var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

            amounts[0] = amounts[0] * -1;
            var internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE);

            //FinancialTypes.AMORTSCHED_table amortisedResult;
            //amortisedResult = wct.AMORTSCHED(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, (internalRateOfReturn / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn / 100.0;
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateIrregularLoanPeriodicSchedule(loanInput);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {                
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return paymentSchedule;
        }


        private List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularLoanPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput) 
        {
            if (loanInput.irregularPaymentSchedule.Count() == 0)
                throw new Exception("Specify a repayment schedule");

            if (loanInput.principalAmount != (loanInput.irregularPaymentSchedule.Sum(x => x.paymentAmount)))
                throw new Exception("Payment Amount is not equal to the principal Amount");


            
            if (loanInput.effectiveDate > (loanInput.irregularPaymentSchedule.Min(x => x.paymentDate)))

                throw new Exception("Effective Date should be less than the payment date(s)");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            output.Add(new LoanPaymentSchedulePeriodicViewModel {paymentNumber = 0, paymentDate = loanInput.effectiveDate, startPrincipalAmount = 0, periodPrincipalAmount = 0,
                          amortisedPeriodInterestAmount = 0, periodPaymentAmount = 0, endPrincipalAmount = loanInput.principalAmount});


            var data = loanInput.irregularPaymentSchedule.OrderBy(x => x.paymentDate);

            int paymentNumber = 1;
            double previousPrincipalAmount = loanInput.principalAmount;
            DateTime previousPaymentDate = loanInput.effectiveDate;
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum) loanInput.accurialBasis);  


            foreach (var item in data)
            {
                LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
                loanPeriod.paymentNumber = paymentNumber;
                loanPeriod.paymentDate = item.paymentDate;
                loanPeriod.startPrincipalAmount = previousPrincipalAmount;
                loanPeriod.periodPrincipalAmount = item.paymentAmount;

                var dateDifferenceCount = (item.paymentDate - previousPaymentDate).TotalDays;

                loanPeriod.periodInterestAmount = (previousPrincipalAmount * (loanInput.interestRate / 100.0)) * (dateDifferenceCount / daysInAYear);
                loanPeriod.periodPaymentAmount = loanPeriod.periodPrincipalAmount + loanPeriod.periodInterestAmount;
                loanPeriod.endPrincipalAmount = loanPeriod.startPrincipalAmount - loanPeriod.periodPrincipalAmount;

                output.Add(loanPeriod);

                previousPrincipalAmount = loanPeriod.endPrincipalAmount;
                previousPaymentDate = loanPeriod.paymentDate;
                paymentNumber += 1;

            }

            return output;

        }

        private List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId)
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
                            feeRateValue =  p.RateValue ,
                            feeDependentAmount =  p.DependentAmount ?? 0,
                            feeAmount = c.Amount ?? 0,
                            feeIntervalId = c.FeeIntervalId,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            feeTypeId = c.FeeTypeId,
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
            return null;
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

        public List<LoanGuarantorViewModel> GetLoanGuarantors(int loanId)
        {
            var data = (from c in context.tbl_Loan_Guarantor
                        where c.LoanId == loanId 
                        select new LoanGuarantorViewModel
                        {
                            loanGuarantorId = c.LoanGuarantorId,
                            loanId = (int)c.LoanId,
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
                            fullName = c.Lastname + " "+ c.Firstname + " "+c.Middlename

                        }).ToList();
            return data;
        }

        public List<LoanCollateralMappingViewModel> GetLoanCollaterals(int loanId)
        {
            var data = (from c in context.tbl_Loan_Collateral_Mapping
                        where c.LoanId == loanId
                        select new LoanCollateralMappingViewModel
                        {
                            loanCollateralMappingId = c.LoanCollateralMappingId,
                            loanId = c.LoanId,
                            collateralCustomerId = c.CollateralCustomerId,
                            loanApplicationId = c.LoanApplicationId

                        }).ToList();
            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel>  GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.tbl_Loan_Application
                        join c in context.tbl_Credit_Appraisal_Memorandum
                        on a.LoanApplicationId equals c.LoanApplicationId
                        join cust in context.tbl_Customer on a.CustomerId equals cust.CustomerId
                        where a.CompanyId == companyId && a.Deleted == false && c.IsCompleted == true
                        select new CamProcessedLoanViewModel
                        {
                            approvalStatusId = a.ApprovalStatusId,
                            loanApplicationId = a.LoanApplicationId,
                            applicationReferenceNumber = a.ApplicationReferenceNumber,

                            customerId = a.CustomerId ?? 0,
                            customerCode = cust.CustomerCode,
                            customerName = a.CustomerId.HasValue ? a.tbl_Customer.FirstName + " " + a.tbl_Customer.MiddleName + " " + a.tbl_Customer.LastName : "",

                            customerGroupId = a.CustomerGroupId ?? 0,
                            customerGroupName = a.CustomerGroupId.HasValue ? a.tbl_Customer_Group.GroupName : "",
                            customerGroupCode = a.tbl_Customer_Group.GroupCode,
                            
                            loanInformation = a.LoanInformation,
                            companyId = a.CompanyId,
                            branchId = (short)a.BranchId,
                            branchName = a.tbl_Branch.BranchName,
                            tenor = a.Tenor,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipOfficerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            relationshipManagerId = a.RelationshipManagerId,
                            relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,

                            currencyId = a.CurrencyId,
                            currencyCode = a.tbl_Currency.CurrencyCode,
                            loanTypeId = a.LoanTypeId,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            loanStatusId = a.LoanStatusId,
                            //loanStatusName = a.tbl_Loan_Status.AccountStatus,
                            camReference = c.CAMRef,
                            loanDetails = c.LoanDetails,
                            productId = (short)a.ProductId,
                            productName = a.tbl_Product.ProductName,

                            misCode = a.MISCode,
                            teamMisCode = a.TeamMISCode,

                            interestRate = a.InterestRate,
                            isRealatedParty = a.IsRealatedParty,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            submittedForAppraisal = a.SubmittedForAppraisal,
                            principalAmount = a.PrincipalAmount,
                            
                            createdBy = a.CreatedBy,
                            applicationDate = a.ApplicationDate,
                            dateTimeCreated = a.DateTimeCreated,

                            loanPreliminaryEvaluationId = a.LoanPreliminaryEvaluationId,
                            exchangeRate = a.ExchangeRate,
                            //nearestLandMark = a.NearestLandMark,
                            //nearestBusStop =a.NearestBusStop,
                            //longitude = a.Longitude,
                            //latitude = a.Latitude

                        }).ToList();
            return data;
        }

        private IEnumerable<LoanViewModel> BookedLoan(int companyId)
        {
            return GetAllLoans().Where(x => x.companyId == companyId);
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId)
        {
            var loans = BookedLoan(companyId);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x=>x.loanReferenceNumber == loanReferenceNumber);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                loan.loanCollateral = GetLoanCollaterals(loan.loanId);
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
                loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }


        #region CAM Approved Loan Applications
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
                            rateValue = (decimal) p.RateValue,
                            dependentAmount = p.DependentAmount

                        }).ToList();
            return data;
        }

        


        #endregion End of CAM Approved Loan Applications

    }

}

