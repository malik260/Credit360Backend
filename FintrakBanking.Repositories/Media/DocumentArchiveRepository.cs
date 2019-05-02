using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.ViewModels.Media;
using FintrakBanking.Entities.DocumentModels;

namespace FintrakBanking.Repositories.Media
{
    public class DocumentArchiveRepository : IDocumentArchiveRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;

        public DocumentArchiveRepository(
                FinTrakBankingDocumentsContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
        }

        public DocumentArchiveViewModel GetDocumentsByCustomerCode(string code)
        {
            /*
desc TBL_LOAN_CONDITION_DOCUMENTS;
desc TBL_MEDIA_CHECKLIST_DOCUMENTS;
desc TBL_MEDIA_KYC_DOCUMENTS;
desc TBL_TEMP_MEDIA_COLLATERAL_DOCS;
desc TBL_CUSTOMER_CREDIT_BUREAU;
desc TBL_DOC_COLLATERAL_VISITATION;
desc TBL_LOAN_COMMITTEE_MINUTES;
desc TBL_MEDIA_COLLATERAL_DOCUMENTS;
desc TBL_MEDIA_JOB_REQUEST_DOCUMENT;
desc TBL_MEDIA_LOAN_DOCUMENTS;
desc TBL_MEDIA_STAFF_SIGNATURE;
desc TBL_TEMP_MEDIA_LOAN_DOCUMENTS;
desc TBL_LOAN_CONTINGENT_USAGE_DOCS;
desc TBL_DOC_COLLATERAL_RELEASE;
desc TBL_JOB_REQUEST_MSG_DOCUMENT;
desc TBL_MEDIA_COMPANY;
desc TBL_MEDIA_LOAN_MATURITY_INSTR;
desc TBL_MEDIA_STAFF_PICTURE;
            return context.TBL_LOAN_CONDITION_DOCUMENTS.Where(x => x. == false)
                .Select(x => new DocumentArchiveViewModel
                {
                    documentArchiveId = x.DOCUMENTARCHIVEID,
                    documentId = x.DOCUMENTID,
                    customerCode = x.CUSTOMERCODE,
                    targetId = x.TARGETID,
                    targetCode = x.TARGETCODE,
                    targetReferenceNumber = x.TARGETREFERENCENUMBER,
                    documentCode = x.DOCUMENTCODE,
                    documentTitle = x.DOCUMENTTITLE,
                    physicalFileNumber = x.PHYSICALFILENUMBER,
                    physicalLocation = x.PHYSICALLOCATION,
                    expiryDate = x.EXPIRYDATE,
                    documentStatusId = x.DOCUMENTSTATUSID,
                    isPrimaryDocument = x.ISPRIMARYDOCUMENT,
                    documentCategoryId = x.DOCUMENTCATEGORYID,
                    documentTypeId = x.DOCUMENTTYPEID,
                    fileName = x.FILENAME,
                    fileExtension = x.FILEEXTENSION,
                    fileSize = x.FILESIZE,
                    fileSizeUnit = x.FILESIZEUNIT,
                    fileData = x.FILEDATA,
                    companyId = x.COMPANYID,
                    approvalStatusId = x.APPROVALSTATUSID,
                })
                .ToList();*/
            throw new NotImplementedException();
        }

        public DocumentArchiveViewModel GetDocumentsByCustomerCodeAndCategory(string code, int categoryId)
        {
            throw new NotImplementedException();
        }
    }
}

           // kernel.Bind<IDocumentArchiveRepository>().To<DocumentArchiveRepository>();
           // DocumentArchiveAdded = ???, DocumentArchiveUpdated = ???, DocumentArchiveDeleted = ???,
