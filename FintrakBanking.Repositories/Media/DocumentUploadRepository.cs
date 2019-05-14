using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Media
{
    public class DocumentUploadRepository : IDocumentUploadRepository
    {
        private FinTrakBankingDocumentsContext docContext; 
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public DocumentUploadRepository(
                FinTrakBankingDocumentsContext _docContext,
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.docContext = _docContext;
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }

        public IEnumerable<DocumentUploadViewModel> GetDocumentUploads()
        {
            return docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED==false)
                .Select(x => new DocumentUploadViewModel
                {
                    documentUploadId = x.DOCUMENTUPLOADID,
                    fileName = x.FILENAME,
                    fileExtension = x.FILEEXTENSION,
                    fileSize = x.FILESIZE,
                    fileSizeUnit = x.FILESIZEUNIT,
                    fileData = x.FILEDATA,
                    companyId = x.COMPANYID,
                    issueDate = x.ISSUEDATE,
                    expiryDate = x.EXPIRYDATE,
                    physicalFilenumber = x.PHYSICALFILENUMBER,
                    physicalLocation = x.PHYSICALLOCATION,
                })
                .ToList();
        }

        public DocumentUploadViewModel GetDocumentUpload(int id)
        {
            var entity = docContext.TBL_DOCUMENT_UPLOAD.FirstOrDefault(x => x.DOCUMENTUPLOADID == id && x.DELETED == false);

            return new DocumentUploadViewModel
            {
                documentUploadId = entity.DOCUMENTUPLOADID,
                fileName = entity.FILENAME,
                fileExtension = entity.FILEEXTENSION,
                fileSize = entity.FILESIZE,
                fileSizeUnit = entity.FILESIZEUNIT,
                fileData = entity.FILEDATA,
                companyId = entity.COMPANYID,
                issueDate = entity.ISSUEDATE,
                expiryDate = entity.EXPIRYDATE,
                physicalFilenumber = entity.PHYSICALFILENUMBER,
                physicalLocation = entity.PHYSICALLOCATION,
            };
        }

        public bool AddDocumentUpload(DocumentUploadViewModel model, byte[] buffer)
        {
            var entity = new TBL_DOCUMENT_UPLOAD
            {
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                FILESIZE = model.fileSize,
                FILESIZEUNIT = model.fileSizeUnit,
                FILEDATA = buffer,
                COMPANYID = model.companyId,
                ISSUEDATE = model.issueDate,
                EXPIRYDATE = model.expiryDate,
                PHYSICALFILENUMBER = model.physicalFilenumber,
                PHYSICALLOCATION = model.physicalLocation,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            docContext.TBL_DOCUMENT_UPLOAD.Add(entity);

            if (docContext.SaveChanges() > 0)
            {
                var usage = new TBL_DOCUMENT_USAGE
                {
                    DOCUMENTUPLOADID = entity.DOCUMENTUPLOADID,
                    TARGETID = model.targetId,
                    TARGETCODE = model.targetCode,
                    TARGETREFERENCENUMBER = model.targetReferenceNumber,
                    DOCUMENTCODE = model.documentCode,
                    DOCUMENTTITLE = model.documentTitle,
                    CUSTOMERCODE = model.customerCode,
                    DOCUMENTCATEGORYID = model.documentCategoryId,
                    DOCUMENTTYPEID = model.documentTypeId,
                    APPROVALSTATUSID = model.approvalStatusId,
                    DOCUMENTSTATUSID = model.documentStatusId,
                    ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                    // COMPANYID = model.companyId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = general.GetApplicationDate(),
                };

                context.TBL_DOCUMENT_USAGE.Add(usage);

                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                //this.audit.AddAuditTrail(new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.DocumentUploadAdded,
                //    STAFFID = model.createdBy,
                //    BRANCHID = (short)model.userBranchId,
                //    DETAIL = $"TBL_Document Upload '{model.targetCode}' created by {auditStaff}",
                //    IPADDRESS = model.userIPAddress,
                //    URL = model.applicationUrl,
                //    APPLICATIONDATE = general.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now
                //});

            }
            
            if (context.SaveChanges()<1)
            {
              var file =  docContext.TBL_DOCUMENT_UPLOAD.Where(o => o.DOCUMENTUPLOADID == entity.DOCUMENTUPLOADID).Select(o => o).FirstOrDefault();
                if (file != null) { docContext.TBL_DOCUMENT_UPLOAD.Remove(file);
                    docContext.SaveChanges();
                }

                return false;
            }
            return true;
        }

        public bool UpdateDocumentUpload(DocumentUploadViewModel model, int id, UserInfo user)
        {
            var entity = this.docContext.TBL_DOCUMENT_UPLOAD.Find(id);
            entity.FILENAME = model.fileName;
            entity.FILEEXTENSION = model.fileExtension;
            entity.FILESIZE = model.fileSize;
            entity.FILESIZEUNIT = model.fileSizeUnit;
            entity.FILEDATA = model.fileData;
            entity.COMPANYID = model.companyId;
            entity.ISSUEDATE = model.issueDate;
            entity.EXPIRYDATE = model.expiryDate;
            entity.PHYSICALFILENUMBER = model.physicalFilenumber;
            entity.PHYSICALLOCATION = model.physicalLocation;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentUploadUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Document Upload '{model.targetCode}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.DOCUMENTUPLOADID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentUpload(int id, UserInfo user)
        {
            var usageCount = 0;

            var usage = context.TBL_DOCUMENT_USAGE.FirstOrDefault(u => u.DOCUMENTUSAGEID == id);
            if (usage != null)
            {// throw new SecureException("An error occured! Cannot find target item to delete.");

                 usageCount = context.TBL_DOCUMENT_USAGE
                    .Where(u => u.DOCUMENTUPLOADID == usage.DOCUMENTUPLOADID)
                    .Count();
           
            usage.DELETED = true;
            usage.DELETEDBY = user.createdBy;
            usage.DATETIMEDELETED = general.GetApplicationDate();
            }
            if (usageCount < 2)
            {
                var upload = docContext.TBL_DOCUMENT_UPLOAD.Find(id);
                upload.DELETED = true;
                upload.DELETEDBY = user.createdBy;
                upload.DATETIMEDELETED = general.GetApplicationDate();

                docContext.SaveChanges();
            }

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentUploadDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Upload '{entity.DOCUMENTTYPEID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTUPLOADID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

       
    }
}

           // kernel.Bind<IDocumentUploadRepository>().To<DocumentUploadRepository>();
           // DocumentUploadAdded = ???, DocumentUploadUpdated = ???, DocumentUploadDeleted = ???,
