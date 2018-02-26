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

        public IQueryable<LoanApplicationViewModel> GetApplications(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;
            IQueryable<LoanApplicationViewModel> applications = null;
            bool isHeadOffice = (branchId == 1) ? true : false;

            // get approval levels 
            var levelIds = GetStaffApprovalLevelIds(staffId, operationId);

            // query
            applications = context.TBL_LOAN_APPLICATION.Where(x =>
                    x.DELETED == false
                    && x.COMPANYID == companyId
                    && (x.BRANCHID == branchId || isHeadOffice) // branch filter
                    && (classId == null) ? true : (x.PRODUCTCLASSID == (short?)classId)
                )
            .GroupJoin(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                a => a.LOANAPPLICATIONID,
                b => b.TARGETID,
                (x, y) => new { a = x, bs = y })
            .SelectMany(
                xy => xy.bs.DefaultIfEmpty(),
                (x, y) => new LoanApplicationViewModel
                {
                    //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
                    loanApplicationId = x.a.LOANAPPLICATIONID,
                    applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                    relatedReferenceNumber = x.a.RELATEDREFERENCENUMBER,
                    customerId = x.a.CUSTOMERID,
                    branchId = x.a.BRANCHID,
                    productClassId = x.a.PRODUCTCLASSID,
                    productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                    customerGroupId = x.a.CUSTOMERGROUPID,
                    loanTypeId = x.a.LOANTYPEID,
                    relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                    newApplicationDate = x.a.APPLICATIONDATE,
                    applicationAmount = x.a.APPLICATIONAMOUNT,
                    approvedAmount = x.a.APPROVEDAMOUNT,
                    interestRate = x.a.INTERESTRATE,
                    applicationTenor = x.a.APPLICATIONTENOR,
                    lastComment = y.COMMENT,
                    currentApprovalStateId = y.APPROVALSTATEID,
                    currentApprovalLevelId = y.TOAPPROVALLEVELID,
                    currentApprovalLevel = y.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                    approvalTrailId = y == null ? 0 : y.APPROVALTRAILID, // for inner sequence ordering
                    toStaffId = y.TOSTAFFID,
                    loanInformation = x.a.LOANINFORMATION,
                    submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                    customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                    isRelatedParty = x.a.ISRELATEDPARTY,
                    isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                    approvalStatusId = x.a.APPROVALSTATUSID,
                    applicationStatusId = x.a.APPLICATIONSTATUSID,
                    branchName = x.a.TBL_BRANCH.BRANCHNAME,
                    relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                    relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                    misCode = x.a.MISCODE,
                    customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                    loanTypeName = x.a.TBL_LOAN_TYPE.LOANTYPENAME,
                    createdBy = x.a.CREATEDBY,
                    loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                    customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
                    operationId = x.a.OPERATIONID,
                })
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.newApplicationDate)
                .ThenByDescending(x => x.loanApplicationId)
                ;

            //var list = applications.ToList();
            //var count = applications.Count();
            //var levs = levelIds.ToList();

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
            context.TBL_LOAN_REVIEW_APPLICATION.Add(new TBL_LOAN_REVIEW_APPLICATION {
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
                DATECREATED = general.GetApplicationDate(),
            });

            // ------------AUDIT CODE HERE! -------------

            return context.SaveChanges() > 0;
        }
    }
}
