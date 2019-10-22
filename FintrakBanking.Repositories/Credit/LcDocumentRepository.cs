using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.credit
{
    public class LcDocumentRepository : ILcDocumentRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public LcDocumentRepository(
                FinTrakBankingContext _context,
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

        public IEnumerable<LcDocumentViewModel> GetLcDocuments()
        {
            return context.TBL_LC_DOCUMENT.Where(x => x.DELETED == false)
                .Select(x => new LcDocumentViewModel
                {
                    lcDocumentId = x.LCDOCUMENTID,
                    lcIssuanceId = x.LCISSUANCEID,
                    documentTitle = x.DOCUMENTTITLE,
                    isSentToIssuingBank = x.ISSENTTOISSUINGBANK,
                    numberOfCopies = x.NUMBEROFCOPIES,
                    isSentToApplicant = x.ISSENTTOAPPLICANT,
                })
                .ToList();
        }

        public LcDocumentViewModel GetLcDocument(int id)
        {
            var entity = context.TBL_LC_DOCUMENT.FirstOrDefault(x => x.LCDOCUMENTID == id && x.DELETED == false);

            return new LcDocumentViewModel
            {
                lcDocumentId = entity.LCDOCUMENTID,
                lcIssuanceId = entity.LCISSUANCEID,
                documentTitle = entity.DOCUMENTTITLE,
                isSentToIssuingBank = entity.ISSENTTOISSUINGBANK,
                numberOfCopies = entity.NUMBEROFCOPIES,
                isSentToApplicant = entity.ISSENTTOAPPLICANT,
            };
        }

        public IEnumerable<LcDocumentViewModel> GetLcDocumentsBylcIssuanceId(int id)
        {
            return context.TBL_LC_DOCUMENT.Where(x => x.LCISSUANCEID == id && x.DELETED == false)
                .Select(x => new LcDocumentViewModel
            {
                lcDocumentId = x.LCDOCUMENTID,
                lcIssuanceId = x.LCISSUANCEID,
                documentTitle = x.DOCUMENTTITLE,
                isSentToIssuingBank = x.ISSENTTOISSUINGBANK,
                numberOfCopies = x.NUMBEROFCOPIES,
                isSentToApplicant = x.ISSENTTOAPPLICANT,
            }).ToList();
        }

        public bool AddLcDocument(LcDocumentViewModel model)
        {
            var entity = new TBL_LC_DOCUMENT
            {
                LCISSUANCEID = model.lcIssuanceId,
                DOCUMENTTITLE = model.documentTitle,
                ISSENTTOISSUINGBANK = model.isSentToIssuingBank,
                NUMBEROFCOPIES = model.numberOfCopies,
                ISSENTTOAPPLICANT = model.isSentToApplicant,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_LC_DOCUMENT.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc Document '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                 URL =model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcDocument(LcDocumentViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_DOCUMENT.Find(id);
            entity.LCISSUANCEID = model.lcIssuanceId;
            entity.DOCUMENTTITLE = model.documentTitle;
            entity.ISSENTTOISSUINGBANK = model.isSentToIssuingBank;
            entity.NUMBEROFCOPIES = model.numberOfCopies;
            entity.ISSENTTOAPPLICANT = model.isSentToApplicant;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcDocumentUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Document '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                TARGETID = entity.LCDOCUMENTID,
                 URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcDocument(int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_DOCUMENT.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcDocumentDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Document '{entity.ToString()}' was deleted by {auditStaff}",

                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCDOCUMENTID,
                 URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<ILcDocumentRepository>().To<LcDocumentRepository>();
           // LcDocumentAdded = ???, LcDocumentUpdated = ???, LcDocumentDeleted = ???,
