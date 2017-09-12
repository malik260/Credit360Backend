
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
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

        IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId);


        Task<string> AddLoanBooking(LoanViewModel entity);

        IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId);

        LoanViewModel GetLoan(int loanId);
        
        IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId);

        IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel);


        IEnumerable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId);

        //IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByProductId(int productId);

        IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId);
        IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId);
        IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId );

        IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId);

        IEnumerable<LoanViewModel> GetLoanBookingAwaitingApproval(int staffId, int companyId);
        Task<bool> GoForApproval(ApprovalViewModel entity);
    }
}