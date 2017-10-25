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
        private bool keepPending = false;
        private bool vote = false;
        private bool politicallyExposed = false;
        private bool deferredExecution = false;

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
        public int? NextLevelId { get { return nextLevelId; } set { nextLevelId = value; } }
        public int? ProductId { set { productId = value; } }
        public int? ProductClassId { set { productClassId = value; } }
        public bool EmailNotification { set { emailNotification = value; } }
        public bool SmsNotification { set { smsNotification = value; } }
        public bool ExternalInitialization { set { externalInitialization = value; } }
        public string Message { get { return message; } }
        public bool Saved { get { return saved; } }
        public int NewState { get { return newStateId; } }
        public bool KeepPending { set { keepPending = value; } }
        public bool DeferredExecution { set { deferredExecution = value; } }

        private List<WorkflowSetup> workflowSetup;
        private WorkflowSetup level;
        private WorkflowSetup next;

        public bool LogActivity()
        {
            if (Validation() == false) { return false; }
            if (Authorization() == false) { return false; }

            var request = context.tbl_Approval_Trail.Where(x =>
                                x.CompanyId == this.companyId
                                && x.OperationId == this.operationId
                                && x.TargetId == this.targetId 
                                && x.ResponseStaffId == null 
                                && (x.ApprovalStateId != (int)ApprovalState.Ended && x.ResponseDate == null)
                            ).OrderByDescending(x => x.ApprovalTrailId).FirstOrDefault();

            if (request == null)
            {
                if (ActionIsApprovalDecision())
                {
                    throw new Exception("Unable to resolve initiating level!");
                }
                this.currentStateId = (int)ApprovalState.Initiation;
            }
            else
            {
                this.currentStateId = request.ApprovalStateId;
                this.requestStaffId = request.RequestStaffId;
                //if (LastActionIsByStaff()) { throw new Exception("Last action is by staff!!"); }
                this.fromLevelId = request.ToApprovalLevelId;
                if (ProcessIsClosed()) { throw new Exception("Process is closed!"); }
            }

            if (ResolveLevelConfigurations() == false) { return false; }

            if (this.useOrganogram == true) { OrganogramRouting(); } // REFACTOR

            if (this.neededNumberOfApproval > 1 && ActionIsApprovalDecision())
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

            if (this.comment == "flow_test") { throw new Exception("flow_test: STATE: " + this.newStateId + ", STATUS:" + this.statusId + ", NEXTL:" + this.nextLevelId); }

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
            
            if (this.deferredExecution) { return true; }

            this.saved = context.SaveChanges() > 0;

            if (this.saved)
            {
                this.SendNotifications();
                this.message = "Workflow process activity log successful!";
                return true;
            }

            throw new Exception("Unable to save record!");
        }

        private DateTime GetApplicationDate()
        {
            return this.general.GetApplicationDate();
        }

        private bool LastActionIsByStaff()
        {
            if (this.staffId == this.requestStaffId)
            {
                throw new Exception("Cannot act on self initiated process!");
            }
            return false;
        }

        private bool ProcessIsClosed()
        {
            if (this.currentStateId == (int)ApprovalState.Ended)
            {
                return true;
            }
            return false;
        }

        private bool ResolveLevelConfigurations()
        {
            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId);

            if (this.externalInitialization == true && this.currentStateId == (int)ApprovalState.Initiation)
            {
                next = approvalLevels.FirstOrDefault();
                if (next != null)
                {
                    this.smsNotification = next.CanRecieveSMS;
                    this.emailNotification = next.CanRecieveEmail;
                    this.nextLevelId = next.ApprovalLevelId;
                    this.useOrganogram = next.RouteViaStaffOrganogram;
                    return true;
                }
                throw new Exception("Unable to resolve initiating level or there is no setup for the specified operation!");
            }

            if (this.fromLevelId != null) // check if staff in level
            {
                level = approvalLevels.Where(x => x.ApprovalLevelId == this.fromLevelId).FirstOrDefault();
                if (level == null)
                {
                    throw new Exception("This Approval Level is not in the workflow setup!");
                }
                var staff = level.Staff.Where(x => x.StaffId == this.staffId);
                if (staff.Any() == false)
                {
                    throw new Exception("This User is not in the workflow setup!");
                }
            }

            if (this.fromLevelId == null) // && externalInitialization == false
            {
                var levelStaff = approvalLevels.SelectMany(x => x.Staff).Where(x => x.StaffId == this.staffId).FirstOrDefault(); // doing
                if (levelStaff == null)
                {
                    throw new Exception("Unable to resolve initiating level. No setup for the specified operation!");
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
            this.statusId = status;// == (int)ApprovalStatusEnum.Disapproved ? (int)ApprovalStatusEnum.Processing : status;
            this.newStateId = (int)ApprovalState.Processing;
        }

        private void EndProcess(int status)
        {
            this.statusId = status;
            this.newStateId = (int)ApprovalState.Ended;
            this.nextLevelId = null; // even if there are other higher level which have been resolve prior
            this.keepPending = false;
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

        private bool OrganogramRouting() // REDUNDANT
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

        private int? GetStaffApprovalLevelId(int staffId) // REDUNDANT
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
                throw new Exception("Staff do not exist in the current process flow!");
            }

            return levelStaff.ApprovalLevelId;
        }

        private void CheckApprovalLimits()
        {
            if (this.nextLevelId != null && this.amount > 0 && ActionIsApprovalDecision())
            {
                if (WithinAllLimits() == true)
                {
                    this.EndProcess(this.statusId);
                }
                else
                {
                    this.ContinueProcess((int)ApprovalStatusEnum.Authorised);
                }
            }
        }

        private bool WithinTenorLimit(tbl_Approval_Level level)
        {
            if (tenor == 0 && level.Tenor == 0) { return true; } // setup
            //if (tenor == 0 && level.Tenor > 0 && level.AuthorizeUntenored == true) { return true; } // untenored for cro
            if (tenor > 0 && level.Tenor >= tenor) { return true; } // gen cam
            return false;
        }

        private bool WithinMaximumLimit(tbl_Approval_Level level)
        {
            if (amount == 0) { return true; }
            if (level.MaximumAmount >= amount) { return true; }
            return false;
        }

        private bool WithinInvestmentGradeLimit(tbl_Approval_Level level)
        {
            if (amount == 0) { return true; }
            if (investmentGrade == false) { return true; }
            if (level.InvestmentGradeAmount >= amount) { return true; }
            return false;
        }

        private bool WithinPoliticallyExposedLimit(tbl_Approval_Level level)
        {
            if (politicallyExposed == false) { return true; }
            if (level.IsPoliticallyExposed == true) { return true; }
            return false;
        }

        private bool WithinAllLimits()
        {
            var level = context.tbl_Approval_Level.Find(this.fromLevelId);

            if (level == null) { throw new Exception("The user is not in the workflow setup!"); } // redundant - wouldnt get here in the first place

            return WithinTenorLimit(level) == true
                && WithinMaximumLimit(level) == true
                && WithinInvestmentGradeLimit(level) == true
                && WithinPoliticallyExposedLimit(level) == true;
        }

        private void SetState()
        {
            if (this.nextLevelId == null && ActionIsApprovalDecision())
            {
                this.EndProcess(this.statusId);
            }

            if (this.keepPending == true)
            {
                this.statusId = (int)ApprovalStatusEnum.Pending;
                this.newStateId = (int)ApprovalState.Processing;
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

            if (mappings.Any() == false)
            {
                throw new Exception("There is no approval workflow setup for the operation");
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
            throw new Exception("Unauthorized action!");
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
            keepPending = model.keepPending;
            deferredExecution = model.deferredExecution;

            var response = LogActivity();

            return response;
        }
    }

    public class WorkflowSetup
    {
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

        public tbl_Approval_Level Level { get; set; }

        public tbl_Approval_Group Group { get; set; }

        public tbl_Approval_Group_Mapping Mapping { get; set; }

        public IEnumerable<tbl_Approval_Level_Staff> Staff { get; set; }
    }
}

/*
Example usage:

    (1)

    Initialisation
    --------------
    workflow.StaffId = model.createdBy;
    workflow.CompanyId = model.companyId;
    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
    workflow.TargetId = collateralMappingId;
    workflow.Comment = "Request for collateral release";
    workflow.OperationId = (int)OperationsEnum.CollateralRelease;
    workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
    workflow.ExternalInitialization = true;
    workflow.LogActivity();

    return context.SaveChanges() > 0;

    Approval
    --------
    workflow.StaffId = model.createdBy;
    workflow.CompanyId = model.companyId;
    workflow.StatusId = (short)entity.approvalStatusId;
    workflow.TargetId = entity.targetId;
    workflow.Comment = entity.comment;
    workflow.OperationId = (int)OperationsEnum.CollateralRelease;
    workflow.DeferredExecution = true;
    workflow.LogActivity();

    return context.SaveChanges() > 0;

    (2)

    // init
    workflow.StaffId = model.createdBy;
    workflow.OperationId = operationId;
    workflow.TargetId = model.applicationId;
    workflow.CompanyId = model.companyId;
    workflow.Vote = model.vote;
    workflow.ProductClassId = model.productClassId;
    workflow.ProductId = model.productId;
    workflow.NextLevelId = model.receiverLevelId;
    workflow.StatusId = model.forwardAction;
    workflow.Comment = model.comment;
    workflow.Amount = model.amount;
    workflow.InvestmentGrade = model.investmentGrade;
    workflow.Tenor = model.tenor;
    workflow.PoliticallyExposed = model.politicallyExposed;

    // log
    workflow.LogActivity();

    if (workflow.Saved)
    {
        // do something
    }

*/
