using FintrakBanking.Common.Enum;
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

        DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId);

        IEnumerable<LookupViewModel> GetAllLoanScheduleCategory();

        IEnumerable<LookupViewModel> GetAllLoanScheduleType();

        IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId);

        IEnumerable<LookupViewModel> GetAllLoanScheduleType(short? productTypeId);

        List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput);

        bool AddLoanSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, int staffId);
    }
}
