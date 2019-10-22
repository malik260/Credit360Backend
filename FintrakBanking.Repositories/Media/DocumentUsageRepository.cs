using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Media
{
    public class DocumentUsageRepository : IDocumentUsageRepository
    {
        FinTrakBankingDocumentsContext docContext;
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public DocumentUsageRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow,
                FinTrakBankingDocumentsContext _docContext
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
            this.docContext = _docContext;
        }

        public IEnumerable<DocumentUsageViewModel> GetDocumentUsages()
        {
            return docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false)
                .Select(x => new DocumentUsageViewModel
                {
                    documentUsageId = x.DOCUMENTUSAGEID,
                    documentUploadId = x.DOCUMENTUPLOADID,
                    targetId = x.TARGETID,
                    targetCode = x.TARGETCODE,
                    targetReferenceNumber = x.TARGETREFERENCENUMBER,
                    documentCode = x.DOCUMENTCODE,
                    documentTitle = x.DOCUMENTTITLE,
                    customerCode = x.CUSTOMERCODE,
                    //documentCategory = x.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                    approvalStatusId = x.APPROVALSTATUSID,
                    documentStatusId = x.DOCUMENTSTATUSID,
                    isPrimaryDocument = x.ISPRIMARYDOCUMENT,
                })
                .ToList();
        }

        public IEnumerable<DocumentUsageViewModel> SearchDocumentUsage(string parameter)
        {
            var usageRecord = (from usage in docContext.TBL_DOCUMENT_USAGE
                               join cus in context.TBL_CUSTOMER on usage.CUSTOMERCODE equals cus.CUSTOMERCODE
                               where parameter.Contains(cus.CUSTOMERCODE)
                               select new DocumentUsageViewModel
                               {
                                   //documentCategory = usage.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                                   isPrimaryDocument = usage.ISPRIMARYDOCUMENT,
                                   documentCode = usage.DOCUMENTCODE,
                                   dateTimeCreated = usage.DATETIMECREATED,
                                   documentUploadId = usage.DOCUMENTUPLOADID,

                               }).ToList();

            foreach (var x in usageRecord)
            {
                var file = docContext.TBL_DOCUMENT_UPLOAD.FirstOrDefault(o => o.DOCUMENTUPLOADID == x.documentUploadId);
                if (file != null)
                {
                    x.documentTitle = docContext.TBL_DOCUMENT_TYPE.FirstOrDefault(o => o.DOCUMENTTYPEID == file.DOCUMENTTYPEID).DOCUMENTTYPENAME;
                    x.fileName = file.FILENAME;
                    x.fileType = file.FILEEXTENSION;
                }

            }

            return usageRecord;
        }

        public DocumentUsageViewModel GetDocumentUsage(int id)
        {
            var entity = docContext.TBL_DOCUMENT_USAGE.FirstOrDefault(x => x.DOCUMENTUSAGEID == id && x.DELETED == false);

            return new DocumentUsageViewModel
            {
                documentUsageId = entity.DOCUMENTUSAGEID,
                documentUploadId = entity.DOCUMENTUPLOADID,
                targetId = entity.TARGETID,
                targetCode = entity.TARGETCODE,
                targetReferenceNumber = entity.TARGETREFERENCENUMBER,
                documentCode = entity.DOCUMENTCODE,
                documentTitle = entity.DOCUMENTTITLE,
                customerCode = entity.CUSTOMERCODE,
                documentCategoryId = entity.OPERATIONID,
                approvalStatusId = entity.APPROVALSTATUSID,
                documentStatusId = entity.DOCUMENTSTATUSID,
                isPrimaryDocument = entity.ISPRIMARYDOCUMENT,
            };
        }

        public bool AddDocumentUsage(DocumentUsageViewModel model)
        {
            var entity = new TBL_DOCUMENT_USAGE
            {
                DOCUMENTUPLOADID = model.documentUploadId,
                TARGETID = model.targetId,
                TARGETCODE = model.targetCode,
                TARGETREFERENCENUMBER = model.targetReferenceNumber,
                DOCUMENTCODE = model.documentCode,
                DOCUMENTTITLE = model.documentTitle,
                CUSTOMERCODE = model.customerCode,
                OPERATIONID = model.documentCategoryId,
                APPROVALSTATUSID = model.approvalStatusId,
                DOCUMENTSTATUSID = model.documentStatusId,
                ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            docContext.TBL_DOCUMENT_USAGE.Add(entity);

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentUsageAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Document Usage '{model.targetCode}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateDocumentUsage(DocumentUsageViewModel model, int id, UserInfo user)
        {
            var entity = this.docContext.TBL_DOCUMENT_USAGE.Find(id);
            entity.DOCUMENTUPLOADID = model.documentUploadId;
            entity.TARGETID = model.targetId;
            entity.TARGETCODE = model.targetCode;
            entity.TARGETREFERENCENUMBER = model.targetReferenceNumber;
            entity.DOCUMENTCODE = model.documentCode;
            entity.DOCUMENTTITLE = model.documentTitle;
            entity.CUSTOMERCODE = model.customerCode;
            entity.OPERATIONID = model.documentCategoryId;
            entity.APPROVALSTATUSID = model.approvalStatusId;
            entity.DOCUMENTSTATUSID = model.documentStatusId;
            entity.ISPRIMARYDOCUMENT = model.isPrimaryDocument;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentUsageUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Usage '{model.targetCode}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTUSAGEID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentUsage(int id, UserInfo user)
        {
            var entity = this.docContext.TBL_DOCUMENT_USAGE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentUsageDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Document Usage '{entity.TARGETCODE}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.DOCUMENTUSAGEID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


    }
}

// kernel.Bind<IDocumentUsageRepository>().To<DocumentUsageRepository>();
// DocumentUsageAdded = ???, DocumentUsageUpdated = ???, DocumentUsageDeleted = ???,
