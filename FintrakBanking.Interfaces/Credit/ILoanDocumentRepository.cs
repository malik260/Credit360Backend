using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanDocumentRepository
    {
        LoanDocumentViewModel GetLoanDocument(int documentId);

        IEnumerable<LoanDocumentViewModel> GetAllLoanDocument();

        IEnumerable<LoanDocumentViewModel> GetApplicationLoanDocument(string applicationNumber);

        bool AddLoanDocument(LoanDocumentViewModel model, byte[] file);

        bool UpdateLoanDocument(LoanDocumentViewModel model, int documentId);

        IEnumerable<LoanDocumentViewModel> GetLoanDocumentByReferenceNumber(string referenceNumber);

    }
}
