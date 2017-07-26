using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using System.Threading.Tasks;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Setups.Approval
{
    [Export(typeof(IApprovalLevelStaffRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ApprovalLevelStaffRepository : IApprovalLevelStaffRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public ApprovalLevelStaffRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaff(int companyId)
        {
            var data = (from a in context.tbl_Approval_Level_Staff
                        where a.tbl_Approval_Level.tbl_Approval_Group_Mapping.tbl_Approval_Group.CompanyId == companyId
                        && a.Deleted == false
                        select new ApprovalLevelStaffViewModel
                        {
                            groupId = a.tbl_Approval_Level.tbl_Approval_Group_Mapping.GroupId,
                            operationId = a.tbl_Approval_Level.tbl_Approval_Group_Mapping.OperationId,
                            maximumAmount = a.MaximumAmount,
                            minimumAmount = a.tbl_Approval_Level.MinimumAmount,
                            position = a.tbl_Approval_Level.Position,
                            approvalLevelId = a.ApprovalLevelId,
                            approvalLevelName = a.tbl_Approval_Level.LevelName,
                            staffId = a.StaffId,
                            staffLevelId = a.StaffLevelId,// added
                            staffLevelName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaff(int companyId)
        {
            return GetApprovalLevelStaff(companyId);
        }
        public IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaffByOperationId(int operationId, int companyId)
        {
            return GetApprovalLevelStaff(companyId).Where(c => c.operationId == operationId);
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaffById(int id, int companyId)
        {
            return GetApprovalLevelStaff(companyId).Where(c => c.approvalLevelId == id);
        }

        public ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId, int operationId)
        {
            var levelStaff = GetApprovalLevelStaff(companyId);
            return levelStaff.Where(c => c.staffId == staffId && c.operationId == operationId).FirstOrDefault();
            
        }

        public bool AddApprovalLevelStaff(ApprovalLevelStaffViewModel model)
        {
            var data = new tbl_Approval_Level_Staff
            {
                MaximumAmount = model.maximumAmount,
                StaffId = model.staffId,
                ApprovalLevelId = model.approvalLevelId,
                DateTimeCreated = _genSetup.GetApplicaionDate(),
                CreatedBy = (int)model.createdBy
            };

            // Audit Section ---------------------------
            var audit_staff_level = (context.tbl_Approval_Level.FirstOrDefault(x => x.ApprovalLevelId == data.ApprovalLevelId));
            var audit_staff = (context.tbl_Staff.FirstOrDefault(x => x.StaffId == data.StaffId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelStaffAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Approval-Level '{audit_staff_level.LevelName}' for user code '{audit_staff.StaffCode}' .",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = model.staffLevelId
            };

            context.tbl_Approval_Level_Staff.Add(data);
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool UpdateApprovalLevelStaff(int StaffLevelId, ApprovalLevelStaffViewModel model)
        {
            var data = this.context.tbl_Approval_Level_Staff.Find(StaffLevelId);
            if (data == null) return false;

            data.StaffId = model.staffId;
            data.ApprovalLevelId = model.approvalLevelId;
            data.MaximumAmount = model.maximumAmount;
            data.DateTimeUpdated = _genSetup.GetApplicaionDate();
            data.LastUpdatedBy = (int)model.createdBy;

            // Audit Section ---------------------------
            //var audit_staff_level = (context.TblApprovalLevel.FirstOrDefault(x => x.ApprovalLevelId == StaffLevelId));
            var audit_staff = (context.tbl_Staff.FirstOrDefault(x => x.StaffId == data.StaffId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelStaffUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Approval Level for staff with code '{audit_staff.StaffCode}' to level {model.staffLevelName}'",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = model.staffLevelId
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public async Task<bool> DeleteApprovalLevelStaff(int StaffLevelId, UserInfo user)
        {
            var data = this.context.tbl_Approval_Level_Staff.Find(StaffLevelId);
            {
                data.DateTimeDeleted = _genSetup.GetApplicaionDate();
                data.Deleted = true;
                data.DeletedBy = user.staffId;
            };

            //Audit Section ---------------------------
            var audit_staff_level = (context.tbl_Approval_Level.FirstOrDefault(x => x.ApprovalLevelId == data.ApprovalLevelId));
            var audit_staff = (context.tbl_Staff.FirstOrDefault(x => x.StaffId == data.StaffId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelDeleted,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Added Approval Level Staff {audit_staff_level.LevelName}' for staff with code '{audit_staff.StaffCode}' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = data.StaffLevelId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }
    }
}
