using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
   public interface IFacilityDetailSummary
   {
        LoanViewModel FacilityDetail(int loanId);
        List<LoanCovenantDetailViewModel> LoanCovenantDetail(int loanId);
        List<LoanChargeFeeViewModel> LoanChargeFee(int loanId);
        List<LoanChargeFeeViewModel> GuarantorDetail(int loanId);
        List<LoanPaymentSchedulePeriodicViewModel> LoanSchedule(int loanId);
        List<CollateralViewModel> Collateral(int loanId);
        List<LoanViewModel> LoanSearch(int productTypeId, string searchQuery);
        List<ProductType> ProductType();
        LoanViewModel OverdraftFacilityDetail(int loanId);
        LoanViewModel ContingentFacilityDetail(int loanId);
        LoanViewModel OverdraftFacilityDetailArchive(int archiveId);
        LoanViewModel FacilityDetailArchive(int loanId);
        List<LoanViewModel> ArchiveLoanFacilityDetail(int archiveId);
        List<LoanViewModel> ArchiveRevolvingLoanFacilityDetail(int archiveId);
        List<LoanViewModel> SearchAllRevolvingLoan(int loanId);
        List<LoanPaymentSchedulePeriodicViewModel> ArchivedLoanSchedule(LoanPaymentSchedulePeriodicViewModel data);
    }
}
