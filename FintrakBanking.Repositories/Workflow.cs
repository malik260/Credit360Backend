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
        private bool isClassifiedReferBack;
        private int? exclusiveFlowChangeId = null;
        private int? businessUnitId = null;
        private TBL_APPROVAL_TRAIL approvalTrail;
        private int? productClassId = null;
        public int? productId = null;

        private string comment = string.Empty;
        private int statusId = (int)ApprovalStatusEnum.Processing;
        private int groupStatusId = (int)ApprovalStatusEnum.Processing;
        private int? nextLevelId = null; // for refer backs
        private bool ignorePostApprovalReviewer = false;
        private bool nextIsReviewer = false;
        private int? finalLevel = null; // preset force to end
        private int? reviewerLevelId = null; // preset force to end
        private bool emailNotification = false;
        private bool smsNotification = false;
        private bool sameDesk = false;

        private int? fromLevelId = null;
        private int? reliefStaffId = null;
        private int? lastOpenRequestFromLevelId = null;
        private int currentStateId;
        private int newStateId = (int)ApprovalState.Processing;
        private int? tenor = null;
        private decimal amount = 0;
        private decimal facilityAmount = 0;
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
        private bool? isFromPc = false;
        private bool? terminateOnApproval = false;
        private string flow_log = String.Empty;
        private bool skipLimitsCheck = false;
        //private WorkflowResponse response = null;

        private float? interestRateConcession = null;
        private float? feeRateConcession = null;
        private int? ownerId = null;

        public int StaffId { set { staffId = value; } }
        public int? ToStaffId { set { toStaffId = value; } }
        public int TargetId { set { targetId = value; } }
        public int CompanyId { set { companyId = value; } }
        public int OperationId { set { operationId = value; } }

        public decimal Amount { set { amount = value; } }
        public decimal FacilityAmount { set { facilityAmount = value; } }
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
        public bool IgnorePostApprovalReviewer { get { return ignorePostApprovalReviewer; } set { ignorePostApprovalReviewer = value; } }
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
        public bool? IsFromPc { set { isFromPc = value; } }
        public bool? TerminateOnApproval { set { terminateOnApproval = value; } }
        
        public bool SkipLimitsCheck { set { skipLimitsCheck = value; } }
        public string Flow_log { set { flow_log = value; } }
        public bool IsClassifiedReferBack { get { return isClassifiedReferBack; } set { isClassifiedReferBack = value; } }
        public List<WorkflowSetup> WorkflowSetup { get; private set; }
        public int? OwnerId { set { ownerId = value; } }
        

        private WorkflowSetup level;
        private WorkflowSetup currentlevel;
        private WorkflowSetup next;
        private List<TBL_APPROVAL_TRAIL> trailLog; 
        private List<TBL_APPROVAL_TRAIL> referredLog;
        private TBL_APPROVAL_TRAIL lastOpenRequest;
        private IEnumerable<WorkflowSetup> approvalGrid;
        private int slaInterval = 780; // 1month
        List<ReportingLine> line = new List<ReportingLine>();
        private List<int> creditOperationIds;
        private TBL_OPERATIONS operation;

        //private WorkflowSetup currentLevel;

        public bool LogActivity()
        {
            ValidateCall();
            InitializeOperation();
            if (Authorization() == false) { return false; }
            creditOperationIds = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Select(o => o.OPERATIONID).ToList();
            int[] drawdownIds = { (int)OperationsEnum.CorporateDrawdownRequest, (int)OperationsEnum.IndividualDrawdownRequest };
            creditOperationIds.AddRange(drawdownIds.ToList());
            this.trailLog = context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == this.companyId
                                && x.OPERATIONID == this.operationId
                                && x.TARGETID == this.targetId
                                && x.RESPONSESTAFFID == null
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null && x.APPROVALSTATUSID != (short)ApprovalStatusEnum.Closed )
                            ).ToList();


            lastOpenRequest = trailLog.OrderByDescending(x => x.APPROVALTRAILID).FirstOrDefault();
            if (this.nextLevelId > 0 && this.statusId != (int)ApprovalStatusEnum.Referred && this.statusId != (int)ApprovalStatusEnum.Reroute && lastOpenRequest != null) //if it is not initiation or referred
            {
                throw new SecureException("An error occured, Next Level can't be preset unless on refer back or re-routing. Kindly refresh your browser and try again.");
            }

            this.referredLog = context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == this.companyId
                                && x.OPERATIONID == this.operationId
                                && x.TARGETID == this.targetId
                               // && x.RESPONSESTAFFID != null
                                && x.REFEREBACKSTATEID != (int)ApprovalState.Ended
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                            ).OrderByDescending(l => l.APPROVALTRAILID).ToList();

            var initiatingRequest = GetAllTrail().OrderByDescending(x => x.APPROVALTRAILID).LastOrDefault();
            SaveFlowLog("Initiation");


            if (lastOpenRequest == null)
            {
                if (ActionIsApprovalDecision()) throw new SecureException("Unable to resolve initiating level or the process is closed!");
                this.currentStateId = (int)ApprovalState.Initiation;
            }
            else
            {
                this.currentStateId = lastOpenRequest.APPROVALSTATEID;
                this.requestStaffId = lastOpenRequest.REQUESTSTAFFID;
                this.fromLevelId = lastOpenRequest.TOAPPROVALLEVELID;
                this.lastOpenRequestFromLevelId = lastOpenRequest.FROMAPPROVALLEVELID;
                this.isCrossOperationProcess = lastOpenRequest.OPERATIONID != this.operationId;
                this.ResolveExternalFlowLoop(lastOpenRequest, initiatingRequest);

                if (this.statusId == (int)ApprovalStatusEnum.Reroute) { this.fromLevelId = ResolveReroute(lastOpenRequest.TOSTAFFID); }
                if (lastOpenRequest.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred) { ResolveReferred(lastOpenRequest.REQUESTSTAFFID, lastOpenRequest.FROMAPPROVALLEVELID, lastOpenRequest.TOAPPROVALLEVELID); }
                if (ProcessIsClosed()) { throw new SecureException("Process is closed!"); }
                //if (lastRequest !=null)
                CustomJump(lastOpenRequest.TOAPPROVALLEVELID, lastOpenRequest.FROMAPPROVALLEVELID);
            }
            SaveFlowLog("After last request Validation");

            if (ResolveLevelConfigurations() == false) { throw new SecureException("Could not resolve approval level configurations!"); }
            // if (next != null && next.LevelTypeId == (int)ApprovalLevelType.SkipLevelByAmount) SkipLevelByAmount();

            if (this.useOrganogram == true) toStaffId = GetReportingLineStaffId();

            //if (this.neededNumberOfApproval > 1 && ActionIsApprovalDecision())
            if (this.neededNumberOfApproval > 1)
            {
                ResolveLevelMultipleApproval();
            }

            ValidateAgainstWorkFlowAnomalies();


            CheckApprovalLimits();

            LastApproverCheck();

            SetState();

            SetReroute();

            FurtherValidations();

            this.applicationDate = GetApplicationDate();

            if (lastOpenRequest != null)
            {
                lastOpenRequest.RESPONSEDATE = this.applicationDate;
                lastOpenRequest.SYSTEMRESPONSEDATETIME = this.systemDate;
                lastOpenRequest.RESPONSESTAFFID = this.staffId;

                if (lastOpenRequest.LOOPEDSTAFFID != null && lastOpenRequest.LOOPEDSTAFFID > 0 && lastOpenRequest.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                { lastOpenRequest.RESPONSESTAFFID = !isLoopResponse ? this.staffId : this.actualRequestStaffId; }
            }

            AllocateBySBU();

            MakerCheckerControl();

            RandomizeAllocation();

            SetResponseInformation();

            SendNotifications();

            if (statusOnly) return true;

            if (this.comment == "flow_test") { throw new SecureException("from (" + this.fromLevelId + ") to (" + this.nextLevelId + "), status: " + response.statusName + ", level: " + response.nextLevelName + ", person: " + response.nextPersonName); }

            SaveFlowLog("Before Final Trail Logging");
            if (this.isFlowTest) return true;

            //=============
            if (this.nextLevelId > 0)
            {
                var level = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
                var isReviewer = (level?.ISPOSTAPPROVALREVIEWER ?? false);
                if (isReviewer && this.loopedStaffId == null)
                {
                    this.statusId = (int)ApprovalStatusEnum.Finishing;
                }
            }

            if (this.vote == null)
            {
                ValidateVote();
            }


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
                DESTINATIONOPERATIONID = this.destinationOperationId,
                ISFROMPC = this.isFromPc,
                FLOW_LOG = this.flow_log
            });

            //throw new SecureException("Unknown Process Flow Error! Unable to save workflow records!");
            if (this.ignorePostApprovalReviewer == false && this.approvalTrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved)
            {
                
                if (this.reviewerLevelId > 0)
                {
                    var level = context.TBL_APPROVAL_LEVEL.Find(this.reviewerLevelId);
                    var isReviewer = (level?.ISPOSTAPPROVALREVIEWER ?? false);
                    if (isReviewer)
                    {
                        StartPostApprovalLevelsReview(this.reviewerLevelId ?? 0);
                    }
                }
            }


            if (this.deferredExecution)
            {
                this.fromLevelId = null;
                return true;
            }
            this.saved = context.SaveChanges() > 0;
            if (this.saved)
            {
                this.fromLevelId = null;
                return true;
            }

            throw new SecureException("Unknown Process Flow Error! Unable to save workflow records!");
        }

        private void StartPostApprovalLevelsReview(int toLevel)
        {
            
            this.approvalTrail = context.TBL_APPROVAL_TRAIL.Add(new TBL_APPROVAL_TRAIL
            {
                FROMAPPROVALLEVELID = this.fromLevelId,
                TOAPPROVALLEVELID = toLevel,
                TARGETID = this.targetId,
                COMPANYID = this.companyId,
                REQUESTSTAFFID = !isLoopResponse ? this.staffId : this.actualRequestStaffId,
                OPERATIONID = this.operationId,
                COMMENT = this.comment,
                ARRIVALDATE = this.applicationDate,
                APPROVALSTATEID = (short)ApprovalState.Processing,
                APPROVALSTATUSID = (short)(int)ApprovalStatusEnum.Finishing,
                SYSTEMARRIVALDATETIME = this.systemDate,
                SLADATETIME = this.systemDate.AddHours(this.slaInterval),
                VOTE = this.vote,
                TOSTAFFID = this.toStaffId,
                LOOPEDROLEID = this.loopedRoleId,
                LOOPEDSTAFFID = this.loopedStaffId,
                REFEREBACKSTATEID = this.referBackStateId,
                DESTINATIONOPERATIONID = this.destinationOperationId,
                ISFROMPC = this.isFromPc,
                FLOW_LOG = this.flow_log
            });
        }
        private void RandomizeAllocation()
        {
            var approvalSetup = context.TBL_APPROVAL_SETUP.FirstOrDefault();
            if(approvalSetup.USEROUNDROBIN == true)
            {
                List<StaffAllocatedjob> staffAllocations = new List<StaffAllocatedjob>();

                if(this.toStaffId != null) { return; }

                if(this.lastOpenRequest?.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && (this.StatusId == (int)ApprovalStatusEnum.Processing || this.StatusId == (int)ApprovalStatusEnum.Pending || this.StatusId == (int)ApprovalStatusEnum.Authorised))
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
                        if (staffBusinessUnit != null && approvalSetup.ISRETAILONLYROUNDROBIN == true)
                        {
                            if (!staffBusinessUnit.BUSINESSCOMMONNAME?.ToLower().Contains("retail") ?? false) return;
                        }
                    }

                    var approvalLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == nextLevelId).ToList();
                    var roles = approvalLevel.Select(c => c.STAFFROLEID).ToList();
                    var staffInrole = context.TBL_STAFF.Where(x => roles.Contains(x.STAFFROLEID) && x.DELETED == false).ToList();

                    var approvalStaff = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.APPROVALLEVELID == nextLevelId).Select(d => d.STAFFID).ToList();
                    approvalStaff.AddRange(staffInrole.Select(d => d.STAFFID).ToList());

                    if (!context.TBL_STAFF_ROLE.Where(x => roles.Contains(x.STAFFROLEID) && x.APPROVALFLOWTYPEID == (short)ApprovalFlowTypeEnum.ROUNDROBIN).Any())
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

                            var isOnRelief = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == item && x.ENDDATE > DateTime.Now && x.ISACTIVE).Any();
                            staffAllocation.isOnRelief = isOnRelief;
                            staffAllocations.Add(staffAllocation);
                        }
                    }

                    var orderedAllocation = staffAllocations.Where(x => x.isOnRelief == false && staffInrole.Select(c=>c.STAFFID).Contains(x.staffId)).OrderBy(x=>x.pendingJobCount).FirstOrDefault();
                    if(this.toStaffId == null) { this.toStaffId = orderedAllocation?.staffId; }
                }
            }
          
        }

        private void AllocateBySBU()
        {
            List<StaffAllocatedjob> staffAllocations = new List<StaffAllocatedjob>();

            if (this.toStaffId != null) { return; }

            if (this.lastOpenRequest?.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && (this.StatusId == (int)ApprovalStatusEnum.Processing || this.StatusId == (int)ApprovalStatusEnum.Pending || this.StatusId == (int)ApprovalStatusEnum.Authorised))
            {
                var pendingTrail = context.TBL_APPROVAL_TRAIL.Where(x =>
                                  x.COMPANYID == this.companyId
                                  && x.OPERATIONID == this.operationId
                                  && x.RESPONSESTAFFID == null
                                  && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                                  ).ToList();

                if (this.businessUnitId == null) return;

                var businessUnit = context.TBL_PROFILE_BUSINESS_UNIT.Find(this.businessUnitId);
                var approvalLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == nextLevelId).ToList();
                var roles = approvalLevel.Select(c => c.STAFFROLEID).ToList();
                var staffInrole = context.TBL_STAFF.Where(x => roles.Contains(x.STAFFROLEID) && ((x.BUSINESSUNITID == this.businessUnitId && x.BUSINESSUNITID != null) || x.MISCODE.Trim() == businessUnit.BUSINESSUNITINITIALS) && x.DELETED == false).ToList();

                var approvalStaff = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.APPROVALLEVELID == nextLevelId).Select(d => d.STAFFID).ToList();
                approvalStaff.AddRange(staffInrole.Select(d => d.STAFFID).ToList());

                if (!context.TBL_STAFF_ROLE.Where(x => roles.Contains(x.STAFFROLEID) && x.APPROVALFLOWTYPEID == (short)ApprovalFlowTypeEnum.SBUROUTING).Any())
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
                SaveFlowLog("After AllocateBySBU");

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

        private bool IsInAllGridLevels(IEnumerable<WorkflowSetup> grid, int staffId)
        {
            foreach(var level in grid)
            {
                var levelRole = context.TBL_APPROVAL_LEVEL.Find(level.ApprovalLevelId).STAFFROLEID;
                var staff = context.TBL_STAFF.Where(x=>x.STAFFID == staffId && x.DELETED == false).FirstOrDefault();
                var levelRoleIsStaffRole = levelRole == staff.STAFFROLEID;
                var staffInLevelStaffs = context.TBL_APPROVAL_LEVEL_STAFF.Any(l => l.STAFFID == staffId && l.APPROVALLEVELID == level.ApprovalLevelId);
                if (!levelRoleIsStaffRole && !staffInLevelStaffs)
                {
                    return false;
                }
            }
            return true;
        }

        private void MakerCheckerControl()
        {
            if (this.approvalGrid.Count() > 1 &&  statusId == (int)ApprovalStatusEnum.Approved && newStateId == (int)ApprovalState.Ended)
            {
                var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
                var isReviewer = (level?.ISPOSTAPPROVALREVIEWER ?? false);
                var firstRequest = trailLog.OrderBy(x => x.APPROVALTRAILID).FirstOrDefault();
                if (firstRequest.REQUESTSTAFFID == this.staffId && IsInAllGridLevels(this.approvalGrid, this.staffId) && !isReviewer) throw new SecureException("You cannot approve a process you initiated!");
            }

            var currentLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == this.fromLevelId).FirstOrDefault();
            var destinationLevel = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == this.nextLevelId).FirstOrDefault();

            if (this.lastOpenRequest != null && this.lastOpenRequest.APPROVALSTATUSID != (short)ApprovalStatusEnum.Referred && this.statusId != (short)ApprovalStatusEnum.Referred)
            {
                
                if(this.statusId != (short)ApprovalStatusEnum.Approved && this.statusId != (short)ApprovalStatusEnum.Referred && this.newStateId != (short)ApprovalState.Ended)
                {
                    if(currentLevel != null && destinationLevel != null)
                    {
                        if(currentLevel.GROUPID == destinationLevel.GROUPID && currentLevel.POSITION > destinationLevel.POSITION)
                        {
                            throw new SecureException("Network Error, Kindly reload the page.");
                        }
                    }

                    if(!context.TBL_APPROVAL_GROUP_MAPPING.Where(x=>x.OPERATIONID == this.operationId).Select(x => x.GROUPID).Contains(currentLevel.GROUPID))
                    {
                        throw new SecureException("Target level is not in the same workflow group setup.");
                    }
                }

                if (this.statusId == (short)ApprovalStatusEnum.Approved || this.newStateId == (short)ApprovalState.Ended)
                {
                    var currentGridRecord = approvalGrid.Where(x => x.ApprovalLevelId == currentLevel.APPROVALLEVELID).FirstOrDefault();
                    var destinationGridRecord = approvalGrid.Where(x => x.ApprovalLevelId == destinationLevel?.APPROVALLEVELID).FirstOrDefault();
                    if(currentGridRecord != null && destinationGridRecord != null && currentGridRecord.GroupPosition < destinationGridRecord.GroupPosition)
                    {
                        throw new SecureException("You cannot move transaction downward on workflow groups.");
                    }
                }
            }
        }

        public void ResolveExternalFlowLoop(TBL_APPROVAL_TRAIL request, TBL_APPROVAL_TRAIL initiatorRequest)
        {
            if(this.StatusId != (int)ApprovalStatusEnum.Referred && this.lastOpenRequest.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && this.referredLog.Count <= 0){ return; }
            if (this.StatusId == (int)ApprovalStatusEnum.Referred )
            {
                this.referBackStateId = (short)ApprovalState.Initiation;

                if (this.nextLevelId == null )
                {
                    this.fromLevelId = request.TOAPPROVALLEVELID;
                    this.nextLevelId = request.TOAPPROVALLEVELID; 
                    this.loopedStaffId = (this.loopedStaffId != null && this.loopedStaffId > 0) ? this.loopedStaffId : initiatorRequest.REQUESTSTAFFID;
                    this.toStaffId = null;
                    //this.toStaffId = staffId;
                    //this.initiatorOrLooped = true;
                }
            }

            if (this.lastOpenRequest.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && this.StatusId == (int)ApprovalStatusEnum.Referred)
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

            if (this.lastOpenRequest.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && this.StatusId != (int)ApprovalStatusEnum.Referred)
            {
                if (this.referredLog.Count <= 0)
                {
                    request.REFEREBACKSTATEID = (short)ApprovalState.Ended;
                    this.toStaffId = request.REQUESTSTAFFID;
                } 

                this.nextLevelId = request.FROMAPPROVALLEVELID;
                //if(this.referredLog.Count <= 0)this.toStaffId = request.REQUESTSTAFFID;
            }

            if (this.referredLog.Count > 0 && this.StatusId != (int)ApprovalStatusEnum.Referred)
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

        private bool RequestIsLoopedStaffReturn()
        {
            if(lastOpenRequest.FROMAPPROVALLEVELID == lastOpenRequest.TOAPPROVALLEVELID && lastOpenRequest.LOOPEDSTAFFID > 0 && this.StatusId != (int)ApprovalStatusEnum.Referred)
            {
                return true;
            }
            return false;
        }

        private int? ResolveReroute(int? toStaffId)
        {
            if (toStaffId == this.toStaffId) throw new SecureException("Already with staff!");
            var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId && x.DELETED == false);
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

            if (lastOpenRequest != null && lastOpenRequest.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && lastOpenRequest.LOOPEDSTAFFID != null)
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
                var p = context.TBL_STAFF.Where(x=>x.STAFFID == this.toStaffId && x.DELETED == false).FirstOrDefault();
                response.nextPersonName = p?.STAFFCODE + " -- " + p?.FIRSTNAME + " " + p?.MIDDLENAME + " " + p?.LASTNAME;
            }

            if (this.loopedStaffId != null && this.loopedStaffId > 0)
            {
                var p = context.TBL_STAFF.Where(x=>x.STAFFID == this.loopedStaffId && x.DELETED == false).FirstOrDefault();
                if (p == null)
                {
                    var staff = context.TBL_STAFF.Where(x => x.STAFFID == this.loopedStaffId).FirstOrDefault();
                    throw new ConditionNotMetException("Staff " + staff.FIRSTNAME + " " + staff.LASTNAME + " has been deleted");
                }
                response.nextPersonId = this.loopedStaffId;
                response.nextLevelName = p.TBL_STAFF_ROLE.STAFFROLENAME;
                response.nextPersonName = p.STAFFCODE + " -- " + p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME;
            }
            response.responseMessage = SetResponseMessage(response);
        }

        private String SetResponseMessage(WorkflowResponse response, string itemHeading = "")
        {
            var statuses = context.TBL_APPROVAL_STATUS.ToList();
            //var isFinishing = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == targetId && x.OPERATIONID == operationId && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Finishing && x.RESPONSESTAFFID == null).ToList();
            if (response.stateId != (int)ApprovalState.Ended)
            {
                if (response.statusId == (int)ApprovalStatusEnum.Referred)
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextPersonName + ", " + response.nextLevelName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextLevelName;
                    }
                }
                else
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextPersonName + ", " + response.nextLevelName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextLevelName;
                    }
                }
            }
            else
            {
                if (this.ignorePostApprovalReviewer == false && this.reviewerLevelId > 0 && response.statusId == (int)ApprovalStatusEnum.Approved)
                {
                    var toLevel = context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == this.reviewerLevelId);
                    var nextLevelName = toLevel.LEVELNAME;
                    response.nextLevelName = nextLevelName;
                    return "The " + itemHeading + " request has been APPROVED and SENT to " + nextLevelName;
                }
                else
                {
                    if (response.statusId == (int)ApprovalStatusEnum.Approved)
                    {
                        return "The " + itemHeading + " request has been APPROVED successfully";
                    }
                    else
                if (response.statusId == (int)ApprovalStatusEnum.Closed)
                    {
                        var status = statuses.FirstOrDefault(s => s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Closed);
                        if (status != null)
                        {
                            return "The " + itemHeading + " request has been " + status.APPROVALSTATUSNAME + " successfully";
                        }
                        return "The " + itemHeading + " request has been CLOSED successfully";
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been DISAPPROVED successfully";
                    }
                }
            }
        }
        //worked on by ifeanyi and zino on 23/06/2021 for account officer offer letter (productId was added)
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
            int? businessUnitId,
            int? finalLevel = null,
            int amount = 0,
            int? productId = null
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
            this.productId = productId;
            this.comment = comment;
            this.externalInitialization = external;
            this.deferredExecution = deferred;
            this.sameDesk = sameDesk;
            this.isFlowTest = isFlowTest;
            this.statusId = (int)ApprovalStatusEnum.Pending;
            this.businessUnitId = businessUnitId;
            this.finalLevel = finalLevel;
            this.amount = amount;

            LogActivity();
        }

        private void InitializeOperation()
        {
            // CAREFUL NOT TO OVERRIDE SUPPLIED values!!!!!!!!
            // set those before calling in
            //this.skipLimitsCheck = false;
            this.fromLevelId = null;
            this.newStateId = (int)ApprovalState.Processing;
            if (this.statusId == (int)ApprovalStatusEnum.Pending) this.statusId = (int)ApprovalStatusEnum.Processing;
            this.operation = context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID == this.operationId);
            if ((this.operation?.USEFACILITYAMOUNTONLY ?? false) && this.facilityAmount > 0)
            {
                this.Amount = this.facilityAmount;
                this.levelBusinessRule.Amount = this.facilityAmount;
                this.levelBusinessRule.PepAmount = this.facilityAmount;
            }
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
            if(lastOpenRequest.LOOPEDSTAFFID != null) { return; }

            if (toId == null) throw new SecureException("Unable to resolve destination level!");
            if (fromId == null) return;
            var referrerGroup = context.TBL_APPROVAL_LEVEL.Find(fromId);
            var recepientGroup = context.TBL_APPROVAL_LEVEL.Find(toId);
            if (referrerGroup.GROUPID != recepientGroup.GROUPID && lastOpenRequest.APPROVALSTATUSID != (short)ApprovalStatusEnum.Referred && this.referredLog.Count <= 0)
            {
                this.toStaffId = referrerId;
                this.nextLevelId = fromId;
            }
        }

        private void ValidateAgainstAlreadyClosedProcess()
        {
            if (this.statusId == (int)ApprovalStatusEnum.Referred || this.referredLog.Count > 0)
            {//to take care of refer backs
                return;
            }
            var allRelatingRequestsDescending = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == this.targetId && t.OPERATIONID == this.operationId).OrderByDescending(t => t.APPROVALTRAILID).ToList();

            if (this.newStateId == (int)ApprovalState.Ended && this.statusId == (int)ApprovalStatusEnum.Closed)
            {// to prevent closing an already closed process
                if(allRelatingRequestsDescending.Exists(r => r.APPROVALSTATEID == (int)ApprovalState.Ended && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Closed))
                {
                    new SecureException("The process is closed already!");
                }
            }

            if (allRelatingRequestsDescending.Exists(r => r.APPROVALSTATEID == (int)ApprovalState.Ended && (r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Closed || r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)))
            {// to prevent general working on an already approved/closed process
                new SecureException("The process is ended already!");
            }

            //if (this.newStateId != (int)ApprovalState.Ended || this.statusId != (int)ApprovalStatusEnum.Approved)
            //{// this should be redundant by now!!
            //    return;
            //}
                
        }

        private void FurtherValidations()
        {
            ValidateAgainstAlreadyClosedProcess();
        }

        private void ValidateVote()
        {
            if (this.statusId == (int)ApprovalStatusEnum.Referred)
            {
                this.vote = (int)ApprovalStatusEnum.Referred;
            }
            else if (this.statusId == (int)ApprovalStatusEnum.Disapproved)
            {
                this.vote = (int)ApprovalStatusEnum.Disapproved;
            }
            if (this.statusId == (int)ApprovalStatusEnum.Reroute)
            {
                this.vote = (int)ApprovalStatusEnum.Reroute;
            }
            if (this.statusId == (int)ApprovalStatusEnum.Escalated)
            {
                this.vote = (int)ApprovalStatusEnum.Escalated;
            }
            else
            {
                this.vote = (int)ApprovalStatusEnum.Approved;
            }
        }

        private void ValidateAgainstReinitiationOfClosedProcess()//might be redundant soon
        {
            if (currentStateId != (int)ApprovalState.Initiation || lastOpenRequest != null)
            {
                return;
            }
            var allRelatingRequestsDescending = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == this.targetId && t.OPERATIONID == this.operationId).OrderByDescending(t => t.APPROVALTRAILID).ToList();
            if (allRelatingRequestsDescending.Count > 0)
            {
                foreach(var req in allRelatingRequestsDescending)
                {
                    if (req.APPROVALSTATEID != (int)ApprovalState.Ended)
                    {
                        continue;
                    }
                    if (IsNormalEnd(req))
                    {
                        new SecureException("The process is closed!");
                    }
                }
            }
        }

        private void ValidateSourceConfiguration()
        {
            bool valid = true;
            if (this.staffId > 0 && this.fromLevelId > 0)
            {
                valid = general.GetStaffApprovalLevelIds((int)this.staffId, this.operationId).ToList().Contains((int)this.fromLevelId);
                if (valid == false) new SecureException("Source Staff is NOT in the Source approval level");
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

        private void ValidateAgainstWorkFlowAnomalies()
        {
            ValidateSourceConfiguration();

            ValidateDestinationConfiguration();

            ValidateAgainstReinitiationOfClosedProcess();
        }

        private bool IsNormalEnd(TBL_APPROVAL_TRAIL request)
        {
            if (request.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && request.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
            {
                return true;
            }
                return false;
        }

        private bool ResolveLevelConfigurations()
        {
            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId);
            
            approvalGrid = approvalLevels;
            SaveFlowLog("After Get Approval Levels");

            next = approvalLevels.FirstOrDefault();

            if (sameDesk) 
            {
                var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId && x.DELETED == false);
                next = approvalLevels.FirstOrDefault(x => x.DefaultRoleId == user.STAFFROLEID);
            }

            //if(next.ISPOSTAPPROVALREVIEWER == true) { }

            if (this.nextLevelId != null) next = approvalLevels.FirstOrDefault(x => x.ApprovalLevelId == (int)this.nextLevelId);
            SaveFlowLog("After Initial Next is Set");

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

            if (this.fromLevelId > 0) // check if staff in level
            {
                level = approvalLevels.Where(x => x.ApprovalLevelId == this.fromLevelId).FirstOrDefault();
                if (level == null)
                {
                    throw new SecureException("This Approval Level is not in the workflow setup!");
                }

                //if (level?.ISPOSTAPPROVALREVIEWER == true && lastRequest.APPROVALSTATUSID != (short)ApprovalStatusEnum.Referred) this.statusId = (int)ApprovalStatusEnum.Closed;

                var staff = level.Staff.Where(x => x.STAFFID == this.staffId); // check if staff is in approval_level_staff

                TBL_STAFF defaultRole = null;
                if (staff.Any() == false)
                {
                    defaultRole = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == this.staffId && x.STAFFROLEID == level.DefaultRoleId && x.DELETED == false);
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

                if (staff.Any() == false && defaultRole == null && relieverStaff == null && IsClassifiedReferBack == false)
                {
                    throw new SecureException("You are not in the current workflow level " + level.Level.LEVELNAME);
                }

                this.neededNumberOfApproval = level.NumberOfApprovals;
                this.currentlevel = level;
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
                var currentLevel = this.currentlevel = approvalLevels.Where(x => x.ApprovalLevelId == this.fromLevelId).FirstOrDefault();
                if (currentLevel == null) throw new SecureException("This Approval Level is not in the workflow setup!");
                next = approvalLevels.FirstOrDefault(x =>
                    (x.GroupPosition > currentLevel.GroupPosition) // next group
                    || (x.LevelPosition > currentLevel.LevelPosition && x.GroupPosition == currentLevel.GroupPosition) // same group
                    );
            SaveFlowLog("if this.nextLevelId == null && fromLevelId != null");
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
             SaveFlowLog("Else of (if this.nextLevelId == null && fromLevelId != null)");
            }

            if (next == null) // end of process
            {
                this.nextLevelId = null;
                SaveFlowLog("if (next == null)");
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
            if(this.statusId == (int)ApprovalStatusEnum.Referred)
            {
                return true;
            }
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
            this.statusId = status == (int)ApprovalStatusEnum.Disapproved ? (int)ApprovalStatusEnum.Processing : status == (int)ApprovalStatusEnum.Finishing ? status : status == (int)ApprovalStatusEnum.Escalated ? status : (int)ApprovalStatusEnum.Authorised;
            this.newStateId = (int)ApprovalState.Processing;
        }

        private bool ContainsPostReviewerAsFromApprovalLevel(List<TBL_APPROVAL_TRAIL> trails)
        {
            if (trails.Count == 0)
            {
                return false;
            }

            foreach(var t in trails)
            {
                if (t.FROMAPPROVALLEVELID == null)
                {
                    continue;
                }
                if (t.TBL_APPROVAL_LEVEL.ISPOSTAPPROVALREVIEWER)
                {
                    return true;
                }
            }
            return false;
        }

        private void EndProcess(int status)
        {
            if(lastOpenRequest?.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && lastOpenRequest?.LOOPEDSTAFFID != null) { maintainFlowStatus();  return; }
            if (this.statusId != (int)ApprovalStatusEnum.Disapproved && ContainsPostReviewerAsFromApprovalLevel(this.referredLog))
            {//to prevent duplicate ending of a workflow again when postReviewer refers back
                return;
            }

            if (this.nextLevelId > 0)
            {//to enable isReviewer trail to be logged later
                var level = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
                var isReviewer = (level?.ISPOSTAPPROVALREVIEWER ?? false);
                if (isReviewer)
                {
                    this.nextIsReviewer = true;
                    this.reviewerLevelId = this.nextLevelId;
                }
            }
            else
            {
                if (this.fromLevelId > 0)
                {//to properly convert finishing to closed
                    var currentLevel = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
                    var isReviewer = (currentLevel?.ISPOSTAPPROVALREVIEWER ?? false);
                    if (isReviewer)
                    {
                        status = (int)ApprovalStatusEnum.Finishing;
                    }
                }
            }
            this.statusId = ResolveLastStatus(status);
            this.nextLevelId = null; // even if there are other higher level which have been resolve prior
            this.newStateId = (int)ApprovalState.Ended;
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
                case 10: return 11;
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
            var s = context.TBL_STAFF.Where(x => x.STAFFID == staffId && x.DELETED == false).FirstOrDefault();
            line.Add(new ReportingLine
            {
                staffId = (int)s.STAFFID,
                levelRoleId = (int)s.STAFFROLEID,
                levelIds = context.TBL_APPROVAL_LEVEL.Where(x => x.STAFFROLEID == s.STAFFROLEID).Select(x => (int)x.APPROVALLEVELID).ToList(),
            });
            if (s.SUPERVISOR_STAFFID == null) return;
            if (staffId == s.SUPERVISOR_STAFFID)
            {
                throw new SecureException("This Staff, "+ s.STAFFCODE + " cannot be setup as self supervisor!");
            }
            GetReportingLine((int)s.SUPERVISOR_STAFFID);
        }

        private int? GetReportingLineStaffId() // if workflow is forced to use organogram
        {
            var businessRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(s => s.STAFFROLEID).ToList();
            var fromStaff = context.TBL_STAFF.Where(x=>x.STAFFID == this.staffId && x.DELETED == false).FirstOrDefault();
            if (next == null) { return null; }
            if (this.toStaffId != null) { return toStaffId; }
            //if (this.toStaffId != null) { return null; }
            //if (this.externalInitialization == true && !businessRoleIds.Contains(fromStaff.STAFFROLEID)) { return null; }
            var staff = context.TBL_STAFF.Where(x => x.STAFFID == this.staffId && x.DELETED == false).FirstOrDefault();
            if (staff == null) { return null; }
            GetReportingLine(staffId);
            //if (this.statusId == (int)ApprovalStatusEnum.Referred || !businessRoleIds.Contains(next.DefaultRoleId ?? 0) || (this.fromLevelId == null && !businessRoleIds.Contains(fromStaff.STAFFROLEID)))
            if (this.statusId == (int)ApprovalStatusEnum.Referred || !businessRoleIds.Contains(next.DefaultRoleId ?? 0) || (!businessRoleIds.Contains(fromStaff.STAFFROLEID)))
            {
                return null;
            }
            ReportingLine super = line.FirstOrDefault(x => x.levelRoleId == next.DefaultRoleId && x.levelIds.Contains(next.ApprovalLevelId));
            if (super == null)
            {
                if (this.isFlowTest)
                {
                    return null;
                }
                throw new SecureException("No Staff Was Setup as Your Supervisor! Please select the approver to route your request to.");
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
            

            //if (IsPresetFinalLevel()) { return; }

            if (ThereIsPresetFinalLevel())
            {//to take care of final approv
                    if (this.statusId == (short)ApprovalStatusEnum.Approved)
                    {
                        this.ContinueProcess((int)ApprovalStatusEnum.Authorised);
                    }
                    return;
            }

            if (this.skipLimitsCheck == true)
            {
                if (this.statusId == (short)ApprovalStatusEnum.Approved)
                {
                    this.ContinueProcess((int)ApprovalStatusEnum.Authorised);
                    return;
                }
            }
            //throw new Exception("");

            if (this.statusId == (int)ApprovalStatusEnum.Escalated)
            {
                return;
            }
            //if (request.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred && this.nextLevelId == null) { this.nextLevelId = this.fromLevelId; }//temporary fix o!!!!!
            //if (this.nextLevelId != null && this.amount > 0 && ActionIsApprovalDecision() || (ActionIsApprovalDecision()) || CanApproveAndNextIsPostReviewer())
            if ((ActionIsApprovalDecision()) || CanApproveAndNextIsPostReviewer())
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
                    else
                    {
                        this.EndProcess(this.statusId);
                    }
                }
            }
            
        }

        private bool ThereIsPresetFinalLevel()
        {
            return this.finalLevel > 0;
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
            if (level.MAXIMUMAMOUNT == 0 && level.TENOR > 0) { return true; } //to pass access bnk only tenor setup as amt>0 and there is no amt setup only tenor setup
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

        //private bool WithinLevelStaffLimit(TBL_APPROVAL_LEVEL level)
        //{

        //}

        private bool WithinAllLimits()
        {
            if (this.level.IsPostApprovalReviewer == true) return true;
            var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
            if (level == null ) { throw new SecureException("The user is not in the workflow setup!"); } // redundant - wouldnt get here in the first place
            if (this.nextLevelId != null)
            {
                var next = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
                if (next.ISPOSTAPPROVALREVIEWER == true) return true;
            }
            if (this.disputed == true && level.CANRESOLVEDISPUTE != true) { return false; }
            return WithinTenorLimit(level) == true
                && WithinMaximumLimit(level) == true
                && WithinInvestmentGradeLimit(level) == true
                && WithinPoliticallyExposedLimit(level) == true;
        }

        private void LastApproverCheck()
        {
            var level = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
            //if (IsLastApprover(level) && (this.statusId == (int)ApprovalStatusEnum.Processing || this.statusId == (int)ApprovalStatusEnum.Authorised || this.statusId == (int)ApprovalStatusEnum.Finishing))
            if (IsLastApprover(level) && (this.statusId == (int)ApprovalStatusEnum.Processing || this.statusId == (int)ApprovalStatusEnum.Pending || this.statusId == (int)ApprovalStatusEnum.Authorised || this.statusId == (int)ApprovalStatusEnum.Escalated))
            {
                this.statusId = (int)ApprovalStatusEnum.Approved;
            }
        }

        private bool IsLastApprover(TBL_APPROVAL_LEVEL level)
        {
            var approvalLevels = GetWorkflowSetup(this.operationId, this.productClassId, this.productId).ToList();
            if (IsLastLevel(approvalLevels, level))
            //if (IsLastLevel(approvalLevels, level) && (level.CANAPPROVE))
                //if (IsLastLevel(approvalLevels, level) && level.CANAPPROVE && !(level.MAXIMUMAMOUNT > 0))
            {
                this.skipLimitsCheck = false;// to remove it from preventing ending of workflow if is last level
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
            if ((this.nextLevelId == null && IsLastApprover(level)) || (this.statusId == (int)ApprovalStatusEnum.Closed))//for test!!
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
                    this.ContinueProcess(this.statusId);
                    return statusId;
                }
            }

            //if (this.nextLevelId > 0 && this.statusId != (int)ApprovalStatusEnum.Referred)
            //{
            //    var nextLevel = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
            //    if (nextLevel.ISPOSTAPPROVALREVIEWER == true)
            //    {
            //        this.statusId = (int)ApprovalStatusEnum.Finishing;
            //    }
            //}

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

            //if (this.statusId == (int)ApprovalStatusEnum.Closed && ) // if its still approval decision end process
            //{
            //    this.EndProcess(this.statusId);
            //    return statusId;
            //}

            if (ActionIsApprovalDecision()) // if its still approval decision end process
            {
                //if (this.skipLimitsCheck)
                //{ //skiplimits will only be approved by the last level
                //    return statusId;
                //}
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

        private bool CanApproveAndNextIsPostReviewer()
        {
            if (this.fromLevelId > 0 && this.nextLevelId > 0)
            {
                var currentLevel = context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId);
                var nextLevel = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);
                return (nextLevel.ISPOSTAPPROVALREVIEWER && currentLevel.CANAPPROVE);
            }
            return false;
        }

        private bool ActionIsApprovalDecision()
        {
            //return (this.statusId == (int)ApprovalStatusEnum.Approved || this.statusId == (int)ApprovalStatusEnum.Disapproved || this.statusId == (int)ApprovalStatusEnum.Authorised);
            
            return (this.statusId == (int)ApprovalStatusEnum.Approved || this.statusId == (int)ApprovalStatusEnum.Disapproved );
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

            TBL_APPROVAL_TRAIL initiator = new TBL_APPROVAL_TRAIL();
            

            List<WorkflowSetup> levels = new List<WorkflowSetup>();

             levels = mappings
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
                               LevelBusinessRule = x.Level.TBL_APPROVAL_BUSINESS_RULE,
                               AllowMultipleInitiator = x.Mapping.ALLOWMULTIPLEINITIATOR,
                               RoleIdToRoute = x.Level.ROLEIDTOROUTE,
                               IsPostApprovalReviewer = x.Level.ISPOSTAPPROVALREVIEWER,
                               InvestmentGradeLimit = x.Level.INVESTMENTGRADEAMOUNT,
                               StandardGradeLimit = x.Level.STANDARDGRADEAMOUNT,
                               RenewalLimit = x.Level.RENEWALLIMIT,
                               IsSyndicated = x.Level.ISSYNDICATED
                           })
                           .OrderBy(x => x.GroupPosition)
                           .ThenBy(x => x.LevelPosition)
                           .ToList();

            

            List<WorkflowSetup> grid = new List<WorkflowSetup>();

            int n = 0;
            foreach (WorkflowSetup level in levels)
            {
                if (mappings.Where(x => x.GROUPID == level.Group.GROUPID && x.ALLOWMULTIPLEINITIATOR == true ).Any())
                {
                    initiator = GetAllTrail().OrderBy(x => x.APPROVALTRAILID).FirstOrDefault();
                    if (initiator != null)
                    {
                        var initiatorStaff = context.TBL_STAFF.Find(initiator?.REQUESTSTAFFID);
                        if (initiatorStaff != null && level.ROLEIDTOROUTE != initiatorStaff.STAFFROLEID && level.ROLEIDTOROUTE != null)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(this.staffId > 0)
                        {
                            var currentRequestStaff = context.TBL_STAFF.Where(x=>x.STAFFID == this.staffId && x.DELETED == false).FirstOrDefault();
                            if (currentRequestStaff != null && level.ROLEIDTOROUTE != currentRequestStaff.STAFFROLEID && level.ROLEIDTOROUTE != null)
                            {
                                continue;
                            }
                        }
                    }
                    // levels = levels.Where(x => x.ROLEIDTOROUTE == requestStaff.STAFFROLEID || x.ROLEIDTOROUTE == null).ToList();
                }
                var testField = level.Level.LEVELNAME;

                if (level.LevelBusinessRuleId != null && !LevelBusinessRuleIsValid(level.LevelBusinessRule)) { continue; }
                if (level.LevelBusinessRuleId != null && !ExecuteStandardBusinessRule(level.LevelBusinessRule)) { continue; }
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

            this.WorkflowSetup = grid;
            //throw new SecureException("");
            return grid;
        }

        private bool LevelBusinessRuleIsValid(TBL_APPROVAL_BUSINESS_RULE rule)
        {
            
            if (levelBusinessRule == null) return true;

            bool validity = true;
            bool flagChecked = true;
            bool limitChecked = true;
            decimal pepAmount = rule.PEPAMOUNT ?? 0;
            decimal minimumAmount = rule.MINIMUMAMOUNT ?? 0;
            decimal maximumAmount = rule.MAXIMUMAMOUNT ?? 0;
            int tenor = rule.TENOR ?? 0;

            if ((minimumAmount > 0 && maximumAmount == 0) && (minimumAmount < levelBusinessRule.Amount)) limitChecked = true;
            if ((minimumAmount == 0 && maximumAmount > 0) && (levelBusinessRule.Amount <= maximumAmount)) limitChecked = true;
            if ((minimumAmount > 0 && maximumAmount > 0) && (minimumAmount < levelBusinessRule.Amount && levelBusinessRule.Amount <= maximumAmount)) limitChecked = true;

            if ((rule.PEP && pepAmount > 0) && (levelBusinessRule.Pep && pepAmount <= levelBusinessRule.PepAmount)) limitChecked = flagChecked = true;

            if ((tenor > 0) && (tenor >= levelBusinessRule.tenor)) limitChecked = true;//by ify to be used later

            //if (rule.PEP && levelBusinessRule.Pep == true) flagChecked = true;
            if (rule.INSIDERRELATED && levelBusinessRule.InsiderRelated == true) flagChecked = true;
            if (rule.PROJECTRELATED && levelBusinessRule.ProjectRelated == true) flagChecked = true;
            if (rule.ONLENDING && levelBusinessRule.OnLending == true) flagChecked = true;
            if (rule.INTERVENTIONFUNDS && levelBusinessRule.InterventionFunds == true) flagChecked = true;
            if (rule.ORRBASEDAPPROVAL && levelBusinessRule.OrrBasedApproval == true) flagChecked = true;
            if (rule.WITHINSTRUCTION && !levelBusinessRule.WithInstruction) flagChecked = true;
            if (rule.DOMICILIATIONNOTINPLACE && levelBusinessRule.DomiciliationNotInPlace == true) flagChecked = true;
            if (rule.ESRM && levelBusinessRule.esrm) flagChecked = true;
            if (rule.ISFORCONTINGENTFACILITY && levelBusinessRule.isContingentFacility) flagChecked = true;
            if (rule.ISSYNDICATED && levelBusinessRule.isSyndicated) flagChecked = true;
            //if (rule.ISFORREVOLVINGFACILITY && levelBusinessRule.isRevolvingFacility) flagChecked = true;
            if (rule.ISFORRENEWAL)
            {
                if (levelBusinessRule.isRenewal)
                {
                    flagChecked = true;
                }
                else if (!levelBusinessRule.isRenewal)
                {// very necessary only renewals should pass through if isForRenewals is selected
                    flagChecked = false;
                    limitChecked = false;
                }
            }
            
            if (rule.EXEMPTRENEWAL)
            {
                if (!levelBusinessRule.isRenewal)
                {
                    flagChecked = true;
                }
                else if (levelBusinessRule.isRenewal)
                {// very necessary only non-renewals should pass through if exemptRenewals is selected
                    flagChecked = false;
                    limitChecked = false;
                }
            }
            //if (rule.EXEMPTREVOLVINGFACILITY && !levelBusinessRule.isRevolvingFacility) flagChecked = true;
            if (rule.EXCLUDELEVEL && !levelBusinessRule.excludeLevel) flagChecked = true;
            if (rule.ISAGRICRELATED && levelBusinessRule.isAgricRelated) flagChecked = true;
            if (rule.EXEMPTCONTINGENTFACILITY)
            {
                if (levelBusinessRule.isContingentFacility)//if contingent, all other rules are overidden
                {
                    flagChecked = false;
                    limitChecked = false;
                }
                else
                {//if not contingent, other rules stand
                    if (minimumAmount == 0 && maximumAmount == 0)//if not contingent & no other rule exists, it passes
                    {
                        flagChecked = true;
                    }
                }
            }



            if (limitChecked && flagChecked) return limitChecked && limitChecked;
            if (limitChecked || flagChecked) return true;

            return validity;
        }


        private bool ExecuteStandardBusinessRule(TBL_APPROVAL_BUSINESS_RULE rule)
        {
            if (levelBusinessRule == null) return true;

            var expression = context.TBL_WORKFLOW_ITEM_EXPRESSION
                .Where(x => x.APPROVALBUSINESSRULEID == rule.APPROVALBUSINESSRULEID).ToList();

            //var ctr = 0;
            var totalExpression = expression.Count();
            foreach (var item in expression)
            {
                //var response = false;
                //var previousResponse = false;
                TBL_WORKFLOW_ITEM_EXPRESSION previousItem = new TBL_WORKFLOW_ITEM_EXPRESSION();
                return ResolveStandardRules(item, rule);
                //if (ctr == 0)
                //{
                //    previousResponse = response = ResolveStandardRules(item, rule);
                //}


                //if (ctr > 0 && previousItem.CONJUCTION == "OR") return response;

                //if (response = ResolveStandardRules(item, rule) != previousResponse && previousItem.CONJUCTION == "AND")
                //{
                //    return false;
                //}
                //else previousResponse = response;

                //ctr++;
                //previousItem = item;
                //if (totalExpression == ctr) return response;
            }
            return true;
        }

        private bool ResolveStandardRules(TBL_WORKFLOW_ITEM_EXPRESSION expression, TBL_APPROVAL_BUSINESS_RULE rule)
        {
            var ruleBase = context.TBL_WORKFLOW_CONTEXT.Find(expression.CONTEXTID);

            var ruleItem = context.TBL_WORKFLOW_DATA_ITEM_DEFINITION
                                    .Where(x => x.DATAITEMID == expression.DATAITEMID).FirstOrDefault();

            var list = context.TBL_WFCONTEXT_VALUE_TYPE.Where(x => x.VALUETYPENAME.ToUpper() == "LIST").FirstOrDefault()?.VALUETYPEID;
            var text = context.TBL_WFCONTEXT_VALUE_TYPE.Where(x => x.VALUETYPENAME.ToUpper() == "TEXT").FirstOrDefault()?.VALUETYPEID;
            var boolean = context.TBL_WFCONTEXT_VALUE_TYPE.Where(x => x.VALUETYPENAME.ToUpper() == "BOOLEAN").FirstOrDefault()?.VALUETYPEID;

            if (ruleBase.CONTEXTNAME.ToUpper() == "CUSTOMER")
            {
                if (ruleItem.DATAITEMNAME.ToUpper() == "BUSINESS UNIT")
                {
                    if (ruleItem.VALUETYPEID == list)
                    {
                        var comparisonString = context.TBL_OPERATORS.Where(x => x.OPERATORID == expression.COMPARISONID).FirstOrDefault()?.OPERATOR.ToString();
                        if (businessUnitId == null) businessUnitId = 0;
                        if (expression.IDVALUE == null) expression.IDVALUE = 0;
                        return Compare(businessUnitId.Value, expression.IDVALUE.Value, comparisonString);
                    }
                }
            }

            //if (ruleBase.CONTEXTNAME.ToUpper() == "COLLATERAL")
            //{
            //    //insert code here
            //    decimal coverange = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == targetId).FirstOrDefault()?.COLLATERALCOVERAGE ?? 0;
            //    if (ruleItem.DATAITEMNAME.ToUpper() == "COLLATERAL COVERAGE" && rule.ISCOLLATERALECOVERED)
            //    {
            //        if (rule.ISCOLLATERALECOVERED) return true;
            //        else { isExceptionaApprover = true; return false; }
            //    }
            //}




            return true;
        }

        public static bool Compare<T>(T value1, T value2, string str) where T : IComparable<T>
        {
            Func<T, T, bool> op = null;

            switch (str)
            {
                case "<":
                    op = (a, b) => a.CompareTo(b) < 0;
                    break;
                case ">":
                    op = (a, b) => a.CompareTo(b) > 0;
                    break;
                case "<=":
                    op = (a, b) => a.CompareTo(b) <= 0;
                    break;
                case ">=":
                    op = (a, b) => a.CompareTo(b) >= 0;
                    break;
                case "=":
                    op = (a, b) => a.CompareTo(b) == 0;
                    break;
                case "!=":
                    op = (a, b) => a.CompareTo(b) != 0;
                    break;
                default:
                    return true;
                    //throw new ArgumentException();
            }

            return op(value1, value2);
        }

        //private void SendNotifications()
        //{
        //    if (statusOnly) return;
        //    if (emailNotification || smsNotification)
        //    {
        //        int tat = next != null ? next.SlaInterval : 0;
        //        var setup = context.TBL_SETUP_GLOBAL.Find(1);
        //        var operation = context.TBL_OPERATIONS.Find(this.operationId);
        //        var applicationUrl = setup.APPLICATION_URL.Length == 0 ? "#" : setup.APPLICATION_URL;
        //        var applicationUrls = "https://credit360.accessbankplc.com";
        //        var links = "<p>Click <a href=\"" + applicationUrls + "\">here to continue...</a></p>";

        //        TBL_STAFF owner;
        //        if (trailLog.Count() == 0)
        //        {
        //            owner = context.TBL_STAFF.Find(this.staffId);
        //        }
        //        else
        //        {
        //            var trails = trailLog.OrderBy(x => x.APPROVALTRAILID);
        //            owner = context.TBL_STAFF.Find(trails.First().REQUESTSTAFFID);
        //        }

        //        int targetId = this.targetId;
        //        int operationId = this.operationId;
        //        var message = new TBL_MESSAGE_LOG();
        //        var reciever = new TBL_STAFF();
        //        string recipientName = "All";
        //        string operationName = operation == null ? "N/A" : operation.OPERATIONNAME;
        //        string messageSubject = "PENDING APPROVAL FOR " + operationName.ToUpper();
        //        string status = GetApprovalStatusName(this.statusId);
        //        string ownerMessageSubject = "YOUR INITIATED " + operationName.ToUpper() + " PROCESS HAVE BEEN " + status.ToUpper();
        //        var level = string.Empty;
        //        List<string> emails = new List<string>();

        //        if (this.fromLevelId != null) level = " by " + context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId)?.LEVELNAME;
        //        if (this.nextLevelId != null && this.toStaffId == null) recipientName = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId)?.LEVELNAME;

        //        if (this.toStaffId != null)
        //        {
        //            reciever = context.TBL_STAFF.Find(this.toStaffId);
        //            recipientName = reciever?.FIRSTNAME;
        //            this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.toStaffId && DateTime.Now <= x.ENDDATE && x.ISACTIVE && x.DELETED == false).Select(x=>x.RELIEFSTAFFID).FirstOrDefault();
        //        }
        //        else if (this.loopedStaffId != null)
        //        {
        //            reciever = context.TBL_STAFF.Find(this.loopedStaffId);
        //            recipientName = reciever.FIRSTNAME;
        //            this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.loopedStaffId && DateTime.Now <= x.ENDDATE && x.ISACTIVE && x.DELETED == false).Select(x => x.RELIEFSTAFFID).FirstOrDefault();
        //        }
        //        else
        //        {
        //            if (this.nextLevelId != null)
        //            {
        //                var nextLevel = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId);

        //                var actorIds = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == operationId && t.TARGETID == targetId)
        //                    .Join(context.TBL_STAFF, t => t.REQUESTSTAFFID, s => s.STAFFID, (t, s) => new { t, s })
        //                    .Select(x => x.s.EMAIL).ToList();

        //                var levelStaffEmails = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false && x.APPROVALLEVELID == nextLevel.APPROVALLEVELID)
        //                    .Select(x => x.TBL_STAFF.EMAIL)
        //                    .Distinct().ToList();

        //                var businessRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(r => r.STAFFROLEID).ToList();
        //                var nextLvlRoleIsABusinessRole = businessRoleIds.Contains(nextLevel.STAFFROLEID ?? 0);
        //                if (nextLvlRoleIsABusinessRole)
        //                {
        //                    emails = actorIds.Union(levelStaffEmails).ToList();
        //                }
        //                else
        //                {
        //                    var nextLevelStaffEmails = context.TBL_STAFF.Where(s => s.STAFFROLEID == nextLevel.STAFFROLEID).Select(x => x.EMAIL);
        //                    emails = actorIds.Union(levelStaffEmails).Union(nextLevelStaffEmails).ToList();
        //                }

        //                if (this.reliefStaffId != null)
        //                {
        //                    var reliefRecord = context.TBL_STAFF.Find(this.reliefStaffId);
        //                    if (!(String.IsNullOrEmpty(reliefRecord.EMAIL)) && !(String.IsNullOrWhiteSpace(reliefRecord.EMAIL)))
        //                    {
        //                        emails.Add(reliefRecord.EMAIL);
        //                    }
        //                }
        //            }
        //        }

        //        var time = String.Format("{0:F}", DateTime.Now);
        //        //throw new Exception("");

        //        if (placeholders == null) placeholders = new AlertPlaceholders();

        //        var ownerMessageBody = $"Dear {owner.FIRSTNAME}, <br /><br />" +
        //                    $"The {operationName} approval process you initiated have been {status}{level}. <br /><br />" +
        //                    $"{placeholders.customerName}" +
        //                    $"{placeholders.referenceNumber}" +
        //                    $"{placeholders.facilityType}" +
        //                    $"{placeholders.operationName}" +
        //                    $"{placeholders.branchName}" +
        //                    $"{placeholders.locationName}" +
        //                    $"<p>Time: { time }</p>"
        //                    ;

        //        var messageBody = $"Dear {recipientName}, <br /><br />" +
        //                    $"You have a new pending {operationName} approval request. <br /><br />" +
        //                    $"{placeholders.customerName}" +
        //                    $"{placeholders.referenceNumber}" +
        //                    $"{placeholders.facilityType}" +
        //                    $"{placeholders.operationName}" +
        //                    $"{placeholders.branchName}" +
        //                    $"{placeholders.locationName}" +
        //                    $"<p>Time: { time }</p>"
        //                    ;

        //        if (tat > 0)
        //        {
        //            messageBody = messageBody + $"<p>TAT: { tat } hour(s)</p>";
        //            ownerMessageBody = ownerMessageBody + $"<p>TAT: { tat } hour(s)</p>";
        //        }

        //        //var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

        //        if (emailNotification)
        //        {
        //            message = new TBL_MESSAGE_LOG // INITIATOR
        //            {
        //                TOADDRESS = owner.EMAIL,
        //                MESSAGESUBJECT = ownerMessageSubject,
        //                MESSAGEBODY = ownerMessageBody + links,
        //                MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
        //                MESSAGETYPEID = (short)MessageTypeEnum.Email,
        //                FROMADDRESS = this.support,
        //                DATETIMERECEIVED = DateTime.Now,
        //                SENDONDATETIME = DateTime.Now,
        //                TARGETID = targetId,
        //                OPERATIONID = operationId
        //            };
        //            context.TBL_MESSAGE_LOG.Add(message);

        //            if (this.toStaffId != null || emails.Any())
        //            {
        //                message = new TBL_MESSAGE_LOG // RECIEVERS
        //                {
        //                    TOADDRESS = this.toStaffId != null ? (reciever?.EMAIL == null ? "N/A" : reciever?.EMAIL) : string.Join(";", emails.Distinct()),
        //                    MESSAGESUBJECT = messageSubject,
        //                    MESSAGEBODY = messageBody + links,
        //                    MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
        //                    MESSAGETYPEID = (short)MessageTypeEnum.Email,
        //                    FROMADDRESS = this.support,
        //                    DATETIMERECEIVED = DateTime.Now,
        //                    SENDONDATETIME = DateTime.Now,
        //                    TARGETID = targetId,
        //                    OPERATIONID = operationId
        //                };
        //                context.TBL_MESSAGE_LOG.Add(message);
        //            }

        //        }
        //    }
        //}

        public string ReplaceNotificationPlaceholders(string messageBody, string ownerFirstName, string recipientName, string fromLevelName, string operationName, string status, string time, string tat, string userInCopyFirstName)
        {
            if (placeholders == null)
            {
                placeholders = new AlertPlaceholders();
            }

            var applicationUrls = context.TBL_SETUP_GLOBAL.FirstOrDefault()?.APPLICATION_URL;
            var link = "<p>Click <a href=\"" + applicationUrls + "\">here to continue...</a></p>";
            string ownerFirstNameHolder = "@{{OwnerFirstName}}";
            string userInCopyFirstNameHolder = "@{{userInCopyFirstName}}";
            string recipientNameHolder = "@{{RecipientName}}";
            string currentLevelHolder = "@{{CurrentLevel}}";
            string fromLevelHolder = "@{{FromLevel}}";
            string operationNameHolder = "@{{OperationName}}";
            string statusHolder = "@{{Status}}";
            string timeHolder = "@{{Time}}";
            string tatHolder = "@{{TAT}}";
            string referenceNumberHolder = "@{{ReferenceNumber}}";
            string facilityTypeHolder = "@{{FacilityType}}";
            string customerNameHolder = "@{{customerName}}";
            string branchNameHolder = "@{{branchName}}";
            string locationNameHolder = "@{{Location}}";
            string linkHolder = "@{{Link}}";

            messageBody = messageBody.Replace(fromLevelHolder, fromLevelName);
            messageBody = messageBody.Replace(userInCopyFirstNameHolder, userInCopyFirstName);
            messageBody = messageBody.Replace(ownerFirstNameHolder, ownerFirstName);
            messageBody = messageBody.Replace(recipientNameHolder, recipientName);
            messageBody = messageBody.Replace(currentLevelHolder, response.nextLevelName);
            messageBody = messageBody.Replace(operationNameHolder, operationName);
            messageBody = messageBody.Replace(statusHolder, status);
            messageBody = messageBody.Replace(timeHolder, time);
            messageBody = messageBody.Replace(tatHolder, tat);
            messageBody = messageBody.Replace(referenceNumberHolder, placeholders.referenceNumber);
            messageBody = messageBody.Replace(facilityTypeHolder, placeholders.facilityType);
            messageBody = messageBody.Replace(customerNameHolder, placeholders.customerName);
            messageBody = messageBody.Replace(branchNameHolder, placeholders.branchName);
            messageBody = messageBody.Replace(locationNameHolder, placeholders.locationName);
            messageBody = messageBody.Replace(linkHolder, link);

            return messageBody;
        }

        public void LogWorkflowNotifications(string fromAddress, string toAddress, string messageSubject, string messageBody)
        {
            if (toAddress == null || toAddress == "") return;
            
            var message = new TBL_MESSAGE_LOG()
            {
                TOADDRESS = toAddress,
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

        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Where(s=>s.DELETED == false).Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.FIRSTNAME + " " + s.LASTNAME
                //name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
            });
        }

        public List<ApprovalTrailViewModel> GetTrailForReferBack(int applicationId, int operationId, int currentLevelId = 0)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;
            var allstaff = this.GetAllStaffNames();


            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == applicationId).ToList();
            
            trail = trail.Where(t => !(t.FROMAPPROVALLEVELID == t.TOAPPROVALLEVELID && t.LOOPEDSTAFFID > 0)).ToList();//to eliminate unnecessary referback data and avoid data duplicates

            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                targetId = x.TARGETID,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                operationId = x.OPERATIONID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
            })?.OrderBy(x => x.systemArrivalDateTime).ToList();

            if (currentLevelId == 0)
            {
                currentLevelId = data.LastOrDefault()?.toApprovalLevelId ?? 0;
            }

            while (data.Exists(d => d.approvalStateId == (int)ApprovalState.Ended && (d.approvalStatusId != (int)ApprovalStatusEnum.Approved && d.approvalStatusId != (int)ApprovalStatusEnum.Closed)))//get only un-ended trail incase of workflow ending&/change
            {
                var firstTrail = data.FirstOrDefault(t => t.approvalStateId == (int)ApprovalState.Ended && (t.approvalStatusId != (int)ApprovalStatusEnum.Approved && t.approvalStatusId != (int)ApprovalStatusEnum.Closed));
                data = data.Where(t => t.approvalTrailId > firstTrail.approvalTrailId).ToList();
            }

            var data3 = data.OrderByDescending(d => d.systemArrivalDateTime);
            //if (data.Count > 0 && currentLevelId > 0)//get only from the current level downwards
            //{
            //    var firstTrail = data.FirstOrDefault(t => t.toApprovalLevelId == currentLevelId);
            //    if (firstTrail != null)
            //    {
            //        data = data.Where(t => t.approvalTrailId <= firstTrail?.approvalTrailId).ToList();
            //    }
            //}

            if (data.Count == 0)
            {
                data = trail.Where(x => x.FROMAPPROVALLEVELID > 0).Select(x => new ApprovalTrailViewModel
                {
                    approvalTrailId = x.APPROVALTRAILID,
                    targetId = x.TARGETID,
                    systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                    responseDate = x.RESPONSEDATE,
                    systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                    responseStaffId = x.RESPONSESTAFFID,
                    requestStaffId = x.REQUESTSTAFFID,
                    operationId = x.OPERATIONID,
                    fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelId = x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                })?.OrderBy(x => x.systemArrivalDateTime).ToList();

                data3 = data.OrderByDescending(d => d.systemArrivalDateTime);
            }


            var data2 = data.ToList();
            var testData = data.ToList();
            foreach (var t in testData)//filter repeated levels as result of refer backs
            {
                var firstTrailForLevel = testData.OrderBy(x => x.approvalTrailId).FirstOrDefault(x => x.fromApprovalLevelId == t.fromApprovalLevelId);
                var multipleTrails = testData.Where(d => d.fromApprovalLevelId == firstTrailForLevel.fromApprovalLevelId && d.approvalTrailId != firstTrailForLevel.approvalTrailId).ToList();
                foreach (var tr in multipleTrails)
                {
                    data2.RemoveAll(d => d.approvalTrailId == tr.approvalTrailId);
                }
            }

            foreach (var d in data2)
            {
                var lastOccurrence = data3.FirstOrDefault(d3 => d3.fromApprovalLevelId == d.fromApprovalLevelId);
                if (lastOccurrence != null)
                {
                    d.requestStaffId = lastOccurrence.requestStaffId;
                }
            }

            data = data2;
            data.OrderByDescending(d => d.systemArrivalDateTime).ToList();

            var redundantRecordSet = data.GroupBy(d => d.requestStaffId).Where(d => d.Count() > 1).ToList();
            foreach (var r in redundantRecordSet)
            {
                var redundantRecord = r.OrderBy(d => d.approvalTrailId).FirstOrDefault();
                data.Remove(redundantRecord);
            }
            return data;
        }//Ify

        private void SendNotifications()
        {
            if (statusOnly) return;
            if (emailNotification || smsNotification)
            {
                int tat = next != null ? next.SlaInterval : 0;
                var time = String.Format("{0:F}", DateTime.Now);
                var operation = context.TBL_OPERATIONS.Find(this.operationId);
                //var applicationUrls = "https://credit360.accessbankplc.com";
                

                TBL_STAFF owner;
                if (trailLog.Count() == 0)
                {
                    owner = context.TBL_STAFF.Where(x=>x.STAFFID == this.staffId && x.DELETED == false).FirstOrDefault();
                }
                else
                {
                    var trails = trailLog.OrderBy(x => x.APPROVALTRAILID);
                    owner = context.TBL_STAFF.Find(trails.First().REQUESTSTAFFID);
                    //owner = context.TBL_STAFF.Where(x => x.STAFFID == trails.First().REQUESTSTAFFID && x.DELETED == false).FirstOrDefault();
                }

                int targetId = this.targetId;
                int operationId = this.operationId;
                var message = new TBL_MESSAGE_LOG();
                var reciever = new TBL_STAFF();
                var recieverInCopy = new TBL_STAFF();
                string recipientName = "All";
                string userInCopyFirstName = "";
                string operationName = operation == null ? "N/A" : operation.OPERATIONNAME.ToUpper();
                //string messageSubject = "PENDING APPROVAL FOR " + operationName.ToUpper();
                var messageBody = string.Empty;
                string status = GetApprovalStatusName(this.statusId).ToUpper();
                //string ownerMessageSubject = "YOUR INITIATED " + operationName.ToUpper() + " PROCESS HAVE BEEN " + status.ToUpper();
                var fromLevelName = string.Empty;
                var worflowNotificationSetups = context.TBL_WORKFLOW_NOTIFICATION.ToList();
                List<string> emails = new List<string>();

                if (this.fromLevelId != null)
                {
                    fromLevelName =  context.TBL_APPROVAL_LEVEL.Find(this.fromLevelId)?.LEVELNAME;
                }
                if (this.nextLevelId != null && this.toStaffId == null)
                {
                    recipientName = context.TBL_APPROVAL_LEVEL.Find(this.nextLevelId)?.LEVELNAME;
                }
                if (this.nextLevelId == this.fromLevelId && this.toStaffId > 0 && this.statusId != (int)ApprovalStatusEnum.Referred)
                {
                    fromLevelName = context.TBL_STAFF.Where(x => x.STAFFID == this.staffId && x.DELETED == false).FirstOrDefault()?.TBL_STAFF_ROLE?.STAFFROLENAME;
                }

                if (this.nextLevelId > 0)
                {   
                    //forNotifyOfPendingTransactions
                    var nextLevel = WorkflowSetup.FirstOrDefault(s => s.ApprovalLevelId == this.nextLevelId);
                    var levelWorkflowNotification = worflowNotificationSetups?.FirstOrDefault(n => n.GROUPOPERATIONMAPPINGID == nextLevel?.Mapping.GROUPOPERATIONMAPPINGID && n.APPROVALLEVELID == nextLevel?.ApprovalLevelId);
                    if (levelWorkflowNotification?.NOTIFYOFPENDINGAPPROVALS ?? false)
                    {
                        var alert = context.TBL_ALERT_TITLE.FirstOrDefault(a => a.ALERTTITLEID == levelWorkflowNotification.PENDINGAPPROVALALERTTITLEID && a.ISACTIVE );
                        if (alert != null)
                        {
                            if (this.toStaffId != null)
                            {
                                reciever = context.TBL_STAFF.Where(x => x.STAFFID == this.toStaffId && x.DELETED == false).FirstOrDefault();
                                recipientName = reciever?.FIRSTNAME;
                                this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.toStaffId && DateTime.Now <= x.ENDDATE && x.ISACTIVE && x.DELETED == false).Select(x => x.RELIEFSTAFFID).FirstOrDefault();
                            }
                            else if (this.loopedStaffId != null)
                            {
                                reciever = context.TBL_STAFF.Where(x => x.STAFFID == this.loopedStaffId && x.DELETED == false).FirstOrDefault();
                                recipientName = reciever.FIRSTNAME;
                                this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.loopedStaffId && DateTime.Now <= x.ENDDATE && x.ISACTIVE && x.DELETED == false).Select(x => x.RELIEFSTAFFID).FirstOrDefault();
                            }
                            userInCopyFirstName = recipientName;
                            messageBody = ReplaceNotificationPlaceholders(alert.TEMPLATE, owner?.FIRSTNAME, recipientName, fromLevelName, operationName, status, time, tat.ToString(), userInCopyFirstName);
                            LogWorkflowNotifications(this.support, reciever?.EMAIL, alert.TITLE, messageBody);
                        }

                        if (levelWorkflowNotification.INCLUDEPOOLINNOTIFICATION)
                        {
                            alert = context.TBL_ALERT_TITLE.FirstOrDefault(a => a.ALERTTITLEID == levelWorkflowNotification.POOLALERTTITLEID);
                            if (alert != null)
                            {
                                var levelStaffEmails = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false && x.APPROVALLEVELID == nextLevel.ApprovalLevelId)
                                .Select(x => x.TBL_STAFF.EMAIL)
                                .Distinct().ToList();

                                var nextLevelStaffEmails = context.TBL_STAFF.Where(s => s.STAFFROLEID == nextLevel.DefaultRoleId && s.STAFFID != reciever.STAFFID && s.DELETED == false).Select(x => x.EMAIL).ToList();//exempt the tostaff already sent
                                emails = levelStaffEmails.Union(nextLevelStaffEmails).ToList();

                                if (this.reliefStaffId > 0)
                                {
                                    var reliefRecord = context.TBL_STAFF.Where(x=>x.STAFFID == this.reliefStaffId && x.DELETED == false).FirstOrDefault();
                                    if (!(String.IsNullOrEmpty(reliefRecord.EMAIL)) && !(String.IsNullOrWhiteSpace(reliefRecord.EMAIL)))
                                    {
                                        emails.Add(reliefRecord.EMAIL);
                                    }
                                }
                                userInCopyFirstName = "All";
                                messageBody = ReplaceNotificationPlaceholders(alert.TEMPLATE, owner?.FIRSTNAME, "All", fromLevelName, operationName, status, time, tat.ToString(), userInCopyFirstName);
                                LogWorkflowNotifications(this.support, string.Join(";", emails.Distinct()), alert.TITLE, messageBody);
                            }
                        }
                    }
                }

                //for notificationOfProceedingActions
                var trailLevels = GetTrailForReferBack(this.targetId, this.operationId, this.fromLevelId ?? 0);
                foreach (var level in trailLevels)
                {
                    var nextLevel = WorkflowSetup.FirstOrDefault(s => s.ApprovalLevelId == level.fromApprovalLevelId);
                    if (nextLevel == null)
                    {
                        continue;
                    }
                    var levelWorkflowNotification = worflowNotificationSetups.FirstOrDefault(n => n.GROUPOPERATIONMAPPINGID == nextLevel.Mapping.GROUPOPERATIONMAPPINGID && n.APPROVALLEVELID == nextLevel.ApprovalLevelId);
                    if (levelWorkflowNotification == null)
                    {
                        continue;
                    }
                    if (levelWorkflowNotification.NOTIFYOFPROCEEDINGWORKFLOWACTIONS)
                    {
                        var alert = context.TBL_ALERT_TITLE.FirstOrDefault(a => a.ALERTTITLEID == levelWorkflowNotification.PROCEEDINGACTIONSALERTTITLEID);
                        if (alert == null)
                        {
                            continue;
                        }
                        if (level.requestStaffId > 0)
                        {
                            ///reciever = context.TBL_STAFF.Find(level.requestStaffId);
                            recieverInCopy = context.TBL_STAFF.Where(x=>x.STAFFID == level.requestStaffId && x.DELETED == false).FirstOrDefault(); //just added
                            userInCopyFirstName = recieverInCopy?.FIRSTNAME; //just added
                            recipientName = reciever?.FIRSTNAME;
                            //this.reliefStaffId = context.TBL_STAFF_RELIEF.Where(x => x.STAFFID == this.toStaffId && DateTime.Now <= x.ENDDATE && x.ISACTIVE && x.DELETED == false).Select(x => x.RELIEFSTAFFID).FirstOrDefault();
                        }
                        messageBody = ReplaceNotificationPlaceholders(alert.TEMPLATE, owner?.FIRSTNAME, recipientName, fromLevelName, operationName, status, time, tat.ToString(), userInCopyFirstName);
                        LogWorkflowNotifications(this.support, recieverInCopy?.EMAIL, alert.TITLE, messageBody);
                    }
                }

                //to send to owner/initiator of a request
                if (this.fromLevelId > 0)
                {
                    var nextLevel = WorkflowSetup.FirstOrDefault(s => s.ApprovalLevelId == fromLevelId);
                    if (nextLevel != null)
                    {
                        var levelWorkflowNotification = worflowNotificationSetups.FirstOrDefault(n => n.GROUPOPERATIONMAPPINGID == nextLevel.Mapping.GROUPOPERATIONMAPPINGID && n.APPROVALLEVELID == nextLevel.ApprovalLevelId);
                        if (levelWorkflowNotification?.NOTIFYONWER ?? false)
                        {
                            var alert = context.TBL_ALERT_TITLE.FirstOrDefault(a => a.ALERTTITLEID == levelWorkflowNotification.OWNERALERTTITLEID);
                            if (alert != null)
                            {
                                if (ownerId > 0)
                                {
                                    ///reciever = context.TBL_STAFF.Find(ownerId);
                                    recieverInCopy = context.TBL_STAFF.Where(x=>x.STAFFID == ownerId && x.DELETED == false).FirstOrDefault(); //just added
                                    userInCopyFirstName = recieverInCopy?.FIRSTNAME; //just added
                                    recipientName = reciever?.FIRSTNAME;
                                    messageBody = ReplaceNotificationPlaceholders(alert.TEMPLATE, reciever?.FIRSTNAME, recipientName, fromLevelName, operationName, status, time, tat.ToString(), userInCopyFirstName);
                                    LogWorkflowNotifications(this.support, reciever?.EMAIL, alert.TITLE, messageBody);
                                }

                                var initiatorId = trailLevels.FirstOrDefault()?.requestStaffId;
                                if (initiatorId > 0)
                                {
                                    reciever = context.TBL_STAFF.Where(x=>x.STAFFID == initiatorId && x.DELETED == false).FirstOrDefault();
                                    recipientName = reciever?.FIRSTNAME;

                                    recieverInCopy = context.TBL_STAFF.Where(x=>x.STAFFID == initiatorId && x.DELETED == false).FirstOrDefault(); //just added
                                    userInCopyFirstName = recieverInCopy?.FIRSTNAME; //just added

                                    messageBody = ReplaceNotificationPlaceholders(alert.TEMPLATE, reciever?.FIRSTNAME, recipientName, fromLevelName, operationName, status, time, tat.ToString(), userInCopyFirstName);
                                    LogWorkflowNotifications(this.support, reciever?.EMAIL, alert.TITLE, messageBody);
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetApprovalStatusName(int statusId) //as requested by the bank
        {
            switch (statusId)
            {
                case 0: return "initiated";
                case 1: return "approved";
                case 2: return "approved";
                case 3: return "rejected";
                case 4: return "approved";
                case 5: return "referred";
                case 6: return "approved";
                case 7: return "approved";
                default: break;
            }
            return "approved";
        }


        /*private string GetApprovalStatusName(int statusId)
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
        }*/


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
            IsFromPc = model.isFromPc;
            destinationOperationId = model.destinationOperationId;
            businessUnitId = model.businessUnitId;
            NextLevelId = 0;
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

        private void SaveFlowLog(string stage)
        {
            if (!creditOperationIds.Contains(this.operationId))
            {
                return;
            }
            String flowLog = String.Empty;
            flowLog += "{" + Environment.NewLine;
            flowLog += "Stage " + stage + "," + Environment.NewLine;
            flowLog += "statusId " + this.statusId + "," + Environment.NewLine;
            flowLog += "fromLevelId " + this.fromLevelId + "," + Environment.NewLine;
            flowLog += "nextLevelId " + this.nextLevelId + "," + Environment.NewLine;
            flowLog += "staffId " + this.staffId + "," + Environment.NewLine;
            flowLog += "toStaffId " + this.toStaffId + "," + Environment.NewLine;
            flowLog += "targetId " + this.targetId + "," + Environment.NewLine;
            flowLog += "operationId " + this.operationId + "," + Environment.NewLine;
            flowLog += "productClassId " + this.productClassId + "," + Environment.NewLine;
            flowLog += "productId " + this.productId + "," + Environment.NewLine;
            flowLog += "exclusiveFlowChangeId " + this.exclusiveFlowChangeId + "," + Environment.NewLine;
            flowLog += "amount " + this.amount + "," + Environment.NewLine;
            flowLog += "requestLevelId " + this.lastOpenRequestFromLevelId + "," + Environment.NewLine;
            flowLog += "requestStaffId " + this.requestStaffId + "," + Environment.NewLine;
            flowLog += "currentStateId " + this.currentStateId + "," + Environment.NewLine;
            flowLog += "approvalLevels " + ((this.approvalGrid == null) ? String.Empty : AppendApprovalDetail(this.approvalGrid.ToList()) + Environment.NewLine);
            flowLog += "next " + ((this.next == null ) ? String.Empty : (this.next.Level?.LEVELNAME + " " + this.next.ApprovalLevelId.ToString()) + "," + Environment.NewLine);
            flowLog += "isLoopResponse " + this.isLoopResponse + "," + Environment.NewLine;
            flowLog += "newStateId " + this.newStateId + "," + Environment.NewLine;
            flowLog += "isFlowTest " + this.isFlowTest + "," + Environment.NewLine;
            flowLog += "isFromPc " + this.isFromPc + "," + Environment.NewLine;
            flowLog += "}," + Environment.NewLine;
            this.flow_log += flowLog;
        }

        private string AppendApprovalDetail(List<WorkflowSetup> approvalGrid)
        {
            var flowLog = String.Empty;
            foreach (var level in approvalGrid)
            {
                flowLog += "{LevelId " + level.ApprovalLevelId + ", ";
                flowLog += "LevelName " + level.Level?.LEVELNAME + "},";
            }
                return flowLog;
        }

        public IEnumerable<dynamic> GetWorkFlowSetupLevelIds()
        {
            var levels = WorkflowSetup.Select(l => new { levelId = l.ApprovalLevelId, roleId = l.DefaultRoleId }).ToList();
            return levels;
        }


    }


    //public class WorkflowSetup
    //{
    //    public int Sn { get; set; }
    //    public int SlaInterval { get; set; }
    //    public int GroupPosition { get; set; }
    //    public int LevelPosition { get; set; }
    //    public int ApprovalLevelId { get; set; }
    //    public int NumberOfUsers { get; set; }
    //    public int NumberOfApprovals { get; set; }
    //    public bool CanRouteBack { get; set; }
    //    public bool IsPoliticallyExposed { get; set; }
    //    //public bool IsInsiderRelated { get; set; }
    //    public bool IsActive { get; set; }
    //    public bool CanEdit { get; set; }
    //    public bool CanRecieveEmail { get; set; }
    //    public bool CanRecieveSMS { get; set; }
    //    public bool RouteViaStaffOrganogram { get; set; }
    //    public int? Tenor { get; set; }
    //    public decimal MaximumAmount { get; set; }
    //    public decimal? InvestmentGradeAmount { get; set; }
    //    public int? DefaultRoleId { get; set; }
    //    public int? LevelTypeId { get; set; }
    //    public int? LevelBusinessRuleId { get; set; }
    //    public TBL_APPROVAL_LEVEL Level { get; set; }
    //    public TBL_APPROVAL_GROUP Group { get; set; }
    //    public TBL_APPROVAL_GROUP_MAPPING Mapping { get; set; }
    //    public IEnumerable<TBL_APPROVAL_LEVEL_STAFF> Staff { get; set; }
    //    public TBL_APPROVAL_BUSINESS_RULE LevelBusinessRule { get; set; }
    //    public bool AllowMultipleInitiator { get; set; }
    //    public int? ROLEIDTOROUTE { get; set; }
    //    public bool ISPOSTAPPROVALREVIEWER { get; set; }
    //}

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
