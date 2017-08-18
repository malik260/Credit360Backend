
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanRepository
    {
        IEnumerable<LookupViewModel> GetAllLoanTypes();

        string AddLoanBooking(LoanViewModel entity);

        IEnumerable<LoanViewModel> GetLoanByCustomerId(int customerId);

        LoanViewModel GetLoan(int loanId);
        
        IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId);

        IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel);

        int CalculateNumberOfInstallments(TenorModeEnum tenorModeId, short frequencyTypeId, int tenor);

        DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId);

        IEnumerable<LookupViewModel> GetAllLoanScheduleCategory();

        IEnumerable<LookupViewModel> GetAllLoanScheduleType();

        IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId);
        IQueryable<LoanPaymentScheduleViewModel> GenerateLoanSchedule(LoanPaymentScheduleInput input);

        List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);
    }
}