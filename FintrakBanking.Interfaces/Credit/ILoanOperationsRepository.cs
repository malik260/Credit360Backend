using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using FintrakBanking.ViewModels.Operations;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanOperationsRepository
    {
        bool AddCollateralSearchLien(CasaLienViewModel model);
        decimal GetCollateralSearchChargeAmount(int stateId);
        bool AddOperationReview(LoanReviewOperationViewModel model);
        IEnumerable<LoanOperationTypeViewModel> GetOperationType();
        IEnumerable<DailyInterestAccrualViewModel> GetDailyTeamLoansInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyPastDueInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyPastDuePrincipalAccrual(DateTime applicationDate);


    }
}