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
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.Entities.DocumentModels;

namespace FintrakBanking.Repositories.Media
{
    public class DocumentTypeRepository : IDocumentTypeRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public DocumentTypeRepository(
                FinTrakBankingDocumentsContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }

        public IEnumerable<DocumentTypeViewModel> GetDocumentTypes()
        {
            return context.TBL_DOCUMENT_TYPE.Where(x => x.DELETED == false)
                .Select(x => new DocumentTypeViewModel
                {
                    documentTypeId = x.DOCUMENTTYPEID,
                    documentTypeName = x.DOCUMENTTYPENAME,
                    documentCategoryName = x.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                    documentCategoryId = x.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYID
                })
                .ToList();
        }

        public DocumentTypeViewModel GetDocumentType(int id)
        {
            var entity = context.TBL_DOCUMENT_TYPE.FirstOrDefault(x => x.DOCUMENTTYPEID == id && x.DELETED == false);

            return new DocumentTypeViewModel
            {
                documentTypeId = entity.DOCUMENTTYPEID,
                documentTypeName = entity.DOCUMENTTYPENAME,
            };
        }

        public bool AddDocumentType(DocumentTypeViewModel model)
        {
            var entity = new TBL_DOCUMENT_TYPE
            {
                DOCUMENTCATEGORYID = model.documentCategoryId,
                DOCUMENTTYPENAME = model.documentTypeName,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_DOCUMENT_TYPE.Add(entity);

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentTypeAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Document Type '{entity.DESCRIPTION}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateDocumentType(DocumentTypeViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_DOCUMENT_TYPE.Find(id);
            entity.DOCUMENTCATEGORYID = model.documentCategoryId;
            entity.DOCUMENTTYPEID = model.documentTypeId;
            entity.DOCUMENTTYPENAME = model.documentTypeName;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentTypeUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Type '{entity.DESCRIPTION}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTTYPEID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentType(int id, UserInfo user)
        {
            var entity = this.context.TBL_DOCUMENT_TYPE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentTypeDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Type '{entity.DESCRIPTION}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTTYPEID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<IDocumentTypeRepository>().To<DocumentTypeRepository>();
           // DocumentTypeAdded = ???, DocumentTypeUpdated = ???, DocumentTypeDeleted = ???,
