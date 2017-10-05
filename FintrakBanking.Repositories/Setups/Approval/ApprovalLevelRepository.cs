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
using System.Data.Entity;

namespace FintrakBanking.Repositories.Setups.Approval
{
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
            var data = (from x in this.context.tbl_Approval_Level
                        join b in context.tbl_Approval_Group_Mapping on x.GroupId equals b.GroupId
                        where x.Deleted == false && x.tbl_Approval_Group.CompanyId == companyId
                        select new ApprovalLevelViewModel
                        {
                            approvalLevelId = x.ApprovalLevelId,
                            levelName = x.LevelName,
                            position = x.Position,
                            tenor = x.Tenor,
                            maximumAmount = x.MaximumAmount,
                            investmentGradeAmount = x.InvestmentGradeAmount,
                            numberOfUsers = x.NumberOfUsers,
                            numberOfApprovals = x.NumberOfApprovals,
                            slaInterval = x.SLAInterval,
                            canRouteBack = x.CanRouteBack,
                            isPoliticallyExposed = x.IsPoliticallyExposed,
                            isActive = x.IsActive,
                            canEdit = x.CanEdit,
                            canDoRiskAssessment = x.CanDoRiskAssessment,
                            canRecieveAdjustment = x.CanRecieveAdjustment,
                            canRecieveEmail = x.CanRecieveEmail,
                            canRecieveSms = x.CanRecieveSMS,
                            hasChecklist = x.HasChecklist,
                            canPerformFinancialAnalysis = x.CanPerformFinancialAnalysis,
                            requireAuthorisation = x.RequireAuthorisation,
                            canOverideAuthorisation = x.CanOverideAuthorisation,
                            routeViaStaffOrganogram = x.RouteViaStaffOrganogram,
                            createdBy = x.CreatedBy,
                            dateTimeCreated = x.DateTimeCreated,
                            dateTimeUpdated = x.DateTimeUpdated,
                            deleted = x.Deleted,
                            deletedBy = x.DeletedBy,
                            dateTimeDeleted = x.DateTimeDeleted,
                            groupId = (int)x.GroupId,
                            operationId = b.OperationId
                        }).OrderBy(x => x.position).ToList();

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

        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByGroupId(int groupId, int companyId)
        {
            return GetApprovalLevel(companyId).Where(c => c.groupId == groupId);
        }

        public bool AddApprovalLevel(ApprovalLevelViewModel model)
        {
            var data = new tbl_Approval_Level
            {
                ApprovalLevelId = model.approvalLevelId,
                LevelName = model.levelName,
                Position = model.position,
                Tenor = model.tenor,
                MaximumAmount = model.maximumAmount,
                InvestmentGradeAmount = model.investmentGradeAmount,
                NumberOfUsers = model.numberOfUsers,
                NumberOfApprovals = model.numberOfApprovals,
                SLAInterval = model.slaInterval,
                CanRouteBack = model.canRouteBack,
                IsPoliticallyExposed = model.isPoliticallyExposed,
                IsActive = model.isActive,
                CanEdit = model.canEdit,
                CanDoRiskAssessment = model.canDoRiskAssessment,
                CanRecieveAdjustment = model.canRecieveAdjustment,
                CanRecieveEmail = model.canRecieveEmail,
                CanRecieveSMS = model.canRecieveSms,
                HasChecklist = model.hasChecklist,
                CanPerformFinancialAnalysis = model.canPerformFinancialAnalysis,
                RequireAuthorisation = model.requireAuthorisation,
                CanOverideAuthorisation = model.canOverideAuthorisation,
                RouteViaStaffOrganogram = model.routeViaStaffOrganogram,
                CreatedBy = model.createdBy,
                GroupId = model.groupId,
                DateTimeCreated = genSetup.GetApplicationDate()
            };

            context.tbl_Approval_Level.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added ApprovalLevel '{ model.levelName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

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

        public bool UpdateApprovalLevel(int approvalLevelId, ApprovalLevelViewModel model)
        {
            var data = this.context.tbl_Approval_Level.Find(approvalLevelId);
            if (data == null) { return false; }

            data.LevelName = model.levelName;
            data.Position = model.position;
            data.Tenor = model.tenor;
            //data.TenorModeId = 1; // model.tenorModeId;
            data.MaximumAmount = model.maximumAmount;
            data.InvestmentGradeAmount = model.investmentGradeAmount;
            data.NumberOfUsers = model.numberOfUsers;
            data.NumberOfApprovals = model.numberOfApprovals;
            data.SLAInterval = model.slaInterval;
            data.CanRouteBack = model.canRouteBack;
            data.IsPoliticallyExposed = model.isPoliticallyExposed;
            data.IsActive = model.isActive;
            data.CanEdit = model.canEdit;
            data.CanDoRiskAssessment = model.canDoRiskAssessment;
            data.CanRecieveAdjustment = model.canRecieveAdjustment;
            data.CanRecieveEmail = model.canRecieveEmail;
            data.CanRecieveSMS = model.canRecieveSms;
            data.HasChecklist = model.hasChecklist;
            data.CanPerformFinancialAnalysis = model.canPerformFinancialAnalysis;
            data.RequireAuthorisation = model.requireAuthorisation;
            data.CanOverideAuthorisation = model.canOverideAuthorisation;
            data.RouteViaStaffOrganogram = model.routeViaStaffOrganogram;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = DateTime.Now;
            data.GroupId = model.groupId;
            data.LastUpdatedBy = model.lastUpdatedBy;

            // Audit Section ---------------------------
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
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public async Task<bool> DeleteApprovalLevel(int id, UserInfo user)
        {
            var data = this.context.tbl_Approval_Level.Find(id);

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalLevelDeleted,
                StaffId = user.staffId,
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

            if (context.tbl_Approval_Trail.Where(x => x.ToApprovalLevelId == id || x.FromApprovalLevelId == id).Any())
            {
                throw new Exception("Can not delete this level because it is being used. You can de activate it.");
            }
            else
            {
                this.context.tbl_Approval_Level.Remove(data);
            }

            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> AddApprovalTrail(tbl_Approval_Trail model)
        {
            try
            {
                context.tbl_Approval_Trail.Add(model);
                var saved = await context.SaveChangesAsync();
                return saved > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
                update.ResponseDate = genSetup.GetApplicationDate();
                update.ResponseStaffId = model.RequestStaffId;
                update.SystemResponseDateTime = DateTime.Now;
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
                          join b in context.tbl_Approval_Level on a.ToApprovalLevelId equals b.ApprovalLevelId
                          join d in context.tbl_Approval_Status on a.ApprovalStatusId equals d.ApprovalStatusId
                          join c in context.tbl_Approval_Level on a.FromApprovalLevelId equals c.ApprovalLevelId into another
                          from c in another.DefaultIfEmpty()
                          where a.OperationId == operationId && a.CompanyId == companyId
                          select new
                          {
                              RequestStaffName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                              RequestApprovalLevel = c == null ? "Initiation" : c.LevelName,
                              ArrivalDate = a.ArrivalDate,
                              //ArrivalDate = a.ArrivalDate + a.SystemArrivalDateTime.TimeOfDay  ,

                              ApprovalStatus = d.ApprovalStatusName,
                              //ResponseDate = a.ResponseDate + a.SystemResponseDateTime.Value.TimeOfDay,

                              ResponseDate = a.ResponseDate.HasValue ? a.ResponseDate : DateTime.Now,
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
            return GetApprovalTrail(operationId, companyId).Where(c => c.TargetId == targetId);
        }
    }
}