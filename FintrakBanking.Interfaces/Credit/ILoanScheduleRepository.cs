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

        List<LoanPaymentSchedulePeriodicViewModel> AnnuityFrequencyChange(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate, DateTime maturityDate);
        List<LoanPaymentSchedulePeriodicViewModel> BallonInterestRateChangePrepayment(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate, double prepaymentAmount);


        List<LoanIrregularScheduleViewModel> GetIrregularSchedule(int loanApplicationDetailId);

        List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularPeriodicScheduleForPrepayment(LoanPaymentScheduleInputViewModel loanInput, bool isArmotisedSchedule);

        List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularPeriodicScheduleForInterestRateChange(LoanPaymentScheduleInputViewModel loanInput, bool isArmotisedSchedule);

        List<LoanPaymentSchedulePeriodicViewModel> BallonInterestRateChange(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate, DateTime maturityDate);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeEvenPrincipalPaymentsKeepExistingNewAnnuityNew(int loanID, DateTime effectiveDate, double interestRate, int frequencyId, DateTime maturityDate);

        int GetDaysInAYear(DayCountConventionEnum dayCountId);

        List<LoanPaymentSchedulePeriodicViewModel> BulletPrepayments(int loanID, DateTime effectiveDate, double prepaymentAmount);

        List<LoanPaymentSchedulePeriodicViewModel> BulletInterestRateChange(int loanID, DateTime effectiveDate, double interestRate, DateTime maturityDate);

        List<LoanPaymentSchedulePeriodicViewModel> EvenPrincipalPaymentsNewAnnuity(int loanID, DateTime effectiveDate, DateTime maturityDate);

        List<LoanPaymentSchedulePeriodicViewModel> PrepaymentWithKeepExistingAnnuityAndUnEqualPayment(int loanID, DateTime effectiveDate, double prepaymentAmount, int principalRepaymentFrequency, int interestRepaymentFrequency);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeWithKeepExistingAnnuityAndUnEqualPayment(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate);

        //List<TBL_LOAN_SCHEDULE_PERIODIC> FrquencyChangeWithNewAnnuity(int loanID, DateTime effectiveDate, int frequencyId);

        DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId);

        List<LoanPaymentSchedulePeriodicViewModel> PrepaymentWithNewAnnuity(int loanID, DateTime effectiveDate, double prepaymentAmount);

        IEnumerable<LookupViewModel> GetAllLoanScheduleCategory();

        IEnumerable<LookupViewModel> GetAllLoanScheduleType();

        IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId);

        IEnumerable<LookupViewModel> GetAllLoanScheduleType(short? productTypeId);

        List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        //List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentRestructureScheduleInputViewModel loanInput);

        List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        List<FeePaymentScheduleViewModel> GenerateFeeSchedule(decimal recurringAmount, DateTime startDate, DateTime endDate, int feeDay, FrequencyTypeEnum frequency);

        bool AddLoanSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, int staffId);

        bool AddLoanFeeSchedule(int loanId, decimal amount, DateTime feeDate, DateTime loanMaturityDate, int feeDay, FrequencyTypeEnum frequency);

        byte[] GenerateLoanScheduleExport(LoanPaymentScheduleInputViewModel loanInput);

        List<LoanPaymentSchedulePeriodicViewModel> PrepaymentWithKeepExistingAnnuity(int loanID, DateTime effectiveDate, double prepaymentAmount);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeWithKeepExistingAnnuity(int loanID, DateTime effectiveDate, double interestRate);

        List<LoanPaymentSchedulePeriodicViewModel> EvenPrincipalPaymentsKeepExistingAnnuity(int loanID, DateTime effectiveDate, double prepaymentAmount);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeEvenPrincipalPaymentsKeepExistingAnnuity(int loanID, DateTime effectiveDate, double interestRate);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeEvenPrincipalPaymentsKeepExistingNewAnnuity(int loanID, DateTime effectiveDate, double interestRate, int frequencyId, DateTime maturityDate);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeEvenPrincipalPaymentsKeepExistingNewAnnuity(int loanID, DateTime effectiveDate, double interestRate, double prepaymentAmount);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeWithNewAnnuityAndUnEqualPayment(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate, DateTime maturityDate);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeWithNewAnnuityAndUnEqualPayment(int loanID, DateTime effectiveDate, int principalRepaymentFrequency, int interestRepaymentFrequency, double interestRate, double prepaymentAmount);

        List<LoanPaymentSchedulePeriodicViewModel> InterestRateChangeWithNewAnnuity(int loanID, DateTime effectiveDate, double interestRate, int frequencyId, DateTime maturityDate);

    }
}
