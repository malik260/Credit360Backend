using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.WorkFlow;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Setups.Approval
{
    [Export(typeof(IApprovalLevelRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ApprovalLevelRepository : IApprovalLevelRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public ApprovalLevelRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository _genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
        }

        private IEnumerable<ApprovalLevelViewModel> GetApprovalLevel(int companyId) 
        {
            var data = (from a in context.tbl_Approval_Level
                        where a.tbl_Approval_Group_Mapping.tbl_Approval_Group.CompanyId == companyId && a.Deleted == false
                        select new ApprovalLevelViewModel
                        {
                            approvalLevelId = a.ApprovalLevelId,
                            levelName = a.LevelName,
                            canEdit = a.CanEdit,
                            operationId = a.tbl_Approval_Group_Mapping.OperationId ,
                            canOverideAuthorisation = a.CanOverideAuthorisation,
                            canPerformFinancialAnalysis = a.CanPerformFinancialAnalysis,
                            canRecieveAdjustment = a.CanRecieveAdjustment,
                            canRecieveEmail = a.CanRecieveEmail,
                            canRecieveSms = a.CanRecieveSMS,
                            hasChecklist = a.HasChecklist,
                            tenor = a.Tenor,
                            tenorModeId = a.TenorModeId,
                            tenorModename = a.tbl_Tenor_Mode.TenorModeName,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            minimumAmount = a.MinimumAmount,
                            numberOfApprovals = a.NumberOfApprovals,
                            numberOfUsers = a.NumberOfUsers,
                            groupOperationMappingId = a.GroupOperationMappingId,
                            operationName = a.tbl_Approval_Group_Mapping.tbl_Operations.OperationName,
                            requireAuthorisation = a.RequireAuthorisation,
                            slaInterval = a.SLAInterval,
                            position = a.Position,
                            routeViaStaffOrganogram = a.RouteViaStaffOrganogram,
                            canDoRiskAssessment = a.CanDoRiskAssessment,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public IEnumerable<ApprovalLevelViewModel> GetAllApprovalLevel(int companyId)
        {
            return GetApprovalLevel(companyId);
        }

        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevelById(int ApprovalLevelId, int companyId)
        {
            return GetApprovalLevel(companyId).Where(c => c.approvalLevelId == ApprovalLevelId);
        }

        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByOperationId(int groupOperationMappingId, int companyId)
        {
            return GetApprovalLevel(companyId).Where(c => c.groupOperationMappingId == groupOperationMappingId);
        }

        public bool AddApprovalLevel(ApprovalLevelViewModel model)
        {
            var data = new tbl_Approval_Level
            {
                ApprovalLevelId = model.approvalLevelId,
                LevelName = model.levelName,
                CanEdit = model.canEdit,
                CanOverideAuthorisation = model.canOverideAuthorisation,
                CanPerformFinancialAnalysis = model.canPerformFinancialAnalysis,
                CanRecieveAdjustment = model.canRecieveAdjustment,
                CanRecieveEmail = model.canRecieveEmail,
                CanRecieveSMS = model.canRecieveSms,
                HasChecklist = model.hasChecklist,
                IsPoliticallyExposed = model.isPoliticallyExposed,
                MinimumAmount = model.minimumAmount,
                Tenor = model.tenor,
                TenorModeId = model.tenorModeId,
                NumberOfApprovals = model.numberOfApprovals,
                NumberOfUsers = model.numberOfUsers,
                GroupOperationMappingId = model.groupOperationMappingId,
                RequireAuthorisation = model.requireAuthorisation,
                SLAInterval = model.slaInterval,
                Position = model.position,
                RouteViaStaffOrganogram = model.routeViaStaffOrganogram,
                CanDoRiskAssessment = model.canDoRiskAssessment,
                DateTimeCreated = genSetup.GetApplicationDate(),
                CreatedBy = (int)model.createdBy
            };

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Approval Level '{model.levelName}'. ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = model.approvalLevelId
            };

            context.tbl_Approval_Level.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------



            return context.SaveChanges() != 0;
        }

        public bool AddMultipleApprovalLevel(List<ApprovalLevelViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (ApprovalLevelViewModel model in models)
            {
                AddApprovalLevel(model);
            }

            return true;
        }

        public bool UpdateApprovalLevel(int ApprovalLevelId, ApprovalLevelViewModel model)
        {
            var data = this.context.tbl_Approval_Level .Find(ApprovalLevelId);
            if (data == null) return false;

            data.LevelName = model.levelName;
            data.CanEdit = model.canEdit;
            data.CanOverideAuthorisation = model.canOverideAuthorisation;
            data.CanPerformFinancialAnalysis = model.canPerformFinancialAnalysis;
            data.CanRecieveAdjustment = model.canRecieveAdjustment;
            data.CanRecieveEmail = model.canRecieveEmail;
            data.CanRecieveSMS = model.canRecieveSms;
            data.HasChecklist = model.hasChecklist;
            data.Tenor = model.tenor;
            data.TenorModeId = model.tenorModeId;
            data.IsPoliticallyExposed = model.isPoliticallyExposed;
            data.MinimumAmount = model.minimumAmount;
            data.NumberOfApprovals = model.numberOfApprovals;
            data.NumberOfUsers = model.numberOfUsers;
            data.GroupOperationMappingId = model.groupOperationMappingId;
            data.RequireAuthorisation = model.requireAuthorisation;
            data.SLAInterval = model.slaInterval;
            data.Position = model.position;
            data.RouteViaStaffOrganogram = model.routeViaStaffOrganogram;
            data.CanDoRiskAssessment = model.canDoRiskAssessment;
            data.DateTimeUpdated = genSetup.GetApplicationDate();
            data.LastUpdatedBy = (int)model.createdBy;

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Approval Level '{model.levelName}'. ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = model.approvalLevelId 
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public async Task<bool> DeleteApprovalLevel(int ApprovalLevelId, UserInfo user)
        {
            var data = this.context.tbl_Approval_Level.Find(ApprovalLevelId);
            {
                data.DateTimeDeleted = genSetup.GetApplicationDate();
                data.Deleted = true;
                data.DeletedBy = user.staffId;
            };

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelDeleted,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Approval Level '{data.LevelName}'. ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = data.ApprovalLevelId 
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;

        }

        public async Task<bool> AddApprovalTrail(tbl_Approval_Trail model)
        {
            context.tbl_Approval_Trail.Add(model);
            return await context.SaveChangesAsync() != 0;
        }
        
        public  bool UpdateApprovalTrail(tbl_Approval_Trail model)
        {
            bool result = false;
            var update = context.tbl_Approval_Trail.SingleOrDefault(m => m.OperationId == model.OperationId
                                                                     && m.ToApprovalLevelId == model.ToApprovalLevelId
                                                                     && m.TargetId == model.TargetId
                                                                 && m.ApprovalStatusId == 0);

            if (update != null)
            {
                update.ApprovalStatusId = model.ApprovalStatusId;
                update.ResponseDate = genSetup.GetApplicationDate();
                update.ResponseStaffId = model.RequestStaffId;
                
                result = context.SaveChanges() != 0;
            }
            return result;
        }

        public IEnumerable <tbl_Staff_Organogram> GetStaffOrganogram(int companyId)
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
            var result= (from a in context.tbl_Approval_Trail
                    join b in context.tbl_Approval_Level on a.ToApprovalLevelId equals b.ApprovalLevelId
                    join d in context.tbl_Approval_Status on a.ApprovalStatusId equals d.ApprovalStatusId 
                    join c in context.tbl_Approval_Level on a.FromApprovalLevelId equals c.ApprovalLevelId into another
                    from c in another.DefaultIfEmpty()
                    where a.OperationId == operationId && a.CompanyId == companyId
                         select  new {
                        RequestStaffName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                        RequestApprovalLevel = c == null ? "Initiation" : c.LevelName,
                        ArrivalDate = a.ArrivalDate.Date + a.SystemArrivalDateTime.Date.TimeOfDay  ,
                        
                        ApprovalStatus = d.ApprovalStatusName ,

                        ResponseDate = a.ResponseDate + a.SystemResponseDateTime.Value .Date.TimeOfDay,
                        ResponseStaffName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                        ResponseApprovalLevel = b.LevelName,
                        TargetId = a.TargetId 
                    })
                        .ToList().AsQueryable();
            return (result.Select(c => new WorkflowTrackerViewModel
            {
                approvalStatus = c.ApprovalStatus,
                arrivalDate = c.ArrivalDate,
                requestApprovalLevel = c.RequestApprovalLevel,
                requestStaffName = c.RequestStaffName,
                responseApprovalLevel = c.RequestApprovalLevel,
                responseDate = (DateTime)c.ResponseDate,
                responseStaffName = c.ResponseStaffName
            }));
        }
        public IQueryable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId)
        {
            return GetApprovalTrail(operationId,  companyId).Where(c => c.TargetId == targetId);
        }

    }
}
