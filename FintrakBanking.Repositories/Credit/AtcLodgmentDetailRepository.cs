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
    public class AtcLodgmentDetailRepository : IAtcLodgmentDetailRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public AtcLodgmentDetailRepository(
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


        public IEnumerable<AtcLodgmentDetailViewModel> GetAtcLodgmentDetail(int id)
        {
            var entity = new List<AtcLodgmentDetailViewModel>();

            entity = context.TBL_ATC_LODGMENT_DETAIL.Where(x => x.ATCLODGMENTID == id && x.DELETED == false)
               .Select(x => new AtcLodgmentDetailViewModel
               {
                   atcLodgmentDetailId = x.ATCLODGMENTDETAILID,
                   detail = x.DETAIL,
                   value = x.VALUE,
                   atcLodgmentId = x.ATCLODGMENTID,
               }).ToList();


            return entity;
        }

        public bool AddAtcLodgmentDetail(AtcLodgmentDetailViewModel model)
        {
            var entity = new TBL_ATC_LODGMENT_DETAIL
            {
                DETAIL = model.detail,
                VALUE = model.value,
                ATCLODGMENTID = model.atcLodgmentId,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_ATC_LODGMENT_DETAIL.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------

            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentDetailAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Atc Lodgment Detail '{entity.DETAIL}' created by {auditStaff}",       
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

       

        public bool DeleteAtcLodgmentDetail(int id, UserInfo user)
        {
            var entity = this.context.TBL_ATC_LODGMENT_DETAIL.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentDetailDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Atc Lodgment Detail '{entity.DETAIL}' was deleted by {auditStaff}",
                URL =user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),            
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<IAtcLodgmentDetailRepository>().To<AtcLodgmentDetailRepository>();
           // AtcLodgmentDetailAdded = ???, AtcLodgmentDetailUpdated = ???, AtcLodgmentDetailDeleted = ???,
