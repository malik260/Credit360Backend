
using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanDocumentRepository : ILoanDocumentRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public LoanDocumentRepository(FinTrakBankingDocumentsContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddLoanDocument(LoanDocumentViewModel model, byte[] file)
        {
            var data = new tbl_Media_Loan_Documents
            {
                FileData = file,
                LoanApplicationNumber = model.loanApplicationNumber,
                LoanReferenceNumber = model.loanReferenceNumber,
                DocumentTitle = model.documentTitle,
                DocumentTypeId = model.documentTypeId,
                FileName = model.fileName,
                FileExtension = model.fileExtension,
                SystemDateTime = DateTime.Now,
                PhysicalFileNumber = model.physicalFileNumber,
                PhysicalLocation = model.physicalLocation,
                CreatedBy = (int)model.createdBy,
            };

            context.tbl_Media_Loan_Documents.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDocumentAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Loan Document '{ model.documentTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLoanDocument(LoanDocumentViewModel model, int documentId)
        {
            var data = this.context.tbl_Media_Loan_Documents.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.LoanApplicationNumber = model.loanApplicationNumber;
            data.LoanReferenceNumber = model.loanReferenceNumber;
            data.DocumentTitle = model.documentTitle;
            data.DocumentTypeId = model.documentTypeId;
            data.FileName = model.fileName;
            data.FileExtension = model.fileExtension;
            data.SystemDateTime = DateTime.Now;
            data.PhysicalFileNumber = model.physicalFileNumber;
            data.PhysicalLocation = model.physicalLocation;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDocumentUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated LoanDocument '{ model.documentTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanDocumentViewModel> GetAllLoanDocument()
        {
            return this.context.tbl_Media_Loan_Documents.Select(x => new LoanDocumentViewModel
            {
                documentId = x.DocumentId,
                loanApplicationNumber = x.LoanApplicationNumber,
                loanReferenceNumber = x.LoanReferenceNumber,
                documentTitle = x.DocumentTitle,
                documentTypeId = x.DocumentTypeId,
                fileData = x.FileData,
                fileName = x.FileName,
                fileExtension = x.FileExtension,
                systemDateTime = x.SystemDateTime,
                physicalFileNumber = x.PhysicalFileNumber,
                physicalLocation = x.PhysicalLocation,
            });
        }

        public LoanDocumentViewModel GetLoanDocument(int documentId)
        {
            var data = this.context.tbl_Media_Loan_Documents.Find(documentId);

            if (data == null)
            {
                return null;
            }

            return new LoanDocumentViewModel
            {
                documentId = data.DocumentId,
                loanApplicationNumber = data.LoanApplicationNumber,
                loanReferenceNumber = data.LoanReferenceNumber,
                documentTitle = data.DocumentTitle,
                documentTypeId = data.DocumentTypeId,
                fileData = data.FileData,
                fileName = data.FileName,
                fileExtension = data.FileExtension,
                systemDateTime = data.SystemDateTime,
                physicalFileNumber = data.PhysicalFileNumber,
                physicalLocation = data.PhysicalLocation,
            };
        }

        public IEnumerable<LoanDocumentViewModel> GetApplicationLoanDocument(string applicationNumber)
        {
            return this.GetAllLoanDocument().Where(x => x.loanApplicationNumber == applicationNumber);
        }

        public IEnumerable<LoanDocumentViewModel> GetLoanDocumentByReferenceNumber(string referenceNumber)
        {
            return this.GetAllLoanDocument().Where(x => x.loanReferenceNumber == referenceNumber);
        }
    }
}
