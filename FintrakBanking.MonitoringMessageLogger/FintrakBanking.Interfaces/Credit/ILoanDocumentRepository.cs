using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanDocumentRepository
    {
        LoanDocumentViewModel GetLoanDocument(int documentId);

        IEnumerable<LoanDocumentViewModel> GetAllLoanDocument();

        int AddLoanDocument(LoanDocumentViewModel model, byte[] file);

        bool UpdateLoanDocument(LoanDocumentViewModel model, int documentId);

        bool UpdateLoanDocument(LoanDocumentViewModel model, int documentId, byte[] file);

        bool DeleteLoanDocument(string invoiceNo, string applicationNumber);

        IEnumerable<LoanDocumentViewModel> GetLoanDocumentByReferenceNumber(string referenceNumber);
        //IEnumerable<OperationDocumentationViewModel> GetAllPendingOperationDocumentation();
        //IEnumerable<ChecklistApprovalViewModel> GetAllPendingDeferralDocumentation(bool checker);
        //IEnumerable<OperationDocumentationViewModel> GetAllPendingOperationDocumentationApproval();
        //bool AddOperationDocumentationApproval(OperationDocumentationViewModel data);

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
        bool AddMediaCollateralDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaCollateralDocument(LoanDocumentViewModel model, byte[] file);
        bool AddMediaJobRequestDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaJobRequestDocument(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaJobRequestDocument(LoanDocumentViewModel model);
        bool AddMediaKYCDocument(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaKYCDocument(LoanDocumentViewModel model);
        bool AddMediaStaffPicture(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaStaffPicture(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaStaffPicture(LoanDocumentViewModel model);
        bool AddMediaStaffSignature(LoanDocumentViewModel model, byte[] file);
        bool UpdateMediaStaffSignature(LoanDocumentViewModel model);
        LoanDocumentViewModel GetMediaStaffSignature(LoanDocumentViewModel model);
        int uploadDocument(LoanDocumentViewModel model, byte[] file);

        LoanDocumentViewModel getUploadedDocument(LoanDocumentViewModel model);

        List<LoanDocumentViewModel> getListOfUploadedDocument(LoanDocumentViewModel model);
        List<LoanDocumentViewModel> getListOfUploadedOperationsDocument(LoanDocumentViewModel model, int operationReviewId);


        void GetApplicationLoanDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        IEnumerable<LoanDocumentViewModel> GetApplicationLoanDocument(string applicationNumber);

        void GetCommitteeDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetCreditBureauReportDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetConditionDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetMediaCheckListDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetMediaCollateralDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetMediaJobRequestDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetMediaKYCDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetMediaStaffSignature(LoanDocumentViewModel model, out LoanDocumentViewModel result);

        void GetMediaStaffPicture(LoanDocumentViewModel model, out LoanDocumentViewModel result);


        void GetAllLoanDocument(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetCommitteeDocument(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetCreditBureauReportDocument(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetConditionDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetMediaCheckListDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetMediaCollateralDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetMediaJobRequestDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void GetMediaKYCDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result);

        void DeleteApplicationLoanDocument(LoanDocumentViewModel model, out int result);

        void DeleteCommitteeDocument(LoanDocumentViewModel model, out int result);

        void DeleteCreditBureauReportDocument(LoanDocumentViewModel model, out int result);

        void DeleteConditionDocument(LoanDocumentViewModel model, out int result);

        void DeleteMediaCheckListDocument(LoanDocumentViewModel model, out int result);

        void DeleteMediaCollateralDocument(LoanDocumentViewModel model, out int result);

        void DeleteMediaJobRequestDocument(LoanDocumentViewModel model, out int result);

        void DeleteMediaKYCDocument(LoanDocumentViewModel model, out int result);

        void DeleteMediaStaffPicture(LoanDocumentViewModel model, out int result);

        void DeleteMediaStaffSignature(LoanDocumentViewModel model, out int result);

        int DeleteUploadedDocument(LoanDocumentViewModel model);

        




    }
}
