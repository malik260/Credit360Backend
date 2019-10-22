using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalGroupRepository : IApprovalGroupRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public ApprovalGroupRepository(
            FinTrakBankingContext _context,
            IGeneralSetupRepository genSetup,
            IAuditTrailRepository _auditTrail
            )
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private IEnumerable<ApprovalGroupViewModel> GetApprovalGroup(int companyId)
        {
            var data = (from a in context.TBL_APPROVAL_GROUP
                        where a.COMPANYID == companyId && a.DELETED == false
                        select new ApprovalGroupViewModel
                        {
                            groupId = a.GROUPID,
                            groupName = a.GROUPNAME,
                            roleId = a.ROLEID,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).OrderBy(x => x.groupName);
            return data;
        }

        public IEnumerable<ApprovalGroupViewModel> GetAllApprovalGroup(int companyId)
        {
            return GetApprovalGroup(companyId);
        }

        public IEnumerable<ApprovalGroupViewModel> GetApprovalGroupById(int GroupId, int companyId)
        {
            return GetApprovalGroup(companyId).Where(c => c.companyId == companyId);
        }

        public bool AddApprovalGroup(ApprovalGroupViewModel model)
        {
            var data = new TBL_APPROVAL_GROUP
            {                
                GROUPNAME = model.groupName,
                ROLEID = model.roleId,
                COMPANYID = model.companyId,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Approval Group '{model.groupName}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.groupId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            context.TBL_APPROVAL_GROUP.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateApprovalGroup(int GroupId, ApprovalGroupViewModel model)
        {
            var data = this.context.TBL_APPROVAL_GROUP .Find(GroupId);
            if (data == null) return false;
            data.GROUPNAME = model.groupName;
            data.ROLEID = model.roleId;
            data.LASTUPDATEDBY = (int)model.createdBy;

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Approval Group '{model.groupName}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.groupId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteApprovalGroup(int GroupId, UserInfo user)
        {
            var data = this.context.TBL_APPROVAL_GROUP.Find(GroupId);
            data.DELETED = true;
            data.DELETEDBY = (int)user.createdBy;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Approval Group '{data.GROUPNAME}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.GROUPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
    }
}
