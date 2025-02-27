using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.External.Customer;
using FintrakBanking.ViewModels.External.Document;
using FintrakBanking.ViewModels.External.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.External
{
    public interface ILoanRepositoryExternal
    {
        List<AffordabilityViewModel> AffordabilityChecks(AffordabilityViewModel model);
        List<LoanPaymentScheduleVM> LoanSchedule(string applicationRefNo);
        List<ScheduleLoans> GetNHFLoans(string nhfNo);
        LoanApplicationForReturn AddLoanApplication(LoanApplicationForCreation loan);
        LoanApplicationForReturn AddBatchedLoanApplication(LoanApplicationForCreation loan);
        Task<List<LoanApplicationForReturn>> GetLoanApplicationByCustomer(string customerCode);
        Task<LoanApplicationForReturn> GetLoanApplicationByRefNo(string applicationRefNo);
        Task<string> DownloadOfferLetter(string applicationRefNo);
        //Task<LoanEligibilityForReturn> LoanEligibility(LoanEligibilityForInquiry forInquiry);
        Task<bool> LaonDocumentUpload(LoanDocumentViewModel model, byte[] file);
        Task<bool> LoanDocumentUpload(List<LoanDocumentViewModel> model);
        Task<List<LoanUploadedDocumentForReturn>> GetLoanDocumentUploadByRefNo(string loanReferenceNumber);
        Task<bool> CheckOutstandingLoan(string nhfNo);
        List<LoanVM> GetDisbursedLoans(int companyId);
        RefinanceViewModel RefinanceLoan(RefinanceViewModel Model);
        Task<List<TblRefinancingLoan>> GetLoanForRefinance1(long CompanyId);
        Task<List<StNmrcEligibility>> GetUUSForObligor();
        List<CustomerUusViewModel> PostCustomersUItems(List<CustomerUusViewModel> Model);
        Task<string> GetCustomerUusItemDoc(string NhfNumber, int ItemId);
        Task<List<TblCustomerUUS>> GetCustomerUusItems(string NhfNumber);
        List<TblRefinancingLoan> ApprovePmbRefinancing(List<int> Model);
        Task<List<TblRefinancingLoan>> GetPmbsChecklistedLoan(long CompanyId);
        Task<List<TblNmrcRefinancing>> GetAppliedLoanForNmrcRefinance();
        Task<List<TblNmrcRefinancingLoan>> GetAppliedSubLoanForNmrcRefinance(string RefNo);
        Task<List<TblNmrcRefinancingLoan>> GetSubLoanForNmrcReview();
        List<UUSReviewalItem> ReviewCustomersUItems(List<UUSReviewalItem> Model);
        List<TblNmrcRefinancingLoan> ReviewalApproval(List<int> Model);
        List<TblNmrcRefinancingLoan> ReviewalDisApproval(List<int> Model);
        Task<List<TblNmrcRefinancingLoan>> GetReviewedForApproval();
        List<TblNmrcRefinancingLoan> ApproveReviewedLoan(List<int> Model);

        string TranchApprovedLoans(List<string> RefNo);
        Task<List<TblNmrcRefinancingLoan>> GetSubLoanForDisbursement(string RefNo);
        Task<List<TblNmrcRefinancingTranches>> GetTranchedLoans();
        Task<List<TblNmrcRefinancingTranches>> GetScheduledLoanForBooking();
        Task<List<TblNmrcRefinancingTranches>> GetScheduledLoanForDisbursement();
<<<<<<< HEAD
        string BookLoanNmrc(int Model);
        string DisburseLoanNmrc(int Model);
=======
        Task<string> BookLoanNmrc(int Model);
        Task<string> DisburseLoanNmrc(int Model);
        Task<List<TBL_NMRC_LOAN_SCHEDULE_PERIODIC>> GetLoanPaymentSchedule(int LoanId);
>>>>>>> 7eb3593623ee10ae28fb454aecaa7deb209bf7d1

    }
}
