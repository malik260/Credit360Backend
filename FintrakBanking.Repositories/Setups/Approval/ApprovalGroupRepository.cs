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

namespace FintrakBanking.Repositories.Setups.Approval
{
    [Export(typeof(IApprovalGroupRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ApprovalGroupRepository : IApprovalGroupRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public ApprovalGroupRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private IEnumerable<ApprovalGroupViewModel> GetApprovalGroup(int companyId)
        {
            var data = (from a in context.tbl_Approval_Group 
                        where a.CompanyId == companyId && a.Deleted == false
                        select new ApprovalGroupViewModel
                        {
                            groupId = a.GroupId,
                            groupName = a.GroupName,
                            isBeforeCamapproval = a.IsBeforeCAMApproval,
                            isCommittee = a.IsCommittee,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
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
            var data = new tbl_Approval_Group
            {                
                GroupName = model.groupName,
                IsCommittee = model.isCommittee,
                IsBeforeCAMApproval = model.isBeforeCamapproval,
                CompanyId = model.companyId,
                DateTimeCreated = _genSetup.GetApplicaionDate(),
                CreatedBy = (int)model.createdBy
            };

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Approval Group '{model.groupName}' with Is Committee: {model.isCommittee} ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = model.groupId
            };

            context.tbl_Approval_Group.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool UpdateApprovalGroup(int GroupId, ApprovalGroupViewModel model)
        {
            var data = this.context.tbl_Approval_Group .Find(GroupId);
            if (data == null) return false;
            data.GroupName = model.groupName;
            data.IsBeforeCAMApproval = model.isBeforeCamapproval;
            data.IsCommittee = model.isCommittee;
            data.DateTimeUpdated = _genSetup.GetApplicaionDate();
            data.LastUpdatedBy = (int)model.createdBy;

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Approval Group '{model.groupName}' with Is Committee: {model.isCommittee} ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = model.groupId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool DeleteApprovalGroup(int GroupId, UserInfo user)
        {
            var data = this.context.tbl_Approval_Group.Find(GroupId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicaionDate();


            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupUpdated,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Approval Group '{data.GroupName}' with Is Committee: {data.IsCommittee} ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = data.GroupId
            };


            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }



       



    }
}
