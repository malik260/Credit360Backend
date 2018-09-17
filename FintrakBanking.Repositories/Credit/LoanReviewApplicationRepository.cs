using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanReviewApplicationRepository : ILoanReviewApplicationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;

        private List<int> camOperationIds = new List<int> { 46, 71, 79 };

        private readonly int classifiedAssetManagementRoleId = 46;

        public LoanReviewApplicationRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;

            bool ignoreBranch = true; // rm = false, ho = true
            if (operationId == 47) if (ProcessInitiator(staffId, operationId, classId, 2)) ignoreBranch = false;
            if (camOperationIds.Contains(operationId)) if (ProcessInitiator(staffId, operationId, classId, 1)) ignoreBranch = false;

            List<int> operationIds = new List<int>();
            operationIds.Add(operationId);
            if (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) operationIds.Add((int)OperationsEnum.NPLoanReviewApprovalAppraisal);
            if (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) operationIds.Add(79);

            IQueryable<LoanReviewApplicationViewModel> applications = null;

            // get approval levels 
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);

            var ids = levelIds.ToList();
            ids.Add(71); // --------------- REMOVE!!!
            ids.Add(79); // --------------- REMOVE!!!

            // query
            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == user.BranchId || ignoreBranch)
            .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
            .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
            .Join(context.TBL_APPROVAL_TRAIL.Where(x => operationIds.Contains(x.OPERATIONID)
                    && x.APPROVALSTATEID != (int)ApprovalState.Ended
                    && x.RESPONSESTAFFID == null
                    && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                    && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)
            ),
                alaba => alaba.ab.a.LOANAPPLICATIONID,
                trail => trail.TARGETID,
                (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
            .Select(x => new LoanReviewApplicationViewModel
            {
                //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                approvalState = x.trail == null ? "Pending" : x.trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                currentApprovalLevel = x.trail == null ? "" : x.trail.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                currentApprovalLevelId = x.trail == null ? 0 : x.trail.TOAPPROVALLEVELID,
                lastComment = x.trail == null ? "" : x.trail.COMMENT,
                toStaffId = x.trail == null ? 0 : x.trail.TOSTAFFID,

                applicationDate = x.application.APPLICATIONDATE,
                approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                approvalStatusId = x.application.APPROVALSTATUSID,
                createdBy = x.application.CREATEDBY,
                loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                branchId = x.branch.BRANCHID,
                branchName = x.branch.BRANCHNAME,
                customerId = x.customer.CUSTOMERID,
                operationId = x.application.OPERATIONID,
                customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,

                // currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,

                applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Select(d => new applicationDetails
                {
                    detailId = d.LOANREVIEWAPPLICATIONID,
                    operationId = d.OPERATIONID,
                    operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                    reviewDetails = d.REVIEWDETAILS,
                    loanId = d.LOANID,
                    loanSystemTypeId = d.LOANSYSTEMTYPEID,
                    loanSystemTypeName = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                    productId = d.PRODUCTID,
                    customerId = d.CUSTOMERID,
                    obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                    proposedTenor = d.PROPOSEDTENOR,
                    proposedRate = d.PROPOSEDINTERESTRATE,
                    proposedAmount = d.PROPOSEDAMOUNT,
                    approvedTenor = d.APPROVEDTENOR,
                    approvedRate = d.APPROVEDINTERESTRATE,
                    approvedAmount = d.APPROVEDAMOUNT,
                    // loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                })
                
            })
            .GroupBy(d => d.loanReviewApplicationId)
            .ToList()
            ;

            applications = query.AsQueryable()
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.loanReviewApplicationId);

            //var list = applications.ToList();
            //var count = applications.Count();
            
            return applications; // .Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        private bool ProcessInitiator(int staffId, int operationId, int? productClassId, int position)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);

            return index == (position - 1);
        }

        public SelectListViewModel GetAllSelectList()
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }).ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x => 
                (x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement 
                || x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft
                || x.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial)
                && x.ISDISABLED == false
            ).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME, typeId = (int)x.OPERATIONTYPEID }).OrderBy(o => o.name).ToList();

            return list;
        }

        public string SubmitLoanReviewApplication(LoanReviewApplicationViewModel model)
        {
            int staffId = model.createdBy;
            var referenceNumber = GenerateReferenceNumber();
            var applicationDate = general.GetApplicationDate();
            int camOperationId = GetCamOperation(model.performanceTypeId);

            var application = context.TBL_LMSR_APPLICATION.Add(new TBL_LMSR_APPLICATION
            {
                APPLICATIONREFERENCENUMBER = referenceNumber,
                COMPANYID = model.companyId,
                CUSTOMERID = model.customerId,
                BRANCHID = model.branchId,
                OPERATIONID = camOperationId,
                // CUSTOMERGROUPID = null,
                DISPUTED = false,
                REQUIRECOLLATERAL = false,
                APPLICATIONDATE = applicationDate,
                CREATEDBY = staffId,
                DATETIMECREATED = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONSTATUSID = (short)1, // -------------------------------------------------- REMOVE COLUMN!!
            });

            LoanViewModel loan = new LoanViewModel();

            foreach (var detail in model.applicationDetails)
            {
                loan = GetLoanInformation(detail.loanSystemTypeId, detail.loanId, applicationDate);
                int tenor = detail.loanSystemTypeId == 4 ? loan.tenorUsed : loan.tenor;

                context.TBL_LMSR_APPLICATION_DETAIL.Add(new TBL_LMSR_APPLICATION_DETAIL
                {
                    LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                    LOANID = detail.loanId,
                    LOANSYSTEMTYPEID = detail.loanSystemTypeId,/*Term/Disbursed Facility..Overdraft Facility..Contingent Liability*/
                    OPERATIONID = detail.operationId, // refactor to operationId from ui!
                    REVIEWDETAILS = detail.reviewDetails,
                    PRODUCTID = detail.productId,
                    REPAYMENTTERMS = String.Empty,
                    REPAYMENTSCHEDULE = String.Empty,
                    CUSTOMERID = loan.customerId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved, // REMOVE DUPLICATE [STATUSID]
                    CREATEDBY = staffId,
                    DATETIMECREATED = applicationDate,
                    PROPOSEDTENOR = tenor,
                    PROPOSEDINTERESTRATE = loan.interestRate,
                    PROPOSEDAMOUNT = loan.outstandingPrincipal,
                    APPROVEDTENOR = tenor,
                    APPROVEDINTERESTRATE = loan.interestRate,
                    APPROVEDAMOUNT = loan.outstandingPrincipal,
                    OPERATIONPERFORMED = false,
                    //LOANAPPLICATIONDETAILID = loan.loanApplicationDetailId,
                });
            }

            // ------------AUDIT CODE HERE! -------------

            if (context.SaveChanges() == 0) throw new SecureException("An error occured while saving the data!"); // this save is necessary to grab targetid

            workflow.ToStaffId = staffId;

            bool assetManagement = false;
            var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == staffId);
            if (user.STAFFROLEID == classifiedAssetManagementRoleId) assetManagement = true;

            if (assetManagement)
                workflow.NextProcess(model.companyId, staffId, 79, application.LOANAPPLICATIONID, null, "NIL", true, true, true);
            else
                workflow.NextProcess(model.companyId, staffId, camOperationId, application.LOANAPPLICATIONID, null, "NIL", true, true, true);

            if (context.SaveChanges() > 0) return "Application with reference number " + referenceNumber + " created.";
            throw new SecureException("An error occured while saving the data!");
        }

        private int GetCamOperation(int performanceTypeId)
        {
            switch (performanceTypeId)
            {
                case 2: return 71;
                case 3: return 79; // ======== recovery ========
            }
            return 46;
        }

        private string GenerateReferenceNumber()
        {
            int length = 10;
            string input = "0000000001";
            string output = "0000000001";
            var appl = context.TBL_LMSR_APPLICATION.OrderByDescending(x => x.LOANAPPLICATIONID).FirstOrDefault();
            if (appl != null) input = appl.APPLICATIONREFERENCENUMBER;
            output = (int.Parse(input) + 1).ToString().PadLeft(length, '0');
            return output;
        }

        public WorkflowResponse ForwardApplication(ForwardReviewViewModel model)
        {
            int nextProcessId = model.operationId + 1;
            int operationId = model.operationId; // beware of nplappraisal!
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);
            int lastOperationId = (int)OperationsEnum.LoanReviewApprovalAvailment;

            // customization for CAM approvals
            //bool operationIsCam = (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) || (operationId == (int)OperationsEnum.NPLoanReviewApprovalAppraisal);
            if (camOperationIds.Contains(operationId))
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                operationId = (int)appl.OPERATIONID;
                nextProcessId = (int)OperationsEnum.LoanReviewApprovalOfferLetter; // redefine
                if (appl.CUSTOMERID > 0) workflow.Amount = GetCustomerTotalOutstandingBalance((int)appl.CUSTOMERID);
            }

            workflow.StaffId = model.lastUpdatedBy;
            workflow.CompanyId = appl.COMPANYID;
            workflow.OperationId = operationId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.ProductClassId = null;
            workflow.StatusId = model.forwardAction;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            context.SaveChanges(); // redundant !


            // DETAIL CHANGES
            List<TBL_LMSR_APPLICATION_DETAIL> items = null;
            if (model.recommendedChanges != null && model.recommendedChanges.Count() > 0) // only approving authority
            {
                //updateApprovedAmount = true;
                items = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID).ToList();
                foreach (var changed in model.recommendedChanges)
                {
                    var detail = items.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == changed.detailId);
                    if (detail != null)
                    {
                        //detail.APPROVEDPRODUCTID = (short)changed.productId;
                        detail.APPROVEDAMOUNT = changed.amount;
                        detail.APPROVEDINTERESTRATE = changed.interestRate;
                        detail.APPROVEDTENOR = changed.tenor;
                        //detail.STATUSID = (short)changed.statusId;
                        //detail.EXCHANGERATE = changed.exchangeRate;
                        //detail.LASTUPDATEDBY = model.createdBy;
                        //detail.DATETIMEUPDATED = DateTime.Now;

                        //if (model.isBusiness) // DELETE OR UPDATE PROPOSED
                        //{
                        //    if (detail.STATUSID == (int)ApprovalStatusEnum.Disapproved) { detail.DELETED = true; }
                        //    else
                        //    {
                        //        detail.PROPOSEDPRODUCTID = (short)changed.productId;
                        //        detail.PROPOSEDAMOUNT = changed.amount;
                        //        detail.PROPOSEDINTERESTRATE = changed.interestRate;
                        //        detail.PROPOSEDTENOR = changed.tenor;
                        //    }
                        //}
                    }
                }
            }


            int lastStatusId = workflow.StatusId;
            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved && operationId != lastOperationId/* && model.operationId != 71*/) // jump process OR end flag
                {
                    if (operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter) workflow.NextLevelId = GetFirstReceiverLevel(model.lastUpdatedBy, (int)OperationsEnum.LoanReviewApprovalAvailment, null, true);
                    workflow.NextProcess(appl.COMPANYID, model.lastUpdatedBy, nextProcessId, appl.LOANAPPLICATIONID, null, "New application", true, true); // model.operationId must be used here!
                }
                if (operationId == lastOperationId/* || model.operationId == 71*/) appl.APPROVALSTATUSID = (short)lastStatusId; // last or cam?
                context.SaveChanges();
            }

            //return lastStatusId;
            return workflow.Response;
        }

        private int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        public decimal GetCustomerTotalOutstandingBalance(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);
            decimal loanBalance = 0;
            decimal overdraftBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN
                               where a.CUSTOMERID == customerId
                               select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }
            else
            {
                loanBalance = 0;
            }

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING
                               where a.CUSTOMERID == customerId
                               select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }
            else
            {
                overdraftBalance = 0;
            }

            decimal totalBalance = loanBalance + overdraftBalance;

            return totalBalance;
        }

        public LoanViewModel GetLoanInformation(int loanSystemTypeId, int loanId, DateTime startDate)
        {
            var result = new LoanViewModel();
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                result = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OUTSTANDINGPRINCIPAL,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                result = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OVERDRAFTLIMIT,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                result = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = 0,
                    outstandingPrincipal = loan.CONTINGENTAMOUNT,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.LineFacility)
            {
                result = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    tenorUsed = loan.APPROVEDTENOR,
                    interestRate = loan.APPROVEDINTERESTRATE,
                    outstandingPrincipal = loan.APPROVEDAMOUNT, // adapting!
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID
                })
                .FirstOrDefault();
            }
            else
            {
                throw new SecureException("The Product type is Invalid");
            }
            return result;
        }

        public LoanApplicationDetailViewModel GetLoanApplicationDetail(int loanId, int loanTypeId)
        {
            int id = GetLoanApplicationDetailId(loanId, loanTypeId);

            var detail = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == id);
            if (detail == null) throw new Exception("Could not find loan application detail with id of " + id);

            return new LoanApplicationDetailViewModel
            {
                loanApplicationDetailId = detail.LOANAPPLICATIONDETAILID,
                loanApplicationId = detail.LOANAPPLICATIONID,
            };
        }

        private int GetLoanApplicationDetailId(int loanId, int loanTypeId)
        {
            int id=0;
            if (loanTypeId == 1)
            {
                var loan = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                id = loan.LOANAPPLICATIONDETAILID;
            }
            if (loanTypeId == 2)
            {
                var loan = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == loanId);
                id = loan.LOANAPPLICATIONDETAILID;
            }
            if (loanTypeId == 3)
            {
                var loan = context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == loanId);
                id = loan.LOANAPPLICATIONDETAILID;
            }
            if (loanTypeId == 4)
            {
                id = loanId;
            }
            return id;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetRegionalLoanApplications(int staffId)
        {
            List<int> levels = general.GetRouteLevels(46, 1);
            //List<int> levels2 = general.GetRouteLevels(71, 1);
            //List<int> levels3 = general.GetRouteLevels(79, 1);

            //var levels = levels1.Union(levels2).Union(levels3).Distinct();

            var branches = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
                                .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
                                .Join(context.TBL_BRANCH, sr => sr.r.REGIONID, b => b.REGIONID, (sr, b) => new { sr, b })
                                .Select(x => new {
                                    BRANCHID = x.b.BRANCHID
                                })
                                .Select(x => x.BRANCHID)
                                .ToList();

            var applications = context.TBL_LMSR_APPLICATION.Where(x =>
                    branches.Contains(x.BRANCHID)
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                )
                .OrderByDescending(x => x.LOANAPPLICATIONID)
                .Join(
                    context.TBL_APPROVAL_TRAIL.Where(x => camOperationIds.Contains(x.OPERATIONID)
                        && levels.Contains((int)x.TOAPPROVALLEVELID)),// && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)),
                    a => a.LOANAPPLICATIONID,
                    b => b.TARGETID,
                    (a, b) => new { a, b, branch = a.TBL_BRANCH, customer = a.TBL_CUSTOMER })
                .Select(x => new LoanReviewApplicationViewModel
                {
                    approvalState = x.b.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalTrailId = x.b.APPROVALTRAILID,
                    currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                        currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                    lastComment = x.b.COMMENT,
                    toStaffId = x.b.TOSTAFFID,

                    applicationDate = x.a.APPLICATIONDATE,
                    approvalStatus = x.a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    approvalStatusId = x.a.APPROVALSTATUSID,
                    createdBy = x.a.CREATEDBY,
                    loanReviewApplicationId = x.a.LOANAPPLICATIONID,
                    referenceNumber = x.a.APPLICATIONREFERENCENUMBER,

                    branchId = x.branch.BRANCHID,
                    branchName = x.branch.BRANCHNAME,
                    customerId = x.customer.CUSTOMERID,
                    operationId = x.a.OPERATIONID,
                    customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,

                    timeIn = x.b.SYSTEMARRIVALDATETIME,
                    timeOut = x.b.SYSTEMRESPONSEDATETIME,
                    responsiblePerson = context.TBL_STAFF
                                            .Where(s => s.STAFFID == x.b.TOSTAFFID)
                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                            .FirstOrDefault().name ?? "",
                    requestStaffId = x.b.REQUESTSTAFFID,
                    toApprovalLevelId = x.b.TOAPPROVALLEVELID,

                    applicationDetails = x.a.TBL_LMSR_APPLICATION_DETAIL.Select(d => new applicationDetails
                    {
                        detailId = d.LOANREVIEWAPPLICATIONID,
                        operationId = d.OPERATIONID,
                        operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                        reviewDetails = d.REVIEWDETAILS,
                        loanId = d.LOANID,
                        loanSystemTypeId = d.LOANSYSTEMTYPEID,
                        loanSystemTypeName = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                        productId = d.PRODUCTID,
                        customerId = d.CUSTOMERID,
                        obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                        proposedTenor = d.PROPOSEDTENOR,
                        proposedRate = d.PROPOSEDINTERESTRATE,
                        proposedAmount = d.PROPOSEDAMOUNT,
                        approvedTenor = d.APPROVEDTENOR,
                        approvedRate = d.APPROVEDINTERESTRATE,
                        approvedAmount = d.APPROVEDAMOUNT,
                    })
                })
                .GroupBy(d => d.timeIn)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                ;

            return applications;

        }
        
        #region
        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            searchString = searchString.Trim().ToLower();

            var applications = from a in context.TBL_LMSR_APPLICATION
                               join d in context.TBL_LMSR_APPLICATION_DETAIL on a.APPLICATIONSTATUSID equals d.LOANAPPLICATIONID
                               join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
                               join l in context.TBL_LOAN on d.LOANID equals l.TERMLOANID
                               join o in context.TBL_CASA on l.CASAACCOUNTID equals o.CASAACCOUNTID
                               join y in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals  y.TARGETID
                               where a.RELATEDREFERENCENUMBER == searchString
                        || g.FIRSTNAME.ToLower().StartsWith(searchString)
                        || g.LASTNAME.ToLower().StartsWith(searchString)
                        || g.MIDDLENAME.ToLower().StartsWith(searchString)
                        || g.CUSTOMERCODE.ToLower().StartsWith(searchString)
                               select new LoanApplicationViewModel
                               {
                                   firstName = g.FIRSTNAME,
                                   middleName = g.MIDDLENAME,
                                   lastName = g.LASTNAME,
                                   customerCode = g.CUSTOMERCODE,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,

                                   loanApplicationId = a.LOANAPPLICATIONID,
                                   customerId = a.CUSTOMERID,
                                   branchId = a.BRANCHID,
                                   customerGroupId = a.CUSTOMERGROUPID,
                                 //  loanTypeId = l.LOANAPPLICATIONTYPEID,
                                   relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                                   relationshipManagerId = l.RELATIONSHIPOFFICERID,
                                   applicationDate = a.APPLICATIONDATE,
                                   applicationAmount = d.PROPOSEDAMOUNT,
                                   approvedAmount = d.APPROVEDAMOUNT,
                                   interestRate = d.PROPOSEDINTERESTRATE,
                                   applicationTenor = d.PROPOSEDTENOR,

                                   //submittedForAppraisal = x.q.o.g.d.SUBMITTEDFORAPPRAISAL,
                                   // customerInfoValidated = x.q.o.g.a.CUSTOMERINFOVALIDATED,
                                   // isRelatedParty = x.q.o.g.a.ISRELATEDPARTY,
                                   // isPoliticallyExposed = x.q.o.g.a.ISPOLITICALLYEXPOSED,
                                   approvalStatusId = (short)a.APPROVALSTATUSID,
                                   approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                   currentApprovalLevel = y.FROMAPPROVALLEVELID !=null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                   approvalTrailId = y.APPROVALTRAILID,
                                   responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                   applicationStatusId = a.APPLICATIONSTATUSID,
                                   applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                   branchName = a.TBL_BRANCH.BRANCHNAME,
                                   relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   //   misCode = x.q.o.g.a.MISCODE,
                                   //  customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                   //  loanTypeName = x.q.o.g.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   createdBy = a.CREATEDBY,
                                   //  loanPreliminaryEvaluationId = x.q.o.g.a.LOANPRELIMINARYEVALUATIONID,
                                   operationId = a.OPERATIONID,
                                   accountNumber = o.PRODUCTACCOUNTNUMBER,
                                   isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
                               };

            return applications;
        }

        private IQueryable<LoanApplicationViewModel> GetLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationViewModel
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = a.CUSTOMERID ?? 0,
                            customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                            loanInformation = a.LOANINFORMATION,
                            companyId = a.COMPANYID,
                            branchId = (short)a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            misCode = a.MISCODE,
                            teamMisCode = a.TEAMMISCODE,
                            interestRate = a.INTERESTRATE,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                            customerGroupId = a.CUSTOMERGROUPID ?? 0,
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                            loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            createdBy = a.CREATEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            applicationTenor = a.APPLICATIONTENOR,
                            applicationAmount = a.APPLICATIONAMOUNT,
                            dateTimeCreated = a.DATETIMECREATED,
                            LoanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
                             .Select(c => new LoanApplicationDetailViewModel()
                             {
                                 equityAmount = c.EQUITYAMOUNT,
                                 equityCasaAccountId = c.EQUITYCASAACCOUNTID,
                                 approvedAmount = c.APPROVEDAMOUNT,
                                 approvedInterestRate = c.APPROVEDINTERESTRATE,
                                 approvedProductId = c.APPROVEDPRODUCTID,
                                 approvedTenor = c.APPROVEDTENOR,
                                 currencyId = c.CURRENCYID,
                                 currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                 customerId = c.CUSTOMERID,
                                 exchangeRate = c.EXCHANGERATE,
                                 loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                 subSectorId = c.SUBSECTORID,
                                 loanApplicationId = c.LOANAPPLICATIONID,
                                 proposedAmount = c.PROPOSEDAMOUNT,
                                 proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                 proposedProductId = c.PROPOSEDPRODUCTID,
                                 proposedProductName = c.TBL_PRODUCT.PRODUCTNAME,
                                 //proposedTenor = Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                                 statusId = c.STATUSID
                             }).ToList()
                        });
            return data;
        }


        #endregion


        public bool AppraisalReviewReferBack(ForwardViewModel model)
        {
            var o = context.TBL_APPROVAL_TRAIL.Find(model.trailId); // here we try to get the staffid on the trail row
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);

            var trail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == model.operationId
                && x.TARGETID == appl.LOANAPPLICATIONID
                && x.REQUESTSTAFFID == o.REQUESTSTAFFID
            );

            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = appl.COMPANYID;
            workflow.ProductClassId = model.productClassId;//.PRODUCTCLASSID;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = trail.FROMAPPROVALLEVELID;//
            workflow.ToStaffId = o.REQUESTSTAFFID;
            workflow.StatusId = (int)ApprovalStatusEnum.Referred;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            // Take out of offer letter screen
            var currentTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == (int)OperationsEnum.LoanReviewApprovalOfferLetter
                && x.RESPONSESTAFFID == null
                && x.TARGETID == appl.LOANAPPLICATIONID
            );
            if (currentTrail != null)
            {
                currentTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                currentTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                currentTrail.COMMENT = model.comment;
                currentTrail.TOAPPROVALLEVELID = null;
                currentTrail.TOSTAFFID = null;
            }
            appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;

            return context.SaveChanges() > 0;
        }


    }
}
