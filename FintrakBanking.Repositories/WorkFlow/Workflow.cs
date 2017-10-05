using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Repositories.WorkFlow
{
    public class Workflow : IWorkflow
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;

        public Workflow(FinTrakBankingContext context, IGeneralSetupRepository general)
        {
            this.context = context;
            this.general = general;
        }

        private int staffId;
        private int targetId;
        private int companyId;
        private int operationId;

        private int? productClassId = null;
        private int? productId = null;
        private string comment = string.Empty;
        private int statusId = (int)ApprovalStatusEnum.Processing;
        private int? nextLevelId = null; // for refer backs
        private bool emailNotification = false;
        private bool smsNotification = false;

        private string message;
        private int? fromLevelId = null;
        private int currentStateId;
        private int newStateId = (int)ApprovalState.Processing;
        private int tenor = 0;
        private decimal amount = 0;
        private bool investmentGrade = false;
        private bool saved = false;
        private bool useOrganogram = false;
        private DateTime systemDate = DateTime.Now;
        private DateTime applicationDate;
        private int requestStaffId;
        private int neededNumberOfApproval;
        private bool externalInitialization = false;
        private bool vote = false;
        private bool politicallyExposed = false;

        public int StaffId { set { staffId = value; } }
        public int TargetId { set { targetId = value; } }
        public int CompanyId { set { companyId = value; } }
        public int OperationId { set { operationId = value; } }
        public decimal Amount { set { amount = value; } }
        public string Comment { set { comment = value; } }
        public int Tenor { set { tenor = value; } }
        public bool InvestmentGrade { set { investmentGrade = value; } }
        public bool PoliticallyExposed { set { politicallyExposed = value; } }
        public bool Vote { set { vote = value; } }
        public int StatusId { get { return statusId; } set { statusId = value; } }
        public int NextLevelId { set { nextLevelId = value; } }
        public int? ProductId { set { productId = value; } }
        public int? ProductClassId { set { productClassId = value; } }
        public bool EmailNotification { set { emailNotification = value; } }
        public bool SmsNotification { set { smsNotification = value; } }
        public bool ExternalInitialization { set { externalInitialization = value; } }
        public string Message { get { return message; } }
        public bool Saved { get { return saved; } }
        public int NewState { get { return newStateId; } }

        private List<WorkflowSetup> workflowSetup;

        public bool LogActivity()
        {
            if (Validation() == false) { return false; }
            if (Authorization() == false) { return false; }
            
            var request = context.tbl_Approval_Trail.Where(x =>
                                x.CompanyId == this.companyId
                                && x.OperationId == this.operationId
                                && x.TargetId == this.targetId
                            ).OrderByDescending(x => x.ApprovalTrailId).FirstOrDefault();

            if (request != null)
            {
                this.currentStateId = request.ApprovalStateId;
                this.requestStaffId = request.RequestStaffId;
                if (LastActionIsByStaff()) { throw new Exception("Last action is by staff!!"); }
                this.fromLevelId = request.ToApprovalLevelId;
            } else
            {
                this.currentStateId = (int)ApprovalState.Initiation;
            }

            if (ResolveLevelConfigurations() == false) { return false; }

            if (ProcessIsClosed()) { return false; }

            if (this.useOrganogram == true) { OrganogramRouting(); }

            if (this.neededNumberOfApproval > 1 && this.statusId == (int)ApprovalStatusEnum.Approved)
            {
                ResolveLevelMultipleApproval();
            }

            CheckApprovalLimits();

            SetState(); 

            this.applicationDate = GetApplicationDate();

            if (request != null)
            {
                request.ResponseDate = this.applicationDate;
                request.SystemResponseDateTime = this.systemDate;
                request.ResponseStaffId = this.staffId;
            }

            var trail = new tbl_Approval_Trail
            {
                FromApprovalLevelId = this.fromLevelId,
                ToApprovalLevelId = this.nextLevelId,
                TargetId = this.targetId,
                CompanyId = this.companyId,
                RequestStaffId = this.staffId,
                OperationId = this.operationId,
                Comment = this.comment,
                ArrivalDate = this.applicationDate,
                ApprovalStateId = (short)this.newStateId,
                ApprovalStatusId = (short)this.statusId,
                SystemArrivalDateTime = this.systemDate,
                SystemResponseDateTime = this.systemDate,
                VotedYes = this.vote,
            };

            context.tbl_Approval_Trail.Add(trail);
            this.saved = context.SaveChanges() > 0;

            if (this.saved)
            {
                this.SendNotifications();
                this.message = "Workflow process activity log successful!";
                return true;
            }

            this.message = "Unable to save record!";
            return false;
        }

        private DateTime GetApplicationDate()
        {
            return this.general.GetApplicationDate();
        }

        private bool LastActionIsByStaff()
        {
            if (this.staffId == this.requestStaffId)
            {
                this.message = "Cannot act on self initiated process!";
                return true;
            }
            return false;
        }

        private bool ProcessIsClosed()
        {
            if (this.currentStateId == (int)ApprovalState.Ended)
            {
                this.message = "Process is closed!";
                return true;
            }
            return false;
        }

        private bool ResolveLevelConfigurations()
        {
            //var approvalLevels = context.tbl_Approval_Group_Mapping.Where(x => x.Deleted == false
            //                    && x.OperationId == this.operationId
            //                    && x.ProductClassId == this.productClassId
            //                    && x.ProductId == this.productId
            //                )
            //                .Join(context.tbl_Approval_Group, m => m.GroupId, g => g.GroupId, (m, g) => new { m, g })
            //                .Join(context.tbl_Approval_Level, mg => mg.m.GroupId, l => l.GroupId, (mg, l) => 
            //                new { Mapping = mg.m, Level = l })
            //                .Where(x => x.Level.IsActive == true)
            //                .Select(x => new WorkflowSetup
            //                {
            //                    GroupPosition = x.Mapping.Position,
            //                    LevelPosition = x.Level.Position,
            //                    Staff = x.Level.tbl_Approval_Level_Staff,
            //                    Level= x.Level,
            //                    Group = x.Level.tbl_Approval_Group,
            //                    Mapping = x.Mapping,
            //                    CanRecieveSMS = x.Level.CanRecieveSMS,
            //                    CanRecieveEmail = x.Level.CanRecieveEmail,
            //                    ApprovalLevelId = x.Level.ApprovalLevelId,
            //                    RouteViaStaffOrganogram = x.Level.RouteViaStaffOrganogram,
            //                })
            //                .OrderBy(x => x.GroupPosition)
            //                .ThenBy(x => x.LevelPosition);

            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId);

            WorkflowSetup next = null;

            if (this.externalInitialization == true && this.currentStateId == (int)ApprovalState.Initiation)
            {
                next = approvalLevels.FirstOrDefault(); //ok
                if (next != null)
                {
                    this.smsNotification = next.CanRecieveSMS;
                    this.emailNotification = next.CanRecieveEmail;
                    this.nextLevelId = next.ApprovalLevelId;
                    this.useOrganogram = next.RouteViaStaffOrganogram;
                    return true;
                }
                this.message = "Unable to resolve initiating level. No setup for the specified operation!";
                return false;
            }

            if (this.fromLevelId == null) // && externalInitialization == false
            {
                var levelStaff = approvalLevels.SelectMany(x => x.Staff).Where(x => x.StaffId == this.staffId).FirstOrDefault(); // doing
                if (levelStaff == null)
                {
                    this.message = "Unable to resolve initiating level. No setup for the specified operation!";
                    return false;
                }
                this.fromLevelId = levelStaff.ApprovalLevelId;
                this.neededNumberOfApproval = levelStaff.tbl_Approval_Level.NumberOfApprovals;
            }

            if (this.nextLevelId == null) // && fromLevelId != null
            {
                var currentLevel = approvalLevels.Where(x => x.ApprovalLevelId == this.fromLevelId).First();

                next = approvalLevels.FirstOrDefault(x =>
                    (x.GroupPosition > currentLevel.GroupPosition) // next group
                    || (x.LevelPosition > currentLevel.LevelPosition && x.GroupPosition == currentLevel.GroupPosition) // same group
                    );
            }
            else
            {
                var nextlevel = context.tbl_Approval_Level.Find(this.nextLevelId);
                if (nextlevel != null)
                {
                    next = new WorkflowSetup();
                    next.CanRecieveSMS = nextlevel.CanRecieveSMS;
                    next.CanRecieveEmail = nextlevel.CanRecieveEmail;
                    next.ApprovalLevelId = nextlevel.ApprovalLevelId;
                    next.RouteViaStaffOrganogram = nextlevel.RouteViaStaffOrganogram;
                }
            }

            if (next == null) // end of process
            {
                this.nextLevelId = null;
                return true;
            }
            else
            {
                this.smsNotification = next.CanRecieveSMS;
                this.emailNotification = next.CanRecieveEmail;
                this.nextLevelId = next.ApprovalLevelId;
                this.useOrganogram = next.RouteViaStaffOrganogram;
            }
            return true;
        }

        private bool ResolveLevelMultipleApproval()
        {
            var votes = context.tbl_Approval_Trail.Where(x =>
                x.OperationId == this.operationId
                && x.TargetId == this.targetId
                && x.ApprovalStateId != (int)ApprovalState.Ended
                && x.FromApprovalLevelId == this.fromLevelId
                );

            bool allVoted = false;
            if ((votes.Count() + 1) == this.neededNumberOfApproval)
            {
                allVoted = true;
            }

            if (allVoted)
            {
                int approvals = votes.Where(x => x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved).Count();
                int disapprovals = votes.Where(x => x.ApprovalStatusId == (int)ApprovalStatusEnum.Disapproved).Count();

                int vetoVote = 0;

                var vetoer = context.tbl_Approval_Level_Staff.FirstOrDefault(x => x.VetoPower == true);
                if (vetoer != null)
                {
                    if (vetoer.StaffId == this.staffId)
                    {
                        vetoVote = this.statusId;
                    }
                    else
                    {
                        var vetoerTrail = votes.FirstOrDefault(x => x.RequestStaffId == vetoer.StaffId);
                        if (vetoerTrail != null)
                        {
                            vetoVote = vetoerTrail.ApprovalStatusId;
                        }
                    }
                }

                if (approvals > disapprovals)
                {
                    if (vetoVote == 0 || vetoVote == (int)ApprovalStatusEnum.Approved)
                    {
                        EndProcess((int)ApprovalStatusEnum.Approved);
                        return true;
                    }
                }

                if (approvals < disapprovals)
                {
                    if (vetoVote == 0 || vetoVote == (int)ApprovalStatusEnum.Disapproved)
                    {
                        EndProcess((int)ApprovalStatusEnum.Disapproved);
                        return true;
                    }
                }
            }

            ContinueProcess(this.statusId);
            return true;
        }

        private void ContinueProcess(int status)
        {
            this.statusId = status;
            this.newStateId = (int)ApprovalState.Processing;
        }

        private void EndProcess(int status)
        {
            this.statusId = status;
            this.newStateId = (int)ApprovalState.Ended;
            this.nextLevelId = null; // even if there are other higher level which have been resolve prior
        }

        private bool Validation()
        {
            if (this.staffId > 0 && this.operationId > 0 && this.targetId > 0 && this.companyId > 0 && this.statusId >= 0)
            {
                if (this.nextLevelId < 1) { this.nextLevelId = null; }
                return true;
            }
            this.message = "Invalid call!";
            return false;
        }

        private bool OrganogramRouting()
        {
            var position = context.tbl_Staff_Organogram.Where(x => x.StaffId == this.staffId).FirstOrDefault();
            if (position == null) { return false; }
            var lineManagerPosition = context.tbl_Staff_Organogram.Where(x => x.StaffCode == position.ParentStaffCode).FirstOrDefault();
            if (lineManagerPosition == null) { return false; }
            var lineManagerLevelId = GetStaffApprovalLevelId(lineManagerPosition.StaffId);
            if (lineManagerLevelId == null) { return false; }
            this.nextLevelId = lineManagerLevelId;
            return true;
        }

        private int? GetStaffApprovalLevelId(int staffId)
        {
            var levelStaff = context.tbl_Approval_Group_Mapping.Where(x => x.Deleted == false
                                && x.OperationId == this.operationId
                                && x.ProductClassId == this.productClassId
                                && x.ProductId == this.productId
                            )
                            .SelectMany(x => x.tbl_Approval_Group.tbl_Approval_Level).Where(x => x.IsActive == true)
                            .OrderBy(x => x.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().Position)
                            .ThenBy(x => x.Position)
                            .SelectMany(x => x.tbl_Approval_Level_Staff).Where(x => x.StaffId == staffId).FirstOrDefault();

            if (levelStaff == null)
            {
                this.message = "Staff do not exist in the current process flow!";
                return null;
            }

            return levelStaff.ApprovalLevelId;
        }

        private void CheckApprovalLimits()
        {
            if (this.nextLevelId != null && this.amount > 0 && ActionIsApprovalDecision())
            {
                if (WithinAllLimits() == true)
                {
                    this.EndProcess((int)ApprovalStatusEnum.Approved);
                } else
                {
                    this.statusId = (int)ApprovalStatusEnum.Authorised;
                }
            }
        }

        private bool WithinTenorLimit(WorkflowSetup setup)
        {
            if (tenor == 0 && setup.Tenor == 0) { return true; }
            if (tenor > 0 && setup.Tenor >= tenor) { return true; }
            return false;
        }

        private bool WithinMaximumLimit(WorkflowSetup setup)
        {
            if (investmentGrade == true) { return true; }
            if (amount == 0) { return true; }
            if (setup.MaximumAmount >= amount) { return true; }
            return false;
        }

        private bool WithinInvestmentGradeLimit(WorkflowSetup setup)
        {
            if (investmentGrade == false) { return true; }
            if (amount == 0) { return true; }
            if (setup.InvestmentGradeAmount >= amount) { return true; }
            return false;
        }

        private bool WithinPoliticallyExposedLimit(WorkflowSetup setup)
        {
            if (politicallyExposed == false) { return true; }
            if (setup.IsPoliticallyExposed == true) { return true; }
            return false;
        }

        private bool WithinAllLimits()
        {
            var setup = GetWorkflowSetup(this.operationId, this.productClassId, this.productId);

            var level = setup.FirstOrDefault(x=>x.Staff.First().StaffId == staffId);

            return WithinTenorLimit(level) == true 
                && WithinMaximumLimit(level) == true 
                && WithinInvestmentGradeLimit(level) == true
                && WithinPoliticallyExposedLimit(level) == true;
        }

        private void SetState()
        {
            if (this.nextLevelId == null && ActionIsApprovalDecision())
            {
                this.newStateId = (int)ApprovalState.Ended;
            }
        }

        private bool ActionIsApprovalDecision()
        {
            return (this.statusId == (int)ApprovalStatusEnum.Approved || this.statusId == (int)ApprovalStatusEnum.Disapproved);
        }

        private IEnumerable<WorkflowSetup> GetWorkflowSetup(int operationId, int? productClassId, int? productId)
        {
            var mappings = context.tbl_Approval_Group_Mapping.Where(x => x.Deleted == false
                               && x.OperationId == this.operationId
                               && x.ProductClassId == this.productClassId
                               && x.ProductId == this.productId
                           );

            if (mappings.Any() == false)
            {
                mappings = context.tbl_Approval_Group_Mapping.Where(x => x.Deleted == false
                               && x.OperationId == this.operationId
                               && x.ProductClassId == this.productClassId
                           );
            }

            var approvalLevels = mappings
                           .Join(context.tbl_Approval_Group, m => m.GroupId, g => g.GroupId, (m, g) => new { m, g })
                           .Join(context.tbl_Approval_Level, mg => mg.m.GroupId, l => l.GroupId, (mg, l) =>
                           new { Mapping = mg.m, Level = l })
                           .Where(x => x.Level.IsActive == true)
                           .Select(x => new WorkflowSetup
                           {
                               GroupPosition = x.Mapping.Position,
                               LevelPosition = x.Level.Position,
                               Staff = x.Level.tbl_Approval_Level_Staff,
                               Level = x.Level,
                               Group = x.Level.tbl_Approval_Group,
                               Mapping = x.Mapping,
                               CanRecieveSMS = x.Level.CanRecieveSMS,
                               CanRecieveEmail = x.Level.CanRecieveEmail,
                               ApprovalLevelId = x.Level.ApprovalLevelId,
                               RouteViaStaffOrganogram = x.Level.RouteViaStaffOrganogram,
                           })
                           .OrderBy(x => x.GroupPosition)
                           .ThenBy(x => x.LevelPosition);

            this.workflowSetup = approvalLevels.ToList();

            return this.workflowSetup;
        }

        private void SendNotifications() // TODO
        {
            if (emailNotification)
            {
                //send(email);
            }

            if (smsNotification)
            {
                //send(sms)
            }
        }

        private bool Authorization() // TODO: intended to manage delegated staff actions
        {
            if (this.staffId > 0) // <---- mockup
            {
                return true;
            }
            this.message = "Unauthorized action!";
            return false;
        }

        public bool LogForApproval(ApprovalViewModel model)
        {
            StaffId = model.staffId;
            OperationId = model.operationId;
            TargetId = model.targetId;
            CompanyId = model.companyId;
            Comment = model.comment;
            ExternalInitialization = model.externalInitialization;
            StatusId = model.approvalStatusId;

            var response = LogActivity();

            return response;
        }
    }

    public class WorkflowSetup
    {
        public IEnumerable<tbl_Approval_Level_Staff> Staff { get; set; }
        public tbl_Approval_Level Level { get; set; }
        public tbl_Approval_Group Group { get; set; }
        public tbl_Approval_Group_Mapping Mapping { get; set; }

        public int GroupPosition { get; set; }

        public int LevelPosition { get; set; }

        public int ApprovalLevelId { get; set; }

        public int NumberOfUsers { get; set; }

        public int NumberOfApprovals { get; set; }

        public bool CanRouteBack { get; set; }

        public bool IsPoliticallyExposed { get; set; }

        public bool IsActive { get; set; }

        public bool CanEdit { get; set; }

        public bool CanRecieveEmail { get; set; }

        public bool CanRecieveSMS { get; set; }

        public bool RouteViaStaffOrganogram { get; set; }

        public int? Tenor { get; set; }

        public decimal MaximumAmount { get; set; }

        public decimal? InvestmentGradeAmount { get; set; }
    }
}
