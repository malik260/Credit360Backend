using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

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
                        where a.tbl_Approval_Level.tbl_Approval_Group_Mapping.tbl_Approval_Group.CompanyId == companyId
                        && a.Deleted == false
                        select new ApprovalLevelStaffViewModel
                        {
                            groupId = a.tbl_Approval_Level.tbl_Approval_Group_Mapping.GroupId,
                            operationId = a.tbl_Approval_Level.tbl_Approval_Group_Mapping.OperationId,
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
            {
                data.DateTimeDeleted = _genSetup.GetApplicationDate();
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
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = data.StaffLevelId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

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

        private IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int companyId)
        {
            var result = (from a in context.tbl_Approval_Trail
                          join b in context.tbl_Approval_Level on a.FromApprovalLevelId equals b.ApprovalLevelId
                          join c in context.tbl_Approval_Group_Mapping on b.GroupOperationMappingId equals c.GroupOperationMappingId
                          join d in context.tbl_Approval_Group on c.GroupId equals d.GroupId
                          join e in context.tbl_Operations on c.OperationId equals e.OperationId

                          join f in context.tbl_Approval_Level on a.ToApprovalLevelId equals f.ApprovalLevelId
                          join g in context.tbl_Approval_Group_Mapping on f.GroupOperationMappingId equals g.GroupOperationMappingId
                          join h in context.tbl_Approval_Group on g.GroupId equals h.GroupId
                          join i in context.tbl_Staff on a.RequestStaffId equals i.StaffId
                          join j in context.tbl_Staff on a.ResponseStaffId equals j.StaffId
                          join k in context.tbl_Approval_Status on a.ApprovalStatusId equals k.ApprovalStatusId
                          where a.CompanyId == companyId
                          select new WorkflowTrackerViewModel

                          {
                              arrivalDate = a.ArrivalDate,
                              responseApprovalLevel = a.ToApprovalLevelId.HasValue ? f.LevelName : "N/A",
                              responseDate = a.SystemResponseDateTime ?? DateTime.Now,
                              systemArrivalDate = a.SystemArrivalDateTime,
                              systemResponseDate = a.SystemResponseDateTime,
                              responseStaffName = !a.ResponseStaffId.HasValue ? "Awaiting Action" : j.FirstName + " " + j.LastName,
                              comment = a.Comment,
                              requestStaffName = i.FirstName + " " + i.LastName,
                              requestApprovalLevel = !a.FromApprovalLevelId.HasValue ? "Initiation" : b.LevelName,
                              TargetId = a.TargetId,
                              operationId = e.OperationId,
                              operationName = e.OperationName,
                              approvalStatus = k.ApprovalStatusName
                          });
            return result;
        }

        public async Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId)
        {
            var result = await GetApprovalTrail(companyId).Where(c => c.TargetId == targetId && c.operationId == operationId).OrderByDescending(c => c.systemArrivalDate).ToListAsync();
            return result;
        }

        public IQueryable<WorkflowTrackerViewModel> GetAllRecordsOnApprovalTrail(int companyId)
        {
            var result = GetApprovalTrail(companyId).OrderByDescending(c => c.systemArrivalDate);

            return result;
        }
    }
}