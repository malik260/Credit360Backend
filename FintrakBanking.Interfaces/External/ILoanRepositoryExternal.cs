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
    }
}
