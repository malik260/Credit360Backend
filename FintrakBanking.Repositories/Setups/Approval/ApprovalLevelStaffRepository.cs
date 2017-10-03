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
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Repositories.Setups.Approval
{ 
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
                        where a.tbl_Approval_Level.tbl_Approval_Group.CompanyId == companyId
                        && a.Deleted == false
                        select new ApprovalLevelStaffViewModel
                        {
                            groupId = a.tbl_Approval_Level.GroupId,
                            //operationId = a.tbl_Approval_Level.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().OperationId,
                            maximumAmount = a.MaximumAmount,
                            processViewScope = a.ProcessViewScopeId,
                            canViewDocument = a.CanViewCAMDocument,
                            canViewUploadedFile = a.CanViewUploadedFile,
                            canViewApproval = a.CanViewApproval,
                            canApprove = a.CanApprove,
                            canUploadFile = a.CanUploadFile,
                            canSendRequest = a.CanSendJobRequest,
                            canEdit = a.CanEdit,
                            vetoPower = a.VetoPower,
                            //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                            position = a.tbl_Approval_Level.Position,
                            approvalLevelId = a.ApprovalLevelId,
                            approvalLevelName = a.tbl_Approval_Level.LevelName,
                            staffId = a.StaffId,
                            staffLevelId = a.StaffLevelId,// added
                            staffLevelName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
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

        public IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaffById(int StaffLevelId, int companyId)
        {
            return GetApprovalLevelStaff(companyId).Where(c => c.approvalLevelId == StaffLevelId);
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
                ProcessViewScopeId = (short)model.processViewScope,
                CanViewCAMDocument = model.canViewDocument,
                CanViewUploadedFile = model.canViewUploadedFile,
                CanViewApproval = model.canViewApproval,
                CanApprove = model.canApprove,
                CanUploadFile = model.canUploadFile,
                CanSendJobRequest = model.canSendRequest,
                CanEdit = model.canEdit,
                VetoPower = model.vetoPower,
                DateTimeCreated = _genSetup.GetApplicationDate(),
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
                ApplicationDate = _genSetup.GetApplicationDate(),
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
            data.ProcessViewScopeId = (short)model.processViewScope;
            data.CanViewCAMDocument = model.canViewDocument;
            data.CanViewUploadedFile = model.canViewUploadedFile;
            data.CanViewApproval = model.canViewApproval;
            data.CanApprove = model.canApprove;
            data.CanUploadFile = model.canUploadFile;
            data.CanSendJobRequest = model.canSendRequest;
            data.CanEdit = model.canEdit;
            data.VetoPower = model.vetoPower;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();
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
                ApplicationDate = _genSetup.GetApplicationDate(),
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


            //Audit Section ---------------------------
            var audit_staff_level = (context.tbl_Approval_Level.FirstOrDefault(x => x.ApprovalLevelId == data.ApprovalLevelId));
            var audit_staff = (context.tbl_Staff.FirstOrDefault(x => x.StaffId == data.StaffId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Added Approval Level Staff {audit_staff_level.LevelName}' for staff with code '{audit_staff.StaffCode}' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = data.StaffLevelId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            this.context.tbl_Approval_Level_Staff.Remove(data);

            return await context.SaveChangesAsync() != 0;
        }


        public bool AddApprovalTrail(tbl_Approval_Trail model)
        {
            context.tbl_Approval_Trail.Add(model);
            return context.SaveChanges() != 0;
        }

        public bool UpdateApprovalTrail(tbl_Approval_Trail model)
        {
            bool result = false;
            var update = context.tbl_Approval_Trail.SingleOrDefault(m => m.OperationId == model.OperationId
                                                                     && m.ToApprovalLevelId == model.ToApprovalLevelId
                                                                     && m.TargetId == model.TargetId
                                                                 && m.ApprovalStatusId == 0);

            if (update != null)
            {
                update.ApprovalStatusId = model.ApprovalStatusId;
                update.ResponseDate = _genSetup.GetApplicationDate();
                update.SystemResponseDateTime = model.SystemResponseDateTime;
                update.ResponseStaffId = model.ResponseStaffId;

                result = context.SaveChanges() != 0;
            }
            return result;
        }

        public IEnumerable<tbl_Staff_Organogram> GetStaffOrganogram(int companyId)
        {
            return context.tbl_Staff_Organogram.Where(c => c.CompanyId == companyId);
        }

        public IQueryable<tbl_Approval_Trail> GetApprovalTrail(int operationId, int targetId, int approvalLevelId, int numberOfApprovals)
        {
            return context.tbl_Approval_Trail
                .Where(c => c.TargetId == targetId &&
                c.OperationId == operationId &&
                c.ToApprovalLevelId == approvalLevelId)
                .Take(numberOfApprovals);
        }
        public IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int operationId, int companyId)
        {
            var result = (from a in context.tbl_Approval_Trail 
                          where a.OperationId == operationId && a.CompanyId == companyId
                          select

                          new WorkflowTrackerViewModel
                          {
                              arrivalDate = a.ArrivalDate,
                              responseApprovalLevel = context.tbl_Approval_Level.FirstOrDefault(c => c.ApprovalLevelId == a.FromApprovalLevelId).LevelName,
                              responseDate = (DateTime)(a.SystemResponseDateTime.HasValue ? a.SystemResponseDateTime : DateTime.Now),
                              systemArrivalDate = a.SystemArrivalDateTime,
                              systemResponseDate = a.SystemResponseDateTime,
                              responseStaffName = !a.ResponseStaffId.HasValue ? "Awaiting Action" : a.tbl_Staff1.FirstName + " " + a.tbl_Staff1.LastName,
                              comment = a.Comment,
                              requestStaffName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                              requestApprovalLevel = !a.FromApprovalLevelId.HasValue ? "Initiation" : context.tbl_Approval_Level.FirstOrDefault(c => c.ApprovalLevelId == a.FromApprovalLevelId).LevelName,
                              TargetId = a.TargetId,
                              approvalStatus = context.tbl_Approval_Status.FirstOrDefault(c => c.ApprovalStatusId == a.ApprovalStatusId).ApprovalStatusName

                          }
                          );
            return result;
        }
        public IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId)
        {
            var result = GetApprovalTrail(operationId, companyId).Where(c => c.TargetId == targetId).OrderByDescending(c => c.systemArrivalDate).ToList();
            return result;
        }

    }
}
