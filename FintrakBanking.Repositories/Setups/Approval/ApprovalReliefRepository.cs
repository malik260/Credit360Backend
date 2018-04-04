using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;

using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalReliefRepository : IApprovalReliefRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository auditTrail;

        public ApprovalReliefRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository general,
            IAuditTrailRepository audit
            )
        {
            this.context = context;
            this.general = general;
            this.auditTrail = audit;
        }

        public bool AddApprovalRelief(ApprovalReliefViewModel model)
        {
            var data = new TBL_STAFF_RELIEF
            {
                STAFFID = model.relievedStaffId,
                RELIEFSTAFFID = model.reliefStaffId,
                RELIEFREASON = model.reliefReason,
                STARTDATE = model.startDate,
                ENDDATE = model.endDate,
                ISACTIVE = model.isActive,
                DATETIMECREATED = general.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffReliefAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added '{model.reliefStaffName}' as Staff Relief for: '{model.staffName}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.reliefId
            };

            context.TBL_STAFF_RELIEF.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ApprovalReliefViewModel> GetAllApprovalRelief(int companyId)
        {
            return context.TBL_STAFF_RELIEF
                .Where(x => x.DELETED == false)
                .OrderByDescending(x => x.RELIEFID)
                .Select(x => new ApprovalReliefViewModel
                {
                    reliefId = x.RELIEFID,
                    relievedStaffId = x.STAFFID,
                    reliefStaffId = x.RELIEFSTAFFID,
                    staffName = context.TBL_STAFF.Where(s => s.STAFFID == x.STAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                .FirstOrDefault().name ?? "",
                    reliefStaffName = context.TBL_STAFF.Where(s => s.STAFFID == x.RELIEFSTAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                .FirstOrDefault().name ?? "",
                    reliefReason = x.RELIEFREASON,
                    startDate = x.STARTDATE,
                    endDate = x.ENDDATE,
                    isActive = x.ISACTIVE,
                });
        }



        public bool UpdateApprovalRelief(int reliefId, ApprovalReliefViewModel model)
        {
            var data = this.context.TBL_STAFF_RELIEF.Find(reliefId);
            if (data == null) return false;

            data.RELIEFSTAFFID = model.reliefStaffId;
            data.RELIEFREASON = model.reliefReason;
            data.STARTDATE = model.startDate;
            data.ENDDATE = model.endDate;
            data.ISACTIVE = model.isActive;
            data.LASTUPDATEDBY = (int)model.createdBy;

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffReliefUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Staff Relief of '{model.staffName}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.reliefId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
    }
}
