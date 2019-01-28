using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanScheduleRepository
    {
        int CalculateNumberOfInstallments(TenorModeEnum tenorModeId, short frequencyTypeId, int tenor);

        int GetDaysInAYear(DayCountConventionEnum dayCountId);

        List<TBL_LOAN_SCHEDULE_PERIODIC> PrepaymentWithKeepExistingAnnuityAndUnEqualPayment(int loanID, DateTime effectiveDate, double prepaymentAmount, int principalRepaymentFrequency, int interestRepaymentFrequency);

        List<TBL_LOAN_SCHEDULE_PERIODIC> InterestRateChangeWithKeepExistingAnnuityAndUnEqualPayment(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate);

        //List<TBL_LOAN_SCHEDULE_PERIODIC> FrquencyChangeWithNewAnnuity(int loanID, DateTime effectiveDate, int frequencyId);

        DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId);

        List<TBL_LOAN_SCHEDULE_PERIODIC> PrepaymentWithNewAnnuity(int loanID, DateTime effectiveDate, double prepaymentAmount);

        IEnumerable<LookupViewModel> GetAllLoanScheduleCategory();

        IEnumerable<LookupViewModel> GetAllLoanScheduleType();

        IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId);

        IEnumerable<LookupViewModel> GetAllLoanScheduleType(short? productTypeId);

        List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        List<FeePaymentScheduleViewModel> GenerateFeeSchedule(decimal recurringAmount, DateTime startDate, DateTime endDate, int feeDay, FrequencyTypeEnum frequency);

        bool AddLoanSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, int staffId);

        bool AddLoanFeeSchedule(int loanId, decimal amount, DateTime feeDate, DateTime loanMaturityDate, int feeDay, FrequencyTypeEnum frequency);

       byte[] GenerateLoanScheduleExport(LoanPaymentScheduleInputViewModel loanInput);

        List<TBL_LOAN_SCHEDULE_PERIODIC> PrepaymentWithKeepExistingAnnuity(int loanID, DateTime effectiveDate, double prepaymentAmount);

        List<TBL_LOAN_SCHEDULE_PERIODIC> InterestRateChangeWithKeepExistingAnnuity(int loanID, DateTime effectiveDate, double interestRate);

    }
}
