using FintrakBanking.Common.CustomException;
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
        private WorkflowResponse response = new WorkflowResponse();

        public Workflow(FinTrakBankingContext context, IGeneralSetupRepository general)
        {
            this.context = context;
            this.general = general;
        }

        private int staffId;
        private int targetId;
        private int companyId;
        private int operationId;
        private int? destinationOperationId;
        private bool isFlowTest;
        private int? exclusiveFlowChangeId = null;
        private int? businessUnitId = null;
        private TBL_APPROVAL_TRAIL approvalTrail;
        private int? productClassId = null;
        public int? productId = null;
        
        private string comment = string.Empty;
        private int statusId = (int)ApprovalStatusEnum.Processing;
        private int groupStatusId = (int)ApprovalStatusEnum.Processing;
        private int? nextLevelId = null; // for refer backs
        private int? finalLevel = null; // preset force to end
        private bool emailNotification = false;
        private bool smsNotification = false;
        private bool sameDesk = false;

        private int? fromLevelId = null;
        private int? reliefStaffId = null;
        private int? requestLevelId = null;
        private int currentStateId;
        private int newStateId = (int)ApprovalState.Processing;
        private int? tenor = null;
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
        private bool setResponse = true;
        private bool deferredExecution = false;
        private bool statusOnly = false;
        private short? vote = null;
        private int? toStaffId = null;
        private int? loopedRoleId = null;
        private int? loopedStaffId = null;
        private short? referBackStateId = null;
        public int actualRequestStaffId = 0;
        public bool isLoopResponse = false;
        private bool endProcess = false;
        private AlertPlaceholders placeholders = null;
        private LevelBusinessRule levelBusinessRule = null;
        //private WorkflowResponse response = null;

        private float? interestRateConcession = null;
        private float? feeRateConcession = null;

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
        public bool PoliticallyExposed { set { politicallyExposed = value; } }
        public bool SetResponse { set { setResponse = value; } }
        public short? Vote { set { vote = value; } }

        public float? InterestRateConcession { set { interestRateConcession = value; } }
        public float? FeeRateConcession { set { feeRateConcession = value; } }

        public int StatusId { get { return statusId; } set { statusId = value; } }
        public int GroupStatusId { get { return groupStatusId; } }
        public int? NextLevelId { get { return nextLevelId; } set { nextLevelId = value; } }
        public int? FinalLevel { set { finalLevel = value; } }
        public int? ProductId { set { productId = value; } }
        public TBL_APPROVAL_TRAIL ApprovalTrail { get { return approvalTrail; } set { approvalTrail = value; } }
        public int? ExclusiveFlowChangeId { get { return exclusiveFlowChangeId; } set { exclusiveFlowChangeId = value; } } 
        public int? BusinessUnitId { get { return businessUnitId; } set { businessUnitId = value; } }
        public int? DestinationOperationId { get { return destinationOperationId; } set { destinationOperationId = value; } }
        public bool IsFlowTest { get { return isFlowTest; } set { isFlowTest = value; } }
        public int? LoopedRoleId { get { return loopedRoleId; } set { loopedRoleId = value; } }
        public int? LoopedStaffId { get { return loopedStaffId; } set { loopedStaffId = value; } }

        public int? ProductClassId { set { productClassId = value; } }
        public bool EmailNotification { set { emailNotification = value; } }
        public bool SmsNotification { set { smsNotification = value; } }
        public bool ExternalInitialization { set { externalInitialization = value; } }
        public bool Saved { get { return saved; } }
        public int NewState { get { return newStateId; } }
        public bool KeepPending { set { keepPending = value; } }
        public bool DeferredExecution { set { deferredExecution = value; } }
        public bool StatusOnly { set { statusOnly = value; } }
        public bool ForcefullyEndProcess { set { endProcess = value; keepPending = false; } } // <----------- this property is deprecated!!!
        public LevelBusinessRule LevelBusinessRule { set { levelBusinessRule = value; } }
        public AlertPlaceholders Placeholders { set { placeholders = value; } }
        public WorkflowResponse Response { get { return response; } set { response = value; } }
        public bool isCrossOperationProcess { get; private set; }
        
        

        private List<WorkflowSetup> workflowSetup;
        private WorkflowSetup level;
        private WorkflowSetup next;
        private List<TBL_APPROVAL_TRAIL> trailLog; 
        private List<TBL_APPROVAL_TRAIL> referredLog;
        private TBL_APPROVAL_TRAIL request;
        private bool skipLimitsCheck = false;
        private IEnumerable<WorkflowSetup> approvalGrid;
        private int slaInterval = 780; // 1month
        List<ReportingLine> line = new List<ReportingLine>();

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


            request = trailLog.OrderByDescending(x => x.APPROVALTRAILID).FirstOrDefault();

            this.referredLog = context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == this.companyId
                                && x.OPERATIONID == this.operationId
                                && x.TARGETID == this.targetId
                               // && x.RESPONSESTAFFID != null
                                && x.REFEREBACKSTATEID != (int)ApprovalState.Ended
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                            ).ToList();

            var initiatingRequest = GetAllTrail().OrderByDescending(x => x.APPROVALTRAILID).LastOrDefault();


            if (request == null)
            {
                if (ActionIsApprovalDecision()) throw new SecureException("Unable to resolve initiating level or the process is closed!");
                this.currentStateId = (int)ApprovalState.Initiation;

            }
            else
            {
                this.currentStateId = request.APPROVALSTATEID;
                this.requestStaffId = request.REQUESTSTAFFID;
                this.fromLevelId = request.TOAPPROVALLEVELID;
                this.requestLevelId = request.FROMAPPROVALLEVELID;
                this.isCrossOperationProcess = request.OPERATIONID != this.operationId;
                this.ResolveExternalFlowLoop(request, initiatingRequest);

                if (this.statusId == (int)ApprovalStatusEnum.Reroute) { this.fromLevelId = ResolveReroute(request.TOSTAFFID); }
                if (request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred) { ResolveReferred(request.REQUESTSTAFFID, request.FROMAPPROVALLEVELID, request.TOAPPROVALLEVELID); }
                if (ProcessIsClosed()) { throw new SecureException("Process is closed!"); }
            }

            if(request !=null) CustomJump(request.TOAPPROVALLEVELID, request.FROMAPPROVALLEVELID);

            if (ResolveLevelConfigurations() == false) { throw new SecureException("Could not resolve approval level configurations!"); }

            // if (next != null && next.LevelTypeId == (int)ApprovalLevelType.SkipLevelByAmount) SkipLevelByAmount();

            if (this.useOrganogram == true) toStaffId = GetReportingLineStaffId();

            if (this.neededNumberOfApproval > 1 && ActionIsApprovalDecision())
            {
                ResolveLevelMultipleApproval();
            }

            ValidateDestinationConfiguration();

            CheckApprovalLimits();

            LastApproverCheck();

            SetState();

            SetReroute();

            this.applicationDate = GetApplicationDate();

            if (request != null)
            {
                request.RESPONSEDATE = this.applicationDate;
                request.SYSTEMRESPONSEDATETIME = this.systemDate;
                request.RESPONSESTAFFID = this.staffId;

                if (request.LOOPEDSTAFFID != null && request.LOOPEDSTAFFID > 0 && request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                { request.RESPONSESTAFFID = !isLoopResponse ? this.staffId : this.actualRequestStaffId; }
            }


            MakerCheckerControl();

            //RandomizeAllocation();

            AllocateBySBU();

            SendNotifications();

            SetResponseInformation();

            if (statusOnly) return true;

            if (this.comment == "flow_test") { throw new SecureException("from (" + this.fromLevelId + ") to (" + this.nextLevelId + "), status: " + response.statusName + ", level: " + response.nextLevelName + ", person: " + response.nextPersonName); }

            if (this.isFlowTest) return true;

            this.approvalTrail = context.TBL_APPROVAL_TRAIL.Add(new TBL_APPROVAL_TRAIL
            {
                FROMAPPROVALLEVELID = this.fromLevelId,
                TOAPPROVALLEVELID = this.nextLevelId,
                TARGETID = this.targetId,
                COMPANYID = this.companyId,
                REQUESTSTAFFID = !isLoopResponse ? this.staffId : this.actualRequestStaffId,
                OPERATIONID = this.operationId,
                COMMENT = this.comment,
                ARRIVALDATE = this.applicationDate,
                APPROVALSTATEID = (short)this.newStateId,
                APPROVALSTATUSID = (short)this.statusId,
                SYSTEMARRIVALDATETIME = this.systemDate,
                SLADATETIME = this.systemDate.AddHours(this.slaInterval),
                VOTE = this.vote,
                TOSTAFFID = this.toStaffId,
                LOOPEDROLEID = this.loopedRoleId,
                LOOPEDSTAFFID = this.loopedStaffId,
                REFEREBACKSTATEID = this.referBackStateId,
                DESTINATIONOPERATIONID = this.destinationOperationId

            });

            if (this.deferredExecution) { return true; }
            this.saved = context.SaveChanges() > 0;
            if (this.saved) return true;

            throw new SecureException("Unknown Process Flow Error! Unable to save workflow records!");
        }

        private void RandomizeAllocation()
        {
            var approvalSetup = context.TBL_APPROVAL_SETUP.FirstOrDefault();
            if(approvalSetup.USEROUNDROBIN == true)
            {
                List<StaffAllocatedjob> staffAllocations = new List<StaffAllocatedjob>();

                if(this.toStaffId != null) { return; }

                if(this.request!= null && this.request?.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && (this.StatusId == (int)ApprovalStatusEnum.Processing || this.StatusId == (int)ApprovalStatusEnum.Pending || this.StatusId == (int)ApprovalStatusEnum.Authorised))
                {
                    var pendingTrail = context.TBL_APPROVAL_TRAIL.Where(x =>
                                   x.COMPANYID == this.companyId
                                   && x.OPERATIONID == this.operationId
                                   && x.RESPONSESTAFFID == null 
                                   && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                                   ).ToList();

                    if(this.businessUnitId != null)
                    {
                        var staffBusinessUnit = context.TBL_PROFILE_BUSINESS_UNIT.Find(this.businessUnitId);
                        if (staffBusinessUnit == null && approvalSetup.ISRETAILONLYROUNDROBIN == true)
                        {
                            if (staffBusinessUnit.BUSINESSCOMMONNAME?.ToLower() != "retail") return;
                        }
                    }

                    var approvalLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == nextLevelId).ToList();
                    var roles = approvalLevel.Select(c => c.STAFFROLEID).ToList();
                    var staffInrole = context.TBL_STAFF.Where(x => roles.Contains(x.STAFFROLEID)).ToList();

                    var approvalStaff = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.APPROVALLEVELID == nextLevelId).Select(d => d.STAFFID).ToList();
                    approvalStaff.AddRange(staffInrole.Select(d => d.STAFFID).ToList());

                    if (!context.TBL_STAFF_ROLE.Where(x => roles.Contains(x.STAFFROLEID) && x.USEROUNDROBIN == true).Any())
                    {
                        return;
                    }

                    foreach (var item in approvalStaff)
                    {
                        if(staffAllocations.Where(x=>x.staffId == item).Count() == 0)
                        {
                            StaffAllocatedjob staffAllocation = new StaffAllocatedjob();
                            staffAllocation.pendingJobCount = pendingTrail.Where(x => x.TOSTAFFID == item).Count();
                            staffAllocation.staffId = item;
                            staffAllocation.counted = true;

                            var isOnRelief = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == item && x.ENDDATE < DateTime.Now).Any();
                            staffAllocation.isOnRelief = isOnRelief;
                            staffAllocations.Add(staffAllocation);
                        }
                    }

                    var orderedAllocation = staffAllocations.Where(x => x.isOnRelief == false && staffInrole.Select(c=>c.STAFFID).Contains(x.staffId)).OrderBy(x=>x.pendingJobCount).FirstOrDefault();
                    if(this.toStaffId == null) { this.toStaffId = orderedAllocation.staffId; }
                }
            }
          
        }

        private void AllocateBySBU()
        {
            List<StaffAllocatedjob> staffAllocations = new List<StaffAllocatedjob>();

            if (this.toStaffId != null) { return; }

            if (this.request != null && this.request?.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && (this.StatusId == (int)ApprovalStatusEnum.Processing || this.StatusId == (int)ApprovalStatusEnum.Pending || this.StatusId == (int)ApprovalStatusEnum.Authorised))
            {
                var pendingTrail = context.TBL_APPROVAL_TRAIL.Where(x =>
                                  x.COMPANYID == this.companyId
                                  && x.OPERATIONID == this.operationId
                                  && x.RESPONSESTAFFID == null
                                  && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                                  ).ToList();

                if (this.businessUnitId != null) return;

                var businessUnit = context.TBL_PROFILE_BUSINESS_UNIT.Find(this.businessUnitId);
                var approvalLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == nextLevelId).ToList();
                var roles = approvalLevel.Select(c => c.STAFFROLEID).ToList();
                var staffInrole = context.TBL_STAFF.Where(x => roles.Contains(x.STAFFROLEID) && (x.BUSINESSUNITID == this.businessUnitId && x.BUSINESSUNITID != null)  && x.MISCODE == businessUnit.BUSINESSUNITINITIALS).ToList();

                var approvalStaff = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.APPROVALLEVELID == nextLevelId).Select(d => d.STAFFID).ToList();
                approvalStaff.AddRange(staffInrole.Select(d => d.STAFFID).ToList());

                if (!context.TBL_STAFF_ROLE.Where(x => roles.Contains(x.STAFFROLEID) && x.USESBUROUTING == true).Any())
                {
                    return;
                }

                foreach (var item in approvalStaff)
                {
                    if (staffAllocations.Where(x => x.staffId == item).Count() == 0)
                    {
                        StaffAllocatedjob staffAllocation = new StaffAllocatedjob();
                        staffAllocation.pendingJobCount = pendingTrail.Where(x => x.TOSTAFFID == item).Count();
                        staffAllocation.staffId = item;
                        staffAllocation.counted = true;

                        var isOnRelief = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == item && x.ENDDATE < DateTime.Now).Any();
                        staffAllocation.isOnRelief = isOnRelief;
                        staffAllocations.Add(staffAllocation);
                    }
                }

                var orderedAllocation = staffAllocations.Where(x => staffInrole.Select(c => c.STAFFID).Contains(x.staffId)).OrderBy(x => x.pendingJobCount).FirstOrDefault();
                if (this.toStaffId == null) { this.toStaffId = orderedAllocation.staffId; }

            }
        }

        private List<TBL_APPROVAL_TRAIL> GetAllTrail()
        {
            return context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == this.companyId
                                && x.OPERATIONID == this.operationId
                                && x.TARGETID == this.targetId
                                //&& x.RESPONSESTAFFID == null
                                //&& (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                            ).ToList();
        }
        private void MakerCheckerControl()
        {
            if (statusId == (int)ApprovalStatusEnum.Approved && newStateId == (int)ApprovalState.Ended)
            {
                var firstRequest = trailLog.OrderBy(x => x.APPROVALTRAILID).FirstOrDefault();
                if (firstRequest.REQUESTSTAFFID == this.staffId) throw new SecureException("You cannot approve a process you initiated!");
            }

            if(this.request != null && this.request.APPROVALSTATUSID != (short)ApprovalStatusEnum.Referred && this.statusId != (short)ApprovalStatusEnum.Referred)
            {
                var currentLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == this.fromLevelId).FirstOrDefault();
                var destinationLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == this.nextLevelId).FirstOrDefault();
                if(this.statusId != (short)ApprovalStatusEnum.Approved && this.newStateId != (short)ApprovalState.Ended)
                {
                    if(currentLevel != null && destinationLevel != null)
                    {
                        if(currentLevel.GROUPID == destinationLevel.GROUPID && currentLevel.POSITION > destinationLevel.POSITION)
                        {
                            throw new SecureException("You cannot move a transaction below the current level.");
                        }
                    }

                    if(!context.TBL_APPROVAL_GROUP_MAPPING.Where(x=>x.OPERATIONID == this.operationId).Select(x => x.GROUPID).Contains(currentLevel.GROUPID))
                    {
                        throw new SecureException("Target level is not in the same workflow group setup.");
                    }
                }
            }
        }

        public void ResolveExternalFlowLoop(TBL_APPROVAL_TRAIL request, TBL_APPROVAL_TRAIL initiatorRequest)
        {
            if(this.StatusId != (int)ApprovalStatusEnum.Referred && this.request.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && this.referredLog.Count <= 0){ return; }

            if (this.StatusId == (int)ApprovalStatusEnum.Referred )
            {
                this.referBackStateId = (short)ApprovalState.Initiation;

                if (this.nextLevelId == null )
                {
                    this.fromLevelId = request.TOAPPROVALLEVELID;
                    this.nextLevelId = request.TOAPPROVALLEVELID; 
                    this.loopedStaffId = (this.loopedStaffId != null && this.loopedStaffId > 0) ? this.loopedStaffId : initiatorRequest.REQUESTSTAFFID;
                    this.toStaffId = staffId;
                    //this.initiatorOrLooped = true;
                }
            }

            if (this.request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && this.StatusId == (int)ApprovalStatusEnum.Referred)
            {
                request.REFEREBACKSTATEID = (short)ApprovalState.Processing;
               
            }

            if (request.LOOPEDSTAFFID != null && request.LOOPEDSTAFFID > 0 && this.StatusId != (int)ApprovalStatusEnum.Referred)
            {
                this.actualRequestStaffId = this.staffId;
                this.staffId = request.REQUESTSTAFFID;
                this.toStaffId = staffId;
                this.isLoopResponse = true;
                this.fromLevelId = request.TOAPPROVALLEVELID;
                this.nextLevelId = this.nextLevelId != null ? this.nextLevelId : request.TOAPPROVALLEVELID;
            }

            if (this.request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && this.StatusId != (int)ApprovalStatusEnum.Referred)
            {
               if(this.referredLog.Count <= 0) request.REFEREBACKSTATEID = (short)ApprovalState.Ended;

                this.nextLevelId = request.FROMAPPROVALLEVELID;
                if(this.referredLog.Count <= 0)this.toStaffId = request.REQUESTSTAFFID;
            }

            if (this.referredLog.Count > 0 && this.StatusId != (int)ApprovalStatusEnum.Referred )
            {
                var initialReferrer = this.referredLog.Where(x => x.REFEREBACKSTATEID == (short)ApprovalState.Initiation).FirstOrDefault();
                var referrer = initialReferrer == null ? this.referredLog.FirstOrDefault() : initialReferrer;

                this.nextLevelId = referrer.FROMAPPROVALLEVELID;
                this.toStaffId = referrer.REQUESTSTAFFID;
                referrer.REFEREBACKSTATEID = (short)ApprovalState.Ended;
            }
        }

        /*
                private void SkipLevelByAmount()
                {
                    if (next.MaximumAmount < amount) SkipToNextApprovalLevel();
                }

                private void SkipToNextApprovalLevel(int? levelId = null) // WORK AROUND
                {
                    if (levelId == null) levelId = this.nextLevelId;
                    bool found = false;
                    foreach (var level in approvalGrid)
                    {
                        if (found == true)
                        {
                            this.nextLevelId = level.ApprovalLevelId;
                            next = level;
                            SetNextLevelConfigurations(level);
                            break;
                        }
                        if (level.ApprovalLevelId == levelId) found = true;
                    }
                }

                private void SetNextLevelConfigurations(WorkflowSetup level)
                {
                    this.smsNotification = level.CanRecieveSMS;
                    this.emailNotification = level.CanRecieveEmail;
                    this.nextLevelId = level.ApprovalLevelId;
                    this.slaInterval = level.SlaInterval;
                    this.useOrganogram = level.RouteViaStaffOrganogram;
                }*/

        private int? ResolveReroute(int? toStaffId)
        {
            if (toStaffId == this.toStaffId) throw new SecureException("Already with staff!");
            var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId);
            var level = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId && x.PRODUCTID == productId)
                .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true && x.DELETED == false && x.LEVELTYPEID == 2 && x.STAFFROLEID == user.STAFFROLEID), mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new ApprovalLevelInfo
                {
                    groupId = l.GROUPID,
                    groupPosition = mg.m.POSITION,
                    levelPosition = l.POSITION,
                    levelId = l.APPROVALLEVELID,
                    levelName = l.LEVELNAME,
                    staffRoleId = l.STAFFROLEID,
                    levelTypeId = l.LEVELTYPEID,
                }).FirstOrDefault();

            if (level == null)
            {
                var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId && x.PRODUCTID == productId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true && x.DELETED == false && x.LEVELTYPEID == 2)
                        , mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })
                    .Join(context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFID == user.STAFFID), mgl => mgl.l.APPROVALLEVELID, ls => ls.APPROVALLEVELID, (mgl, ls) => new ApprovalLevelInfo
                    {
                        groupId = mgl.l.GROUPID,
                        groupPosition = mgl.mg.m.POSITION,
                        levelPosition = mgl.l.POSITION,
                        levelId = mgl.l.APPROVALLEVELID,
                        levelName = mgl.l.LEVELNAME,
                        staffRoleId = mgl.l.STAFFROLEID,
                        levelTypeId = mgl.l.LEVELTYPEID,
                    }).FirstOrDefault();

                if (levels == null) throw new SecureException("User is not setup to reroute process!");
                return levels.levelId;
            }

            return level.levelId;
        }

        private void SetResponseInformation()
        {
            if (this.setResponse == false) return;

            response.fromLevelId = this.fromLevelId;
            response.stateId = this.newStateId;
            response.nextLevelId = this.nextLevelId;
            response.nextPersonId = this.toStaffId;
            int finalStatusId = this.statusId;

            if (request != null && request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && request.LOOPEDSTAFFID != null)
            { response.nextLevelId = this.fromLevelId;}

            if (response.nextLevelId == null && finalStatusId == (int)ApprovalStatusEnum.Processing) finalStatusId = (int)ApprovalStatusEnum.Approved;
            response.statusId = finalStatusId;


            var s = context.TBL_APPROVAL_STATUS.Find(finalStatusId);
            response.statusName = s.APPROVALSTATUSNAME;

            response.nextLevelName = String.Empty;
            response.nextPersonName = String.Empty;

            if (this.nextLevelId != null)
            {
                var l = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
                response.nextLevelName = l.LEVELNAME;
            }

            if (this.toStaffId != null)
            {
                var p = context.TBL_STAFF.Find(this.toStaffId);
                response.nextPersonName = p.STAFFCODE + " -- " + p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME;
            }
        }

        public void NextProcess(
            int companyId,
            int staffId,
            int operationId,
           // int destinationOperationId,
            int? exclusiveFlowChangeId,
            int targetId,
            int? productClassId,
            string comment,
            bool external,
            bool deferred,
            bool sameDesk,
            bool isFlowTest,
            int? businessUnitId
            )
        {
            InitializeOperation();
            this.companyId = companyId;
            this.staffId = staffId;
            this.operationId = operationId;
            //this.destinationOperationId = destinationOperationId;
            this.exclusiveFlowChangeId = exclusiveFlowChangeId;
            this.targetId = targetId;
           
            this.productClassId = productClassId;
            this.comment = comment;
            this.externalInitialization = external;
            this.deferredExecution = deferred;
            this.sameDesk = sameDesk;
            this.isFlowTest = isFlowTest;
            this.statusId = (int)ApprovalStatusEnum.Pending;
            this.businessUnitId = businessUnitId;


            LogActivity();
        }

        private void InitializeOperation()
        {
            // CAREFUL NOT TO OVERRIDE SUPPLIED values!!!!!!!!
            // set those before calling in
            this.skipLimitsCheck = false;
            this.fromLevelId = null;
            this.newStateId = (int)ApprovalState.Processing;
            if (this.statusId == (int)ApprovalStatusEnum.Pending) this.statusId = (int)ApprovalStatusEnum.Processing;
            // if (IsSpecialReferedBackResponse()) this.statusId = (int)ApprovalStatusEnum.Processing;
            
        }

        private DateTime GetApplicationDate()
        {
            return this.general.GetApplicationDate();
        }

        private bool LastActionIsByStaff()
        {
            if (this.staffId == this.requestStaffId)
            {
                throw new SecureException("Cannot act on self initiated process!");
            }
            return false;
        }

        private bool ProcessIsClosed()
        {
            if (this.isCrossOperationProcess) return false;
            if (this.currentStateId == (int)ApprovalState.Ended) return true;
            return false;
        }

        private void ResolveReferred(int referrerId, int? fromId, int? toId)
        {
            if(request.LOOPEDSTAFFID != null) { return; }

            if (toId == null) throw new SecureException("Unable to resolve destination level!");
            if (fromId == null) return;
            var referrerGroup = context.TBL_APPROVAL_LEVEL.Find(fromId);
            var recepientGroup = context.TBL_APPROVAL_LEVEL.Find(toId);
            if (referrerGroup.GROUPID != recepientGroup.GROUPID && request.APPROVALSTATUSID != (short)ApprovalStatusEnum.Referred && this.referredLog.Count <= 0)
            {
                this.toStaffId = referrerId;
                this.nextLevelId = fromId;
            }
        }

        private void ValidateDestinationConfiguration()
        {
            bool valid = true;
            if (this.toStaffId > 0 && this.nextLevelId > 0)
            {
                valid = general.GetStaffApprovalLevelIds((int)this.toStaffId, this.operationId).ToList().Contains((int)this.nextLevelId);
                if (valid == false) new SecureException("Target Staff is NOT in the destination approval level");
            }
        }

        private bool ResolveLevelConfigurations()
        {
            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId);
            
            approvalGrid = approvalLevels;
            next = approvalLevels.FirstOrDefault();

            if (sameDesk) 
            {
                var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId);
                next = approvalLevels.FirstOrDefault(x => x.DefaultRoleId == user.STAFFROLEID);
            }

            if (this.nextLevelId != null) next = approvalLevels.FirstOrDefault(x => x.ApprovalLevelId == (int)this.nextLevelId);

            if (this.externalInitialization == true && this.currentStateId == (int)ApprovalState.Initiation)
            {
                if (next != null)
                {
                    this.smsNotification = next.CanRecieveSMS;
                    this.emailNotification = next.CanRecieveEmail;
                    this.nextLevelId = next.ApprovalLevelId;
                    this.slaInterval = next.SlaInterval;
                    this.useOrganogram = next.RouteViaStaffOrganogram;
                    return true;
                }
                throw new SecureException("Unable to resolve initiating level or there is no setup for the specified operation!");
            }

            //if (this.fromLevelId == null && this.StatusId == (short)ApprovalStatusEnum.Referred) 
            //{
            //    var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId);
            //    this.fromLevelId = approvalLevels.Where(x => x.DefaultRoleId == user.STAFFROLEID).FirstOrDefault()?.ApprovalLevelId;
            //}

            if (this.fromLevelId != null) // check if staff in level
            {
                level = approvalLevels.Where(x => x.ApprovalLevelId == this.fromLevelId).FirstOrDefault();
                if (level == null)
                {
                    throw new SecureException("This Approval Level is not in the workflow setup!");
                }
                

                var staff = level.Staff.Where(x => x.STAFFID == this.staffId); // check if staff is in approval_level_staff

                TBL_STAFF defaultRole = null;
                if (staff.Any() == false)
                {
                    defaultRole = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId && x.STAFFROLEID == level.DefaultRoleId);
                }

                TBL_STAFF_RELIEF relieverStaff = null;
                if (defaultRole == null)
                {
                    relieverStaff = context.TBL_STAFF_RELIEF.FirstOrDefault(x => x.DELETED == false // check if staff is in staff_relief
                        && x.ISACTIVE == true
                        && x.STARTDATE <= systemDate
                        && x.ENDDATE >= systemDate
                        && x.RELIEFSTAFFID == this.staffId
                    );
                    if (relieverStaff != null) staff = level.Staff.Where(x => x.STAFFID == relieverStaff.STAFFID); // ?
                }

                if (staff.Any() == false && defaultRole == null && relieverStaff == null)
                {
                    throw new SecureException("You are not in the current workflow level " + level.Level.LEVELNAME);
                }

                this.neededNumberOfApproval = level.NumberOfApprovals;
            }

            //if (this.fromLevelId == null) // && externalInitialization == false
            //{
            //    var levelStaff = approvalLevels.SelectMany(x => x.Staff).Where(x => x.STAFFID == this.staffId).FirstOrDefault(); // doing
            //    if (levelStaff == null)
            //    {
            //        throw new SecureException("Unable to resolve initiating level OR there may be no setup for this operation!");
            //    }
            //    this.fromLevelId = levelStaff.APPROVALLEVELID;
            //    this.neededNumberOfApproval = levelStaff.TBL_APPROVAL_LEVEL.NUMBEROFAPPROVALS;
            //}

           // if ((this.statusId == (int)ApprovalStatusEnum.Referred || this.statusId == (int)ApprovalStatusEnum.LoopedIn) && this.nextLevelId == null) { this.nextLevelId = this.requestLevelId; } // default return back to sender

            if (this.nextLevelId == null && fromLevelId != null)
            {
                var currentLevel = approvalLevels.Where(x => x.ApprovalLevelId == this.fromLevelId).FirstOrDefault();
                if (currentLevel == null) throw new SecureException("This Approval Level is not in the workflow setup!");
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
                    this.slaInterval = nextlevel.SLAINTERVAL;
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
                this.slaInterval = next.SlaInterval;
                this.useOrganogram = next.RouteViaStaffOrganogram;
            }
            return true;
        }

        private void CustomJump(int? destinationLevelId, int? originLevelId)
        {
            if (originLevelId == null || destinationLevelId == null) return; // -- changed
            var origin = context.TBL_APPROVAL_LEVEL.Find(originLevelId);
            if (origin.GROUPID != 9) return;
            var destination = context.TBL_APPROVAL_LEVEL.Find(destinationLevelId);
            if (destination.GROUPID != 1) return;
            this.nextLevelId = originLevelId;
        }

        //private bool IsSpecialReferedBackResponse()
        //{
        //    return this.statusId == (int)ApprovalStatusEnum.RePresent || this.statusId == (int)ApprovalStatusEnum.StepDown;
        //}

        private bool ResolveLevelMultipleApproval()
        {
            var votes = context.TBL_APPROVAL_TRAIL.Where(x =>
                x.OPERATIONID == this.operationId
                && x.TARGETID == this.targetId
                && x.APPROVALSTATEID != (int)ApprovalState.Ended
                && x.FROMAPPROVALLEVELID == this.fromLevelId
                )
                .ToList();

            if (votes.FirstOrDefault(x => x.REQUESTSTAFFID == (int)this.staffId) != null) throw new SecureException("You have already acted on this item.");

            // APPROVING ORDER VALIDATION
            var approvers = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false && x.APPROVALLEVELID == fromLevelId).ToList();
            var current = approvers.FirstOrDefault(x => x.STAFFID == this.staffId);
            if (current != null)
            {
                var subs = approvers.Where(x => x.POSITION == (current.POSITION - 1)).ToList();

                var first = context.TBL_APPROVAL_LEVEL_STAFF
                    .Where(x => x.DELETED == false && x.APPROVALLEVELID == fromLevelId && x.STAFFID != this.staffId && x.POSITION < current.POSITION)
                    .Select(x => x.STAFFID);//.ToList();
                //if (first.Count() > 0)
                if (subs.Count() > 0)
                {
                    bool allow = false;
                    string message = "You are not next in line for approval on this approval level. You will be notified by email when required.";
                    if (votes.Count() == 0) throw new SecureException(message);
                    foreach (var vote in votes)
                    {
                        //if (!first.Contains((int)vote.REQUESTSTAFFID))
                        if (subs.Where(x => x.STAFFID == vote.REQUESTSTAFFID).Any()) allow = true;
                    }
                    if (allow == false) throw new SecureException(message);
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
                                    .Where(x => x.DELETED == false && x.APPROVALLEVELID == this.fromLevelId && x.VETOPOWER == true)
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
            if(request.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && request.LOOPEDSTAFFID != null) { maintainFlowStatus();  return; }

            this.statusId = ResolveLastStatus(status);
            this.newStateId = (int)ApprovalState.Ended;
            this.nextLevelId = null; // even if there are other higher level which have been resolve prior
            this.keepPending = false;
            this.toStaffId = null;
        }

        private int ResolveLastStatus(int statusId)
        {
            switch (statusId)
            {
                case 0: return 2;
                case 1: return 2;
                case 2: return 2;
                case 3: return 3;
                case 4: return 2;
                case 5: return 3;
                case 6: return 2;
                case 7: return 2;
                case 8: return 2;
                case 9: return 2;
                default: break;
            }
            return statusId;
        }

        private void ValidateCall()
        {
            if (this.staffId <= 0) throw new SecureException("Invalid Call! staffid cannot be " + this.staffId);
            if (this.operationId <= 0) throw new SecureException("Invalid Call! operationId cannot be " + this.operationId);
            if (this.targetId <= 0 && !this.deferredExecution) throw new SecureException("Invalid Call! targetId cannot be " + this.targetId);
            if (this.companyId <= 0) throw new SecureException("Invalid Call! companyId cannot be " + this.companyId);
            if (this.statusId < 0) throw new SecureException("Invalid Call! statusId cannot be " + this.statusId);
            if (this.nextLevelId < 1) { this.nextLevelId = null; }
        }

        public void GetReportingLine(int staffId)
        {
            var s = context.TBL_STAFF.Where(x => x.STAFFID == staffId).FirstOrDefault();
            line.Add(new ReportingLine
            {
                staffId = (int)s.STAFFID,
                levelRoleId = (int)s.STAFFROLEID,
                levelIds = context.TBL_APPROVAL_LEVEL.Where(x => x.STAFFROLEID == s.STAFFROLEID).Select(x => (int)x.APPROVALLEVELID).ToList(),
            });
            if (s.SUPERVISOR_STAFFID == null) return;
            GetReportingLine((int)s.SUPERVISOR_STAFFID);
        }

        private int? GetReportingLineStaffId() // if workflow is forced to use organogram
        {
            var businessRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(s => s.STAFFROLEID).ToList();
            if (next == null) { return null; }
            if (this.toStaffId != null) { return toStaffId; }
            //if (this.toStaffId != null) { return null; }
            if (this.externalInitialization == true) { return null; }
            var staff = context.TBL_STAFF.Where(x => x.STAFFID == this.staffId).FirstOrDefault();
            if (staff == null) { return null; }
            GetReportingLine(staffId);
            if (this.statusId == (int)ApprovalStatusEnum.Referred || !businessRoleIds.Contains(next.DefaultRoleId ?? 0) || this.fromLevelId == null)
            {
                return null;
            }
            ReportingLine super = line.FirstOrDefault(x => x.levelRoleId == next.DefaultRoleId && x.levelIds.Contains(next.ApprovalLevelId));
            if (super == null)
            {
                throw new SecureException("No Staff Was Setup as Your Supervisor!");
                //return null;
            }

            return super.staffId;
        }

        private void CheckApprovalLimits()
        {
            if (this.statusId == (short)ApprovalStatusEnum.Referred) { return; }
            // allow business to drop process unconditionally 
            if (this.statusId == (int)ApprovalStatusEnum.Disapproved && GroupRole() == (int)ApprovalGroupEnum.Business) // for optimization the more expensive conditions are placed last. GroupRole() may not be called
            {
                return;
            }

            if (this.skipLimitsCheck == true || IsPresetFinalLevel()) { return; }
            //if (request.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && this.nextLevelId == null) { this.nextLevelId = this.fromLevelId; }//temporary fix o!!!!!
            if (this.nextLevelId != null && this.amount > 0 || ActionIsApprovalDecision())
            {
                
                if (WithinAllLimits() == true)
                {
                    this.EndProcess(this.statusId);
                }
                else if (ActionIsApprovalDecision())
                {
                    if(this.statusId != (short)ApprovalStatusEnum.Disapproved)
                    {
                        this.ContinueProcess((int)ApprovalStatusEnum.Authorised);
                    }
                    else { this.EndProcess(this.statusId); }
                    
                }
            }
            
        }

        private bool IsPresetFinalLevel()
        {
            return this.fromLevelId == this.finalLevel;
        }

        private int GroupRole()
        {
            if (this.fromLevelId == null) return 1;
            var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
            if (this.level == null) return 1;
            return level.TBL_APPROVAL_GROUP.ROLEID;
        }

        private bool WithinTenorLimit(TBL_APPROVAL_LEVEL level)
        {
            if (tenor == null) return true;
            if (this.untenored == true) { return level.CANAPPROVEUNTENORED == true ? true : false; }
            if (tenor == 0 && level.TENOR == 0) { return true; } // setup
            if (tenor == 0 && level.TENOR == null) { return true; } // setup
            if (tenor > 0 && level.TENOR >= tenor) { return true; } // gen cam
            if (level.TENOR == null && ActionIsApprovalDecision()) return true; // access bank no tenor setup

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
            if (level == null ) { throw new SecureException("The user is not in the workflow setup!"); } // redundant - wouldnt get here in the first place
            if (this.disputed == true && level.CANRESOLVEDISPUTE != true) { return false; }
            return WithinTenorLimit(level) == true
                && WithinMaximumLimit(level) == true
                && WithinInvestmentGradeLimit(level) == true
                && WithinPoliticallyExposedLimit(level) == true;
        }

        private void LastApproverCheck()
        {
            var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
            if (IsLastApprover(level) && (this.statusId == (int)ApprovalStatusEnum.Processing || this.statusId == (int)ApprovalStatusEnum.Authorised))
            {
                this.statusId = (int)ApprovalStatusEnum.Approved;
            }
        }

        private bool IsLastApprover(TBL_APPROVAL_LEVEL level)
        {
            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId).ToList();
            if (IsLastLevel(approvalLevels, level) && level.CANAPPROVE)
            //if (IsLastLevel(approvalLevels, level) && level.CANAPPROVE && !(level.MAXIMUMAMOUNT > 0))
                {
                return true;
            }
                return false;
        }

        private bool IsLastLevel(List<WorkflowSetup> levels, TBL_APPROVAL_LEVEL level)
        {
            if (levels.Count() < 1 || level == null) return false;
            var isLastLevel = (levels.FindLastIndex(l => l.Level.APPROVALLEVELID == level.APPROVALLEVELID)) == (levels.Count() - 1);
            return isLastLevel;
        }

        private int SetState()
        {
            var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);//for test!!
            if (this.nextLevelId == null && IsLastApprover(level))//for test!!
            {
                if (ActionIsApprovalDecision() || this.amount == 0)
                {
                    this.EndProcess(this.statusId);
                }
                else
                {
                    throw new SecureException("Workflow is missing an approval authority!");
                }
                return statusId;
            }
            else
            {
                if (this.statusId == (int)ApprovalStatusEnum.Escalated)
                {
                    this.ContinueProcess((int)ApprovalStatusEnum.Processing);
                    return statusId;
                }
            }

            if (this.fromLevelId != null && IsPresetFinalLevel())
            {
                if (this.statusId == (int)ApprovalStatusEnum.Approved 
                    || this.statusId == (int)ApprovalStatusEnum.Authorised
                    || this.statusId == (int)ApprovalStatusEnum.Processing
                    )
                {
                    EndProcess((int)ApprovalStatusEnum.Approved);
                } else if (this.statusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    EndProcess((int)ApprovalStatusEnum.Disapproved);
                }
                return statusId;
            }

            if (this.keepPending == true) // DEPRECATED!!!
            {
                this.statusId = (int)ApprovalStatusEnum.Pending;
                this.newStateId = (int)ApprovalState.Processing;
                return statusId;
            }

            if (this.endProcess == true) // PENDING UPDATE (To forcefully end the process at a particular level)
            {
                this.statusId = (int)ApprovalStatusEnum.Approved;
                this.EndProcess(this.statusId);
                return statusId;
            }

            if (ActionIsApprovalDecision()) // if its still approval decision end process
            {
                this.EndProcess(this.statusId);
                return statusId;
            }

            return statusId;
        }

        private void maintainFlowStatus()
        {
            this.statusId = (int)ApprovalStatusEnum.Pending;
            this.newStateId = (int)ApprovalState.Processing;

        }

        private void SetReroute()
        {
            // if (this.statusId == (int)ApprovalStatusEnum.Reroute) { this.nextLevelId = this.fromLevelId; }
            if (this.statusId == (int)ApprovalStatusEnum.Reroute) { this.statusId = (int)ApprovalStatusEnum.Processing; }
        }

        private bool ActionIsApprovalDecision()
        {
            //return (this.statusId == (int)ApprovalStatusEnum.Approved || this.statusId == (int)ApprovalStatusEnum.Disapproved || this.statusId == (int)ApprovalStatusEnum.Authorised);
            return (this.statusId == (int)ApprovalStatusEnum.Approved || this.statusId == (int)ApprovalStatusEnum.Disapproved);
        }

        private IEnumerable<WorkflowSetup> GetWorkflowSetup(int operationId, int? productClassId, int? productId)
        {
            if (productId == 0) productId = null;

            var mappingsOnProducts = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && (x.PRODUCTCLASSID == productClassId && x.PRODUCTCLASSID != null)
                               && (x.PRODUCTID == productId && x.PRODUCTID != null)
                           )
                           .ToList();

            var mappingsOnProductClass = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && (x.PRODUCTCLASSID == productClassId && x.PRODUCTCLASSID != null)
                               && x.PRODUCTID == null
                           )
                           .ToList();

            var mappingsOnOperations = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && x.PRODUCTCLASSID == null
                               && x.PRODUCTID == null
                           )
                           .ToList();

            List<TBL_APPROVAL_GROUP_MAPPING> mappingsOnExclusiveOperations = new List<TBL_APPROVAL_GROUP_MAPPING>();

            if (exclusiveFlowChangeId != null && exclusiveFlowChangeId > 0)
            {
                var flowChangePartern = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(this.exclusiveFlowChangeId);

                if (flowChangePartern != null)
                {
                    mappingsOnExclusiveOperations = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == flowChangePartern.OPERATIONID
                               && x.PRODUCTCLASSID == null
                               && x.PRODUCTID == null
                           )
                           .ToList();
                }
            }

            List<TBL_APPROVAL_GROUP_MAPPING> mappings = new List<TBL_APPROVAL_GROUP_MAPPING>();

            if (mappingsOnOperations.Any()) mappings = mappingsOnOperations;

            if (mappingsOnProductClass.Any()) mappings = mappingsOnProductClass;

            if (mappingsOnProducts.Any()) mappings = mappingsOnProducts;

            if (mappingsOnExclusiveOperations.Any()) mappings = mappingsOnExclusiveOperations;

            if (mappingsOnProducts.Any() == false && mappingsOnProductClass.Any() == false && mappingsOnOperations.Any() == false && mappingsOnExclusiveOperations.Any() == false)
            {
                var operation = context.TBL_OPERATIONS.Find(operationId);
                if (operation == null) throw new SecureException("Operation ID didn't match");
                if (productClassId != null)
                {
                    var productClass = context.TBL_PRODUCT_CLASS.Find(productClassId);
                    throw new SecureException("There is no approval workflow setup for the OPERATION: " + operation.OPERATIONNAME + ", PRODUCT CLASS: " + productClass.PRODUCTCLASSNAME);
                }
                throw new SecureException("There is no approval workflow setup for the OPERATION: " + operation.OPERATIONNAME);
            }

            var levels = mappings
                           .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                           .Join(context.TBL_APPROVAL_LEVEL, mg => mg.m.GROUPID, l => l.GROUPID, (mg, l) =>
                           new { Mapping = mg.m, Level = l })
                           .Where(x => x.Level.ISACTIVE == true && x.Level.DELETED == false)
                           .Select(x => new WorkflowSetup
                           {
                               // Sn = index + 1,
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
                               DefaultRoleId = x.Level.STAFFROLEID,
                               SlaInterval = x.Level.SLAINTERVAL,
                               LevelTypeId = x.Level.LEVELTYPEID,
                               LevelBusinessRuleId = x.Level.APPROVALBUSINESSRULEID,
                               LevelBusinessRule = x.Level.TBL_APPROVAL_BUSINESS_RULE
                           })
                           .OrderBy(x => x.GroupPosition)
                           .ThenBy(x => x.LevelPosition)
                           .ToList();

           
            List<WorkflowSetup> grid = new List<WorkflowSetup>();
            //bool canSkipRule = levelBusinessRule.InsiderRelated == true;
            //levelBusinessRule.Amount = this.amount;

            int n = 0;
            foreach (WorkflowSetup level in levels)
            {
                //this.levelBusinessRule = level?.LevelBusinessRule;

                if (level.LevelBusinessRuleId != null && !LevelBusinessRuleIsValid(level.LevelBusinessRule)) continue;
                //if (level.LevelBusinessRuleId != null && !LevelBusinessRuleIsValid(level.LevelBusinessRule) && !canSkipRule) continue;
                n++;
                grid.Add(new WorkflowSetup
                {
                    Sn = n,
                    GroupPosition = level.GroupPosition,
                    LevelPosition = level.LevelPosition,
                    ApprovalLevelId = level.ApprovalLevelId,
                    NumberOfUsers = level.NumberOfUsers,
                    NumberOfApprovals = level.NumberOfApprovals,
                    CanRouteBack = level.CanRouteBack,
                    IsPoliticallyExposed = level.IsPoliticallyExposed,
                    IsActive = level.IsActive,
                    CanEdit = level.CanEdit,
                    CanRecieveEmail = level.CanRecieveEmail,
                    CanRecieveSMS = level.CanRecieveSMS,
                    RouteViaStaffOrganogram = level.RouteViaStaffOrganogram,
                    Tenor = level.Tenor,
                    MaximumAmount = level.MaximumAmount,
                    InvestmentGradeAmount = level.InvestmentGradeAmount,
                    DefaultRoleId = level.DefaultRoleId,
                    LevelTypeId = level.LevelTypeId,
                    LevelBusinessRuleId = level.LevelBusinessRuleId,
                    Level = level.Level,
                    Group = level.Group,
                    Mapping = level.Mapping,
                    Staff = level.Staff,
                    LevelBusinessRule = level.LevelBusinessRule,
                });
            }

            this.workflowSetup = grid;
            //throw new SecureException("");
            return grid;
        }

        private bool LevelBusinessRuleIsValid(TBL_APPROVAL_BUSINESS_RULE rule)
        {
            
            if (levelBusinessRule == null) return true;

            bool validity = false;
            bool flagChecked = false;
            bool limitChecked = false;
            decimal pepAmount = rule.PEPAMOUNT ?? 0;
            decimal minimumAmount = rule.MINIMUMAMOUNT ?? 0;
            decimal maximumAmount = rule.MAXIMUMAMOUNT ?? 0;

            if ((minimumAmount > 0 && maximumAmount == 0) && (minimumAmount < levelBusinessRule.Amount)) limitChecked = true;
            if ((minimumAmount == 0 && maximumAmount > 0) && (levelBusinessRule.Amount <= maximumAmount)) limitChecked = true;
            if ((minimumAmount > 0 && maximumAmount > 0) && (minimumAmount < levelBusinessRule.Amount && levelBusinessRule.Amount <= maximumAmount)) limitChecked = true;

            if ((rule.PEP && pepAmount > 0) && (levelBusinessRule.Pep && pepAmount <= levelBusinessRule.PepAmount)) limitChecked = flagChecked = true;

            //if (rule.PEP && levelBusinessRule.Pep == true) flagChecked = true;
            if (rule.INSIDERRELATED && levelBusinessRule.InsiderRelated == true) flagChecked = true;
            if (rule.PROJECTRELATED && levelBusinessRule.ProjectRelated == true) flagChecked = true;
            if (rule.ONLENDING && levelBusinessRule.OnLending == true) flagChecked = true;
            if (rule.INTERVENTIONFUNDS && levelBusinessRule.InterventionFunds == true) flagChecked = true;
            if (rule.ORRBASEDAPPROVAL && levelBusinessRule.OrrBasedApproval == true) flagChecked = true;
            if (rule.WITHOUTINSTRUCTION && levelBusinessRule.WithoutInstruction == true) flagChecked = true;
            if (rule.DOMICILIATIONNOTINPLACE && levelBusinessRule.DomiciliationNotInPlace == true) flagChecked = true;

            if (limitChecked && flagChecked) return limitChecked && limitChecked;
            if (limitChecked || flagChecked) return true;

            return validity;
        }

        private void SendNotifications()
        {
            if (statusOnly) return;
            if (emailNotification || smsNotification)
            {
                int tat = next != null ? next.SlaInterval : 0;
                var setup = context.TBL_SETUP_GLOBAL.Find(1);
                var operation = context.TBL_OPERATIONS.Find(this.operationId);
                var applicationUrl = setup.APPLICATION_URL.Length == 0 ? "#" : setup.APPLICATION_URL;
                var applicationUrls = "https://credit360.accessbankplc.com";
                var links = "<p>Click <a href=\"" + applicationUrls + "\">here to continue...</a></p>";

                TBL_STAFF owner;
                if (trailLog.Count() == 0)
                {
                    owner = context.TBL_STAFF.Find(this.staffId);
                }
                else
                {
                    var trails = trailLog.OrderBy(x => x.APPROVALTRAILID);
                    owner = context.TBL_STAFF.Find(trails.First().REQUESTSTAFFID);
                }

                int targetId = this.targetId;
                int operationId = this.operationId;
                var message = new TBL_MESSAGE_LOG();
                var reciever = new TBL_STAFF();
                string recipientName = "All";
                string operationName = operation == null ? "N/A" : operation.OPERATIONNAME;
                string messageSubject = "PENDING APPROVAL FOR " + operationName.ToUpper();
                string status = GetApprovalStatusName(this.statusId);
                string ownerMessageSubject = "YOUR INITIATED " + operationName.ToUpper() + " PROCESS HAVE BEEN " + status.ToUpper();
                var level = string.Empty;
                List<string> emails = new List<string>();

                if (this.fromLevelId != null) level = " by " + context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId)?.LEVELNAME;
                if (this.nextLevelId != null && this.toStaffId == null) recipientName = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId)?.LEVELNAME;

                if (this.toStaffId != null)
                {
                    reciever = context.TBL_STAFF.Find(this.toStaffId);
                    recipientName = reciever.FIRSTNAME;
                    this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.toStaffId && x.ENDDATE < DateTime.Now).Select(x=>x.RELIEFSTAFFID).FirstOrDefault();
                }
                else if (this.loopedStaffId != null)
                {
                    reciever = context.TBL_STAFF.Find(this.loopedStaffId);
                    recipientName = reciever.FIRSTNAME;
                    this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.loopedStaffId && x.ENDDATE < DateTime.Now).Select(x => x.RELIEFSTAFFID).FirstOrDefault();
                }
                else
                {
                    if (this.nextLevelId != null)
                    {
                        var nextLevel = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);

                        var actorIds = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == operationId && t.TARGETID == targetId)
                            .Join(context.TBL_STAFF, t => t.REQUESTSTAFFID, s => s.STAFFID, (t, s) => new { t, s })
                            .Select(x => x.s.EMAIL).ToList();

                        var levelStaffEmails = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false && x.APPROVALLEVELID == nextLevel.APPROVALLEVELID)
                            .Select(x => x.TBL_STAFF.EMAIL)
                            .Distinct().ToList();

                        var businessRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(r => r.STAFFROLEID).ToList();
                        var nextLvlRoleIsABusinessRole = businessRoleIds.Contains(nextLevel.STAFFROLEID ?? 0);
                        if (nextLvlRoleIsABusinessRole)
                        {
                            emails = actorIds.Union(levelStaffEmails).ToList();
                        }
                        else
                        {
                            var nextLevelStaffEmails = context.TBL_STAFF.Where(s => s.STAFFROLEID == nextLevel.STAFFROLEID).Select(x => x.EMAIL);
                            emails = actorIds.Union(levelStaffEmails).Union(nextLevelStaffEmails).ToList();
                        }

                        if (this.reliefStaffId != null)
                        {
                            var reliefRecord = context.TBL_STAFF.Find(this.reliefStaffId);
                           // emails.Add(reliefRecord.EMAIL);
                        }
                    }
                }

                var time = String.Format("{0:F}", DateTime.Now);
                //throw new Exception("");

                if (placeholders == null) placeholders = new AlertPlaceholders();

                var ownerMessageBody = $"Dear {owner.FIRSTNAME}, <br /><br />" +
                            $"The {operationName} approval process you initiated have been {status}{level}. <br /><br />" +
                            $"{placeholders.customerName}" +
                            $"{placeholders.referenceNumber}" +
                            $"{placeholders.facilityType}" +
                            $"{placeholders.operationName}" +
                            $"{placeholders.branchName}" +
                            $"{placeholders.locationName}" +
                            $"<p>Time: { time }</p>"
                            ;

                var messageBody = $"Dear {recipientName}, <br /><br />" +
                            $"You have a new pending {operationName} approval request. <br /><br />" +
                            $"{placeholders.customerName}" +
                            $"{placeholders.referenceNumber}" +
                            $"{placeholders.facilityType}" +
                            $"{placeholders.operationName}" +
                            $"{placeholders.branchName}" +
                            $"{placeholders.locationName}" +
                            $"<p>Time: { time }</p>"
                            ;

                if (tat > 0)
                {
                    messageBody = messageBody + $"<p>TAT: { tat } hour(s)</p>";
                    ownerMessageBody = ownerMessageBody + $"<p>TAT: { tat } hour(s)</p>";
                }

                //var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                if (emailNotification)
                {
                    message = new TBL_MESSAGE_LOG // INITIATOR
                    {
                        TOADDRESS = owner.EMAIL,
                        MESSAGESUBJECT = ownerMessageSubject,
                        MESSAGEBODY = ownerMessageBody + links,
                        MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                        MESSAGETYPEID = (short)MessageTypeEnum.Email,
                        FROMADDRESS = this.support,
                        DATETIMERECEIVED = DateTime.Now,
                        SENDONDATETIME = DateTime.Now,
                        TARGETID = targetId,
                        OPERATIONID = operationId
                    };
                    context.TBL_MESSAGE_LOG.Add(message);

                    if (this.toStaffId != null || emails.Any())
                    {
                        message = new TBL_MESSAGE_LOG // RECIEVERS
                        {
                            TOADDRESS = this.toStaffId != null ? reciever.EMAIL : string.Join(";", emails.Distinct()),
                            MESSAGESUBJECT = messageSubject,
                            MESSAGEBODY = messageBody + links,
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
            throw new SecureException("Unauthorized action!");
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
            exclusiveFlowChangeId = model.exclusiveFlowChangeId;
            loopedStaffId = model.loopedStaffId;
            LoopedRoleId = model.loopedRoleId;
            keepPending = model.keepPending;
            deferredExecution = model.deferredExecution;
            IsFlowTest = model.isFlowTest;
            destinationOperationId = model.destinationOperationId;
            businessUnitId = model.businessUnitId;
            var response = LogActivity();

            return response;
        }
        
        public void ResolveMultipleProductPath(int operationId, List<short> productIds)
        {
            TBL_PRODUCT product;
            this.operationId = operationId;
            if (productIds.Count() == 0) return;
            var operationProducts = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && x.OPERATIONID == operationId && productIds.Contains((short)x.PRODUCTID)).Distinct().ToList();

            // SINGLE PRODUCT
            if (productIds.Count() == 1)
            {
                product= context.TBL_PRODUCT.Find(productIds.FirstOrDefault());
                if (operationProducts.Any())
                {
                    this.productId = product.PRODUCTID;
                    this.productClassId = product.PRODUCTCLASSID;
                }
                else
                {
                    if (context.TBL_APPROVAL_GROUP_MAPPING
                        .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.PRODUCTCLASSID == product.PRODUCTCLASSID)
                        .Distinct().Any())
                    {
                        this.productClassId = product.PRODUCTCLASSID;
                    }
                }
                return;
            };

            // MULTIPLE PRODUCT CLASS
            List<short> productClassIds = context.TBL_PRODUCT.Where(x => productIds.Contains((short)x.PRODUCTID)).Select(x => x.PRODUCTCLASSID).Distinct().ToList();
            if (productClassIds.Count() == 1) this.productClassId = productClassIds.FirstOrDefault();
            else this.productClassId = PreferedProductClassId(productClassIds);
            // MULTIPLE PRODUCT
            List<short> classProductIds = context.TBL_PRODUCT.Where(x => x.PRODUCTCLASSID == this.productClassId).Select(x => x.PRODUCTID).Distinct().ToList();
            if (classProductIds.Count() == 1) this.productId = classProductIds.FirstOrDefault();
            else this.productId = PreferedProductId(classProductIds);

        }

        private int? PreferedProductId(List<short> productIds)
        {
            var product = context.TBL_PRODUCT.Find(productIds.FirstOrDefault()); // TODO: COUTION! which product to be prioritized?
            return product.PRODUCTID;
        }

        private int? PreferedProductClassId(List<short> productClassIds)
        {
            var productClass = context.TBL_PRODUCT_CLASS.Find(productClassIds.FirstOrDefault()); // TODO: COUTION! which product to be prioritized?
            return productClass.PRODUCTCLASSID;
        }
    }

    public class WorkflowSetup
    {
        public int Sn { get; set; }
        public int SlaInterval { get; set; }
        public int GroupPosition { get; set; }
        public int LevelPosition { get; set; }
        public int ApprovalLevelId { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfApprovals { get; set; }
        public bool CanRouteBack { get; set; }
        public bool IsPoliticallyExposed { get; set; }
        //public bool IsInsiderRelated { get; set; }
        public bool IsActive { get; set; }
        public bool CanEdit { get; set; }
        public bool CanRecieveEmail { get; set; }
        public bool CanRecieveSMS { get; set; }
        public bool RouteViaStaffOrganogram { get; set; }
        public int? Tenor { get; set; }
        public decimal MaximumAmount { get; set; }
        public decimal? InvestmentGradeAmount { get; set; }
        public int? DefaultRoleId { get; set; }
        public int? LevelTypeId { get; set; }
        public int? LevelBusinessRuleId { get; set; }
        public TBL_APPROVAL_LEVEL Level { get; set; }
        public TBL_APPROVAL_GROUP Group { get; set; }
        public TBL_APPROVAL_GROUP_MAPPING Mapping { get; set; }
        public IEnumerable<TBL_APPROVAL_LEVEL_STAFF> Staff { get; set; }
        public TBL_APPROVAL_BUSINESS_RULE LevelBusinessRule { get; set; }
    }

    public class ReportingLine
    {
        public int staffId { get; set; }
        public List<int> levelIds { get; set; }
        public int levelRoleId { get; set; }

    }

    public class StaffAllocatedjob
    {
        internal bool isOnRelief { get; set; }

        public int staffId { get; set; }
        public int pendingJobCount { get; set; }
        public bool counted { get; set; }
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
