using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FintrakBanking.Repositories.WorkFlow
{
    public class Workflow : IWorkflow
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private readonly string support = ConfigurationManager.AppSettings["SupportEmailAddr"];

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
        private int groupStatusId = (int)ApprovalStatusEnum.Processing;
        private int? nextLevelId = null; // for refer backs
        //private int? minimumLevelId = null; // for dispute resolution
        private bool emailNotification = false;
        private bool smsNotification = false;

        //private string message;
        private int? fromLevelId = null;
        private int? requestLevelId = null;
        private int currentStateId;
        private int newStateId = (int)ApprovalState.Processing;
        private int tenor = 0;
        private decimal amount = 0;
        private bool investmentGrade = false;
        private bool untenored = false;
        private bool disputed = false;
        private bool saved = false;
        private bool useOrganogram = false;
        private DateTime systemDate = DateTime.Now;
        private DateTime applicationDate;
        private int requestStaffId;
        private int neededNumberOfApproval;
        private bool externalInitialization = false;
        private bool keepPending = false;
        private bool politicallyExposed = false;
        private bool deferredExecution = false;
        private short? vote = null;
        private int? toStaffId = null;
        private bool endProcess = false;

        public int StaffId { set { staffId = value; } }
        public int? ToStaffId { set { toStaffId = value; } }
        public int TargetId { set { targetId = value; } }
        public int CompanyId { set { companyId = value; } }
        public int OperationId { set { operationId = value; } }
        public decimal Amount { set { amount = value; } }
        public string Comment { set { comment = value; } }
        public int Tenor { set { tenor = value; } }
        public bool InvestmentGrade { set { investmentGrade = value; } }
        public bool Untenored { set { untenored = value; } }
        public bool Disputed { set { disputed = value; } }
        //public int? MinimumLevelId { set { minimumLevelId = value; } }
        public bool PoliticallyExposed { set { politicallyExposed = value; } }
        public short? Vote { set { vote = value; } }
        public int StatusId { get { return statusId; } set { statusId = value; } }
        public int GroupStatusId { get { return groupStatusId; } }
        public int? NextLevelId { get { return nextLevelId; } set { nextLevelId = value; } }
        public int? ProductId { set { productId = value; } }
        public int? ProductClassId { set { productClassId = value; } }
        public bool EmailNotification { set { emailNotification = value; } }
        public bool SmsNotification { set { smsNotification = value; } }
        public bool ExternalInitialization { set { externalInitialization = value; } }
        //public string Message { get { return message; } }
        public bool Saved { get { return saved; } }
        public int NewState { get { return newStateId; } }
        public bool KeepPending { set { keepPending = value; } }
        public bool DeferredExecution { set { deferredExecution = value; } }
        public bool ForcefullyEndProcess { set { endProcess = value; keepPending = false; } } // <----------- this property is deprecated!!!

        private List<WorkflowSetup> workflowSetup;
        private WorkflowSetup level;
        private WorkflowSetup next;
        private List<TBL_APPROVAL_TRAIL> trailLog;
        private bool skipLimitsCheck = false;
        private IEnumerable<WorkflowSetup> approvalGrid;

        public bool LogActivity()
        {
            ValidateCall();
            InitializeOperation();
            if (Authorization() == false) { return false; }

            this.trailLog = context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == this.companyId
                                && x.OPERATIONID == this.operationId
                                && x.TARGETID == this.targetId
                                && x.RESPONSESTAFFID == null
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                            ).ToList();

            var request = trailLog.OrderByDescending(x => x.APPROVALTRAILID).FirstOrDefault();

            if (request == null)
            {
                if (ActionIsApprovalDecision())
                {
                    throw new Exception("Unable to resolve initiating level or the process is closed!");
                }
                this.currentStateId = (int)ApprovalState.Initiation;
            }
            else
            {
                this.currentStateId = request.APPROVALSTATEID;
                this.requestStaffId = request.REQUESTSTAFFID;
                //if (LastActionIsByStaff()) { throw new Exception("Last action is by staff!!"); }this.requestLevelId = request.FROMAPPROVALLEVELID;
                this.fromLevelId = request.TOAPPROVALLEVELID;
                this.requestLevelId = request.FROMAPPROVALLEVELID;
                if (this.statusId == (int)ApprovalStatusEnum.Reroute && request.REQUESTSTAFFID == this.staffId) { this.fromLevelId = request.FROMAPPROVALLEVELID; }
                if (request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred) { ResolveReferred(request.REQUESTSTAFFID, request.FROMAPPROVALLEVELID, request.TOAPPROVALLEVELID); }
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

            SetReroute();

            this.applicationDate = GetApplicationDate();

            if (request != null)
            {
                request.RESPONSEDATE = this.applicationDate;
                request.SYSTEMRESPONSEDATETIME = this.systemDate;
                request.RESPONSESTAFFID = this.staffId;
            }

            this.SendNotifications();

            if (this.comment == "flow_test") { throw new Exception("flow_test: STATE: " + this.newStateId + ", STATUS:" + this.statusId + ", CURRL:" + this.fromLevelId + ", NEXTL:" + this.nextLevelId + ", TO:" + this.toStaffId); }

            var trail = new TBL_APPROVAL_TRAIL
            {
                FROMAPPROVALLEVELID = this.fromLevelId,
                TOAPPROVALLEVELID = this.nextLevelId,
                TARGETID = this.targetId,
                COMPANYID = this.companyId,
                REQUESTSTAFFID = this.staffId,
                OPERATIONID = this.operationId,
                COMMENT = this.comment,
                ARRIVALDATE = this.applicationDate,
                APPROVALSTATEID = (short)this.newStateId,
                APPROVALSTATUSID = (short)this.statusId,
                SYSTEMARRIVALDATETIME = this.systemDate,
                VOTE = this.vote,
                TOSTAFFID = this.toStaffId,
            };

            context.TBL_APPROVAL_TRAIL.Add(trail);

            if (this.deferredExecution) { return true; }
            this.saved = context.SaveChanges() > 0;
            if (this.saved) return true;

            throw new Exception("Unknown Process Flow Error! Unable to save workflow records!");
        }

        private void InitializeOperation()
        {
            // CAREFUL NOT TO OVERRIDE SUPPLIED values!!!!!!!!
            // set those before calling in
            this.skipLimitsCheck = false;
            this.fromLevelId = null;
            this.newStateId = (int)ApprovalState.Processing;
            if (this.statusId == (int)ApprovalStatusEnum.Pending) this.statusId = (int)ApprovalStatusEnum.Processing;
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

        private void ResolveReferred(int referrerId, int? fromId, int? toId)
        {
            var referrerGroup = context.TBL_APPROVAL_LEVEL.Find(fromId);
            var recepientGroup = context.TBL_APPROVAL_LEVEL.Find(toId);
            if (referrerGroup.GROUPID != recepientGroup.GROUPID)
            {
                this.toStaffId = referrerId;
                this.nextLevelId = fromId;
            }
        }

        private bool ResolveLevelConfigurations() // REFACTOR!!!
        {
            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId);
            approvalGrid = approvalLevels;
            next = approvalLevels.FirstOrDefault();
            if (this.nextLevelId != null) next = approvalLevels.FirstOrDefault(x => x.ApprovalLevelId == (int)this.nextLevelId);

            if (this.externalInitialization == true && this.currentStateId == (int)ApprovalState.Initiation)
            {

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
                var staff = level.Staff.Where(x => x.STAFFID == this.staffId);
                if (staff.Any() == false)
                {
                    throw new Exception("This User is not in the current workflow level of the process!");
                }
                this.neededNumberOfApproval = level.NumberOfApprovals;
            }

            if (this.fromLevelId == null) // && externalInitialization == false
            {
                var levelStaff = approvalLevels.SelectMany(x => x.Staff).Where(x => x.STAFFID == this.staffId).FirstOrDefault(); // doing
                if (levelStaff == null)
                {
                    throw new Exception("Unable to resolve initiating level OR there may be no setup for this operation!");
                }
                this.fromLevelId = levelStaff.APPROVALLEVELID;
                this.neededNumberOfApproval = levelStaff.TBL_APPROVAL_LEVEL.NUMBEROFAPPROVALS;
            }

            if (this.statusId == (int)ApprovalStatusEnum.Referred && this.nextLevelId == null) { this.nextLevelId = this.requestLevelId; } // default return back to sender

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
                var nextlevel = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
                if (nextlevel != null)
                {
                    next = new WorkflowSetup();
                    next.CanRecieveSMS = nextlevel.CANRECIEVESMS;
                    next.CanRecieveEmail = nextlevel.CANRECIEVEEMAIL;
                    next.ApprovalLevelId = nextlevel.APPROVALLEVELID;
                    next.RouteViaStaffOrganogram = nextlevel.ROUTEVIASTAFFORGANOGRAM;
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
            var votes = context.TBL_APPROVAL_TRAIL.Where(x =>
                x.OPERATIONID == this.operationId
                && x.TARGETID == this.targetId
                && x.APPROVALSTATEID != (int)ApprovalState.Ended
                && x.FROMAPPROVALLEVELID == this.fromLevelId
                )
                .ToList();

            if (votes.FirstOrDefault(x => x.REQUESTSTAFFID == (int)this.staffId) != null) throw new Exception("You have already acted on this item.");

            // APPROVING ORDER VALIDATION
            var approvers = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.APPROVALLEVELID == fromLevelId).ToList();
            var current = approvers.FirstOrDefault(x => x.STAFFID == this.staffId);
            if (current != null)
            {
                var subs = approvers.Where(x => x.POSITION == (current.POSITION - 1)).ToList();

                var first = context.TBL_APPROVAL_LEVEL_STAFF
                    .Where(x => x.APPROVALLEVELID == fromLevelId && x.STAFFID != this.staffId && x.POSITION < current.POSITION)
                    .Select(x => x.STAFFID);//.ToList();
                //if (first.Count() > 0)
                if (subs.Count() > 0)
                {
                    bool allow = false;
                    string message = "You are not next in line for approval on this approval level. You will be notified by email when required.";
                    if (votes.Count() == 0) throw new Exception(message);
                    foreach (var vote in votes)
                    {
                        //if (!first.Contains((int)vote.REQUESTSTAFFID))
                        if (subs.Where(x => x.STAFFID == vote.REQUESTSTAFFID).Any()) allow = true;
                    }
                    if (allow == false) throw new Exception(message);
                }
            }

            bool allVoted = false;
            if ((votes.Count() + 1) == this.neededNumberOfApproval)
            {
                // this.skipLimitsCheck = true; // COMMENT OUT IF COMMITTEE IS AFFECTED BY LIMITS!!!!
                allVoted = true;
            }
            else
            {
                this.skipLimitsCheck = true; // avoid approval state changed to 4.processing
                this.smsNotification = false;
                this.emailNotification = false;
                this.nextLevelId = this.fromLevelId;
            }

            if (allVoted)
            {
                int approvals = votes.Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count();
                int disapprovals = votes.Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved).Count();
                approvals = (this.statusId == (int)ApprovalStatusEnum.Approved) ? approvals + 1 : approvals;
                disapprovals = (this.statusId == (int)ApprovalStatusEnum.Disapproved) ? disapprovals + 1 : disapprovals;

                if (approvals != disapprovals)
                {
                    int vetoVote = 0;
                    int voteResult = 0;

                    voteResult = (approvals > disapprovals) ? (int)ApprovalStatusEnum.Approved : (int)ApprovalStatusEnum.Disapproved;
                    this.groupStatusId = (approvals > disapprovals) ? (int)ApprovalStatusEnum.Approved : (int)ApprovalStatusEnum.Disapproved;

                    var vetoers = context.TBL_APPROVAL_LEVEL_STAFF
                                    .Where(x => x.APPROVALLEVELID == this.fromLevelId && x.VETOPOWER == true)
                                    .Select(x => x.STAFFID)
                                    .ToList();

                    vetoVote = votes.Where(x => x.APPROVALSTATUSID != voteResult && vetoers.Contains(x.REQUESTSTAFFID)).Count();

                    if (vetoers.Contains(this.staffId) && voteResult != this.statusId) { vetoVote = vetoVote + 1; } // for current process not yet saved in trail

                    if (vetoVote == 0)
                    {
                        if (this.skipLimitsCheck == true)
                            EndProcess(voteResult);
                        else
                            this.statusId = voteResult;

                        return true;
                    }
                }
            }

            ContinueProcess(this.statusId);
            return true;
        }

        private void ContinueProcess(int status)
        {
            this.statusId = status == (int)ApprovalStatusEnum.Disapproved ? (int)ApprovalStatusEnum.Processing : (int)ApprovalStatusEnum.Authorised;
            this.newStateId = (int)ApprovalState.Processing;
        }

        private void EndProcess(int status)
        {
            this.statusId = status;
            this.newStateId = (int)ApprovalState.Ended;
            this.nextLevelId = null; // even if there are other higher level which have been resolve prior
            this.keepPending = false;
        }

        private void ValidateCall()
        {
            if (this.staffId <= 0) throw new Exception("Invalid Call! staffid cannot be " + this.staffId);
            if (this.operationId <= 0) throw new Exception("Invalid Call! operationId cannot be " + this.operationId);
            if (this.targetId <= 0) throw new Exception("Invalid Call! targetId cannot be " + this.targetId);
            if (this.companyId <= 0) throw new Exception("Invalid Call! companyId cannot be " + this.companyId);
            if (this.statusId < 0) throw new Exception("Invalid Call! statusId cannot be " + this.statusId);
            if (this.nextLevelId < 1) { this.nextLevelId = null; }
        }

        private bool OrganogramRouting() // REDUNDANT
        {
            var position = context.TBL_STAFF_ORGANOGRAM.Where(x => x.STAFFID == this.staffId).FirstOrDefault();
            if (position == null) { return false; }
            var lineManagerPosition = context.TBL_STAFF_ORGANOGRAM.Where(x => x.STAFFCODE == position.PARENTSTAFFCODE).FirstOrDefault();
            if (lineManagerPosition == null) { return false; }
            var lineManagerLevelId = GetStaffApprovalLevelId(lineManagerPosition.STAFFID);
            if (lineManagerLevelId == null) { return false; }
            this.nextLevelId = lineManagerLevelId;
            return true;
        }

        private int? GetStaffApprovalLevelId(int staffId) // REDUNDANT
        {
            var levelStaff = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                                && x.OPERATIONID == this.operationId
                                && x.PRODUCTCLASSID == this.productClassId
                                && x.PRODUCTID == this.productId
                            )
                            .SelectMany(x => x.TBL_APPROVAL_GROUP.TBL_APPROVAL_LEVEL).Where(x => x.ISACTIVE == true)
                            .OrderBy(x => x.TBL_APPROVAL_GROUP.TBL_APPROVAL_GROUP_MAPPING.FirstOrDefault().POSITION)
                            .ThenBy(x => x.POSITION)
                            .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId).FirstOrDefault();

            if (levelStaff == null)
            {
                throw new Exception("Staff do not exist in the current process flow!");
            }

            return levelStaff.APPROVALLEVELID;
        }

        private void CheckApprovalLimits()
        {
            if (this.skipLimitsCheck == true) { return; }
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

        private bool WithinTenorLimit(TBL_APPROVAL_LEVEL level)
        {
            if (this.untenored == true) { return level.CANAPPROVEUNTENORED == true ? true : false; } 
            if (tenor == 0 && level.TENOR == 0) { return true; } // setup
            if (tenor == 0 && level.TENOR == null) { return true; } // setup
            if (tenor > 0 && level.TENOR >= tenor) { return true; } // gen cam
            return false;
        }
        
        private bool WithinMaximumLimit(TBL_APPROVAL_LEVEL level)
        {
            if (amount == 0) { return true; }
            if (investmentGrade == true) { return true; }
            if (level.MAXIMUMAMOUNT >= amount) { return true; }
            return false;
        }

        private bool WithinInvestmentGradeLimit(TBL_APPROVAL_LEVEL level)
        {
            if (amount == 0) { return true; }
            if (investmentGrade == false) { return true; }
            if (level.INVESTMENTGRADEAMOUNT >= amount) { return true; }
            return false;
        }

        private bool WithinPoliticallyExposedLimit(TBL_APPROVAL_LEVEL level)
        {
            if (politicallyExposed == false) { return true; }
            if (level.ISPOLITICALLYEXPOSED == true) { return true; }
            return false;
        }

        private bool WithinAllLimits()
        {
            var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
            if (level == null) { throw new Exception("The user is not in the workflow setup!"); } // redundant - wouldnt get here in the first place
            if (this.disputed == true && level.CANRESOLVEDISPUTE != true) { return false; }

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

            if (this.nextLevelId == null && this.amount == 0)
            {
                this.EndProcess((int)ApprovalStatusEnum.Approved);
            }

            if (this.keepPending == true)
            {
                this.statusId = (int)ApprovalStatusEnum.Pending;
                this.newStateId = (int)ApprovalState.Processing;
            }

            if (this.endProcess == true) // PENDING UPDATE (To forcefully end the process at a particular level)
            {
                this.statusId = (int)ApprovalStatusEnum.Approved;
                this.EndProcess(this.statusId);
            }

            if (this.nextLevelId != null && this.statusId == (int)ApprovalStatusEnum.Escalated)
            {
                this.ContinueProcess((int)ApprovalStatusEnum.Processing);
            }
        }

        private void SetReroute()
        {
            // if (this.statusId == (int)ApprovalStatusEnum.Reroute) { this.nextLevelId = this.fromLevelId; }
            if (this.statusId == (int)ApprovalStatusEnum.Reroute) { this.statusId = (int)ApprovalStatusEnum.Processing; }
        }

        private bool ActionIsApprovalDecision()
        {
            return (this.statusId == (int)ApprovalStatusEnum.Approved || this.statusId == (int)ApprovalStatusEnum.Disapproved);
        }

        private IEnumerable<WorkflowSetup> GetWorkflowSetup(int operationId, int? productClassId, int? productId)
        {
            var mappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == this.operationId
                               && x.PRODUCTCLASSID == this.productClassId
                               && x.PRODUCTID == this.productId
                           );

            if (mappings.Any() == false)
            {
                mappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == this.operationId
                               && x.PRODUCTCLASSID == this.productClassId
                           );
            }

            if (mappings.Any() == false)
            {
                var operarion = context.TBL_OPERATIONS.Find(operationId);
                var productclass = "NULL";
                if (productClassId != null)
                {
                    var productClass = context.TBL_PRODUCT_CLASS.Find(productClassId);
                    productclass = productClass.PRODUCTCLASSNAME;
                }
                throw new Exception("There is no approval workflow setup for the OPERATION: " + operarion.OPERATIONNAME + ", PRODUCT CLASS: " + productclass);
            }

            var approvalLevels = mappings
                           .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                           .Join(context.TBL_APPROVAL_LEVEL, mg => mg.m.GROUPID, l => l.GROUPID, (mg, l) =>
                           new { Mapping = mg.m, Level = l })
                           .Where(x => x.Level.ISACTIVE == true)
                           .Select(x => new WorkflowSetup
                           {
                               GroupPosition = x.Mapping.POSITION,
                               LevelPosition = x.Level.POSITION,
                               Staff = x.Level.TBL_APPROVAL_LEVEL_STAFF,
                               Level = x.Level,
                               NumberOfApprovals = x.Level.NUMBEROFAPPROVALS,
                               MaximumAmount = x.Level.MAXIMUMAMOUNT,
                               NumberOfUsers = x.Level.NUMBEROFUSERS,
                               Tenor = x.Level.TENOR,
                               InvestmentGradeAmount = x.Level.INVESTMENTGRADEAMOUNT,
                               IsPoliticallyExposed = x.Level.ISPOLITICALLYEXPOSED,
                               IsActive = x.Level.ISACTIVE,
                               Group = x.Level.TBL_APPROVAL_GROUP,
                               Mapping = x.Mapping,
                               CanRecieveSMS = x.Level.CANRECIEVESMS,
                               CanRecieveEmail = x.Level.CANRECIEVEEMAIL,
                               ApprovalLevelId = x.Level.APPROVALLEVELID,
                               RouteViaStaffOrganogram = x.Level.ROUTEVIASTAFFORGANOGRAM,
                           })
                           .OrderBy(x => x.GroupPosition)
                           .ThenBy(x => x.LevelPosition);

            this.workflowSetup = approvalLevels.ToList();

            return this.workflowSetup;
        }

        private void SendNotifications()
        {
            if (emailNotification || smsNotification)
            {
                var operation = context.TBL_OPERATIONS.Find(this.operationId);
                var trails = trailLog.OrderBy(x => x.APPROVALTRAILID);
                var owner = context.TBL_STAFF.Find(trails.First().REQUESTSTAFFID);

                int targetId = this.targetId;
                int operationId = this.operationId;
                var message = new TBL_MESSAGE_LOG();
                var reciever = new TBL_STAFF();
                string recipientName = "All";
                string operationName = operation == null ? "N/A" : operation.OPERATIONNAME;
                string messageSubject = "PENDING APPROVAL FOR " + operationName.ToUpper();
                string status = GetApprovalStatusName(this.statusId);
                string ownerMessageSubject = "YOUR INITIATED " + operationName.ToUpper() + " PROCESS HAVE BEEN " + status.ToUpper();
                string link = "";
                var level = string.Empty;
                string[] emails = new string[100];

                if (this.fromLevelId != null) level = " by " + context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId)?.LEVELNAME; 
                if (this.nextLevelId != null && this.toStaffId == null) recipientName = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId)?.LEVELNAME;

                if (this.toStaffId != null)
                {
                    reciever = context.TBL_STAFF.Find(this.toStaffId);
                    recipientName = reciever.FIRSTNAME;
                }
                else
                {
                    emails = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == this.nextLevelId)
                        .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF)
                        .Select(x => x.TBL_STAFF.EMAIL)
                        .Distinct()
                        .ToArray();
                }

                var ownerMessageBody = $"Dear {owner.FIRSTNAME}, <br /><br />" +
                            $"The {operationName} approval process you initiated have been {status}{level}. <br /><br />" +
                            $"See details here {link}";

                var messageBody = $"Dear {recipientName}, <br /><br />" +
                            $"You have a new pending {operationName} approval request. <br /><br />" +
                            $"See details here {link}";

                //var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                if (emailNotification)
                {
                    message = new TBL_MESSAGE_LOG // INITIATOR
                    {
                        TOADDRESS = owner.EMAIL,
                        MESSAGESUBJECT = ownerMessageSubject,
                        MESSAGEBODY = ownerMessageBody,
                        MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                        MESSAGETYPEID = (short)MessageTypeEnum.Email,
                        FROMADDRESS = this.support,
                        DATETIMERECEIVED = DateTime.Now,
                        SENDONDATETIME = DateTime.Now,
                        TARGETID = targetId,
                        OPERATIONID = operationId
                    };
                    context.TBL_MESSAGE_LOG.Add(message);

                    message = new TBL_MESSAGE_LOG // RECIEVERS
                    {
                        TOADDRESS = this.toStaffId != null ? reciever.EMAIL : string.Join(";", emails),
                        MESSAGESUBJECT = messageSubject,
                        MESSAGEBODY = messageBody,
                        MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                        MESSAGETYPEID = (short)MessageTypeEnum.Email,
                        FROMADDRESS = this.support,
                        DATETIMERECEIVED = DateTime.Now,
                        SENDONDATETIME = DateTime.Now,
                        TARGETID = targetId,
                        OPERATIONID = operationId
                    };
                    context.TBL_MESSAGE_LOG.Add(message);
                }
            }
        }

        private string GetApprovalStatusName(int statusId)
        {
            switch (statusId)
            {
                case 0: return "initiated";
                case 1: return "forwarded";
                case 2: return "approved";
                case 3: return "disapproved";
                case 4: return "authorised";
                case 5: return "referred";
                case 6: return "rerouted";
                case 7: return "escalated";
                default: break;
            }
            return "forwarded";
        }
        
        private bool Authorization() // TODO: intended to manage delegated staff actions
        {

            if (this.staffId > 0) // <---- mockup
            {
                return true;
            }
            throw new Exception("Unauthorized action!");
        }

        public bool LogForApproval(ApprovalViewModel model) // <----------- this method is deprecated!!!
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

        public TBL_APPROVAL_LEVEL Level { get; set; }

        public TBL_APPROVAL_GROUP Group { get; set; }

        public TBL_APPROVAL_GROUP_MAPPING Mapping { get; set; }

        public IEnumerable<TBL_APPROVAL_LEVEL_STAFF> Staff { get; set; }
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
