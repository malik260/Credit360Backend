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

        bool DeleteLoanDocument(string invoiceNo, string applicationNumber);

        IEnumerable<LoanDocumentViewModel> GetLoanDocumentByReferenceNumber(string referenceNumber);

        LoanDocumentViewModel GetLoanDocumentByAppNoRefNo(string refNo, string applicationNumber);

        bool AddCommitteeDocument(LoanDocumentViewModel entity, byte[] buffer);

        IEnumerable<LoanDocumentViewModel> GetCommitteeDocument(string applicationNumber);

        LoanDocumentViewModel GetCommitteeDocument(int documentId);

        #region CREDIT BUREAU REPORT DOCUMENTS
        List<LoanDocumentViewModel> GetCreditBureauReportDocument(int customerCreditBureauId);

        LoanDocumentViewModel GetCreditBureauReportDocumentByDocumentID(int customerCreditBureauId, int documentId);

        bool AddCreditBureauReportDocument(LoanDocumentViewModel model, byte[] file);

        bool UpdateCreditBureauReportDocument(LoanDocumentViewModel model, int documentId);

#endregion

        bool AddMediaCheckListDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaCheckListDocument(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaCheckListDocument(LoanDocumentViewModel model);
        List<LoanDocumentViewModel> GetMediaCheckListDocuments(LoanDocumentViewModel model);
        bool AddMediaCollateralDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaCollateralDocument(LoanDocumentViewModel model, byte[] file);
        LoanDocumentViewModel GetMediaCollateralDocument(LoanDocumentViewModel model);
        List<LoanDocumentViewModel> GetMediaCollateralDocuments(LoanDocumentViewModel model);
        bool AddMediaJobRequestDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaJobRequestDocument(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaJobRequestDocument(LoanDocumentViewModel model);
        List<LoanDocumentViewModel> GetMediaJobRequestDocuments(LoanDocumentViewModel model);
        bool AddMediaKYCDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaKYCDocument(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaKYCDocument(LoanDocumentViewModel model);
        List<LoanDocumentViewModel> GetMediaKYCDocuments(LoanDocumentViewModel model);
        bool AddMediaStaffPicture(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaStaffPicture(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaStaffPicture(LoanDocumentViewModel model);
        bool AddMediaStaffSignature(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaStaffSignature(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaStaffSignature(LoanDocumentViewModel model);
        bool uploadDocument(LoanDocumentViewModel model, byte[] file);
        bool getUploadedDocument(LoanDocumentViewModel model);
        bool getListOfUploadedDocument(LoanDocumentViewModel model);


  
    }
}
