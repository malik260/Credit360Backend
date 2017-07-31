using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using System.Data;

namespace FintrakBanking.Repositories.Credit
{
    using wct = XLeratorDLL_financial.XLeratorDLL_financial;
    using FinancialTypes = XLeratorDLL_financial.FinancialTypes;

    [Export(typeof(ILoanRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanRepository : ILoanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;        

        public LoanRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;            
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
            var data = ((this.context.tbl_Loan.Count(x => x.CustomerId == customerId && x.ProductId == productId)) +1);
            return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}"; 
        }
       
        private string GenerateLoanTypeBatchCode()
        {
            var data = this.context.tbl_Loan_Type_Batch.Count();
            int counter = data + 1;

            var numberCode = string.Format("{0}", counter.ToString().PadLeft(9, '0'));
            
            return numberCode;
        }

        private int  GetLoanTypeBatchId(LoanTypeEnum loanTypeId, int customerId, int customerGroupId, decimal groupAmount)
        {
            if (loanTypeId == LoanTypeEnum.Single)
            { return 1; }
            else if (loanTypeId == LoanTypeEnum.Batch)
            {
                int ? loanInfo = (from data in context.tbl_Loan
                                where data.CustomerId == customerId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
                                select data.LoanTypeBatchId).FirstOrDefault();

                if (loanInfo.HasValue)
                    return loanInfo.Value;
                else
                {
                    var data = new tbl_Loan_Type_Batch()
                    {
                        LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
                        CustomerId = customerId,
                        LoanTypeId = (short)loanTypeId,
                        DateCreated = generalSetup.GetApplicaionDate()
                    };

                    this.context.tbl_Loan_Type_Batch.Add(data);

                    context.SaveChanges();

                    return data.LoanTypeBatchId;
                }
            }
            else if (loanTypeId == LoanTypeEnum.CustomerGroup)
            {
                int ? loanInfo = (from data in context.tbl_Loan
                                where data.CustomerGroupId == customerGroupId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
                                select data.LoanTypeBatchId).FirstOrDefault();

                if (loanInfo.HasValue)
                    return loanInfo.Value;
                else
                {
                    var data = new tbl_Loan_Type_Batch()
                    {
                        LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
                        CustomerGroupId = customerGroupId,
                        GroupAmount = groupAmount,
                        LoanTypeId = (short)loanTypeId,
                        DateCreated = generalSetup.GetApplicaionDate()
                    };

                    this.context.tbl_Loan_Type_Batch.Add(data);

                    context.SaveChanges();

                    return data.LoanTypeBatchId;
                }
            }

            return -1;

        }

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

        public int CalculateNumberOfInstallments(short frequencyTypeId, int tenor, int daysInAYear)
        {
            double totalTenor = 0;

            //if (tenorModeId == TenorModeEnum.Days)
            totalTenor = tenor / daysInAYear; //365 days in a year
 

            if (frequencyTypeId == 10 || frequencyTypeId == 11) // 10 = end of period and 11 = now
                return 1;

            var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == frequencyTypeId).Value;

            var installments = totalTenor * frequencyValue;

            return Convert.ToInt32(installments);
        }

        public DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == frequencyTypeId).Value;

            DateTime output = effectiveDate.AddMonths( 12/(int)frequencyValue);
            return output;
        }


        public string AddLoanBooking(LoanViewModel entity)
        {
            if (entity.terminalDate <= entity.effectiveDate)
                throw new Exception("Loan terminal date should be more than effective date");

            var data = new tbl_Loan
            {
                LoanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId),
                LoanStatusId = (short)LoanStatusEnum.Inactive,
                IsDisbursed = false,
                PrincipalNumberOfInstallment = CalculateNumberOfInstallments((TenorModeEnum) entity.tenorModeId, entity.principalFrequencyTypeId, entity.tenor),
                InterestNumberOfInstallment = CalculateNumberOfInstallments((TenorModeEnum) entity.tenorModeId, entity.interestFrequencyTypeId, entity.tenor),
                FirstPrincipalPaymentDate = entity.firstPrincipalPaymentDate,
                FirstInterestPaymentDate = entity.firstInterestPaymentDate,
                IsScheduledPrepayment = entity.isScheduledPrepayment,
                ScheduledPrepaymentAmount = entity.scheduledPrepaymentAmount,
                ScheduledPrepaymentDate = entity.scheduledPrepaymentDate,
                ScheduledPrepaymentFrequencyTypeId = entity.scheduledPrepaymentFrequencyTypeId,

                CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                LoanTypeBatchId = GetLoanTypeBatchId((LoanTypeEnum) entity.loanTypeId, entity.customerId, entity.customerGroupId ?? -1, entity.groupAmount ?? 0),


                EffectiveDate = entity.effectiveDate,
                TerminalDate = entity.terminalDate,
                HasLien = false,
                HasOfferLetter = false,
                DischargeLetter = false,
                SuspendInterest = false,
                CanDisburse = false,
                Booked = false,
                CreditAppraisalCompleted = false,
                Scheduled = entity.scheduled,
                CustomerId = entity.customerId,
                ProductId =(short) entity.productId,
                CompanyId = entity.companyId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,                
                Tenor = entity.tenor,
                TenorModeId = entity.tenorModeId,
                PrincipalFrequencyTypeId = entity.principalFrequencyTypeId,
                InterestFrequencyTypeId = entity.interestFrequencyTypeId,
                FeeFrequencyTypeId = entity.feeFrequencyTypeId,                
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
                ApprovedAmount = entity.approvedAmount, 
                OperationId = entity.operationId,
                TrancheBatchCode = entity.trancheBatchCode,
                EquityContribution = entity.equityContribution,
                FeePercent = entity.feePercent,                
                OutstandingPrincipal = entity.outstandingPrincipal,
                PrincipalAdditionCount = entity.principalAdditionCount,
                PrincipalReductionCount = entity.principalReductionCount,
                FixedPrincipal = entity.fixedPrincipal,
                ProfileLoan = entity.profileLoan,                                
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,
                DateCreated = generalSetup.GetApplicaionDate(),
                CreatedBy = (int)entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicaionDate()
            };

            context.tbl_Loan.Add(data);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanApplication,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {entity.loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            var dataCount = context.SaveChanges();

            if (dataCount > 0)
                return entity.loanReferenceNumber;
            else
                return "";
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
                        tenorModeId = data.TenorModeId,
                        principalFrequencyTypeId = data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = data.InterestFrequencyTypeId,
                        feeFrequencyTypeId = data.FeeFrequencyTypeId,
                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        terminalDate = data.TerminalDate,
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
                        creditAppraisalCompleted = data.CreditAppraisalCompleted,
                        operationId = data.OperationId,
                        hasLien = data.HasLien,
                        hasOfferLetter = data.HasOfferLetter,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        loanTypeBatchId = data.LoanTypeBatchId,
                        trancheBatchCode = data.TrancheBatchCode,
                        equityContribution = data.EquityContribution,
                        feePercent = data.FeePercent,
                        firstPrincipalPaymentDate = data.FirstPrincipalPaymentDate,
                        firstInterestPaymentDate = data.FirstInterestPaymentDate,
                        outstandingPrincipal = data.OutstandingPrincipal,
                        principalAdditionCount = data.PrincipalAdditionCount,
                        principalReductionCount = data.PrincipalReductionCount,
                        fixedPrincipal = data.FixedPrincipal,
                        profileLoan = data.ProfileLoan,
                        dischargeLetter = data.DischargeLetter,
                        suspendInterest = data.SuspendInterest,
                        canDisburse = data.CanDisburse,
                        booked = data.Booked,
                        scheduled = data.Scheduled,
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
                        tenorModeId = data.TenorModeId,
                        principalFrequencyTypeId = data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = data.InterestFrequencyTypeId,
                        feeFrequencyTypeId = data.FeeFrequencyTypeId,
                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        terminalDate = data.TerminalDate,
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
                        creditAppraisalCompleted = data.CreditAppraisalCompleted,
                        operationId = data.OperationId,
                        hasLien = data.HasLien,
                        hasOfferLetter = data.HasOfferLetter,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        loanTypeBatchId = data.LoanTypeBatchId,
                        trancheBatchCode = data.TrancheBatchCode,
                        equityContribution = data.EquityContribution,
                        feePercent = data.FeePercent,
                        firstPrincipalPaymentDate = data.FirstPrincipalPaymentDate,
                        outstandingPrincipal = data.OutstandingPrincipal,
                        principalAdditionCount = data.PrincipalAdditionCount,
                        principalReductionCount = data.PrincipalReductionCount,
                        fixedPrincipal = data.FixedPrincipal,
                        profileLoan = data.ProfileLoan,
                        dischargeLetter = data.DischargeLetter,
                        suspendInterest = data.SuspendInterest,
                        canDisburse = data.CanDisburse,
                        booked = data.Booked,
                        scheduled = data.Scheduled,
                        customerSensitivityLevelId = data.CustomerSensitivityLevelId,
                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated

                    });
        }

        public IQueryable<LoanViewModel> GetLoansByCompanyId(int companyId)
        {
            return (context.tbl_Loan.Include("tbl_Customer").Include("tbl_CASA_AccountStatus")
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
                    tenorModeId = o.TenorModeId,
                    principalFrequencyTypeId = o.PrincipalFrequencyTypeId,
                    interestFrequencyTypeId = o.InterestFrequencyTypeId,
                    feeFrequencyTypeId = o.FeeFrequencyTypeId,
                    principalNumberOfInstallment = o.PrincipalNumberOfInstallment,
                    interestNumberOfInstallment = o.InterestNumberOfInstallment,
                    relationshipOfficerId = o.RelationshipOfficerId,
                    relationshipManagerId = o.RelationshipManagerId,
                    misCode = o.MISCode,
                    teamMiscode = o.TeamMISCode,
                    interestRate = o.InterestRate,
                    effectiveDate = o.EffectiveDate,
                    terminalDate = o.TerminalDate,
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

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            IEnumerable<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            if (loanInput.scheduleMethodId == 5) //irregular schedule
                output = GenerateIrregularLoanPeriodicSchedule(loanInput, loanInput.irregularPaymentSchedule).ToList();

            return output;
        }

        private int GetDaysInAYear(short dayCountId)
        {
            if (dayCountId == 1)
            {
                var currentDate = DateTime.Now;
                var firstDate = new DateTime(currentDate.Year, 1, 1); //  DateTime.ParseExact(user, "MM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var lastdate = new DateTime(currentDate.Year, 12, 31);
                var difference = (lastdate - firstDate).TotalDays;

                return Convert.ToInt32(difference);
            }

            var value = context.tbl_Day_Count.FirstOrDefault(x => x.DayCountId == dayCountId).DaysInAYear;

            return value;
        }

        /// <summary>
        /// USE FOR NORMAL ANNUITY SCHEDULE AS SHOWN WHERE INTEREST AND PRINCIPAL DROPS THE SAME DAY
        /// </summary>
        /// <param name="loanInput"></param>
        /// <returns></returns>         
        private IEnumerable<LoanPaymentSchedulePeriodicViewModel> GenerateNormalAnnuityPeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear(loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFrequency, loanInput.tenor, daysInAYear);
            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value; ;
            
            Double FV;
            FinancialTypes.InterestRuleType IntRule;
            FinancialTypes.AMORTSCHED_table result;
            FinancialTypes.AMORTSCHED_table amortisedResult;

            FV = wct.NULL_DOUBLE;
            IntRule = FinancialTypes.InterestRuleType.Actuarial;

            //result = wct.AMORTSCHED(PV, LoanDate, rate, FirstPayDate, NumPmts, Pmtpyr, DaysInYr, FV, IntRule);

            result = wct.AMORTSCHED(loanInput.principalAmount, loanInput.effectiveDate, (loanInput.interestRate/100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            amortisedResult = wct.AMORTSCHED(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, (loanInput.interestRate / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            //var schedule = from a in result.Rows.c join b in amortisedResult on a.num_pmt equals b.num_pmt

            //public int num_pmt;
            //public DateTime date_pmt;
            //public double amt_prin_init;
            //public double amt_pmt;
            //public double amt_int_pay;
            //public double amt_prin_pay;
            //public double amt_int_def;
            //public double amt_prin_end;

            int counter = 1;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);
                payment.amortisedPaymentNumber = Convert.ToInt32(amortisedResult.Rows[counter]["num_pmt"]);


                output.Add(payment);

                counter += 1;
            }

            return output;

        }

        private IEnumerable<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularLoanPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput, IEnumerable<IrregularLoanScheduleInputViewModel> paymentSchedule)
        {
            if (paymentSchedule.Count() == 0)
                throw new Exception("Specify a repayment schedule");

            if (loanInput.principalAmount != (paymentSchedule.Sum(x => x.paymentAmount)))
                throw new Exception("Payment Amount is not equal to the principal Amount");

            if (loanInput.effectiveDate > (paymentSchedule.Min(x => x.paymentDate)))
                throw new Exception("Effective Date should be less than the payment date(s)");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            var data = paymentSchedule.OrderBy(x => x.paymentDate);

            int paymentNumber = 1;
            double previousPrincipalAmount = loanInput.principalAmount;
            DateTime previousPaymentDate = loanInput.effectiveDate;
            int daysInAYear = GetDaysInAYear(loanInput.accurialBasis);  


            foreach (var item in data)
            {
                LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
                loanPeriod.paymentNumber = paymentNumber;
                loanPeriod.paymentDate = item.paymentDate;
                loanPeriod.startPrincipalAmount = previousPrincipalAmount;
                loanPeriod.periodPrincipalAmount = item.paymentAmount;

                var dateDifference = (item.paymentDate - previousPaymentDate).TotalDays;

                loanPeriod.periodInterestAmount = (previousPrincipalAmount * (loanInput.interestRate / 100.0)) * (dateDifference / daysInAYear);
                loanPeriod.periodicPaymentAmount = loanPeriod.periodPrincipalAmount + loanPeriod.periodInterestAmount;
                loanPeriod.endPrincipalAmount = loanPeriod.startPrincipalAmount - loanPeriod.periodPrincipalAmount;

                output.Add(loanPeriod);

                previousPrincipalAmount = loanPeriod.endPrincipalAmount;
                previousPaymentDate = loanPeriod.paymentDate;
                paymentNumber += 1;

            }

            return output;

        }
    }
}

