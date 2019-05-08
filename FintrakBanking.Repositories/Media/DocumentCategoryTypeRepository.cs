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
    public class DocumentCategoryTypeRepository : IDocumentCategoryTypeRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public DocumentCategoryTypeRepository(
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

        public IEnumerable<DocumentCategoryTypeViewModel> GetDocumentCategoryTypes()
        {
            return context.TBL_DOCUMENT_CATEGORY_TYPE.Where(x => x.DELETED == false)
                .Select(x => new DocumentCategoryTypeViewModel
                {
                    documentCategoryTypeId = x.DOCUMENTCATEGORYTYPEID,
                    documentTypeId = x.DOCUMENTTYPEID,
                    documentCategoryId = x.DOCUMENTCATEGORYID,
                })
                .ToList();
        }

        public DocumentCategoryTypeViewModel GetDocumentCategoryType(int id)
        {
            var entity = context.TBL_DOCUMENT_CATEGORY_TYPE.FirstOrDefault(x => x.DOCUMENTCATEGORYTYPEID == id && x.DELETED == false);

            return new DocumentCategoryTypeViewModel
            {
                documentCategoryTypeId = entity.DOCUMENTCATEGORYTYPEID,
                documentTypeId = entity.DOCUMENTTYPEID,
                documentCategoryId = entity.DOCUMENTCATEGORYID,
            };
        }

        public bool AddDocumentCategoryType(DocumentCategoryTypeViewModel model)
        {
            var entity = new TBL_DOCUMENT_CATEGORY_TYPE
            {
                DOCUMENTTYPEID = model.documentTypeId,
                DOCUMENTCATEGORYID = model.documentCategoryId,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_DOCUMENT_CATEGORY_TYPE.Add(entity);

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentCategoryTypeAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Document Category Type '{entity.DESCRIPTION}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateDocumentCategoryType(DocumentCategoryTypeViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_DOCUMENT_CATEGORY_TYPE.Find(id);
            entity.DOCUMENTTYPEID = model.documentTypeId;
            entity.DOCUMENTCATEGORYID = model.documentCategoryId;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentCategoryTypeUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Category Type '{entity.DESCRIPTION}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTCATEGORYTYPEID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentCategoryType(int id, UserInfo user)
        {
            var entity = this.context.TBL_DOCUMENT_CATEGORY_TYPE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentCategoryTypeDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Category Type '{entity.DESCRIPTION}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTCATEGORYTYPEID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<IDocumentCategoryTypeRepository>().To<DocumentCategoryTypeRepository>();
           // DocumentCategoryTypeAdded = ???, DocumentCategoryTypeUpdated = ???, DocumentCategoryTypeDeleted = ???,
