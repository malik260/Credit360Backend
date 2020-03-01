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
    public class DocumentCategoryRepository : IDocumentCategoryRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public DocumentCategoryRepository(
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

        public IEnumerable<DocumentCategoryViewModel> GetDocumentCategorys()
        {
            var docCategories = context.TBL_DOCUMENT_CATEGORY.Where(x => x.DELETED == false)
                .Select(x => new DocumentCategoryViewModel
                {
                    documentCategoryId = (int)x.DOCUMENTCATEGORYID,
                    documentCategoryName = x.DOCUMENTCATEGORYNAME,
                }).ToArray(); 

            return docCategories?.OrderBy(l => l.documentCategoryName); ;
        }

        public DocumentCategoryViewModel GetDocumentCategory(int id)
        {
            var entity = context.TBL_DOCUMENT_CATEGORY.FirstOrDefault(x => x.DOCUMENTCATEGORYID == id && x.DELETED == false);

            return new DocumentCategoryViewModel
            {
                documentCategoryId = entity.DOCUMENTCATEGORYID,
                documentCategoryName = entity.DOCUMENTCATEGORYNAME,
            };
        }

        public bool AddDocumentCategory(DocumentCategoryViewModel model)
        {
            var entity = new TBL_DOCUMENT_CATEGORY
            {
                DOCUMENTCATEGORYNAME = model.documentCategoryName,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_DOCUMENT_CATEGORY.Add(entity);

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentCategoryAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Document Category '{entity.DESCRIPTION}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateDocumentCategory(DocumentCategoryViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_DOCUMENT_CATEGORY.Find(id);
            entity.DOCUMENTCATEGORYNAME = model.documentCategoryName;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentCategoryUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Category '{entity.DESCRIPTION}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTCATEGORYID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentCategory(int id, UserInfo user)
        {
            var entity = this.context.TBL_DOCUMENT_CATEGORY.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentCategoryDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Category '{entity.DESCRIPTION}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTCATEGORYID
            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

       
    }
}

           // kernel.Bind<IDocumentCategoryRepository>().To<DocumentCategoryRepository>();
           // DocumentCategoryAdded = ???, DocumentCategoryUpdated = ???, DocumentCategoryDeleted = ???,
