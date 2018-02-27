using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanReviewApplicationRepository : ILoanReviewApplicationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;

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
            IQueryable<LoanReviewApplicationViewModel> applications = null;
            bool isHeadOffice = (branchId == 1) ? true : false;
            bool screenCanViewAll = operationId == (int)OperationsEnum.LoanReviewApprovalApplication;

            // get approval levels 
            var levelIds = GetStaffApprovalLevelIds(staffId, operationId);

            // query
            applications = context.TBL_LOAN_REVIEW_APPLICATION.Where(x =>
                    (x.BRANCHID == branchId || isHeadOffice)
                )
            .Join(context.TBL_LOAN, a=>a.LOANID,l=>l.TERMLOANID, (a,l)=>new { a,l})
            .GroupJoin(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                al => al.a.LOANREVIEWAPPLICATIONID,
                t => t.TARGETID,
                (al, t) => new { a = al.a, l = al.l, t })
            .SelectMany(
                xy => xy.t.DefaultIfEmpty(),
                (x, t) => new LoanReviewApplicationViewModel
                {
                    //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
                    loanReviewApplicationId = x.a.LOANREVIEWAPPLICATIONID,
                    applicationDate = x.a.DATECREATED,
                    operationTypeId = x.a.OPERATIONTYPEID,
                    referenceNumber = x.l.LOANREFERENCENUMBER,
                    principalAmount = x.l.PRINCIPALAMOUNT,
                    effectiveDate = x.l.EFFECTIVEDATE,
                    maturityDate = x.l.MATURITYDATE,
                    interestRate = x.l.INTERESTRATE,
                    loanId = x.a.LOANID,

                    //applicationReferenceNumber = 
                    //relatedReferenceNumber = x.a.RELATEDREFERENCENUMBER,
                    //customerId = x.a.CUSTOMERID,
                    //branchId = x.a.BRANCHID,
                    //productClassId = x.a.PRODUCTCLASSID,
                    //productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                    //customerGroupId = x.a.CUSTOMERGROUPID,
                    //loanTypeId = x.a.LOANTYPEID,
                    //relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                    //relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                    //applicationAmount = x.a.APPLICATIONAMOUNT,
                    //approvedAmount = x.a.APPROVEDAMOUNT,
                    //interestRate = x.a.INTERESTRATE,
                    //applicationTenor = x.a.APPLICATIONTENOR,
                    //lastComment = y.COMMENT,
                    //currentApprovalStateId = y.APPROVALSTATEID,
                    currentApprovalLevelId = t.TOAPPROVALLEVELID,
                    //currentApprovalLevel = y.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                    approvalTrailId = t == null ? 0 : t.APPROVALTRAILID, // for inner sequence ordering
                    toStaffId = t.TOSTAFFID,
                    //loanInformation = x.a.LOANINFORMATION,
                    //submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                    //customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                    //isRelatedParty = x.a.ISRELATEDPARTY,
                    //isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                    //approvalStatusId = x.a.APPROVALSTATUSID,
                    //applicationStatusId = x.a.APPLICATIONSTATUSID,
                    //branchName = x.a.TBL_BRANCH.BRANCHNAME,
                    //relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                    //relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                    //misCode = x.a.MISCODE,
                    //customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                    //loanTypeName = x.a.TBL_LOAN_TYPE.LOANTYPENAME,
                    //createdBy = x.a.CREATEDBY,
                    //loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                    customerName = x.l.TBL_CUSTOMER.FIRSTNAME + " " + x.l.TBL_CUSTOMER.MIDDLENAME + " " + x.l.TBL_CUSTOMER.LASTNAME,
                    //operationId = x.a.OPERATIONID,
                })
                .GroupBy(d => d.loanReviewApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.applicationDate)
                .ThenByDescending(x => x.loanReviewApplicationId)
                ;

            //var list = applications.ToList();
            //var count = applications.Count();
            //var levs = levelIds.ToList();

            if (screenCanViewAll) { return applications; };

            return applications.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        private IQueryable<int> GetStaffApprovalLevelIds(int staffId, int operationId)
        {
            int scope = (int)ProcessViewScopeEnum.Level; // default 1

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.ISACTIVE == true));

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

            if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

            if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct();

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            if (scope == 2)
            {
                var groups = context.TBL_APPROVAL_LEVEL.Where(x => staffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
                return context.TBL_APPROVAL_LEVEL.Where(x => groups.Contains(x.GROUPID)).Select(x => x.APPROVALLEVELID).Distinct();
            }

            return staffLevels;
        }

        public SelectListViewModel GetAllSelectList()
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }) .ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x=> new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x=> new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x => x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME }).ToList();

            return list;
        }

        public bool SubmitLoanReviewApplication(LoanReviewApplicationViewModel model)
        {
            var application = new TBL_LOAN_REVIEW_APPLICATION
            {
                LOANID = model.loanId,
                PRODUCTTYPEID = model.productTypeId,
                OPERATIONTYPEID = model.operationTypeId,
                REVIEWDETAILS = model.reviewDetails,
                INTERATERATE = model.interateRate,
                PREPAYMENT = model.prepayment,
                PRINCIPALFREQUENCYTYPEID = model.principalFrequencyTypeId,
                INTERESTFREQUENCYTYPEID = model.interestFrequencyTypeId,
                PRINCIPALFIRSTPAYMENTDATE = model.principalFirstPaymentDate,
                INTERESTFIRSTPAYMENTDATE = model.interestFirstPaymentDate,
                MATURITYDATE = model.maturityDate,
                TENOR = model.tenor,
                CASA_ACCOUNTID = model.casaAccountId,
                OVERDRAFTTOPUP = model.overDraftTopup,
                FEE_CHARGES = model.feeCharges,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                ISMANAGEMENTINTERESTRATE = model.isManagementInterestRate,
                CREATEDBY = model.createdBy,
                BRANCHID = model.branchId,
                DATECREATED = general.GetApplicationDate(),
            };

            context.TBL_LOAN_REVIEW_APPLICATION.Add(application);

            // ------------AUDIT CODE HERE! -------------

            if (context.SaveChanges() == 0) return false;

            PassApplicationToOperation(application.LOANREVIEWAPPLICATIONID, (int)OperationsEnum.LoanReviewApprovalAppraisal, model.createdBy, "New loan review application");

            return true;
        }

        private void PassApplicationToOperation(int applicationId, int operationId, int staffId, string comment)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            workflow.StaffId = staffId;
            workflow.CompanyId = staff.COMPANYID;
            workflow.OperationId = operationId;
            workflow.TargetId = applicationId;
            workflow.ProductClassId = null;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = comment;
            workflow.ExternalInitialization = true;
            //workflow.DeferredExecution = true;
            workflow.LogActivity();
        }
    }
}
