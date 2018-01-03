
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
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_LOAN_DOCUMENTS
                {
                    FILEDATA = file,
                    LOANAPPLICATIONNUMBER = model.loanApplicationNumber,
                    LOANREFERENCENUMBER = model.loanReferenceNumber,
                    DOCUMENTTITLE = model.documentTitle,
                    DOCUMENTTYPEID = model.documentTypeId,
                    LOAN_BOOKING_REQUESTID = model.SourceId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                };

                context.TBL_MEDIA_LOAN_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Loan Document '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public bool UpdateLoanDocument(LoanDocumentViewModel model, int documentId)
        {
            var data = this.context.TBL_MEDIA_LOAN_DOCUMENTS.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.LOANAPPLICATIONNUMBER = model.loanApplicationNumber;
            data.LOANREFERENCENUMBER = model.loanReferenceNumber;
            data.DOCUMENTTITLE = model.documentTitle;
            data.DOCUMENTTYPEID = model.documentTypeId;
            //data
            data.FILENAME = model.fileName;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;
            data.PHYSICALFILENUMBER = model.physicalFileNumber;
            data.PHYSICALLOCATION = model.physicalLocation;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated LoanDocument '{ model.documentTitle }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanDocumentViewModel> GetAllLoanDocument()
        {
            return this.context.TBL_MEDIA_LOAN_DOCUMENTS.Select(x => new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
               // fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
            });
        }

        public LoanDocumentViewModel GetLoanDocument(int documentId)
        {
            var data = this.context.TBL_MEDIA_LOAN_DOCUMENTS.Find(documentId);

            if (data == null)
            {
                return null;
            }

            return new LoanDocumentViewModel
            {
                documentId = data.DOCUMENTID,
                loanApplicationNumber = data.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = data.LOANREFERENCENUMBER,
                documentTitle = data.DOCUMENTTITLE,
                documentTypeId = data.DOCUMENTTYPEID,
                fileData = data.FILEDATA,
                fileName = data.FILENAME,
                fileExtension = data.FILEEXTENSION,
                systemDateTime = data.SYSTEMDATETIME,
                physicalFileNumber = data.PHYSICALFILENUMBER,
                physicalLocation = data.PHYSICALLOCATION,
            };
        }

        public IEnumerable<LoanDocumentViewModel> GetApplicationLoanDocument(string applicationNumber)
        {
            return this.GetAllLoanDocument().Where(x => x.loanApplicationNumber == applicationNumber);
        }
        public LoanDocumentViewModel GetLoanDocumentByAppNoRefNo(string refNo, string applicationNumber)
        {
            var media =  this.context.TBL_MEDIA_LOAN_DOCUMENTS.
                Where(h => h.LOANAPPLICATIONNUMBER== applicationNumber && h.LOANREFERENCENUMBER == refNo).
                Select(x => new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
               // fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
            }).FirstOrDefault();
            return media;
        }
        public IEnumerable<LoanDocumentViewModel> GetLoanDocumentByReferenceNumber(string referenceNumber)
        {
            return this.GetAllLoanDocument().Where(x =>
                string.Equals(x.loanReferenceNumber.ToLower(), referenceNumber.ToLower(), StringComparison.Ordinal));
        }

        public bool DeleteLoanDocument(string invoiceNo, string applicationNumber)
        {
            var data = (from a in context.TBL_MEDIA_LOAN_DOCUMENTS where a.LOANREFERENCENUMBER == invoiceNo
                        && a.LOANAPPLICATIONNUMBER == applicationNumber select a).FirstOrDefault();
            if (data != null)
            {
                this.context.TBL_MEDIA_LOAN_DOCUMENTS.Remove(data);
                return context.SaveChanges() != 0;
            }
            return false;
        }
    }
}
