using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanReviewApplicationRepository : ILoanReviewApplicationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;
        private IAdminRepository admin;
        private IOfferLetterAndAvailmentRepository offerLetter;
        private CreditCommonRepository creditCommon;
        private MemorandumRepository memo;


        private List<int> camOperationIds = new List<int> { 46, 71, 79 }; // RMU(71), CAM(79)
        private List<int> apsOperationIds = new List<int> { 107, 108, 109 }; // 

        private readonly int classifiedAssetManagementRoleId = 46;

        public LoanReviewApplicationRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository general,
            IAuditTrailRepository audit,
            IWorkflow workflow,
            IAdminRepository admin,
            IOfferLetterAndAvailmentRepository _offerLetter,
            CreditCommonRepository creditCommon,
            MemorandumRepository memo
            )
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.admin = admin;
            this.creditCommon = creditCommon;
            this.offerLetter = _offerLetter;
            this.memo = memo;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetLoanReviewForCRMS(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;

            bool ignoreBranch = true;

            IQueryable<LoanReviewApplicationViewModel> applications = null;

            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);
            var staffs = general.GetStaffRlieved(staffId);
            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == user.BranchId || ignoreBranch)
             .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
             .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
             .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Finishing)
                     && x.RESPONSESTAFFID == null
                     && x.OPERATIONID != (int)OperationsEnum.APSReleaseApproval
                     && ((levelIds.Contains((int)x.TOAPPROVALLEVELID) && x.TOSTAFFID == null) || (levelIds.Contains((int)x.TOAPPROVALLEVELID) && staffs.Contains(x.TOSTAFFID ?? 0))
                     || (!levelIds.Contains((int)x.TOAPPROVALLEVELID)) && staffs.Contains(x.TOSTAFFID ?? 0))
             ),
                 alaba => alaba.ab.a.LOANAPPLICATIONID,
                 trail => trail.TARGETID,
                 (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
             .Select(x => new LoanReviewApplicationViewModel
             {
                 lmsApplicationId = x.application.LOANAPPLICATIONID,
                 lmsOperationId = x.application.OPERATIONID,

                 //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                 approvalState = x.trail == null ? "Pending" : x.trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                 approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                 currentApprovalLevel = x.trail == null ? "" : x.trail.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                                                                                      // currentApprovalLevelTypeId = x.trail == null ? null : x.trail.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                 currentApprovalLevelId = x.trail == null ? 0 : x.trail.TOAPPROVALLEVELID,
                 lastComment = x.trail == null ? "" : x.trail.COMMENT,
                 toStaffId = x.trail == null ? 0 : x.trail.TOSTAFFID,
                 requestStaffId = x.trail == null ? 0 : x.trail.REQUESTSTAFFID,
                 systemArrivalDate = x.application.DATETIMECREATED,
                 applicationDate = x.application.APPLICATIONDATE,
                 approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 approvalStatusId = (int)x.application.APPROVALSTATUSID,
                 createdBy = x.application.CREATEDBY,
                 loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                 referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                 relatedReferenceNumber = x.application.RELATEDREFERENCENUMBER,
                 branchId = x.branch.BRANCHID,
                 branchName = x.branch.BRANCHNAME,
                 customerId = x.customer.CUSTOMERID,
                 operationId = x.application.OPERATIONID,
                 customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,
                 atInitiator = x.application.CREATEDBY == staffId,
                 timeIn = x.trail.SYSTEMARRIVALDATETIME,
                 slaTime = x.trail.SLADATETIME,
                 currentApprovalStatus = x.trail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 currentApprovalStateId = x.trail.APPROVALSTATEID,


                 customerGroupName = context.TBL_CUSTOMER_GROUP.Where(c => c.CUSTOMERGROUPID == x.application.CUSTOMERGROUPID).Select(c => c.GROUPNAME).FirstOrDefault() ?? "",

                 loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(l => l.LOANAPPLICATIONTYPEID == x.application.LOANAPPLICATIONTYPEID).Select(l => l.LOANAPPLICATIONTYPENAME).FirstOrDefault() ?? "N/A",
                 facility = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LMSR_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
                 approvedAmount = x.application.APPROVEDAMOUNT == null ? 0 : x.application.APPROVEDAMOUNT,
                 productClassProcessId = x.application.PRODUCT_CLASS_PROCESSID == null ? 0 : x.application.PRODUCT_CLASS_PROCESSID,
                 divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.application.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                 globalsla = !context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).Any() ? 0 : context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).FirstOrDefault(),
                 currentApprovalLevelSlaInterval = x.trail.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
                 dateTimeCreated = x.application.DATETIMECREATED,
                 operationTypeName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.application.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),

                 responsiblePerson = context.TBL_STAFF
                                             .Where(s => s.STAFFID == x.trail.TOSTAFFID)
                                             .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                             .FirstOrDefault().name ?? "",
                 toApprovalLevelId = x.trail.TOAPPROVALLEVELID,

                 // currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,
                 creditOperationType = context.TBL_LMSR_APPLICATION_DETAIL.Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count() > 1
                     ? "Multiple"
                     : context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID ==
                             context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).OPERATIONID
                         ).OPERATIONNAME,

                 facilityType = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                 applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                    .Select(d => new applicationDetails
                    {
                        creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                        creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

                        detailId = d.LOANREVIEWAPPLICATIONID,
                        loanApplicationId = d.LOANAPPLICATIONID,
                        operationId = d.OPERATIONID,
                        operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                        reviewDetails = d.REVIEWDETAILS,
                        reviewStageId = d.REVIEWSTAGEID,
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
                        customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                        statusId = d.APPROVALSTATUSID,
                        terms = d.REPAYMENTTERMS,
                        currencyId = d.CURRENCYID,
                        schedule = context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault() == null ? "" : context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault(),

                    })

             }).GroupBy(d => d.loanReviewApplicationId).ToList();

            applications = query.AsQueryable()
           .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
           .OrderByDescending(x => x.approvalTrailId);

            var list = applications.ToList();
            //var count = applications.Count();

            return applications;
        }


        public IQueryable<LoanReviewApplicationViewModel> GetLoanReviewDrawdownApproval(UserInfo user, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;
            bool ignoreBranch = true;

            List<short> approvalOperations = new List<short> { (short)OperationsEnum.LoanReviewDrawdownForExtension, (short)OperationsEnum.OverdraftReviewDrawdownForExtension, (short)OperationsEnum.ContingentReviewDrawdownForExtension};

            IQueryable<LoanReviewApplicationViewModel> applications = null;

            List<int> levelIds = new List<int>();
            foreach (var i in approvalOperations)
            {
                levelIds.AddRange(general.GetStaffApprovalLevelIds(staffId, i).ToList());
            }

            var staffs = general.GetStaffRlieved(staffId);

            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == user.BranchId || ignoreBranch)
             .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
             .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
             .Join(context.TBL_APPROVAL_TRAIL.Where(x => approvalOperations.Contains((short)x.OPERATIONID)
                    && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Authorised
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                     && x.RESPONSESTAFFID == null
                     && x.OPERATIONID != (int)OperationsEnum.APSReleaseApproval
                     && ((levelIds.Contains((int)x.TOAPPROVALLEVELID)))
                     && ((x.TOSTAFFID == null) || staffs.Contains(x.TOSTAFFID ?? 0))
             ),
                 alaba => alaba.ab.a.LOANAPPLICATIONID,
                 trail => trail.TARGETID,
                 (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
             .Select(x => new LoanReviewApplicationViewModel
             {
                 lmsApplicationId = x.application.LOANAPPLICATIONID,
                 lmsOperationId = x.application.OPERATIONID,

                 //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                 approvalState = x.trail == null ? "Pending" : x.trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                 approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                 currentApprovalLevel = x.trail == null ? "" : x.trail.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                                                                                      // currentApprovalLevelTypeId = x.trail == null ? null : x.trail.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                 currentApprovalLevelId = x.trail == null ? 0 : x.trail.TOAPPROVALLEVELID,
                 lastComment = x.trail == null ? "" : x.trail.COMMENT,
                 toStaffId = x.trail == null ? 0 : x.trail.TOSTAFFID,
                 requestStaffId = x.trail == null ? 0 : x.trail.REQUESTSTAFFID,
                 systemArrivalDate = x.application.DATETIMECREATED,
                 applicationDate = x.application.APPLICATIONDATE,
                 approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 approvalStatusId = (int)x.application.APPROVALSTATUSID,
                 createdBy = x.application.CREATEDBY,
                 loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                 referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                 relatedReferenceNumber = x.application.RELATEDREFERENCENUMBER,
                 branchId = x.branch.BRANCHID,
                 branchName = x.branch.BRANCHNAME,
                 customerId = x.customer.CUSTOMERID,
                 operationId = context.TBL_PRODUCT.Where(p=>p.PRODUCTID ==  x.application.PRODUCTID).Select(d=>d.PRODUCTTYPEID).FirstOrDefault() == (short)LoanProductTypeEnum.RevolvingLoan ? (short)OperationsEnum.OverdraftReviewDrawdownForExtension 
                    : context.TBL_PRODUCT.Where(p => p.PRODUCTID == x.application.PRODUCTID).Select(d => d.PRODUCTTYPEID).FirstOrDefault() == (short)LoanProductTypeEnum.ContingentLiability ? (short)OperationsEnum.ContingentReviewDrawdownForExtension 
                    : (short) OperationsEnum.LoanReviewDrawdownForExtension ,
                 customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,
                 atInitiator = x.application.CREATEDBY == staffId,
                 timeIn = x.trail.SYSTEMARRIVALDATETIME,
                 slaTime = x.trail.SLADATETIME,
                 currentApprovalStatus = x.trail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 currentApprovalStateId = x.trail.APPROVALSTATEID,


                 customerGroupName = context.TBL_CUSTOMER_GROUP.Where(c => c.CUSTOMERGROUPID == x.application.CUSTOMERGROUPID).Select(c => c.GROUPNAME).FirstOrDefault() ?? "",

                 loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(l => l.LOANAPPLICATIONTYPEID == x.application.LOANAPPLICATIONTYPEID).Select(l => l.LOANAPPLICATIONTYPENAME).FirstOrDefault() ?? "N/A",
                 facility = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LMSR_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
                 approvedAmount = x.application.APPROVEDAMOUNT == null ? 0 : x.application.APPROVEDAMOUNT,
                 productClassProcessId = x.application.PRODUCT_CLASS_PROCESSID == null ? 0 : x.application.PRODUCT_CLASS_PROCESSID,
                 divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.application.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                 globalsla = !context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).Any() ? 0 : context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).FirstOrDefault(),
                 currentApprovalLevelSlaInterval = x.trail.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
                 dateTimeCreated = x.application.DATETIMECREATED,
                 operationTypeName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.application.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),

                 responsiblePerson = context.TBL_STAFF
                                             .Where(s => s.STAFFID == x.trail.TOSTAFFID)
                                             .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                             .FirstOrDefault().name ?? "",
                 toApprovalLevelId = x.trail.TOAPPROVALLEVELID,

                 // currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,
                 creditOperationType = context.TBL_LMSR_APPLICATION_DETAIL.Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count() > 1
                     ? "Multiple"
                     : context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID ==
                             context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).OPERATIONID
                         ).OPERATIONNAME,

                 facilityType = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                 applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                    .Select(d => new applicationDetails
                    {
                        creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                        creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

                        detailId = d.LOANREVIEWAPPLICATIONID,
                        loanApplicationId = d.LOANAPPLICATIONID,
                        operationId = d.OPERATIONID,
                        operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                        reviewDetails = d.REVIEWDETAILS,
                        reviewStageId = d.REVIEWSTAGEID,
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
                        customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                        statusId = d.APPROVALSTATUSID,
                        terms = d.REPAYMENTTERMS,
                        currencyId = d.CURRENCYID,
                        schedule = context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault() == null ? "" : context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault(),

                    })

             }).GroupBy(d => d.loanReviewApplicationId).ToList();

            applications = query.AsQueryable()
           .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
           .OrderByDescending(x => x.loanReviewApplicationId);

            var list = applications.ToList();
            //var count = applications.Count();

            return applications;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetLoanReviewAvailmentAwaitingApproval(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;

            bool ignoreBranch = true;
            var operationIds = GetAllLMSApprovalOperationList();

            IQueryable<LoanReviewApplicationViewModel> applications = null;
            
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);
            var staffs = general.GetStaffRlieved(staffId);
            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == user.BranchId || ignoreBranch)
             .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
             .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
             .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    
                    && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Authorised
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                     && x.RESPONSESTAFFID == null
                     && x.OPERATIONID != (int)OperationsEnum.APSReleaseApproval
                     && ((levelIds.Contains((int)x.TOAPPROVALLEVELID) && x.TOSTAFFID == null) || (levelIds.Contains((int)x.TOAPPROVALLEVELID) && staffs.Contains(x.TOSTAFFID ?? 0))
                     || (!levelIds.Contains((int)x.TOAPPROVALLEVELID)) && staffs.Contains(x.TOSTAFFID ?? 0))
             ),
                 alaba => alaba.ab.a.LOANAPPLICATIONID,
                 trail => trail.TARGETID,
                 (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
             .Select(x => new LoanReviewApplicationViewModel
             {
                 lmsApplicationId = x.application.LOANAPPLICATIONID,
                 lmsOperationId = x.application.OPERATIONID,

                 //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                 approvalState = x.trail == null ? "Pending" : x.trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                 approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                 currentApprovalLevel = x.trail == null ? "" : x.trail.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                                                                                      // currentApprovalLevelTypeId = x.trail == null ? null : x.trail.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                 currentApprovalLevelId = x.trail == null ? 0 : x.trail.TOAPPROVALLEVELID,
                 lastComment = x.trail == null ? "" : x.trail.COMMENT,
                 toStaffId = x.trail == null ? 0 : x.trail.TOSTAFFID,
                 requestStaffId = x.trail == null ? 0 : x.trail.REQUESTSTAFFID,
                 systemArrivalDate = x.application.DATETIMECREATED,
                 applicationDate = x.application.APPLICATIONDATE,
                 approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 approvalStatusId = (int)x.application.APPROVALSTATUSID,
                 createdBy = x.application.CREATEDBY,
                 loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                 referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                 relatedReferenceNumber = x.application.RELATEDREFERENCENUMBER,
                 branchId = x.branch.BRANCHID,
                 branchName = x.branch.BRANCHNAME,
                 customerId = x.customer.CUSTOMERID,
                 operationId = x.application.OPERATIONID,
                 customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,
                 atInitiator = x.application.CREATEDBY == staffId,
                 timeIn = x.trail.SYSTEMARRIVALDATETIME,
                 slaTime = x.trail.SLADATETIME,
                 currentApprovalStatus = x.trail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 currentApprovalStateId = x.trail.APPROVALSTATEID,


                 customerGroupName = context.TBL_CUSTOMER_GROUP.Where(c => c.CUSTOMERGROUPID == x.application.CUSTOMERGROUPID).Select(c => c.GROUPNAME).FirstOrDefault() ?? "",

                 loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(l => l.LOANAPPLICATIONTYPEID == x.application.LOANAPPLICATIONTYPEID).Select(l => l.LOANAPPLICATIONTYPENAME).FirstOrDefault() ?? "N/A",
                 facility = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LMSR_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
                 approvedAmount = x.application.APPROVEDAMOUNT == null ? 0 : x.application.APPROVEDAMOUNT,
                 productClassProcessId = x.application.PRODUCT_CLASS_PROCESSID == null ? 0 : x.application.PRODUCT_CLASS_PROCESSID,
                 divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.application.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                 globalsla = !context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).Any() ? 0 : context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).FirstOrDefault(),
                 currentApprovalLevelSlaInterval = x.trail.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
                 dateTimeCreated = x.application.DATETIMECREATED,
                 operationTypeName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.application.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),

                 responsiblePerson = context.TBL_STAFF
                                             .Where(s => s.STAFFID == x.trail.TOSTAFFID)
                                             .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                             .FirstOrDefault().name ?? "",
                 toApprovalLevelId = x.trail.TOAPPROVALLEVELID,

                 // currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,
                 creditOperationType = context.TBL_LMSR_APPLICATION_DETAIL.Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count() > 1
                     ? "Multiple"
                     : context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID ==
                             context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).OPERATIONID
                         ).OPERATIONNAME,

                 facilityType = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                 applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                    .Select(d => new applicationDetails
                    {
                        creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0:
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                        creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

                        detailId = d.LOANREVIEWAPPLICATIONID,
                        loanApplicationId = d.LOANAPPLICATIONID,
                        operationId = d.OPERATIONID,
                        operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                        reviewDetails = d.REVIEWDETAILS,
                        reviewStageId = d.REVIEWSTAGEID,
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
                        customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                        statusId = d.APPROVALSTATUSID,
                        terms = d.REPAYMENTTERMS,
                        currencyId = d.CURRENCYID,
                        schedule = context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault() == null ? "" : context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault(),

                    })

             }).GroupBy(d => d.loanReviewApplicationId).ToList();

            applications = query.AsQueryable()
           .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
           .OrderByDescending(x => x.approvalTrailId);

            var list = applications.ToList();
            //var count = applications.Count();

            return applications;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;
            var staff = context.TBL_STAFF.FirstOrDefault(O => O.STAFFID == staffId);

            List<int> approvalOperations = context.TBL_OPERATIONS.Where(x => x.OPERATIONTYPEID == (short)OperationTypeEnum.LoanReviewApplication
                                                                        && x.OPERATIONID != (short)OperationsEnum.LoanReviewApprovalAvailment
                                                                        && x.OPERATIONID != (short)OperationsEnum.LoanReviewDrawdownForExtension
                                                                        && x.OPERATIONID != (short)OperationsEnum.ContingentReviewDrawdownForExtension
                                                                        && x.OPERATIONID != (short)OperationsEnum.OverdraftReviewDrawdownForExtension)
                .Select(x => x.OPERATIONID).ToList();


            bool ignoreBranch = true;
            if (operationId == 47) if (ProcessInitiator(staffId, operationId, classId, 2)) ignoreBranch = false;
            if (approvalOperations.Contains(operationId)) if (ProcessInitiator(staffId, operationId, classId, 1)) ignoreBranch = false;

            List<int> operationIds = new List<int>();
            operationIds.Add(operationId);
            operationIds.AddRange(approvalOperations);

            IQueryable<LoanReviewApplicationViewModel> applications = null;

            List<int> levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId);

            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == user.BranchId || ignoreBranch).Where(
                x => x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted)
            // .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
             .Join(context.TBL_CUSTOMER, ab => ab.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab })
             .Join(context.TBL_APPROVAL_TRAIL.Where(x => operationIds.Contains(x.OPERATIONID)
                    // && x.APPROVALSTATEID != (int)ApprovalState.Ended
                    && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Authorised
                    || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                    && x.RESPONSESTAFFID == null
                    && x.APPROVALSTATEID != (int)ApprovalState.Ended
                    && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                    && ((x.TOSTAFFID == null || staffs.Contains((int)x.TOSTAFFID))
                    //&& ((levelIds.Contains((int)x.TOAPPROVALLEVELID) && x.TOSTAFFID == null) || (levelIds.Contains((int)x.TOAPPROVALLEVELID) && x.TOSTAFFID == staffId)
                    //|| (!levelIds.Contains((int)x.TOAPPROVALLEVELID) && (x.TOSTAFFID == staffId))
                    )
             ),
                 alaba => alaba.ab.LOANAPPLICATIONID,
                 trail => trail.TARGETID,
                 (alaba, trail) => new { application = alaba.ab, trail, branch = alaba.b, customer = alaba.c })
             .Select(x => new LoanReviewApplicationViewModel
             {

                 lmsApplicationId = x.application.LOANAPPLICATIONID,
                 lmsOperationId = x.application.OPERATIONID,

                 staffId = staffId,
                 staffRoleCode = context.TBL_STAFF_ROLE.FirstOrDefault(O => O.STAFFROLEID == staff.STAFFROLEID).STAFFROLECODE,
                 //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                 approvalState = x.trail == null ? "Pending" : x.trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                 approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                 currentApprovalLevelId = x.trail == null ? 0 : x.trail.TOAPPROVALLEVELID,
                 currentApprovalLevel = x.trail == null ? "" : x.trail.TBL_APPROVAL_LEVEL1.LEVELNAME,
                 lastComment = x.trail == null ? "" : x.trail.COMMENT,
                 toStaffId = x.trail == null ? 0 : x.trail.TOSTAFFID,
                 requestStaffId = x.trail == null ? 0 : x.trail.REQUESTSTAFFID,
                 //creditAppraisalOperationId = (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where p.TERMLOANID == d.LOANID select aa.OPERATIONID).FirstOrDefault(),
                 //creditAppraisalLoanApplicationId = (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where p.TERMLOANID == d.LOANID select aa.LOANAPPLICATIONID).FirstOrDefault(),
                 applicationDate = x.application.APPLICATIONDATE,
                 approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 systemArrivalDate = x.application.DATETIMECREATED,
                 approvalStatusId = (int)x.application.APPROVALSTATUSID,
                 createdBy = x.application.CREATEDBY,
                 loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                 loanReviewApplicationDetailId = context.TBL_LMSR_APPLICATION_DETAIL.Where(t=>t.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID).Select(t=>t.LOANREVIEWAPPLICATIONID).FirstOrDefault(),
                 referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                 relatedReferenceNumber = x.application.RELATEDREFERENCENUMBER,
                 branchId = x.branch.BRANCHID,
                 // branchName = x.branch.BRANCHNAME,
                 customerId = x.customer.CUSTOMERID,
                 operationId = x.application.OPERATIONID,
                 customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,
                 atInitiator = x.application.CREATEDBY == staffId,
                 timeIn = x.trail.SYSTEMARRIVALDATETIME,
                 slaTime = x.trail.SLADATETIME,
                 currentApprovalStatus = x.trail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 currentApprovalStateId = x.trail.APPROVALSTATEID,

                 customerGroupName = context.TBL_CUSTOMER_GROUP.Where(c => c.CUSTOMERGROUPID == x.application.CUSTOMERGROUPID).Select(c => c.GROUPNAME).FirstOrDefault() ?? "",

                 loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(l => l.LOANAPPLICATIONTYPEID == x.application.LOANAPPLICATIONTYPEID).Select(l => l.LOANAPPLICATIONTYPENAME).FirstOrDefault() ?? "N/A",
                 facility = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.application.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LMSR_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
                 approvedAmount = x.application.APPROVEDAMOUNT == null ? 0 : x.application.APPROVEDAMOUNT,
                 productClassProcessId = x.application.PRODUCT_CLASS_PROCESSID == null ? 0 : x.application.PRODUCT_CLASS_PROCESSID,
                 divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.application.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                 globalsla = !context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).Any() ? 0 : context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.application.PRODUCTCLASSID).Select(c => c.GLOBALSLA).FirstOrDefault(),
                 currentApprovalLevelSlaInterval = x.trail.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
                 dateTimeCreated = x.application.DATETIMECREATED,
                 operationTypeName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.application.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                 //submittedForAppraisal = x.trail.SUBMITTEDFORAPPRAISAL,
                 responsiblePerson = context.TBL_STAFF
                                             .Where(s => s.STAFFID == x.trail.TOSTAFFID)
                                             .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                             .FirstOrDefault().name ?? "",
                 toApprovalLevelId = x.trail.TOAPPROVALLEVELID,

                 // currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,
                 creditOperationType = context.TBL_LMSR_APPLICATION_DETAIL.Where(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count() > 1
                     ? "Multiple"
                     : context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID ==
                             context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).OPERATIONID
                         ).OPERATIONNAME,

                 facilityType = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                 //applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                 //   .Select(d => new applicationDetails
                 //   {
                 //       detailId = d.LOANREVIEWAPPLICATIONID,
                 //       loanApplicationId = d.LOANAPPLICATIONID,
                 //       operationId = d.OPERATIONID,
                 //       operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                 //       reviewDetails = d.REVIEWDETAILS,
                 //       reviewStageId = d.REVIEWSTAGEID,
                 //       loanId = d.LOANID,
                 loanSystemTypeId = (short)x.application.LOANAPPLICATIONTYPEID, //context.TBL_LMSR_APPLICATION_DETAIL.Where(y => y.LOANAPPLICATIONID == x.application.LOANAPPLICATIONID).Select(y => y.LOANSYSTEMTYPEID).FirstOrDefault(),
                 //       loanSystemTypeName = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                 //       productId = d.PRODUCTID,
                 //       customerId = d.CUSTOMERID,
                 //       obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                 //       proposedTenor = d.PROPOSEDTENOR,
                 //       proposedRate = d.PROPOSEDINTERESTRATE,
                 //       proposedAmount = d.PROPOSEDAMOUNT,
                 //       approvedTenor = d.APPROVEDTENOR,
                 //       approvedRate = d.APPROVEDINTERESTRATE,
                 //       approvedAmount = d.APPROVEDAMOUNT,
                 //       customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                 //       statusId = d.APPROVALSTATUSID,
                 //       accountName = d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN.Where(M => M.TERMLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME :
                 //                                                                                               d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ExternalFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_EXTERNAL.Where(M => M.EXTERNALLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME :
                 //                                                                                               d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_REVOLVING.Where(M => M.REVOLVINGLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_CONTINGENT.Where(M => M.CONTINGENTLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_APPLICATION_DETAIL.Where(M => M.LOANAPPLICATIONDETAILID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME,
                 //       accountNumber = d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN.Where(M => M.TERMLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER :
                 //          d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ExternalFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_EXTERNAL.Where(M => M.EXTERNALLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME :
                 //         d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_REVOLVING.Where(M => M.REVOLVINGLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_CONTINGENT.Where(M => M.CONTINGENTLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_APPLICATION_DETAIL.Where(M => M.LOANAPPLICATIONDETAILID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                 //       terms = d.REPAYMENTTERMS,
                 //       currencyId = d.CURRENCYID,
                 //       schedule = context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault() == null ? "" : context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault(),
                 //   })

             }).GroupBy(d => d.loanReviewApplicationId).ToList();

            applications = query.AsQueryable()
           .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
           //.OrderByDescending(x => x.loanReviewApplicationId)
           ;

            foreach (var d in applications)
            {
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                {
                    d.creditAppraisalOperationId = (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select aa.OPERATIONID).FirstOrDefault();
                }
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.ExternalFacility)
                {
                    d.creditAppraisalOperationId = (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select c.OPERATIONID).FirstOrDefault();
                }
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                {
                    d.creditAppraisalOperationId = (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select aa.OPERATIONID).FirstOrDefault();
                }
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                {
                    d.creditAppraisalOperationId = (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select aa.OPERATIONID).FirstOrDefault();
                }


                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                {
                    d.creditAppraisalLoanApplicationId = (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select aa.LOANAPPLICATIONID).FirstOrDefault();
                }
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.ExternalFacility)
                {
                    d.creditAppraisalLoanApplicationId = (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select c.LOANAPPLICATIONID).FirstOrDefault();
                }
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                {
                    d.creditAppraisalLoanApplicationId = (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select aa.LOANAPPLICATIONID).FirstOrDefault();
                }
                if (d.loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                {
                    d.creditAppraisalLoanApplicationId = (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationDetailId select aa.LOANAPPLICATIONID).FirstOrDefault();
                }



                //d.creditAppraisalLoanApplicationId = (d.loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationId select aa.LOANAPPLICATIONID).FirstOrDefault() :
                //                                   (d.loanSystemTypeId == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                //                                   (d.loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationId select aa.LOANAPPLICATIONID).FirstOrDefault() :
                //                                    (d.loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility) ? (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.loanReviewApplicationId select aa.LOANAPPLICATIONID).FirstOrDefault() : 0;

            }
            //var list = applications.ToList();
            //var count = applications.Where(x=>x.referenceNumber == "0000001040" || x.referenceNumber == "0000001039").ToList();

            return applications;
        }

        public IEnumerable<SubsidiaryViewModel> GetSubsidiaryApplications(UserInfo user, int operationId, int? classId, string staffRoleCode)
        {
            try
            {
                var data = (from a in context.TBL_LMS_SUB_BASICTRANSACTION
                            where
                            (a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                            || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                            || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            || a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                            || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                            && a.STAFFROLECODE == staffRoleCode && a.ACTEDON == false

                            select new SubsidiaryViewModel
                            {
                                subBasicId = a.ID,
                                loanApplicationId = a.LOANAPPLICATIONID,
                                loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                relatedReferenceNumber = a.RELATEDREFERENCENUMBER,
                                customerId = a.CUSTOMERID,
                                customerGlobalId = a.CUSTOMERGLOBALID,
                                countryCode = a.COUNTRYCODE,
                                productClassName = a.PRODUCTCLASSNAME,
                                productClassProcess = a.PRODUCT_CLASS_PROCESS,
                                subsidiaryId = a.SUBSIDIARYID,
                                applicationDate = a.APPLICATIONDATE,
                                systemDateTime = (DateTime)a.SYSTEMDATETIME,
                                applicationAmount = a.APPLICATIONAMOUNT,
                                totalExposureAmount = a.TOTALEXPOSUREAMOUNT,
                                interestRate = a.INTERESTRATE,
                                applicationTenor = a.APPLICATIONTENOR,
                                currentApprovalLevelId = a.APPROVALLEVELID,
                                currentApprovalLevelTypeId = a.APPROVALLEVELGLOBALCODE,
                                toStaffId = a.TOSTAFFID,
                                divisionCode = a.BUSINESSUNITSHORTCODE,
                                timeIn = a.SYSTEMARRIVALDATETIME,
                                approvalStatusId = (short)a.APPROVALSTATUSID,
                                applicationStatusId = (short)a.APPLICATIONSTATUSID,
                                operationName = a.OPERATIONNAME,
                                customerName = a.CUSTOMERID.HasValue ? a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME : "",
                                dateTimeCreated = a.DATETIMECREATED,
                                createdBy = a.CREATEDBY,
                                createdByName = a.CREATEDBYNAME,
                                targetId = a.TARGETID,
                                operationId = a.OPERATIONID,
                                actedOn = a.ACTEDON,
                                loanTypeName = a.LOANAPPLICATIONTYPENAME,
                                facility = a.PRODUCTNAME,
                                divisionShortCode = a.BUSINESSUNITSHORTCODE,
                                submitted = true
                            }).ToList();

                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<applicationDetails> GetApplicationsById(UserInfo user, int lmsApplicationId)
        {

            var applicationDetails = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false && d.LOANAPPLICATIONID == lmsApplicationId)
               .Select(d => new applicationDetails
               {
                   detailId = d.LOANREVIEWAPPLICATIONID,
                   loanApplicationId = d.LOANAPPLICATIONID,
                   operationId = d.OPERATIONID,
                   operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                   reviewDetails = d.REVIEWDETAILS,
                   reviewStageId = d.REVIEWSTAGEID,
                   loanId = d.LOANID,
                   creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select c.OPERATIONID).FirstOrDefault() :
                                                (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                   creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                               (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

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
                   customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                   statusId = d.APPROVALSTATUSID,
                   accountName = d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN.Where(M => M.TERMLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME :
                                                                                                           d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ExternalFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_EXTERNAL.Where(M => M.EXTERNALLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME :
                                                                                                           d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_REVOLVING.Where(M => M.REVOLVINGLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_CONTINGENT.Where(M => M.CONTINGENTLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_APPLICATION_DETAIL.Where(M => M.LOANAPPLICATIONDETAILID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME,
                   accountNumber = d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN.Where(M => M.TERMLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER :
                      d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ExternalFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_EXTERNAL.Where(M => M.EXTERNALLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME :
                     d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_REVOLVING.Where(M => M.REVOLVINGLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_CONTINGENT.Where(M => M.CONTINGENTLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_APPLICATION_DETAIL.Where(M => M.LOANAPPLICATIONDETAILID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                   terms = d.REPAYMENTTERMS,
                   currencyId = d.CURRENCYID,
                   schedule = context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault() == null ? "" : context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault(),
               }).ToList();




            return applicationDetails; 
        }

        public List<LoanReviewApplicationViewModel> CalculateSLA(List<LoanReviewApplicationViewModel> apps)
        {
            foreach (var app in apps)
            {
                app.slaGlobalStatus = GetSlaGlobalStatus(app);
                app.slaInduvidualStatus = GetSlaInduvidualStatus(app);
            }
            return apps;
        }

        private string GetSlaInduvidualStatus(LoanReviewApplicationViewModel app)
        {
            float sla = app.currentApprovalLevelSlaInterval;
            //int? elapse = (DateTime.Now - timeIn)?.Hours;
            int? elapse = (int)GetTimeIntervalHours(app.timeIn.Value, DateTime.Now);
            return SlaStatus(sla, elapse);
        }

        public string GetSlaGlobalStatus(LoanReviewApplicationViewModel app)
        {
            float sla = app.globalsla;
            //int? elapse = (DateTime.Now - dateTimeCreated).Hours;
            int? elapse = (int)GetTimeIntervalHours(app.dateTimeCreated, DateTime.Now);
            return SlaStatus(sla, elapse);
        }

        private string SlaStatus(float sla, int? elapse)
        {
            if (sla == 0) return "success";
            if (elapse == 0 || elapse == null) return "success";
            float factor = (float)(elapse / sla) * 100;
            if (factor <= 30) return "success";
            if (factor <= 70) return "warning";
            if (factor <= 100) return "danger";
            return "danger";
        }

        public IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            }
            yield return startDate;

            while (startDate.Date < endDate.Date && startDate.AddDays(1).Date < endDate.Date)
            {
                yield return new DateTime(startDate.AddDays(1).Year, startDate.AddDays(1).Month, startDate.AddDays(1).Day, 23, 59, 59);
                startDate = startDate.AddDays(1);
            }
            yield return endDate;
        }

        public bool IsInHolidays(DateTime date, int countryId)
        {
            List<TBL_PUBLIC_HOLIDAY> holidays;
            //holidays = context.TBL_PUBLIC_HOLIDAY.ToList();
            var output = context.TBL_PUBLIC_HOLIDAY.Any(x => x.DATE == date.Date);
            return output;
        }

        public IEnumerable<DateTime> FilterHolidaysFromDateIntervals(IEnumerable<DateTime> dateTimes)
        {
            var list = dateTimes.ToList();
            var countryId = context.TBL_COUNTRY.FirstOrDefault().COUNTRYID;
            List<TBL_PUBLIC_HOLIDAY> holidays = context.TBL_PUBLIC_HOLIDAY.Where(h => h.COUNTRYID == countryId).ToList();
            var dates = holidays.Select(h => h.DATE);
            list = list.FindAll(l => !IsInDatesList(l, dates));
            return list;
        }

        public bool IsInDatesList(DateTime dateTime, IEnumerable<DateTime> dateTimes)
        {
            var output = dateTimes.Any(x => x.Date == dateTime.Date);
            return output;
        }

        public double GetTimeIntervalHours(DateTime startDate, DateTime endDate)
        {
            double hours = 0;
            var second = new TimeSpan(0, 0, 1);
            var range = GetDateRange(startDate, endDate);
            var test = range.ToList();
            range = FilterHolidaysFromDateIntervals(range);
            var intervals = range.Select(r => new DateTimeAndTimeOfDayViewModel
            {
                dateTime = r
            });
            var list = intervals.ToList();
            //dateTimeAndTimeOfDay = list;
            for (int i = 0; i < list.Count - 1; i++)
            {
                var elapsed = list[i + 1].dateTime.Subtract(list[i].dateTime);
                if (elapsed.Days <= 1)
                {
                    list[i + 1].timeOfDay = elapsed;
                    hours += list[i + 1].timeOfDay.TotalHours;
                }
                else
                {
                    var elapsedDays = elapsed.Days * 24;
                    list[i + 1].timeOfDay = elapsed;
                    hours += (list[i + 1].timeOfDay.TotalHours - elapsedDays);
                }
            }
            return hours;
        }

        private bool ProcessInitiator(int staffId, int operationId, int? productClassId, int position)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.TBL_OPERATIONS.OPERATIONTYPEID == 11 && x.PRODUCTCLASSID == productClassId)
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

        public List<LMSOperationListViewModel> GetApplicationOperations()
        {

            var operations = (from a in context.TBL_OPERATIONS
                              select a).ToList();

            List<LMSOperationListViewModel> output = new List<LMSOperationListViewModel>();

            //LMSOperationListViewModel value = new LMSOperationListViewModel {operationId = (short)OperationsEnum.TenorChange, operationName = "Tenor Change", loanSystemTypeId = (short)LoanSystemTypeEnum.ContingentLiability };

            // ----------------contingent liability --------------------------------------
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.ContingentLiabilityRenewal,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.ContingentLiabilityRenewal).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.ContingentLiability,
                productTypeId = (short)LoanProductTypeEnum.ContingentLiability
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.ContingentLiabilityTermination,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.ContingentLiabilityTermination).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.ContingentLiability,
                productTypeId = (short)LoanProductTypeEnum.ContingentLiability
            });

            //---------------------overdraft/revolving------------------------------------
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftInterestRate,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftInterestRate).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftRenewal,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftRenewal).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftSubAllocation,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftSubAllocation).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftTenorExtension,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftTenorExtension).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftTopup,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftTopup).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });

            return output;
        }

        public SelectListViewModel GetAllLMSApprovalOperationList()
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }).ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x =>
               // (
                x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanReviewApplication
                //|| x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft
                // || x.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial)
                && x.ISDISABLED == false
            ).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME, typeId = (int)x.OPERATIONTYPEID , productTypeId=x.PRODUCTTYPEID}).OrderBy(o => o.name).ToList();
            list.feeCharges = context.TBL_CHARGE_FEE.Select(x => new DropDownSelect { id = x.CHARGEFEEID, name = x.CHARGEFEENAME }).ToList();

            return list;
        }


        public SelectListViewModel GetAllLMSApprovalOperationListByProductTypeId(int productTypeId)
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }).ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            if (productTypeId == (int)LoanProductTypeEnum.SelfLiquidating
                || productTypeId == (int)LoanProductTypeEnum.ForeignXRevolving
                || productTypeId == (int)LoanProductTypeEnum.SyndicatedTermLoan
                || productTypeId == (int)LoanProductTypeEnum.SyndicatedTermLoan) { productTypeId = (int)LoanProductTypeEnum.TermLoan; }

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x =>
                // (
                x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanReviewApplication
                //|| x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft
                // || x.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial)
                && x.ISDISABLED == false
            ).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME, typeId = (int)x.OPERATIONTYPEID, productTypeId = x.PRODUCTTYPEID }).Where(t => t.productTypeId == productTypeId || t.productTypeId == null).OrderBy(o => o.name).ToList();
            list.feeCharges = context.TBL_CHARGE_FEE.Select(x => new DropDownSelect { id = x.CHARGEFEEID, name = x.CHARGEFEENAME }).ToList();

            return list;
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
            list.feeCharges = context.TBL_CHARGE_FEE.Select(x => new DropDownSelect { id = x.CHARGEFEEID, name = x.CHARGEFEENAME }).ToList();
            return list;
        }
        public LoanChargeFeeViewModel GetChargeFeeDetails(int id)
        {
            var list = new LoanChargeFeeViewModel();
             list = context.TBL_CHARGE_FEE.Where(a=>a.CHARGEFEEID == id).Select(x => new LoanChargeFeeViewModel {
                 chargeFeeId = x.CHARGEFEEID,
                 chargeFeeName = x.CHARGEFEENAME,
                 rate = x.RATE==null? 0 : x.RATE,
                 amount = x.AMOUNT,
                 feeTypeId = x.FEETYPEID

             }).FirstOrDefault();
            return list;
        }

        public string SubmitLoanReviewApplication(LoanReviewApplicationViewModel model)
        {
            if (model.applicationDetails.Count() > 1 &&
                model.applicationDetails.Any(x => apsOperationIds.Contains(x.operationId)))
            {
                throw new SecureException("Only one operation request is allowed for APS release related applications!");
            }
            List<int> tenorExtensionOperations = new List<int> { (int)OperationsEnum.TenorExtensionApproval, (int)OperationsEnum.OverdraftTenorExtensionApproval,
                (int)OperationsEnum.ContingentLiabilityTenorExtensionApproval };
            var operationIsTenorExtension = tenorExtensionOperations.Contains(model.operationId ?? 0);

            /*if (model.applicationDetails.Count() > 0 &&
                model.applicationDetails.Any(x => apsOperationIds.Contains(x.operationId)) ||model.operationId == (int)OperationsEnum.APSReleaseApproval)
            {
                var lien = context.TBL_CASA_LIEN.FirstOrDefault(x => x.SOURCEREFERENCENUMBER == model.loanReferenceNumber && x.LIENTYPEID == (int)LienTypeEnum.APGBooking);
                if (lien == null) throw new SecureException("No lien has been placed");
            }*/


            //var synOperationId = context.TBL_OPERATIONS.Find(model.operationId).SYNCHOPERATIONID;

            var doesOperationExist = (from a in context.TBL_LOAN_REVIEW_OPERATION
                                              where a.OPERATIONTYPEID == model.operationId
                                              && (a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                                              && a.OPERATIONCOMPLETED == false
                                             select a).ToList();
            if (doesOperationExist.Count() > 0)
            {
                //throw new SecureException("The requested operation already exist and going through approval");
            }

            int staffId = model.createdBy;
            var referenceNumber = GenerateReferenceNumber();
            var applicationDate = general.GetApplicationDate();
            // int camOperationId = GetCamOperation(model.performanceTypeId);

            int loanId = 0;
            if (model.loanSystemTypeId == (short)LoanSystemTypeEnum.ExternalFacility)
            {
                var thirdpatyLoan = context.TBL_LOAN_EXTERNAL.Where(x => x.LOANREFERENCENUMBER == model.loanReferenceNumber).FirstOrDefault();
                if (thirdpatyLoan != null)
                {
                    model.customerId = thirdpatyLoan.CUSTOMERID;
                    loanId = thirdpatyLoan.EXTERNALLOANID;
                }
            }


            bool result = true;

            foreach (var detail in model.applicationDetails)
            {
                if (apsOperationIds.Contains(detail.operationId)) model.operationId = detail.operationId; // for APS release

                if (detail.operationId == (int)OperationsEnum.CommercialLoanSubAllocation)
                {
                    result = ValidateNewSubAllocationOperation(detail.detailId, model.customerId, detail.loanSystemTypeId);

                    if (result == false)
                        throw new ConditionNotMetException("Customer Must Have More Than One Tranche to Proceed With Sub Allocation");

                }
                else if (detail.operationId == (int)OperationsEnum.OverdraftSubAllocation)
                {
                    result = ValidateNewSubAllocationOperation(detail.detailId, model.customerId, detail.loanSystemTypeId);

                    if (result == false)
                        throw new ConditionNotMetException("Customer Must Have More Than One Tranche to Proceed With Sub Allocation");

                }
                //else if(detail.operationId == (int)OperationsEnum.)
                //{

                //}

            }

            var application = context.TBL_LMSR_APPLICATION.Add(new TBL_LMSR_APPLICATION
            {
                APPLICATIONREFERENCENUMBER = referenceNumber,
                COMPANYID = model.companyId,
                CUSTOMERID = model.customerId,
                BRANCHID = model.branchId,
                OPERATIONID = (short)model.operationId, //camOperationId,
                CAPREGIONID = model.regionId,
                // CUSTOMERGROUPID = null,
                DISPUTED = false,
                REQUIRECOLLATERAL = false,
                APPLICATIONDATE = applicationDate,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONSTATUSID = (short)1, // remove magic numbers
               // PROPOSEDTENOR = model.proposedTenor,
               // PROPOSEDINTEREST = model.proposedInterest,
               PRODUCTCLASSID = model.productClassId,
               PRODUCTID = model.productId,
               LOANAPPLICATIONTYPEID = model.loanApplicationTypeId,
               PRODUCT_CLASS_PROCESSID = model.productClassProcessId,
               APPROVEDAMOUNT = model.approvedAmount,
                
            });

            List<int> customerIds = new List<int>();
            LoanViewModel loan = new LoanViewModel();

            foreach (var detail in model.applicationDetails)
            {
                
                if (model.loanSystemTypeId != (short)LoanSystemTypeEnum.ExternalFacility) { loanId = detail.loanId; }

                loan = GetLoanInformation(detail.loanSystemTypeId, loanId, applicationDate);
                int tenor = detail.loanSystemTypeId == 4 ? loan.tenorUsed : loan.tenor;

                context.TBL_LMSR_APPLICATION_DETAIL.Add(new TBL_LMSR_APPLICATION_DETAIL
                {
                    LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                    LOANID = loanId,
                    LOANSYSTEMTYPEID = detail.loanSystemTypeId,/*Term/Disbursed Facility..Overdraft Facilizzty..Contingent Liability*/
                    OPERATIONID = (short) model.operationId, // detail.operationId, // refactor to operationId from ui!
                    REVIEWDETAILS = detail.reviewDetails,
                    PRODUCTID = detail.productId,
                    REPAYMENTTERMS = String.Empty,
                    REPAYMENTSCHEDULEID = detail.repaymentScheduleId,
                    //REPAYMENTSCHEDULE = context.TBL_REPAYMENT_TERM.Find(x.d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
                    CUSTOMERID = loan.customerId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved,
                    CREATEDBY = staffId,
                    DATETIMECREATED = applicationDate,
                    //PROPOSEDTENOR = tenor,
                    PROPOSEDTENOR = detail.duration == 0 ? tenor : detail.duration,
                    PROPOSEDINTERESTRATE = loan.interestRate,
                    PROPOSEDAMOUNT = loan.outstandingPrincipal,
                    //APPROVEDTENOR = tenor,
                    APPROVEDTENOR = detail.duration == 0 ? tenor : detail.duration,
                    APPROVEDINTERESTRATE = loan.interestRate,
                    APPROVEDAMOUNT = loan.outstandingPrincipal,
                    OPERATIONPERFORMED = false,
                    CUSTOMERPROPOSEDAMOUNT = detail.customerProposedAmount,
                    DELETED = false,
                    CURRENCYID = (short)loan.currencyId,
                    //LOANREFERENCENUMBER = loan.loanReferenceNumber
                    //LOANAPPLICATIONDETAILID = loan.loanApplicationDetailId,
                });

                customerIds.Add(loan.customerId);
            }

            // ------------AUDIT CODE HERE! -------------

            if (context.SaveChanges() == 0) throw new SecureException("An error occured while saving the data!"); // this save is necessary to grab targetid

            workflow.ToStaffId = staffId;

            bool assetManagement = false;
            var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == staffId);
            if (user.STAFFROLEID == classifiedAssetManagementRoleId) assetManagement = true;

            if (assetManagement)
            {

                workflow.NextProcess(model.companyId, staffId, (short)model.operationId, null, application.LOANAPPLICATIONID, null, "Initiation", true, true, true);
                application.OPERATIONID = (short)model.operationId;//79;
                context.Entry(application).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                workflow.NextProcess(model.companyId, staffId, (short)model.operationId, null, application.LOANAPPLICATIONID, null, "Initiation", true, true, true);
            }

            if (model.loanSystemTypeId != (short)LoanSystemTypeEnum.ExternalFacility && operationIsTenorExtension)
            {
                var loanApp = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == model.loanReferenceNumber);
                if (loanApp != null)
                {
                    application.FINALAPPROVAL_LEVELID = GetSimilarLevelId(loanApp.FINALAPPROVAL_LEVELID ?? 0, workflow.GetWorkFlowSetupLevelIds());
                    //loanId = loanApp.EXTERNALLOANID;
                }
            }

            if (context.SaveChanges() > 0)
            {
                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
               // if (setup.USE_THIRD_PARTY_INTEGRATION) creditCommon.LoadCustomerTurnover(application.LOANAPPLICATIONID,customerIds,staffId, true);

                return "Application with reference number " + referenceNumber + " created.";
            }

            throw new SecureException("An error occured while saving the data!");
        }

        public int GetSimilarLevelId(int levelId, IEnumerable<dynamic> levelIds)
        {
            if (levelId <= 0)
            {
                return 0;
            }
            var roleId = context.TBL_APPROVAL_LEVEL.FirstOrDefault(l => l.APPROVALLEVELID == levelId).STAFFROLEID;
            //levelIds = levelIds.Select(l => new { l.roleId, })
            var result = levelIds.FirstOrDefault(l => l.roleId == roleId)?.levelId;
            return result ?? 0;
        }


        public bool ValidateSubAllocationOperation(int loanApplicationDetailId, int customerId)
        {

            if (loanApplicationDetailId != 0)
            {
                var loanData = (from a in context.TBL_LOAN
                                join b in context.TBL_PRODUCT
                                on a.PRODUCTID equals b.PRODUCTID
                                where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                && b.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                if (loanData.Count < 2 || loanData == null)
                {
                    return false; // throw new ConditionNotMetException("Customer Must Have More Than One Tranch to Proceed With Sub Allocation");  
                }
                else
                {
                    return true;
                }
            }
            else
            {
                var appplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId).Select(x => x.LOANAPPLICATIONID).FirstOrDefault();

                var loanData = (from a in context.TBL_LOAN_REVOLVING
                                where a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == appplicationId // .CUSTOMERID == customerId
                                && a.MATURITYDATE < context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                //var test = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

                if (loanData.Count < 2 || loanData == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }



        }

        public bool ValidateNewSubAllocationOperation(int loanApplicationDetailId, int customerId, int loanSystemTypeId)
        {

            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                var loanData = (from a in context.TBL_LOAN
                                join b in context.TBL_PRODUCT
                                on a.PRODUCTID equals b.PRODUCTID
                                where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                && b.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                if (loanData.Count < 2 || loanData == null)
                {
                    return false; // throw new ConditionNotMetException("Customer Must Have More Than One Tranch to Proceed With Sub Allocation");  
                }
                else
                {
                    return true;
                }
            }
            else
            {
                var appplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId).Select(x => x.LOANAPPLICATIONID).FirstOrDefault();

                var loanData = (from a in context.TBL_LOAN_REVOLVING
                                join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                where b.LOANAPPLICATIONID == appplicationId
                                && a.MATURITYDATE >= context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                //var test = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

                if (loanData.Count < 2 || loanData == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }



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

        public WorkflowResponse ForwardApplicationAppraisal (ForwardReviewViewModel model)
        {
            int nextProcessId = model.operationId + 1;
            int operationId = model.operationId; // beware of nplappraisal!
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);
            int lastOperationId = (int)OperationsEnum.LoanReviewApprovalAvailment;

            var checklistValidation = ChecklistCompleted(model.applicationId);
            if (appl.CREATEDBY == model.createdBy && model.operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter && checklistValidation == false)
            {
                throw new SecureException("Checklist not completed!");
            }

            // customization for CAM approvals
            //bool operationIsCam = (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) || (operationId == (int)OperationsEnum.NPLoanReviewApprovalAppraisal);
            if (camOperationIds.Contains(operationId))
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                operationId = (int)appl.OPERATIONID;
                nextProcessId = (int)OperationsEnum.LoanReviewApprovalOfferLetter; // redefine
            }


            if (apsOperationIds.Contains(operationId))
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                operationId = (int)appl.OPERATIONID;
                nextProcessId = (int)OperationsEnum.LoanReviewApprovalAvailment; // redefine
                workflow.Amount = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID).Sum(x => x.CUSTOMERPROPOSEDAMOUNT) ?? 0;
            }

            if (camOperationIds.Contains(operationId) || (operationId == (int)OperationsEnum.LoanReviewApprovalAvailment))
            {
                workflow.Amount = GetMaximumApplicationOutstandingBalance(appl.LOANAPPLICATIONID);
            }
            var lmsrDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
            workflow.BusinessUnitId = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == lmsrDetail.FirstOrDefault().CUSTOMERID).BUSINESSUNTID;
            workflow.StaffId = model.lastUpdatedBy;
            workflow.CompanyId = appl.COMPANYID;
            workflow.OperationId = operationId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.ProductClassId = null;
            workflow.StatusId = model.forwardAction;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.Comment = model.comment;
            workflow.Vote = model.vote;
            workflow.DeferredExecution = true;
            workflow.FinalLevel = appl.FINALAPPROVAL_LEVELID;
            workflow.IsFlowTest = model.isFlowTest;
            workflow.IsFromPc = model.isFromPc;
            workflow.Tenor = lmsrDetail.Max(d => d.APPROVEDTENOR);
            workflow.LevelBusinessRule = new LevelBusinessRule
            {
                Amount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT) ?? 0, // totalApplicationAmount,
                PepAmount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT) ?? 0, // totalApplicationAmount,
                Pep = model.politicallyExposed,
                //InsiderRelated = appl.ISRELATEDPARTY ?? false,
                ProjectRelated = appl.ISPROJECTRELATED ?? false,
                OnLending = appl.ISONLENDING ?? false,
                InterventionFunds = appl.ISINTERVENTIONFUNDS ?? false,
                WithInstruction = appl.WITHINSTRUCTION ?? false,
                //OrrBasedApproval = appl.ISORRBASEDAPPROVAL ?? false,
                DomiciliationNotInPlace = appl.DOMICILIATIONNOTINPLACE ?? false,
                tenor = lmsrDetail.Max(d => d.APPROVEDTENOR),
            };


            if (model.forwardAction == 8 || model.forwardAction == 9)
            {
                var dictionary = GetRepresentStepdownItems(appl.LOANAPPLICATIONID, model.forwardAction, operationId);
                workflow.NextLevelId = dictionary["levelId"];
                workflow.ToStaffId = dictionary["staffId"];
                if (model.forwardAction == 8) workflow.ToStaffId = null;
            }

            workflow.LogActivity();

            // DETAIL CHANGES
            List<TBL_LMSR_APPLICATION_DETAIL> items = null;
            if (model.recommendedChanges != null && model.recommendedChanges.Count() > 0) // only approving authority
            {
                //updateApprovedAmount = true;
                items = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();
                foreach (var changed in model.recommendedChanges)
                {
                    var detail = items.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == changed.detailId);
                    if (detail != null)
                    {
                        //detail.APPROVEDPRODUCTID = (short)changed.productId;
                        detail.APPROVEDAMOUNT = changed.amount;
                        detail.APPROVEDINTERESTRATE = changed.interestRate;
                        detail.APPROVEDTENOR = changed.tenor;
                        detail.APPROVALSTATUSID = changed.statusId;
                        //detail.LASTUPDATEDBY = model.createdBy;
                        //detail.DATETIMEUPDATED = DateTime.Now;

                        if (model.isBusiness) // DELETE OR UPDATE PROPOSED
                        {
                            if (detail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved) { detail.DELETED = true; }
                            else
                            {
                                detail.PROPOSEDAMOUNT = changed.amount;
                                detail.PROPOSEDINTERESTRATE = changed.interestRate;
                                detail.PROPOSEDTENOR = changed.tenor;
                            }
                        }
                    }
                }
            }

            context.SaveChanges();

            int lastStatusId = workflow.StatusId;
            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved && operationId != lastOperationId/* && model.operationId != 71*/) // jump process OR end flag
                {
                    if (apsOperationIds.Contains(operationId)) workflow.NextLevelId = GetFirstAvailmentLevelId((int)OperationsEnum.LoanReviewApprovalAvailment);
                    if (operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter) workflow.NextLevelId = GetFirstAvailmentLevelId((int)OperationsEnum.LoanReviewApprovalAvailment);
                    workflow.SetResponse = false;
                    workflow.NextProcess(appl.COMPANYID, model.lastUpdatedBy, nextProcessId, null, appl.LOANAPPLICATIONID, null, "New application", true, true); // model.operationId must be used here!
                }

                if (operationId == lastOperationId/* || model.operationId == 71*/) appl.APPROVALSTATUSID = (short)lastStatusId; // last or cam?

                //generate offer letter doc
                offerLetter.AddOfferLetterClauses(model.applicationId, model.staffId, true, false);

                context.SaveChanges();

                AddLoanCollateralMapping(model.applicationId);//, appl., (short)LoanSystemTypeEnum.OverdraftFacility);
            }

            //return lastStatusId;
            return workflow.Response;
        }


        public WorkflowResponse ForwardApplication(ForwardReviewViewModel model)
        {
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);
            int operationId = model.operationId; // beware of nplappraisal!
            if (appl == null)
            {
                throw new SecureException("Please Kindly refresh your browser and try again, Thanks");
            }
            if (appl.TOTALEXPOSUREAMOUNT <= 0)
            {
                var lmsrSystemTypeId = appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID;
                if (lmsrSystemTypeId == (int)LoanSystemTypeEnum.ExternalFacility)
                {
                    memo.InitForThirdpartyLoans(operationId, model.applicationId);
                    appl.TOTALEXPOSUREAMOUNT = memo.GetApprovalAmount(true);
                }
                else
                {
                    memo.Init(operationId, model.applicationId);
                    appl.TOTALEXPOSUREAMOUNT = memo.GetApprovalAmount(true);
                }
            }
            List<short> tenorExtensionOperations = new List<short> { (int)OperationsEnum.TenorExtensionApproval, (int)OperationsEnum.OverdraftTenorExtensionApproval, (int)OperationsEnum.ContingentLiabilityTenorExtensionApproval };
            var operationIsTenorExtension = tenorExtensionOperations.Contains((short)appl.OPERATIONID);

            if (model.forwardAction != (int)ApprovalStatusEnum.Referred)
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    var classifiedTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                    x.OPERATIONID == (int)model.operationId
                    && x.DESTINATIONOPERATIONID > 0
                    && x.REFEREBACKSTATEID != (int)ApprovalState.Ended
                    && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Finishing)
                    && x.TARGETID == model.applicationId
                );//any pending classified refer back

                var previousTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                    x.OPERATIONID == (int)model.operationId
                    //&& x.RESPONSESTAFFID == null
                    && x.REFEREBACKSTATEID != (int)ApprovalState.Ended
                    && x.DESTINATIONOPERATIONID == null
                    && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred
                    && x.TARGETID == model.applicationId
                );//any pending refer back done after classified refer back

                    if (classifiedTrail != null && previousTrail == null)
                    {
                        //if there is a pending classified refer back and no child pending refer backs
                        if (classifiedTrail.RESPONSESTAFFID == null)
                        {
                            if (classifiedTrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                            {
                                classifiedTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Closed;
                            }
                            //else
                            //{
                            //    classifiedTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            //}
                            classifiedTrail.APPROVALSTATEID = (short)ApprovalState.Ended;
                            classifiedTrail.RESPONSESTAFFID = model.createdBy;
                            classifiedTrail.RESPONSEDATE = DateTime.Now;
                            classifiedTrail.SYSTEMRESPONSEDATETIME = DateTime.Now;
                        }

                        classifiedTrail.REFEREBACKSTATEID = (short)ApprovalState.Ended;
                        context.SaveChanges();

                        var previousTrail2 = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                         x.OPERATIONID == (int)model.operationId
                         && x.RESPONSESTAFFID == null
                         && x.DESTINATIONOPERATIONID == null
                         && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Finishing || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                         && x.TARGETID == model.applicationId
                        );

                        if (previousTrail2 != null)
                        {

                            if (previousTrail2.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                            {
                                previousTrail2.APPROVALSTATUSID = (int)ApprovalStatusEnum.Closed;
                            }
                            else
                            {
                                previousTrail2.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }
                            previousTrail2.APPROVALSTATEID = (short)ApprovalState.Ended;
                            previousTrail2.RESPONSESTAFFID = model.createdBy;
                            previousTrail2.RESPONSEDATE = DateTime.Now;
                            previousTrail2.SYSTEMRESPONSEDATETIME = DateTime.Now;
                        }

                        if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAvailment)
                        {
                            appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                            workflow.Response.responseMessage = "The Request has been Approved and Sent to Credit Inputter";
                        }
                        else
                        {
                            var lmsrDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                            workflow.BusinessUnitId = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == lmsrDetail.FirstOrDefault().CUSTOMERID).BUSINESSUNTID;
                            workflow.StaffId = model.lastUpdatedBy;
                            workflow.CompanyId = appl.COMPANYID;
                            workflow.OperationId = classifiedTrail.DESTINATIONOPERATIONID ?? 0;
                            workflow.TargetId = appl.LOANAPPLICATIONID;
                            workflow.ProductClassId = null;
                            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                            workflow.ToStaffId = classifiedTrail.REQUESTSTAFFID;
                            //workflow.NextLevelId = model.receiverLevelId;
                            workflow.Comment = model.comment;
                            workflow.Vote = (short)ApprovalStatusEnum.Approved;
                            workflow.DeferredExecution = true;
                            workflow.IsFlowTest = model.isFlowTest;
                            workflow.IsFromPc = model.isFromPc;
                            workflow.Tenor = operationIsTenorExtension ? lmsrDetail.Max(d => d.APPROVEDTENOR) : 0;
                            workflow.IgnorePostApprovalReviewer = true;
                            workflow.LevelBusinessRule = new LevelBusinessRule
                            {
                                Amount = appl.TOTALEXPOSUREAMOUNT, // totalApplicationAmount,
                                                                   //Amount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT) ?? 0, // totalApplicationAmount,
                                                                   //PepAmount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT) ?? 0, // totalApplicationAmount,
                                PepAmount = appl.TOTALEXPOSUREAMOUNT, // totalApplicationAmount,
                                Pep = model.politicallyExposed,
                                //InsiderRelated = appl.ISRELATEDPARTY ?? false,
                                ProjectRelated = appl.ISPROJECTRELATED ?? false,
                                OnLending = appl.ISONLENDING ?? false,
                                InterventionFunds = appl.ISINTERVENTIONFUNDS ?? false,
                                WithInstruction = appl.WITHINSTRUCTION ?? false,
                                //OrrBasedApproval = appl.ISORRBASEDAPPROVAL ?? false,
                                DomiciliationNotInPlace = appl.DOMICILIATIONNOTINPLACE ?? false,
                                tenor = operationIsTenorExtension ? lmsrDetail.Max(d => d.APPROVEDTENOR) : 0,
                            };
                            workflow.LogActivity();
                        }
                        var saved = context.SaveChanges() > 0;
                        if (model.isFlowTest == false) { trans.Commit(); } else { trans.Rollback(); }

                        return workflow.Response;
                    }
                }
            }

            var product = context.TBL_PRODUCT.Find(appl.PRODUCTID);
            //string staffRole = (from x in context.TBL_STAFF join r in context.TBL_STAFF_ROLE on x.STAFFROLEID equals r.STAFFROLEID where x.STAFFID == model.staffId select r.STAFFROLECODE).FirstOrDefault();

            //var checklistValidation = ChecklistCompleted(model.applicationId);
            List<short> drawdownOperations = new List<short> { (int)OperationsEnum.LoanReviewDrawdownForExtension, (int)OperationsEnum.OverdraftReviewDrawdownForExtension, (int)OperationsEnum.ContingentReviewDrawdownForExtension };
            if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAvailment)
            {
                bool checklistValidation = true;
                if (appl.CREATEDBY == model.createdBy && model.operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter && checklistValidation == false)
                {
                    throw new SecureException("Checklist not completed!");
                }

                if (appl.CREATEDBY == model.createdBy && checklistValidation == false)
                {
                    throw new SecureException("Checklist not completed!");
                }

                var currentOperationType = context.TBL_OPERATIONS.Where(x => x.OPERATIONID == operationId).FirstOrDefault()?.OPERATIONTYPEID;

                if (currentOperationType == (short)OperationTypeEnum.LoanReviewApplication && (operationId != (int)OperationsEnum.LoanReviewApprovalAvailment))
                {
                    workflow.Amount = GetMaximumApplicationOutstandingBalance(appl.LOANAPPLICATIONID);
                    //workflow.Amount = appl.TOTALEXPOSUREAMOUNT;
                }

                var isDrawdownOperation = drawdownOperations.Contains((short)model.operationId);

                using (var trans = context.Database.BeginTransaction())
                {
                    var lmsrDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                    workflow.BusinessUnitId = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == lmsrDetail.FirstOrDefault().CUSTOMERID).BUSINESSUNTID;
                    workflow.StaffId = model.lastUpdatedBy;
                    workflow.CompanyId = appl.COMPANYID;
                    workflow.OperationId = model.operationId;
                    workflow.TargetId = appl.LOANAPPLICATIONID;
                    workflow.ProductClassId = null;
                    workflow.StatusId = model.forwardAction;
                    workflow.ToStaffId = model.receiverStaffId;
                    workflow.NextLevelId = model.receiverLevelId;
                    workflow.Comment = model.comment;
                    workflow.Vote = model.vote;
                    workflow.DeferredExecution = true;
                    workflow.IsFlowTest = model.isFlowTest;
                    workflow.IsFromPc = model.isFromPc;
                    workflow.Tenor = operationIsTenorExtension ? lmsrDetail.Max(d => d.APPROVEDTENOR) : 0;
                    workflow.IgnorePostApprovalReviewer = (appl.OPERATIONID != (int)OperationsEnum.OverdraftSubAllocationApproval &&
                                                            appl.OPERATIONID != (int)OperationsEnum.LoanRecapitilizationApproval &&
                                                            appl.OPERATIONID != (int)OperationsEnum.OverdraftTopUpApproval &&
                                                            appl.OPERATIONID != (int)OperationsEnum.OverdraftTenorExtensionApproval &&
                                                            !(appl.OPERATIONID == (int)OperationsEnum.TenorExtensionApproval &&
                                                            appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID != (short)LoanSystemTypeEnum.ContingentLiability));
                    workflow.LevelBusinessRule = new LevelBusinessRule
                    {
                        Amount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT), // totalApplicationAmount,
                        PepAmount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT), // totalApplicationAmount,
                        Pep = model.politicallyExposed,
                        //InsiderRelated = appl.ISRELATEDPARTY ?? false,
                        ProjectRelated = appl.ISPROJECTRELATED ?? false,
                        OnLending = appl.ISONLENDING ?? false,
                        InterventionFunds = appl.ISINTERVENTIONFUNDS ?? false,
                        WithInstruction = appl.WITHINSTRUCTION ?? false,
                        isAgricRelated = appl.ISAGRICRELATED,
                        //OrrBasedApproval = appl.ISORRBASEDAPPROVAL ?? false,
                        DomiciliationNotInPlace = appl.DOMICILIATIONNOTINPLACE ?? false,
                        tenor = operationIsTenorExtension ? lmsrDetail.Max(d => d.APPROVEDTENOR) : 0,
                        isContingentFacility = product.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability
                    };

                    if (model.receiverLevelId == 0) workflow.NextLevelId = null;

                    if (model.forwardAction == (short)ApprovalStatusEnum.RePresent || model.forwardAction == (short)ApprovalStatusEnum.StepDown)
                    {
                        var dictionary = GetRepresentStepdownItems(appl.LOANAPPLICATIONID, model.forwardAction, operationId);
                        workflow.NextLevelId = dictionary["levelId"];
                    }

                    workflow.LogActivity();

                    List<TBL_LMSR_APPLICATION_DETAIL> items = null;
                    items = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();
                    if (model.recommendedChanges != null && model.recommendedChanges.Count() > 0)
                    {
                        foreach (var changed in model.recommendedChanges)
                        {
                            var detail = items.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == changed.detailId);
                            if (detail != null)
                            {
                                detail.APPROVEDAMOUNT = changed.amount;
                                detail.APPROVEDINTERESTRATE = changed.interestRate;
                                detail.APPROVEDTENOR = changed.tenor;
                                detail.APPROVALSTATUSID = changed.statusId;

                                if (model.isBusiness)
                                {
                                    if (detail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved) { detail.DELETED = true; }
                                    else
                                    {
                                        detail.PROPOSEDAMOUNT = changed.amount;
                                        detail.PROPOSEDINTERESTRATE = changed.interestRate;
                                        detail.PROPOSEDTENOR = changed.tenor;
                                    }
                                }
                            }
                        }
                    }

                    context.SaveChanges();
                    var isClosed = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == model.applicationId && x.OPERATIONID == model.operationId && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Closed).Any();

                    int lastStatusId = workflow.StatusId;
                    if (currentOperationType == (short)OperationsEnum.LoanReviewApprovalAvailment) appl.APPROVALSTATUSID = (short)lastStatusId;

                    if (workflow.NewState == (int)ApprovalState.Ended && model.isFlowTest == false && (isClosed == true || workflow.IgnorePostApprovalReviewer == true))
                    {
                        if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                        {
                            short nextOperatioId = 0;
                            var flowOrder = context.TBL_LMSR_FLOW_ORDER.Where(x => x.OPERATIONID == model.operationId).FirstOrDefault();
                            var defaultFlowOrder = context.TBL_LMSR_FLOW_ORDER.Where(x => x.OPERATIONID == 0).FirstOrDefault();
                            if (model.operationId == (short)OperationsEnum.LoanReviewApprovalAvailment)
                            {
                                appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                            }
                            
                            if (flowOrder == null)
                            {
                                if (defaultFlowOrder.REQUIREOFFERLETTER && currentOperationType != (short)OperationsEnum.LoanReviewApprovalAvailment)
                                {
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalOfferLetter;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                if (defaultFlowOrder.REQUIREDRAWDOWN && !isDrawdownOperation)
                                {
                                    if(product.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan) { nextOperatioId = (short)OperationsEnum.OverdraftReviewDrawdownForExtension; }
                                    else if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability) { nextOperatioId = (short)OperationsEnum.ContingentReviewDrawdownForExtension; }
                                    else { nextOperatioId = (short)OperationsEnum.LoanReviewDrawdownForExtension; }
                                    //nextOperatioId = (short)OperationsEnum.LoanReviewDrawdownForExtension;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                else if (defaultFlowOrder.REQUIREAVAILMENT)
                                {
                                    if (workflow.Response.nextLevelName == null) workflow.Response.nextLevelName = "Credit Admin";
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalAvailment;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                    //appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                }
                                else if (flowOrder.REQUIREOPERATIONS)
                                {
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                                    workflow.Response.responseMessage += " and Sent to Credit Inputter";
                                }
                                else
                                {
                                    appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationCompleted;
                                }
                                //else
                                //{
                                //    LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                                //}
                            }

                            if (flowOrder != null)
                            {
                                if (flowOrder.REQUIREOFFERLETTER)
                                {
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalOfferLetter;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                if (flowOrder.REQUIREDRAWDOWN && !isDrawdownOperation)
                                {
                                    if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan) { nextOperatioId = (short)OperationsEnum.OverdraftReviewDrawdownForExtension; }
                                    else if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability) { nextOperatioId = (short)OperationsEnum.ContingentReviewDrawdownForExtension; }
                                    else { nextOperatioId = (short)OperationsEnum.LoanReviewDrawdownForExtension; }
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                else if (flowOrder.REQUIREAVAILMENT)
                                {
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalAvailment;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                    //appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                }
                                else if (flowOrder.REQUIREOPERATIONS)
                                {
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                                    workflow.Response.responseMessage += " and Sent to Credit Inputter";
                                }
                                else
                                {
                                    appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationCompleted;
                                }
                                //else
                                //{
                                //    LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                                //}
                            }
                        }

                        if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                        {
                            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected;
                        }
                        context.SaveChanges();
                    }

                    if (model.isFlowTest == false) { trans.Commit(); } else { trans.Rollback(); }

                    return workflow.Response;
                }
            }
            else
            {
                bool checklistValidation = true;
                if (appl.CREATEDBY == model.createdBy && model.operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter && checklistValidation == false)
                {
                    throw new SecureException("Checklist not completed!");
                }

                if (appl.CREATEDBY == model.createdBy && checklistValidation == false)
                {
                    throw new SecureException("Checklist not completed!");
                }

                var currentOperationType = context.TBL_OPERATIONS.Where(x => x.OPERATIONID == operationId).FirstOrDefault()?.OPERATIONTYPEID;

                if (currentOperationType == (short)OperationTypeEnum.LoanReviewApplication && (operationId != (int)OperationsEnum.LoanReviewApprovalAvailment))
                {
                    workflow.Amount = appl.TOTALEXPOSUREAMOUNT;
                    //workflow.Amount = GetMaximumApplicationOutstandingBalance(appl.LOANAPPLICATIONID);
                }

                
                using (var trans = context.Database.BeginTransaction())
                {
                    var lmsrDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                    var exchangeRate = context.TBL_CURRENCY_EXCHANGERATE.Where(e => e.CURRENCYID == lmsrDetail.FirstOrDefault().CURRENCYID).Select(e => e.EXCHANGERATE).FirstOrDefault();
                    var realAmt = context.TBL_LOAN.FirstOrDefault(l => l.TERMLOANID == lmsrDetail.FirstOrDefault().LOANID) != null ? appl.TOTALEXPOSUREAMOUNT * (decimal)context.TBL_LOAN.FirstOrDefault(l => l.TERMLOANID == lmsrDetail.FirstOrDefault().LOANID)?.EXCHANGERATE : (decimal)appl.TOTALEXPOSUREAMOUNT * (decimal)exchangeRate;
                    workflow.FacilityAmount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT) > 0 ? lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT) : (appl.APPROVEDAMOUNT ?? 0);
                    workflow.BusinessUnitId = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == lmsrDetail.FirstOrDefault().CUSTOMERID).BUSINESSUNTID;
                    workflow.StaffId = model.lastUpdatedBy;
                    workflow.CompanyId = appl.COMPANYID;
                    workflow.OperationId = model.operationId;
                    workflow.TargetId = appl.LOANAPPLICATIONID;
                    workflow.ProductClassId = null;
                    workflow.StatusId = model.forwardAction;
                    workflow.ToStaffId = model.receiverStaffId;
                    workflow.NextLevelId = model.receiverLevelId;
                    workflow.Comment = model.comment;
                    workflow.Vote = model.vote;
                    workflow.DeferredExecution = true;
                    workflow.IsFlowTest = model.isFlowTest;
                    workflow.IsFromPc = model.isFromPc;
                    workflow.Tenor = operationIsTenorExtension ? lmsrDetail.Max(d => d.APPROVEDTENOR) : 0;
                    workflow.IgnorePostApprovalReviewer = true;
                    workflow.FinalLevel = appl.FINALAPPROVAL_LEVELID;
                    workflow.LevelBusinessRule = new LevelBusinessRule
                    {
                        Amount = realAmt,
                    //Amount = appl.TOTALEXPOSUREAMOUNT, // totalApplicationAmount,
                    //Amount = lmsrDetail.Sum(x => x.CUSTOMERPROPOSEDAMOUNT) ?? 0, // totalApplicationAmount,
                    PepAmount = appl.TOTALEXPOSUREAMOUNT, // totalApplicationAmount,
                        Pep = model.politicallyExposed,
                        //InsiderRelated = appl.ISRELATEDPARTY ?? false,
                        ProjectRelated = appl.ISPROJECTRELATED ?? false,
                        OnLending = appl.ISONLENDING ?? false,
                        InterventionFunds = appl.ISINTERVENTIONFUNDS ?? false,
                        WithInstruction = appl.WITHINSTRUCTION ?? false,
                        //OrrBasedApproval = appl.ISORRBASEDAPPROVAL ?? false,
                        DomiciliationNotInPlace = appl.DOMICILIATIONNOTINPLACE ?? false,
                        tenor = operationIsTenorExtension ? lmsrDetail.Max(d => d.APPROVEDTENOR) : 0,
                        isContingentFacility = product.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability
                    };

                    if (model.receiverLevelId == 0) workflow.NextLevelId = null;

                    if (model.forwardAction == (short)ApprovalStatusEnum.RePresent || model.forwardAction == (short)ApprovalStatusEnum.StepDown)
                    {
                        var dictionary = GetRepresentStepdownItems(appl.LOANAPPLICATIONID, model.forwardAction, operationId);
                        workflow.NextLevelId = dictionary["levelId"];
                    }

                    workflow.LogActivity();

                    List<TBL_LMSR_APPLICATION_DETAIL> items = null;
                    items = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();
                    if (model.recommendedChanges != null && model.recommendedChanges.Count() > 0)
                    {
                        foreach (var changed in model.recommendedChanges)
                        {
                            var detail = items.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == changed.detailId);
                            if (detail != null)
                            {
                                detail.APPROVEDAMOUNT = changed.amount;
                                detail.APPROVEDINTERESTRATE = changed.interestRate;
                                detail.APPROVEDTENOR = changed.tenor;
                                detail.APPROVALSTATUSID = changed.statusId;

                                if (model.isBusiness)
                                {
                                    if (detail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved) { detail.DELETED = true; }
                                    else
                                    {
                                        detail.PROPOSEDAMOUNT = changed.amount;
                                        detail.PROPOSEDINTERESTRATE = changed.interestRate;
                                        detail.PROPOSEDTENOR = changed.tenor;
                                    }
                                }
                            }
                        }
                    }

                    context.SaveChanges();
                    var isClosed = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == model.applicationId && x.OPERATIONID == model.operationId && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Closed).Any();

                    int lastStatusId = workflow.StatusId;
                    if (currentOperationType == (short)OperationsEnum.LoanReviewApprovalAvailment) appl.APPROVALSTATUSID = (short)lastStatusId;

                    if (workflow.NewState == (int)ApprovalState.Ended && model.isFlowTest == false && (isClosed == true || workflow.IgnorePostApprovalReviewer == true))
                    {
                        if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                        {
                            short nextOperatioId = 0;
                            var flowOrder = context.TBL_LMSR_FLOW_ORDER.Where(x => x.OPERATIONID == model.operationId).FirstOrDefault();
                            var defaultFlowOrder = context.TBL_LMSR_FLOW_ORDER.Where(x => x.OPERATIONID == 0).FirstOrDefault();
                            if (model.operationId == (short)OperationsEnum.LoanReviewApprovalAvailment)
                            {
                                appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                            }

                            if (flowOrder == null)
                            {
                                if (defaultFlowOrder.REQUIREOFFERLETTER && currentOperationType != (short)OperationsEnum.LoanReviewApprovalAvailment)
                                {
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalOfferLetter;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                if (defaultFlowOrder.REQUIREDRAWDOWN)
                                {
                                    if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan) { nextOperatioId = (short)OperationsEnum.OverdraftReviewDrawdownForExtension; }

                                    else if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability) { nextOperatioId = (short)OperationsEnum.ContingentReviewDrawdownForExtension; }

                                    else { nextOperatioId = (short)OperationsEnum.LoanReviewDrawdownForExtension; }

                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                else if (defaultFlowOrder.REQUIREAVAILMENT)
                                {
                                    if (workflow.Response.nextLevelName == null) workflow.Response.nextLevelName = "Credit Admin";
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalAvailment;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                    //appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                }
                                else if (defaultFlowOrder.REQUIREOPERATIONS)
                                {
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                                    workflow.Response.responseMessage += " and Sent to Credit Inputter";
                                }
                                else
                                {
                                    appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationCompleted;
                                }
                            }

                            if (flowOrder != null)
                            {
                                if (flowOrder.REQUIREOFFERLETTER)
                                {
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalOfferLetter;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                if (flowOrder.REQUIREDRAWDOWN)
                                {
                                    if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan) { nextOperatioId = (short)OperationsEnum.OverdraftReviewDrawdownForExtension; }
                                    else if (product.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability) { nextOperatioId = (short)OperationsEnum.ContingentReviewDrawdownForExtension; }
                                    else { nextOperatioId = (short)OperationsEnum.LoanReviewDrawdownForExtension; }

                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                }
                                else if (flowOrder.REQUIREAVAILMENT)
                                {
                                    nextOperatioId = (short)OperationsEnum.LoanReviewApprovalAvailment;
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                                    //appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                }
                                else if (flowOrder.REQUIREOPERATIONS)
                                {
                                    LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                                    workflow.Response.responseMessage += " and Sent to Credit Inputter";
                                }
                                else
                                {
                                    appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationCompleted;
                                }
                            }
                        }

                        if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                        {
                            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected;
                        }
                        context.SaveChanges();
                    }

                    if (model.isFlowTest == false) { trans.Commit(); } else { trans.Rollback(); }

                    return workflow.Response;
                }
            }
            
        }

        public List<CurrentCustomerExposure> GetCustomerExposureLMS(TBL_LMSR_APPLICATION lmsrApplication, int loanTypeId, int companyId) // not used!
        {
            var customerIds = new List<CustomerExposure>();
            if (loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                customerIds.Add(new CustomerExposure { customerId = (int)lmsrApplication.CUSTOMERGROUPID });
            }
            else
            {
                customerIds.Add(new CustomerExposure { customerId = (int)lmsrApplication.CUSTOMERID });
            }
            return GetCurrentCustomerExposure(customerIds, loanTypeId, companyId);
        }

        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int loanTypeId, int companyId)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            var customerId = customer.FirstOrDefault()?.customerId;
            var allGroupMappings = GetCustomerGroupMapping();
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            if (loanTypeId == (int)LoanTypeEnum.CustomerGroup && customer.Count() == 1)
            {
                var customerGroupMappings = new List<CustomerGroupMappingViewModel>();
                var mappings = allGroupMappings.Where(m => m.customerGroupId == customerId).ToList();

                if (mappings.Count() > 0)
                {
                    customer = mappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                }
            }
            else
            {
                var customerIsAGroupMember = allGroupMappings.Any(m => m.customerId == customerId);
                if (customerIsAGroupMember)
                {
                    var mappings = new List<CustomerGroupMappingViewModel>();
                    var customerGroups = allGroupMappings.Where(m => m.customerId == customerId).ToList();
                    var allGroupIds = customerGroups.Select(m => m.customerGroupId).Distinct().ToList();
                    foreach (var groupId in allGroupIds)
                    {
                        var mapping = allGroupMappings.Where(m => m.customerGroupId == groupId).ToList();
                        mappings.AddRange(mapping);
                    }
                    if (mappings.Count() > 0)
                    {
                        customer = mappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                    }
                }
            }

            foreach (var item in customer)
            {
                var customerCode = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == item.customerId)?.CUSTOMERCODE.Trim();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                customerName = a.CUSTOMERNAME,
                                customerCode = a.CUSTOMERID.Trim(),
                                facilityType = a.ADJFACILITYTYPE,
                                approvedAmount = a.LOANAMOUNYTCY ?? 0,
                                approvedAmountLcy = a.LOANAMOUNYLCY ?? 0,
                                currency = a.CURRENCYNAME,
                                currencyType = a.CURRENCYTYPE,
                                exposureTypeCodeString = a.EXPOSURETYPECODE,
                                adjFacilityTypeString = a.ADJFACILITYTYPE,
                                adjFacilityTypeCode = a.ADJFACILITYTYPEid.Trim(),
                                productIdString = a.PRODUCTID,
                                productCode = a.PRODUCTCODE,
                                productName = a.PRODUCTNAME,
                                currencyCode = a.ALPHACODE,
                                //existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                //proposedLimit = a.LOANAMOUNYLCY ?? 0,
                                outstandings = a.PRINCIPALOUTSTANDINGBALTCY ?? 0,
                                outstandingsLcy = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                bookingDate = a.BOOKINGDATE,
                                maturityDate = a.MATURITYDATE,
                                tenorString = a.TENOR,
                                //maturityDateString = a.MATURITYDATE,
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).ToList();

                if (exposure.Count() > 0)
                {
                    foreach (var e in exposure)
                    {
                        e.exposureTypeId = int.Parse(e.exposureTypeCodeString);
                        e.tenor = int.Parse(String.IsNullOrEmpty(e.tenorString) ? "0" : e.tenorString);
                        e.bookingDate = e.bookingDate?.Date;
                        e.maturityDate = e.maturityDate?.Date;
                        //e.productId = int.Parse(e.productIdString);
                        e.exposureTypeCode = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.adjFacilityTypeId = int.Parse(String.IsNullOrEmpty(e.adjFacilityTypeCode) ? "0" : e.adjFacilityTypeCode);
                    }
                    exposures.AddRange(exposure);
                }


                //exposure = from a in context.TBL_LOAN
                //           join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                //           join b in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                //           where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //           select new CurrentCustomerExposure
                //           {
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               existingLimit = a.PRINCIPALAMOUNT,
                //               //proposedLimit = a.OUTSTANDINGPRINCIPAL,
                //               proposedLimit = 0,
                //               //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                //               outstandings = a.OUTSTANDINGPRINCIPAL,
                //               recommendedLimit = 0,
                //               PastDueObligationsInterest = a.PASTDUEINTEREST,
                //               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                //               reviewDate = DateTime.Now,
                //               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //               loanStatus = "Running",
                //               referenceNumber = a.LOANREFERENCENUMBER,
                //               applicationStatusId = b.APPLICATIONSTATUSID
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = (from a in context.TBL_LOAN_REVOLVING
                //            join b in context.TBL_LOAN_APPLICATION on a.LOANREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //            where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //            select new CurrentCustomerExposure
                //            {
                //                facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //                existingLimit = a.OVERDRAFTLIMIT,
                //                //proposedLimit = a.OVERDRAFTLIMIT,
                //                proposedLimit = 0,
                //                //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,   
                //                outstandings = a.OVERDRAFTLIMIT,
                //                recommendedLimit = 0,
                //                casaAccountId = a.CASAACCOUNTID,
                //                PastDueObligationsInterest = a.PASTDUEINTEREST,
                //                PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                //                reviewDate = DateTime.Now,
                //                prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //                loanStatus = "Running",
                //                referenceNumber = a.LOANREFERENCENUMBER,
                //                applicationStatusId = b.APPLICATIONSTATUSID
                //            }).ToList();
                //.Select(x =>
                //{
                //    //var availableBalance = transRepo.GetCASABalance((int)x.casaAccountId).availableBalance;
                //    var availableBalance = context.TBL_CASA.FirstOrDefault(m=>m.CASAACCOUNTID == (int)x.casaAccountId).AVAILABLEBALANCE;

                //    if ( availableBalance >= 0)
                //    {
                //        x.outstandings = 0;
                //    }
                //    else
                //    {
                //        x.outstandings = Math.Abs(availableBalance); 
                //    }
                //    return x;
                //});

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                //           join b in context.TBL_LOAN_APPLICATION on a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //           join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                //           where a.CUSTOMERID == item.customerId && a.TBL_LOAN_APPLICATION.COMPANYID == companyId && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                //           select new CurrentCustomerExposure
                //           {

                //               applicationStatusId = b.APPLICATIONSTATUSID,
                //               customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                //               customerCode = c.CUSTOMERCODE.Trim(),
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               approvedAmount = a.APPROVEDAMOUNT,
                //               currency = a.TBL_CURRENCY.CURRENCYNAME,
                //               //exposureTypeId = int.Parse(a.EXPOSURETYPECODE),
                //               //adjFacilityType = a.ADJFACILITYTYPE,
                //               productId = a.TBL_PRODUCT.PRODUCTID,
                //               productName = a.TBL_PRODUCT.PRODUCTNAME,
                //               outstandings = 0,
                //               pastDueObligationsPrincipal = 0,
                //               reviewDate = DateTime.Now,
                //               //bookingDate = DateTime.Parse(a.BOOKINGDATE),
                //               //maturityDate = DateTime.Parse(a.MATURITYDATE),
                //               loanStatus = "Processing",
                //               referenceNumber = b.APPLICATIONREFERENCENUMBER
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);


                //var staggingLoan = from a in stgCon.STG_LOAN_MART
                //                   where a.CUST_ID == customCode
                //                   select new CurrentCustomerExposure
                //                   {
                //                       facilityType = a.SCHM_TYPE,
                //                       existingLimit = a.FAC_GRANT_AMT,
                //                       //proposedLimit = a.FINAL_BALANCE,
                //                       proposedLimit = 0,
                //                       recommendedLimit = 0,
                //                       outstandings = a.FINAL_BALANCE,
                //                       PastDueObligationsInterest = a.INT_DUE,
                //                       PastDueObligationsPrincipal = a.DAYS_PAST_DUE,// 0,
                //                       reviewDate = DateTime.Now,
                //                       prudentialGuideline = a.USER_CLASSIFICATION == "1" ? "Performing" : "Non-Performing",
                //                       loanStatus = "Running"
                //                   };

                //exposures.Union(staggingLoan);

            }

            if (exposures.Count() == 0)
            {
                return exposures;
            }

            //exposures.Add(new CurrentCustomerExposure
            //{
            //    facilityType = "TOTAL",
            //    existingLimit = exposures.Sum(t => t.existingLimit),
            //    proposedLimit = exposures.Sum(t => t.proposedLimit),
            //    bookingDate = exposures.Max(t => t.bookingDate),
            //    maturityDate = exposures.Max(t => t.maturityDate),
            //    //approvedAmount = exposures.Sum(t => t.approvedAmount),
            //    recommendedLimit = exposures.Sum(t => t.recommendedLimit),
            //    outstandings = exposures.Sum(t => t.outstandings),
            //    PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
            //    pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
            //    reviewDate = DateTime.Now,
            //});

            return exposures;
        }
        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping()
        {
            var customerGroupMapping = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                        where a.DELETED == false
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                            customerGroupId = a.CUSTOMERGROUPID,
                                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                                            //createdBy = a.CreatedBy,
                                            customerId = a.CUSTOMERID,
                                            //dateTimeCreated = a.DateTimeCreated
                                        }).ToList();

            return customerGroupMapping;
        }


        private void LogLMSOperationForRouting(ForwardReviewViewModel model, List<TBL_LMSR_APPLICATION_DETAIL> details, short nextOperationId, short lastOperationId)
        {
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);
            if (lastOperationId != (int)OperationsEnum.LoanReviewApprovalAvailment)
            {
                var existingTrail = context.TBL_APPROVAL_TRAIL.Where(x =>
                               x.COMPANYID == model.companyId
                               && x.OPERATIONID == nextOperationId
                               //&& x.TARGETID == appl.LOANAPPLICATIONID
                               && x.TARGETID == appl.LOANAPPLICATIONID
                               && x.RESPONSESTAFFID == null
                               && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                           ).ToList();
                if (existingTrail.Count() == 0)
                {
                    Workflow workflowlms = new Workflow(context, general);

                    if ((appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility
                      || appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility
                      || appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability
                      || appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ExternalFacility
                      || appl.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility))
                    {
                        workflowlms.StaffId = model.createdBy;
                        workflowlms.CompanyId = model.companyId;
                        workflowlms.StatusId = (short)ApprovalStatusEnum.Processing;
                        //workflowlms.TargetId = i.LOANREVIEWAPPLICATIONID;
                        workflowlms.TargetId = appl.LOANAPPLICATIONID;
                        workflowlms.Comment = model.comment;
                        workflowlms.OperationId = (int)nextOperationId;
                        workflowlms.DeferredExecution = true;
                        workflowlms.ExternalInitialization = true;
                        workflowlms.LogActivity();
                        context.SaveChanges();
                    }
                }
            }

            //if (lastOperationId == (int)OperationsEnum.LoanReviewApprovalAvailment)
            //{
            //    foreach (var i in details)
            //    {
            //        var operation = context.TBL_OPERATIONS.Where(x => x.OPERATIONID == appl.OPERATIONID)?.FirstOrDefault();
            //        var synchOperationId = operation?.SYNCHOPERATIONID;

            //        if (synchOperationId == null)
            //            throw new ConditionNotMetException("Operation not in synch with final operation.");

            //        nextOperationId = (short)synchOperationId;

            //        var existingTrail = context.TBL_APPROVAL_TRAIL.Where(x =>
            //                        x.COMPANYID == model.companyId
            //                        && x.OPERATIONID == nextOperationId
            //                        && x.TARGETID == i.LOANREVIEWAPPLICATIONID
            //                        && x.RESPONSESTAFFID == null
            //                        && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
            //                    ).ToList();
            //        if (existingTrail.Count() == 0)
            //        {
            //            Workflow workflowlms = new Workflow(context, general);

            //            if ((i.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility
            //              || i.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility
            //              || i.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability
            //              || i.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility))
            //            {
            //                workflowlms.StaffId = model.createdBy;
            //                workflowlms.CompanyId = model.companyId;
            //                workflowlms.StatusId = (short)ApprovalStatusEnum.Pending;
            //                workflowlms.TargetId = i.LOANREVIEWAPPLICATIONID;
            //                workflowlms.Comment = model.comment;
            //                workflowlms.OperationId = (int)nextOperationId;
            //                workflowlms.DeferredExecution = true;
            //                workflowlms.ExternalInitialization = true;
            //                workflowlms.LogActivity();
            //                context.SaveChanges();
            //            }
            //        }

            //    }
            //}
        }


      
        /*private int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false)
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
        }*/


        private int GetFirstAvailmentLevelId(int operationId)
        {
            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                 .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                     {
                         groupPosition = mg.m.POSITION,
                         levelPosition = l.POSITION,
                         levelId = l.APPROVALLEVELID,
                         levelName = l.LEVELNAME,
                         levelTypeId = l.LEVELTYPEID,
                         staffRoleId = l.STAFFROLEID,
                     })
                     .OrderBy(x => x.groupPosition)
                     .ThenBy(x => x.levelPosition)
                     .ToList()
                     ;

            var level = levels.FirstOrDefault(x => x.levelTypeId == (int)ApprovalLevelType.Routing); // routing
            if (level == null) level = levels.FirstOrDefault(x => x.levelPosition == 1);

            return level.levelId;
        }


        public decimal GetCustomerTotalOutstandingBalance(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var contingentData = context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CUSTOMERID == customerId);

            decimal loanBalance = 0;
            decimal overdraftBalance = 0;
            decimal contingentBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN where a.CUSTOMERID == customerId select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING where a.CUSTOMERID == customerId select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }

            if (contingentData != null)
            {
                var balance = (from a in context.TBL_LOAN_CONTINGENT where a.CUSTOMERID == customerId select a.CONTINGENTAMOUNT).Sum();
                contingentBalance = balance;
            }

            decimal totalBalance = loanBalance + overdraftBalance + contingentBalance;

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
                    currencyId = loan.CURRENCYID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OUTSTANDINGPRINCIPAL,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ExternalFacility)
            {
                result = context.TBL_LOAN_EXTERNAL.Where(x => x.EXTERNALLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    currencyId = loan.CURRENCYID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OUTSTANDINGPRINCIPAL,
                    //loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                result = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    currencyId = loan.CURRENCYID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OVERDRAFTLIMIT,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                result = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    currencyId = loan.CURRENCYID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = 0,
                    outstandingPrincipal = loan.CONTINGENTAMOUNT,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.LineFacility)
            {
                result = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    currencyId = loan.CURRENCYID,
                    effectiveDate = startDate,
                    tenorUsed = loan.APPROVEDTENOR,
                    interestRate = loan.APPROVEDINTERESTRATE,
                    outstandingPrincipal = loan.APPROVEDAMOUNT, // adapting!
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
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
            if (loanTypeId == 5)
            {
                return new LoanApplicationDetailViewModel
                {
                    loanApplicationDetailId = 0,
                    loanApplicationId = 0,

                };
            }
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
            int id = 0;
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
            if(loanTypeId == 5)
            {
                var loan = context.TBL_LOAN_EXTERNAL.FirstOrDefault(x => x.EXTERNALLOANID == loanId);
            }
            return id;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetRegionalLoanApplications(int staffId)
        {
            List<int> levels1 = general.GetRouteLevels(46, 1);
            List<int> levels2 = general.GetRouteLevels(71, 1);
            List<int> levels3 = general.GetRouteLevels(79, 1);
            var camLevels = levels1.Union(levels2).Union(levels3).Distinct();

             levels1 = general.GetRouteLevels(107, 1);
             levels2 = general.GetRouteLevels(108, 1);
             levels3 = general.GetRouteLevels(109, 1);
            var apsLevels = levels1.Union(levels2).Union(levels3).Distinct();

          var  levels = camLevels.Union(apsLevels).Distinct();

            var operations = camOperationIds.Union(apsOperationIds);

            //var branches = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
            //                    .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
            //                    .Join(context.TBL_BRANCH, sr => sr.r.REGIONID, b => b.REGIONID, (sr, b) => new { sr, b })
            //                    .Select(x => new {
            //                        BRANCHID = x.b.BRANCHID
            //                    })
            //                    .Select(x => x.BRANCHID)
            //                    .ToList();

            var regions = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
                            .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
                            .Select(x => new
                            {
                                REGIONID = x.r.REGIONID
                            })
                            .Select(x => x.REGIONID)
                            .ToList();

            var applications = context.TBL_LMSR_APPLICATION.Where(x =>
                    regions.Contains((int)x.CAPREGIONID)
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                )
                .OrderByDescending(x => x.LOANAPPLICATIONID)
                .Join(
                    context.TBL_APPROVAL_TRAIL.Where(x => operations.Contains(x.OPERATIONID)
                        && levels.Contains((int)x.TOAPPROVALLEVELID)
                        && (x.RESPONSESTAFFID == null || (x.RESPONSESTAFFID == staffId && x.TOSTAFFID != null))
                        ),// && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)),

                    a => a.LOANAPPLICATIONID,
                    b => b.TARGETID,
                    (a, b) => new { a, b, branch = a.TBL_BRANCH, customer = a.TBL_CUSTOMER })
                .Select(x => new LoanReviewApplicationViewModel
                {
                    approvalState = x.b.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalTrailId = x.b.APPROVALTRAILID,

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

                    currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                    currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                    timeIn = x.b.SYSTEMARRIVALDATETIME,
                    timeOut = x.b.SYSTEMRESPONSEDATETIME,
                    responsiblePerson = context.TBL_STAFF
                                            .Where(s => s.STAFFID == x.b.TOSTAFFID)
                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                            .FirstOrDefault().name ?? "",
                    requestStaffId = x.b.REQUESTSTAFFID,
                    toApprovalLevelId = x.b.TOAPPROVALLEVELID,

                    creditOperationType = context.TBL_LMSR_APPLICATION_DETAIL.Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count() > 1
                    ? "Multiple"
                    : context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID ==
                            context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).OPERATIONID
                        ).OPERATIONNAME,

                    facilityType = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                    applicationDetails = x.a.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                    .Select(d => new applicationDetails
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
                .GroupBy(d => d.loanReviewApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                ;

            return applications;
        }

        #region
        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            searchString = searchString.Trim().ToLower(); //46, 71, 79

            // int[] operations = { (int)OperationsEnum.LoanReviewApprovalAppraisal, (int)OperationsEnum.LoanReviewApprovalOfferLetter, (int)OperationsEnum.LoanReviewApprovalAvailment ,
            //(int)OperationsEnum.NPLoanReviewApprovalAppraisal,(int)OperationsEnum.WrittenOffLoanReviewApprovalAppraisal};
            var staffs = context.TBL_STAFF.ToList();
            var operations = context.TBL_OPERATIONS.Where(o => o.OPERATIONTYPEID == (int)OperationTypeEnum.LoanReviewApplication).Select(o => o.OPERATIONID).ToList();
            var operations2 = context.TBL_OPERATIONS.Where(o => o.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement || o.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft).Select(o => o.OPERATIONID).ToList();
            int staffId = context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault();

            var applications = (from a in context.TBL_LMSR_APPLICATION
                                join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
                                join l in context.TBL_LOAN on d.LOANID equals l.TERMLOANID into dl
                                join r in context.TBL_LOAN_REVOLVING on d.LOANID equals r.REVOLVINGLOANID into lr
                                join c in context.TBL_LOAN_CONTINGENT on d.LOANID equals c.CONTINGENTLOANID into dc
                                join ex in context.TBL_LOAN_EXTERNAL on d.LOANID equals ex.EXTERNALLOANID into dex
                                from ex in dex.DefaultIfEmpty()
                                from l in dl.DefaultIfEmpty()
                                join ldl in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ldl.LOANAPPLICATIONDETAILID into x
                                from r in lr.DefaultIfEmpty()
                                join ldr in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals ldr.LOANAPPLICATIONDETAILID into y
                                from c in dc.DefaultIfEmpty()
                                join ldc in context.TBL_LOAN_APPLICATION_DETAIL on c.LOANAPPLICATIONDETAILID equals ldc.LOANAPPLICATIONDETAILID into z
                                from x1 in x.DefaultIfEmpty()
                                from y1 in y.DefaultIfEmpty()
                                from z1 in z.DefaultIfEmpty()
                                    //join y in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals y.TARGETID
                                    // let staffcode = context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault()

                                where //y.RESPONSESTAFFID == null && operations.Contains(y.OPERATIONID) &&
                               (a.APPLICATIONREFERENCENUMBER == searchString
                               || x1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == searchString
                               || y1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == searchString
                               || z1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == searchString
                               || l.LOANREFERENCENUMBER == searchString
                               || r.LOANREFERENCENUMBER == searchString
                               || c.LOANREFERENCENUMBER == searchString
                               || g.FIRSTNAME.ToLower().Contains(searchString)
                               || g.LASTNAME.ToLower().Contains(searchString)
                               || g.MIDDLENAME.ToLower().Contains(searchString)
                               || d.CREATEDBY == staffId)
                                select new LoanApplicationViewModel
                                {
                                    creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select c.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                                    creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                    (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

                                    relatedReferenceNumber = x1 != null ? x1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (y1 != null ? y1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (z1 != null ? z1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (ex != null ? ex.LOANREFERENCENUMBER : "N/A"))),
                                    firstName = g.FIRSTNAME,
                                    referenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    middleName = g.MIDDLENAME,
                                    lastName = g.LASTNAME,
                                    customerCode = g.CUSTOMERCODE,
                                    customerName = g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME,
                                    applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = a.LOANAPPLICATIONID,
                                    loanApplicationIdForOperation = d.LOANID,
                                   customerId = a.CUSTOMERID,
                                   branchId = a.BRANCHID,
                                   customerGroupId = a.CUSTOMERGROUPID,
                                   applicationDate = a.APPLICATIONDATE,
                                   applicationAmount = d.PROPOSEDAMOUNT,
                                   reviewLoanDetaile = d.REVIEWDETAILS,
                                   operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   approvedAmount = d.APPROVEDAMOUNT,
                                   interestRate = d.PROPOSEDINTERESTRATE,
                                   applicationTenor = d.PROPOSEDTENOR,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                   //currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                   //approvalTrailId = y.APPROVALTRAILID,
                                   //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                   applicationStatusId = a.APPLICATIONSTATUSID,
                                   applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                   branchName = a.TBL_BRANCH.BRANCHNAME,
                                   //relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   //relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o=>o.FIRSTNAME +" "+o.MIDDLENAME+ " "+o.LASTNAME).FirstOrDefault(),
                                   relationshipManagerName = context.TBL_STAFF.Where(s => s.STAFFID == l.RELATIONSHIPMANAGERID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),

                                   createdBy = a.CREATEDBY,
                                   operationId = a.OPERATIONID,
                                   isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
                               }).ToList();

            var archivedloans = (from a in context.TBL_LMSR_APPLICATION
                                join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
                                join l in context.TBL_LOAN on d.LOANID equals l.TERMLOANID into dl
                                join r in context.TBL_LOAN_REVOLVING on d.LOANID equals r.REVOLVINGLOANID into lr
                                join c in context.TBL_LOAN_CONTINGENT on d.LOANID equals c.CONTINGENTLOANID into dc
                                join ex in context.TBL_LOAN_EXTERNAL on d.LOANID equals ex.EXTERNALLOANID into dex
                                from ex in dex.DefaultIfEmpty()
                                from l in dl.DefaultIfEmpty()
                                join ldl in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ldl.LOANAPPLICATIONDETAILID into x
                                from r in lr.DefaultIfEmpty()
                                join ldr in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals ldr.LOANAPPLICATIONDETAILID into y
                                from c in dc.DefaultIfEmpty()
                                join ldc in context.TBL_LOAN_APPLICATION_DETAIL on c.LOANAPPLICATIONDETAILID equals ldc.LOANAPPLICATIONDETAILID into z
                                from x1 in x.DefaultIfEmpty()
                                from y1 in y.DefaultIfEmpty()
                                from z1 in z.DefaultIfEmpty()
                                
                                where ((l.LOANREFERENCENUMBER == searchString && l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && l.ISPRINTED == true)
                               || (r.LOANREFERENCENUMBER == searchString && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && r.ISPRINTED == true)
                               || (c.LOANREFERENCENUMBER == searchString && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISPRINTED == true))
                                select new LoanApplicationViewModel
                                {
                                    creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select c.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                                    creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                    (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

                                    relatedReferenceNumber = x1 != null ? x1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (y1 != null ? y1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (z1 != null ? z1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (ex != null ? ex.LOANREFERENCENUMBER : "N/A"))),
                                    firstName = g.FIRSTNAME,
                                    referenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    middleName = g.MIDDLENAME,
                                    lastName = g.LASTNAME,
                                    customerCode = g.CUSTOMERCODE,
                                    applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = a.LOANAPPLICATIONID,
                                    loanApplicationIdForOperation = d.LOANID,
                                    customerId = a.CUSTOMERID,
                                    branchId = a.BRANCHID,
                                    customerName = g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME,
                                    customerGroupId = a.CUSTOMERGROUPID,
                                    applicationDate = a.APPLICATIONDATE,
                                    applicationAmount = d.PROPOSEDAMOUNT,
                                    reviewLoanDetaile = d.REVIEWDETAILS,
                                    operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                    approvedAmount = d.APPROVEDAMOUNT,
                                    interestRate = d.PROPOSEDINTERESTRATE,
                                    applicationTenor = d.PROPOSEDTENOR,
                                    approvalStatusId = a.APPROVALSTATUSID,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                    //currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                    //approvalTrailId = y.APPROVALTRAILID,
                                    //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                    applicationStatusId = a.APPLICATIONSTATUSID,
                                    applicationStatus = "Archived",
                                    //applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                    branchName = a.TBL_BRANCH.BRANCHNAME,
                                    //relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    //relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerName = context.TBL_STAFF.Where(s => s.STAFFID == l.RELATIONSHIPMANAGERID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),

                                    createdBy = a.CREATEDBY,
                                    operationId = a.OPERATIONID,
                                    isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
                                }).ToList();

            var groupApplications = (from a in context.TBL_LMSR_APPLICATION
                                    join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                    join c in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals c.CUSTOMERGROUPID
                                     join l in context.TBL_LOAN on d.LOANID equals l.TERMLOANID into dl
                                     join r in context.TBL_LOAN_REVOLVING on d.LOANID equals r.REVOLVINGLOANID into lr
                                     join cg in context.TBL_LOAN_CONTINGENT on d.LOANID equals cg.CONTINGENTLOANID into dc
                                     join ex in context.TBL_LOAN_EXTERNAL on d.LOANID equals ex.EXTERNALLOANID into dex
                                     from ex in dex.DefaultIfEmpty()
                                     from l in dl.DefaultIfEmpty()
                                     join ldl in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ldl.LOANAPPLICATIONDETAILID into x
                                     from r in lr.DefaultIfEmpty()
                                     join ldr in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals ldr.LOANAPPLICATIONDETAILID into y
                                     from cg in dc.DefaultIfEmpty()
                                     join ldc in context.TBL_LOAN_APPLICATION_DETAIL on cg.LOANAPPLICATIONDETAILID equals ldc.LOANAPPLICATIONDETAILID into z
                                     from x1 in x.DefaultIfEmpty()
                                     from y1 in y.DefaultIfEmpty()
                                     from z1 in z.DefaultIfEmpty()
                                         //join y in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals y.TARGETID
                                         // let staffcode = context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault()
                                     where //y.RESPONSESTAFFID == null && operations.Contains(y.OPERATIONID) &&
                                    (a.APPLICATIONREFERENCENUMBER == searchString
                                    || x1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == searchString
                                    || y1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == searchString
                                    || z1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == searchString
                                    || l.LOANREFERENCENUMBER == searchString
                                    || r.LOANREFERENCENUMBER == searchString
                                    || cg.LOANREFERENCENUMBER == searchString
                                    || c.GROUPNAME.ToLower().Contains(searchString)
                                    || c.GROUPCODE.ToLower().Contains(searchString)
                                    || c.GROUPDESCRIPTION.ToLower().Contains(searchString)
                                    || d.CREATEDBY == staffId)
                                    select new LoanApplicationViewModel
                                    {
                                        creditAppraisalOperationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select c.OPERATIONID).FirstOrDefault() :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.OPERATIONID).FirstOrDefault(),


                                        creditAppraisalLoanApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                    (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? 0 :
                                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault(),

                                        //firstName = c.FIRSTNAME,
                                        //middleName = c.MIDDLENAME,
                                        //lastName = c.LASTNAME,
                                        relatedReferenceNumber = x1 != null ? x1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (y1 != null ? y1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (z1 != null ? z1.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : (ex != null ? ex.LOANREFERENCENUMBER : "N/A"))),
                                        customerName = c.GROUPNAME,
                                        customerCode = c.GROUPCODE,
                                        referenceNumber = a.APPLICATIONREFERENCENUMBER,
                                        applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                        loanApplicationIdForOperation = d.LOANID,
                                        loanApplicationId = a.LOANAPPLICATIONID,
                                        
                                        customerId = a.CUSTOMERID,
                                        branchId = a.BRANCHID,
                                        customerGroupId = a.CUSTOMERGROUPID,
                                        applicationDate = a.APPLICATIONDATE,
                                        reviewLoanDetaile = d.REVIEWDETAILS,
                                        operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                        applicationAmount = d.PROPOSEDAMOUNT,
                                        approvedAmount = d.APPROVEDAMOUNT,
                                        interestRate = d.PROPOSEDINTERESTRATE,
                                        applicationTenor = d.PROPOSEDTENOR,
                                        approvalStatusId = (short)a.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        //currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                        //approvalTrailId = y.APPROVALTRAILID,
                                        //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                        applicationStatusId = a.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        branchName = a.TBL_BRANCH.BRANCHNAME,
                                        //relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                        //relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),

                                        createdBy = a.CREATEDBY,
                                        operationId = a.OPERATIONID,
                                        isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
                                    }).ToList();

            var allRecord = applications.Union(groupApplications).Union(archivedloans).ToList();

            foreach (var x in allRecord)
            {
                if (x.approvalStatusId == (int)ApprovalStatusEnum.Approved)
                {
                    var operationRec = context.TBL_LOAN_REVIEW_OPERATION.FirstOrDefault(op => op.LOANID == x.loanApplicationIdForOperation.Value && op.LOANREVIEWAPPLICATIONID == x.loanApplicationId);
                    if(operationRec != null)
                    {
                        var appRecord2 = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == operationRec.LOANREVIEWOPERATIONID && operations2.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).FirstOrDefault();
                        if (appRecord2 != null)
                        {
                            x.currentApprovalLevel = appRecord2.TOAPPROVALLEVELID != null ? appRecord2.TBL_APPROVAL_LEVEL1.LEVELNAME : "Credit Operations Inputer";
                            x.responsiblePerson = appRecord2.LOOPEDSTAFFID != null ? staffs.FirstOrDefault(s => s.STAFFID == appRecord2.LOOPEDSTAFFID).FIRSTNAME + " " + staffs.FirstOrDefault(s => s.STAFFID == appRecord2.LOOPEDSTAFFID).LASTNAME : appRecord2.TOSTAFFID == null ? appRecord2.TOAPPROVALLEVELID != null ? appRecord2.TBL_APPROVAL_LEVEL1.LEVELNAME : "Credit Operations Inputer" : appRecord2.TBL_STAFF1.FIRSTNAME + " " + appRecord2.TBL_STAFF1.MIDDLENAME + " " + appRecord2.TBL_STAFF1.LASTNAME;
                            x.approvalTrailId = appRecord2.APPROVALTRAILID;
                            x.currentOperationId = appRecord2.OPERATIONID;
                            x.approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == appRecord2.APPROVALSTATUSID).APPROVALSTATUSNAME;
                        }
                    }
                    else if (x.applicationStatusId == (int)LoanApplicationStatusEnum.ApplicationCompleted)
                    {
                        x.currentApprovalLevel = "N/A";
                        x.responsiblePerson = "N/A";
                    }
                    else
                    {
                        x.currentApprovalLevel = "Credit Operations Inputer";
                        x.responsiblePerson = "Credit Operations Inputer";
                    }
                    
                }
                else
                {
                    var appRecord = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.loanApplicationId && operations.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).FirstOrDefault();
                    if (appRecord != null)
                    {
                        var singleRec = appRecord;
                        x.currentApprovalLevel = singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : x.isFacilityCreated == true ? "DISBURSED" : "N/A";
                        x.approvalTrailId = singleRec.APPROVALTRAILID;
                        x.responsiblePerson = singleRec.TOSTAFFID == null ? singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A" : singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;// y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                        x.currentOperationId = singleRec.OPERATIONID;
                    }
                    else
                    {
                        x.currentApprovalLevel = x.isFacilityCreated == true ? "DISBURSED" : "N/A";
                        x.responsiblePerson = x.isFacilityCreated == true ? "DISBURSED" : "N/A";
                    }
                }
            }
            allRecord = allRecord.OrderByDescending(r => r.approvalTrailId).ToList();
            return allRecord;

            //var applications = from a in context.TBL_LMSR_APPLICATION
            //                   join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
            //                   join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
            //                   join y in context.TBL_APPROVAL_TRAIL on d.LOANREVIEWAPPLICATIONID equals y.TARGETID
            //                   let staffcode = context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault()
            //                   where (a.APPLICATIONREFERENCENUMBER == searchString
            //                   || g.FIRSTNAME.ToLower().StartsWith(searchString)
            //                   || g.LASTNAME.ToLower().StartsWith(searchString)
            //                   || g.MIDDLENAME.ToLower().StartsWith(searchString)
            //                   || d.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
            //                   select new LoanApplicationViewModel
            //                   {
            //                       firstName = g.FIRSTNAME,
            //                       middleName = g.MIDDLENAME,
            //                       lastName = g.LASTNAME,
            //                       customerCode = g.CUSTOMERCODE,
            //                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
            //                       loanApplicationId = a.LOANAPPLICATIONID,
            //                       customerId = a.CUSTOMERID,
            //                       branchId = a.BRANCHID,
            //                       customerGroupId = a.CUSTOMERGROUPID,
            //                       applicationDate = a.APPLICATIONDATE,
            //                       applicationAmount = d.PROPOSEDAMOUNT,
            //                       approvedAmount = d.APPROVEDAMOUNT,
            //                       interestRate = d.PROPOSEDINTERESTRATE,
            //                       applicationTenor = d.PROPOSEDTENOR,
            //                       approvalStatusId = (short)a.APPROVALSTATUSID,
            //                       approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,
            //                       currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
            //                       approvalTrailId = y.APPROVALTRAILID,
            //                       responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,
            //                       applicationStatusId = a.APPLICATIONSTATUSID,
            //                       applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
            //                       branchName = a.TBL_BRANCH.BRANCHNAME,
            //                       relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
            //                       relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
            //                       createdBy = a.CREATEDBY,
            //                       operationId = a.OPERATIONID,
            //                       isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
            //                   };

            //return applications;
        }


        public IEnumerable<LoanReviewOperationViewModel> ContingentSearch(string searchString)
        {
            searchString = searchString.Trim().ToLower();
            int staffId = context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault();

            var ops = (from op in context.TBL_LOAN_REVIEW_OPERATION
                       join l in context.TBL_LOAN_CONTINGENT on op.LOANID equals l.CONTINGENTLOANID
                       join g in context.TBL_CUSTOMER on l.CUSTOMERID equals g.CUSTOMERID
                       where
                        op.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability
                        && ( l.LOANREFERENCENUMBER == searchString
                        || l.RELATED_LOAN_REFERENCE_NUMBER == searchString
                        || g.FIRSTNAME.ToLower().Contains(searchString)
                        || g.LASTNAME.ToLower().Contains(searchString)
                        || g.MIDDLENAME.ToLower().Contains(searchString)
                        || op.CREATEDBY == staffId)
                        select new LoanReviewOperationViewModel
                        {
                            customersName = g.FIRSTNAME+ " "+ g.MIDDLENAME+ " "+ g.LASTNAME,
                            loanReferenceNumber = l.LOANREFERENCENUMBER,
                            operationId = op.OPERATIONTYPEID,
                            operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == op.OPERATIONTYPEID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                            reviewDetails = op.REVIEWDETAILS,
                            proposedEffectiveDate = op.EFFECTIVEDATE,
                            approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == op.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                            operationCompleted = op.OPERATIONCOMPLETED,
                            loanReviewOperationsId = op.LOANREVIEWOPERATIONID,
                            loanReviewApplicationId = op.LOANREVIEWAPPLICATIONID,
                            rebookAmount = op.CONTINGENTOUTSTANDINGPRINCIPAL,
                            dateRebook = op.DATECREATED,
                            rebookDate = op.REBOOKDATE,
                            loanId = op.LOANID,
                            bondAmount = l.CONTINGENTAMOUNT,
                            contingentOutstandingPrincipal = op.CONTINGENTOUTSTANDINGPRINCIPAL,
                        }).ToList();
                foreach (var p in ops)
                {
                    var staff = context.TBL_LOAN_REVIEW_OPERATION.Where(o => DbFunctions.TruncateTime(o.DATECREATED) != p.dateTimeCreated && o.LOANID == p.loanId && o.OPERATIONTYPEID == p.operationId).Select(o => o.CREATEDBY).FirstOrDefault();
                    p.previousOperator = context.TBL_STAFF.Where(s => s.STAFFID == staff).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                    p.exposureBeforeRebook = p.bondAmount;
                    p.reviewOperationId = (from a in context.TBL_LMSR_APPLICATION_DETAIL join b in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID where a.LOANREVIEWAPPLICATIONID == p.loanReviewApplicationId select b.OPERATIONID).FirstOrDefault();
                    p.loanApplicationId = (from a in context.TBL_LMSR_APPLICATION_DETAIL join b in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID where a.LOANREVIEWAPPLICATIONID == p.loanReviewApplicationId select b.LOANAPPLICATIONID).FirstOrDefault();

                }

               return ops.ToList();

        }


        public IEnumerable<LoanApplicationDetailViewModel> ExceptionalSearch(string searchString)
        {
            searchString = searchString.Trim().ToLower();
            int staffId = context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault();

            var exceptionalLoansForApproval = (from d in context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL
                                               join e in context.TBL_EXCEPTIONAL_LOAN_APPLICATION on d.EXCEPTIONALLOANAPPLICATIONID equals e.EXCEPTIONALLOANAPPLICATIONID
                                               join t in context.TBL_APPROVAL_TRAIL on d.EXCEPTIONALLOANAPPLDETAILID equals t.TARGETID
                                               join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
                                               where d.DELETED == false && t.OPERATIONID == (int)OperationsEnum.ExceptionalLoan
                                                && (e.APPLICATIONREFERENCENUMBER == searchString
                                                || e.RELATEDREFERENCENUMBER == searchString
                                                || g.FIRSTNAME.ToLower().Contains(searchString)
                                                || g.LASTNAME.ToLower().Contains(searchString)
                                                || g.MIDDLENAME.ToLower().Contains(searchString)
                                                || d.CREATEDBY == staffId)
                                               select new LoanApplicationDetailViewModel
                                               {
                                                   customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == e.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault(),
                                                   loanApplicationId = e.EXCEPTIONALLOANAPPLICATIONID,
                                                   dateTimeCreated = d.DATETIMECREATED,
                                                   proposedAmount = d.PROPOSEDAMOUNT,
                                                   proposedInterestRate = d.PROPOSEDINTERESTRATE,
                                                   proposedProductId = d.PROPOSEDPRODUCTID,
                                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER,
                                                   approvedAmount = d.APPROVEDAMOUNT,
                                                   approvedInterestRate = d.APPROVEDINTERESTRATE,
                                                   approvedProductId = d.APPROVEDPRODUCTID,
                                                   approvedTenor = d.APPROVEDTENOR,
                                                   exchangeRate = d.EXCHANGERATE,
                                                   currencyId = d.CURRENCYID,
                                                   customerId = d.CUSTOMERID,
                                                   equityCasaAccountId = d.EQUITYCASAACCOUNTID,
                                                   equityAmount = d.EQUITYAMOUNT,
                                                   subSectorId = d.SUBSECTORID,
                                                   loanPurpose = d.LOANPURPOSE,
                                                   casaAccountId = d.CASAACCOUNTID,
                                                   repaymentTerm = d.REPAYMENTTERMS,
                                                   repaymentScheduleId = d.REPAYMENTSCHEDULEID,
                                                   isTakeOverApplication = d.ISTAKEOVERAPPLICATION,
                                                   crmsFundingSourceId = d.CRMSFUNDINGSOURCEID,
                                                   crmsPaymentSourceId = d.CRMSREPAYMENTSOURCEID,
                                                   crmsFundingSourceCategory = d.CRMSFUNDINGSOURCECATEGORY,
                                                   productPriceIndexId = d.PRODUCTPRICEINDEXID,
                                                   productPriceIndexRate = d.PRODUCTPRICEINDEXRATE,
                                                   operatingCasaAccountId = d.OPERATINGCASAACCOUNTID,
                                                   loanDetailReviewTypeId = d.LOANDETAILREVIEWTYPEID,
                                                   tenorModeId = d.TENORFREQUENCYTYPEID,
                                                   flowChangeId = d.TBL_EXCEPTIONAL_LOAN_APPLICATION.FLOWCHANGEID,
                                                   isLineFacility = d.ISLINEFACILITY,
                                                   approvedLineLimit = d.APPROVEDLINELIMIT,
                                                   interestRepaymentId = d.INTERESTREPAYMENTID,
                                                   interestRepayment = d.INTERESTREPAYMENT,
                                                   isMoratorium = d.ISMORATORIUM,
                                                   moratorium = d.MORATORIUM,
                                                   proposedTenor = d.PROPOSEDTENOR,
                                                   breachedLimitName = d.BREACHEDLIMITNAME,
                                                   loanApplicationDetailId = d.EXCEPTIONALLOANAPPLDETAILID,
                                                   operationId = t.OPERATIONID,
                                                   approvalStatusId = t.APPROVALSTATUSID,
                                                   approvalTrailId = t.APPROVALTRAILID,
                                                   currentApprovalLevelId = t.TOAPPROVALLEVELID,
                                                   currentApprovalLevel = t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                                   approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(a => a.APPROVALSTATUSID == t.APPROVALSTATUSID).APPROVALSTATUSNAME.ToUpper(),
                                               }).GroupBy(d => d.loanApplicationDetailId)
                                                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault()).ToList();
            foreach (var x in exceptionalLoansForApproval)
            {
                var templateExist = context.TBL_DOC_TEMPLATE_DETAIL.Where(p => p.TARGETID == x.loanApplicationId && p.OPERATIONID == (int)OperationsEnum.ExceptionalLoan).ToList();
                if (templateExist.Any())
                {
                    x.isTemplateUploaded = true;
                }
                else
                {
                    x.isTemplateUploaded = false;
                }

            }
            return exceptionalLoansForApproval;
        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> SearchLien(string searchString)
        {
            searchString = searchString.Trim().ToLower();
            var dataLoan = (from ln in context.TBL_LOAN
                            join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                            join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                            join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                            join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                            join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                            join ca in context.TBL_CASA_LIEN on ln.LOANREFERENCENUMBER equals ca.SOURCEREFERENCENUMBER into cal
                            from ca in cal.DefaultIfEmpty()
                            where
                            (cu.FIRSTNAME.ToLower().Contains(searchString)
                            || cu.LASTNAME.ToLower().Contains(searchString)
                            || cu.MIDDLENAME.ToLower().Contains(searchString)
                            || ca.SOURCEREFERENCENUMBER == searchString
                            || ca.PRODUCTACCOUNTNUMBER == searchString
                            || st.STAFFCODE == searchString)
                            && (ca.LIENSTATUS == (int)LienStatusEnum.Active || ca.LIENSTATUS == null)

                            select new LoanReviewOperationApprovalViewModel
                            {
                                loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                loanId = ln.TERMLOANID,
                                customerId = ln.CUSTOMERID,
                                productId = ln.PRODUCTID,
                                productTypeId = pr.PRODUCTTYPEID,
                                casaAccountId = ln.CASAACCOUNTID,
                                casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                branchId = ln.BRANCHID,
                                loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                misCode = ln.MISCODE,

                                sourceReferenceNumber = ca.SOURCEREFERENCENUMBER,
                                lienReferenceNumber = ca.LIENREFERENCENUMBER,
                                lienAmount = ca.LIENAMOUNT,
                                lienDateTimeCreated = ca.DATETIMECREATED,
                                productAccountNumber = ca.PRODUCTACCOUNTNUMBER,
                                casaLienAccountId = ca.LIENID,

                                teamMiscode = ln.TEAMMISCODE,
                                interestRate = ln.INTERESTRATE,
                                effectiveDate = ln.EFFECTIVEDATE,
                                maturityDate = ln.MATURITYDATE,
                                bookingDate = ln.BOOKINGDATE,
                                principalAmount = ln.OUTSTANDINGPRINCIPAL, //\\\ln.PrincipalAmount,
                                principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                approvedBy = (int)ln.APPROVEDBY,
                                approverComment = ln.APPROVERCOMMENT,
                                dateApproved = ln.DATEAPPROVED,
                                loanStatusId = ln.LOANSTATUSID,
                                scheduleTypeId = ln.SCHEDULETYPEID,
                                isDisbursed = ln.ISDISBURSED,
                                disbursedBy = (int)ln.DISBURSEDBY,
                                disburserComment = ln.DISBURSERCOMMENT,
                                disburseDate = ln.DISBURSEDATE,
                                operationId = ln.OPERATIONID,
                                equityContribution = ln.EQUITYCONTRIBUTION,
                                subSectorId = ln.SUBSECTORID,
                                subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                fixedPrincipal = ln.FIXEDPRINCIPAL,
                                profileLoan = ln.PROFILELOAN,
                                dischargeLetter = ln.DISCHARGELETTER,
                                suspendInterest = ln.SUSPENDINTEREST,
                                scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                customerCode = cu.CUSTOMERCODE,
                                
                                customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                currencyId = ln.CURRENCYID,
                                branchName = br.BRANCHNAME,
                                relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                productName = pr.PRODUCTNAME,
                                comment = "",
                                pastDueInterest = ln.PASTDUEINTEREST,
                                pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                outstandingInterest = ln.OUTSTANDINGINTEREST,
                            }).ToList();

            var dataRevolvingLoan = (from ln in context.TBL_LOAN_REVOLVING
                                     join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                     join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     join ca in context.TBL_CASA_LIEN on ln.LOANREFERENCENUMBER equals ca.SOURCEREFERENCENUMBER into cal
                                     from ca in cal.DefaultIfEmpty()
                                     where
                                     (cu.FIRSTNAME.ToLower().Contains(searchString)
                                     || cu.LASTNAME.ToLower().Contains(searchString)
                                     || cu.MIDDLENAME.ToLower().Contains(searchString)
                                     || ca.SOURCEREFERENCENUMBER == searchString
                                     || ca.PRODUCTACCOUNTNUMBER == searchString
                                     || st.STAFFCODE == searchString)
                                     && (ca.LIENSTATUS == (int)LienStatusEnum.Active || ca.LIENSTATUS == null)

                                     select new LoanReviewOperationApprovalViewModel
                                     {
                                         loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                         loanId = ln.REVOLVINGLOANID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         productTypeId = pr.PRODUCTTYPEID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                         misCode = ln.MISCODE,

                                         sourceReferenceNumber = ca.SOURCEREFERENCENUMBER,
                                         lienReferenceNumber = ca.LIENREFERENCENUMBER,
                                         lienAmount = ca.LIENAMOUNT,
                                         lienDateTimeCreated = ca.DATETIMECREATED,
                                         productAccountNumber = ca.PRODUCTACCOUNTNUMBER,
                                         casaLienAccountId = ca.LIENID,

                                         teamMiscode = ln.TEAMMISCODE,
                                         interestRate = ln.INTERESTRATE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         approvedBy = (int)ln.APPROVEDBY,
                                         approverComment = ln.APPROVERCOMMENT,
                                         dateApproved = ln.DATEAPPROVED,
                                         loanStatusId = ln.LOANSTATUSID,
                                         isDisbursed = ln.ISDISBURSED,
                                         disburserComment = ln.DISBURSERCOMMENT,
                                         disburseDate = ln.DISBURSEDATE,
                                         operationId = ln.OPERATIONID,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         dischargeLetter = ln.DISCHARGELETTER,
                                         suspendInterest = ln.SUSPENDINTEREST,
                                         customerCode = cu.CUSTOMERCODE,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         currencyId = ln.CURRENCYID,
                                         branchName = br.BRANCHNAME,
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         comment = "",
                                         pastDueInterest = ln.PASTDUEINTEREST,
                                         pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                         interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                         interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                     }).ToList();

            var dataContingent = (from ln in context.TBL_LOAN_CONTINGENT
                                     join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                     join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     join ca in context.TBL_CASA_LIEN on ln.LOANREFERENCENUMBER equals ca.SOURCEREFERENCENUMBER into cal
                                     from ca in cal.DefaultIfEmpty()
                                     where
                                     (cu.FIRSTNAME.ToLower().Contains(searchString)
                                     || cu.LASTNAME.ToLower().Contains(searchString)
                                     || cu.MIDDLENAME.ToLower().Contains(searchString)
                                     || ca.SOURCEREFERENCENUMBER == searchString
                                     || ca.PRODUCTACCOUNTNUMBER == searchString
                                     || st.STAFFCODE == searchString)
                                     && (ca.LIENSTATUS == (int)LienStatusEnum.Active || ca.LIENSTATUS == null)

                                     select new LoanReviewOperationApprovalViewModel
                                     {
                                         loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                         loanId = ln.CONTINGENTLOANID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         productTypeId = pr.PRODUCTTYPEID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                         misCode = ln.MISCODE,

                                         sourceReferenceNumber = ca.SOURCEREFERENCENUMBER,
                                         lienReferenceNumber = ca.LIENREFERENCENUMBER,
                                         lienAmount = ca.LIENAMOUNT,
                                         lienDateTimeCreated = ca.DATETIMECREATED,
                                         productAccountNumber = ca.PRODUCTACCOUNTNUMBER,
                                         casaLienAccountId = ca.LIENID,

                                         teamMiscode = ln.TEAMMISCODE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         approvedBy = (int)ln.APPROVEDBY,
                                         approverComment = ln.APPROVERCOMMENT,
                                         dateApproved = ln.DATEAPPROVED,
                                         loanStatusId = ln.LOANSTATUSID,
                                         isDisbursed = ln.ISDISBURSED,
                                         disburserComment = ln.DISBURSERCOMMENT,
                                         disburseDate = ln.DISBURSEDATE,
                                         operationId = ln.OPERATIONID,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         dischargeLetter = ln.DISCHARGELETTER,
                                         customerCode = cu.CUSTOMERCODE,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         currencyId = ln.CURRENCYID,
                                         branchName = br.BRANCHNAME,
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         comment = "",
                                     }).ToList();


            var unionAll = dataLoan.Union(dataRevolvingLoan).Union(dataContingent);

            return unionAll;
        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetAllLienRemovalApplications(int staffId, int companyId)
        {

            var dataLoan = (from ln in context.TBL_LOAN
                            join ca in context.TBL_CASA_LIEN on ln.LOANREFERENCENUMBER equals ca.SOURCEREFERENCENUMBER into cal
                            from ca in cal.DefaultIfEmpty()
                            join lr in context.TBL_LIEN_REMOVAL on ca.LIENID equals lr.CASALIENACCOUNTID
                            join atrail in context.TBL_APPROVAL_TRAIL on lr.UNFREEZELIENACCOUNTID equals atrail.TARGETID into atraila
                            from atrail in atraila.DefaultIfEmpty()
                            join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                            join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                            join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                            join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                            join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                            where
                            (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                            || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                            || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised
                            || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                            && atrail.OPERATIONID == lr.OPERATIONID
                            
                            select new LoanReviewOperationApprovalViewModel
                            {
                                currentApprovalLevelId = (int)atrail.TOAPPROVALLEVELID,
                                loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                loanId = ln.TERMLOANID,
                                customerId = ln.CUSTOMERID,
                                productId = ln.PRODUCTID,
                                productTypeId = pr.PRODUCTTYPEID,
                                casaAccountId = ln.CASAACCOUNTID,
                                casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                branchId = ln.BRANCHID,
                                loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                misCode = ln.MISCODE,

                                sourceReferenceNumber = ca.SOURCEREFERENCENUMBER,
                                lienReferenceNumber = ca.LIENREFERENCENUMBER,
                                lienAmount = ca.LIENAMOUNT,
                                lienDateTimeCreated = ca.DATETIMECREATED,
                                productAccountNumber = ca.PRODUCTACCOUNTNUMBER,
                                casaLienAccountId = ca.LIENID,
                                lienRemovalId = lr.UNFREEZELIENACCOUNTID,
                                lienRemovalOperationId = lr.OPERATIONID,

                                teamMiscode = ln.TEAMMISCODE,
                                interestRate = ln.INTERESTRATE,
                                effectiveDate = ln.EFFECTIVEDATE,
                                maturityDate = ln.MATURITYDATE,
                                bookingDate = ln.BOOKINGDATE,
                                principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                approvalStatusId = lr.APPROVALSTATUSID,
                                approvedBy = (int)ln.APPROVEDBY,
                                approverComment = ln.APPROVERCOMMENT,
                                dateApproved = ln.DATEAPPROVED,
                                loanStatusId = ln.LOANSTATUSID,
                                scheduleTypeId = ln.SCHEDULETYPEID,
                                isDisbursed = ln.ISDISBURSED,
                                disbursedBy = (int)ln.DISBURSEDBY,
                                disburserComment = ln.DISBURSERCOMMENT,
                                disburseDate = ln.DISBURSEDATE,
                                equityContribution = ln.EQUITYCONTRIBUTION,
                                subSectorId = ln.SUBSECTORID,
                                subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                fixedPrincipal = ln.FIXEDPRINCIPAL,
                                profileLoan = ln.PROFILELOAN,
                                dischargeLetter = ln.DISCHARGELETTER,
                                suspendInterest = ln.SUSPENDINTEREST,
                                scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                customerCode = cu.CUSTOMERCODE,
                                customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                currencyId = ln.CURRENCYID,
                                branchName = br.BRANCHNAME,
                                relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                productName = pr.PRODUCTNAME,
                                comment = "",
                                pastDueInterest = ln.PASTDUEINTEREST,
                                pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                outstandingInterest = ln.OUTSTANDINGINTEREST,
                            }).ToList();

            var dataRevolvingLoan = (from ln in context.TBL_LOAN_REVOLVING
                                     join ca in context.TBL_CASA_LIEN on ln.LOANREFERENCENUMBER equals ca.SOURCEREFERENCENUMBER into cal
                                     from ca in cal.DefaultIfEmpty()
                                     join lr in context.TBL_LIEN_REMOVAL on ca.LIENID equals lr.CASALIENACCOUNTID
                                     join atrail in context.TBL_APPROVAL_TRAIL on lr.UNFREEZELIENACCOUNTID equals atrail.TARGETID into atraila
                                     from atrail in atraila.DefaultIfEmpty()
                                     join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                     join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     where
                                     (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                     || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised
                                     || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                                     && atrail.OPERATIONID == lr.OPERATIONID

                                     select new LoanReviewOperationApprovalViewModel
                                     {
                                         currentApprovalLevelId = (int)atrail.TOAPPROVALLEVELID,
                                         loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                         loanId = ln.REVOLVINGLOANID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         productTypeId = pr.PRODUCTTYPEID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                         misCode = ln.MISCODE,

                                         sourceReferenceNumber = ca.SOURCEREFERENCENUMBER,
                                         lienReferenceNumber = ca.LIENREFERENCENUMBER,
                                         lienAmount = ca.LIENAMOUNT,
                                         lienDateTimeCreated = ca.DATETIMECREATED,
                                         productAccountNumber = ca.PRODUCTACCOUNTNUMBER,
                                         casaLienAccountId = ca.LIENID,
                                         lienRemovalId = lr.UNFREEZELIENACCOUNTID,
                                         lienRemovalOperationId = lr.OPERATIONID,

                                         teamMiscode = ln.TEAMMISCODE,
                                         interestRate = ln.INTERESTRATE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         approvalStatusId = lr.APPROVALSTATUSID,
                                         approvedBy = (int)ln.APPROVEDBY,
                                         approverComment = ln.APPROVERCOMMENT,
                                         dateApproved = ln.DATEAPPROVED,
                                         loanStatusId = ln.LOANSTATUSID,
                                         isDisbursed = ln.ISDISBURSED,
                                         disburserComment = ln.DISBURSERCOMMENT,
                                         disburseDate = ln.DISBURSEDATE,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         dischargeLetter = ln.DISCHARGELETTER,
                                         suspendInterest = ln.SUSPENDINTEREST,
                                         customerCode = cu.CUSTOMERCODE,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         currencyId = ln.CURRENCYID,
                                         branchName = br.BRANCHNAME,
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         comment = "",
                                         pastDueInterest = ln.PASTDUEINTEREST,
                                         pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                         interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                         interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                     }).ToList();

            var dataContingent = (from ln in context.TBL_LOAN_CONTINGENT
                                     join ca in context.TBL_CASA_LIEN on ln.LOANREFERENCENUMBER equals ca.SOURCEREFERENCENUMBER into cal
                                     from ca in cal.DefaultIfEmpty()
                                     join lr in context.TBL_LIEN_REMOVAL on ca.LIENID equals lr.CASALIENACCOUNTID
                                     join atrail in context.TBL_APPROVAL_TRAIL on lr.UNFREEZELIENACCOUNTID equals atrail.TARGETID into atraila
                                     from atrail in atraila.DefaultIfEmpty()
                                     join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                     join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     where
                                     (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                     || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised
                                     || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                                     && atrail.OPERATIONID == lr.OPERATIONID

                                     select new LoanReviewOperationApprovalViewModel
                                     {
                                         currentApprovalLevelId = (int)atrail.TOAPPROVALLEVELID,
                                         loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                         loanId = ln.CONTINGENTLOANID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         productTypeId = pr.PRODUCTTYPEID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                         misCode = ln.MISCODE,

                                         sourceReferenceNumber = ca.SOURCEREFERENCENUMBER,
                                         lienReferenceNumber = ca.LIENREFERENCENUMBER,
                                         lienAmount = ca.LIENAMOUNT,
                                         lienDateTimeCreated = ca.DATETIMECREATED,
                                         productAccountNumber = ca.PRODUCTACCOUNTNUMBER,
                                         casaLienAccountId = ca.LIENID,
                                         lienRemovalId = lr.UNFREEZELIENACCOUNTID,
                                         lienRemovalOperationId = lr.OPERATIONID,

                                         teamMiscode = ln.TEAMMISCODE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         approvalStatusId = lr.APPROVALSTATUSID,
                                         approvedBy = (int)ln.APPROVEDBY,
                                         approverComment = ln.APPROVERCOMMENT,
                                         dateApproved = ln.DATEAPPROVED,
                                         loanStatusId = ln.LOANSTATUSID,
                                         isDisbursed = ln.ISDISBURSED,
                                         disburserComment = ln.DISBURSERCOMMENT,
                                         disburseDate = ln.DISBURSEDATE,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         dischargeLetter = ln.DISCHARGELETTER,
                                         customerCode = cu.CUSTOMERCODE,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         currencyId = ln.CURRENCYID,
                                         branchName = br.BRANCHNAME,
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         comment = "",
                                     }).ToList();


            var unionAll = dataLoan.Union(dataRevolvingLoan).Union(dataContingent);
            return unionAll;
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
                            createdBy = a.OWNEDBY,
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
            appl.OPERATIONID = model.operationId;

            return context.SaveChanges() > 0;
        }

        public bool UpdateManagementPosition(ManagementPositionViewModel model)
        {
            var entity = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == model.applicationDetailId);
            if (entity == null) return false;
            entity.MANAGEMENTPOSITION = model.managementPosition;
            context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            return context.SaveChanges() > 0;
        }

        public ManagementPositionViewModel GetManagementPosition(int detailId)
        {
            var entity = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == detailId);

            ManagementPositionViewModel position = new ManagementPositionViewModel();
            if (entity == null) return position;
            position.managementPosition = entity.MANAGEMENTPOSITION;
            position.applicationDetailId = detailId;
            return position;
        }

        public bool ChecklistCompleted(int applicationId)
        {
            var condition = (from c in context.TBL_LMSR_CONDITION_PRECEDENT
                             where c.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId
                             && c.TBL_LMSR_APPLICATION_DETAIL.DELETED != true
                             && c.ISEXTERNAL == true && c.ISSUBSEQUENT == false
                             select c).ToList();

            var status = (from c in context.TBL_LMSR_CONDITION_PRECEDENT
                          where c.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId
                             && c.TBL_LMSR_APPLICATION_DETAIL.DELETED != true
                          && c.ISEXTERNAL == true && c.ISSUBSEQUENT == false && c.CHECKLISTSTATUSID != null
                          select c).ToList();

            return condition.Count == status.Count;
        }

        public List<LoanReviewOperationViewModel> GetMaturityInstruction(int loanId, short loansystemTypeId)
        {
            var ops = (from op in context.TBL_LOAN_MATURITY_INSTRUCTION
                       where op.LOANID == loanId && op.LOANSYSTEMTYPEID == loansystemTypeId
                       select new LoanReviewOperationViewModel
                       {
                           isUsed = op.ISUSED == false ? "False" : "True",
                           tenor = op.TENOR,
                           approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == op.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           instructionType = context.TBL_LOAN_MATURITY_INSTRU_TYPE.Where(x=> x.INSTRUCTIONTYPEID == op.INSTRUCTIONTYPEID).Select(x=> x.INSTRUCTIONTYPENAME).FirstOrDefault(),
                           actionBy = context.TBL_STAFF.Where(o=> o.STAFFID == op.CREATEDBY).Select( x=> new { Name = x.LASTNAME + ", " + x.FIRSTNAME }).FirstOrDefault().Name,
                       });

            return ops.ToList();
        }

        public List<LoanReviewOperationViewModel> GetLMSOperation(int loanId, short loansystemTypeId)
        {
            var ops = (from op in context.TBL_LOAN_REVIEW_OPERATION
                       where op.LOANID == loanId && op.LOANSYSTEMTYPEID == loansystemTypeId && op.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                       select new LoanReviewOperationViewModel
                       {
                           loanReferenceNumber = (op.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook) ? context.TBL_LOAN_CONTINGENT.Where(x=>x.CONTINGENTLOANID == op.LOANID).Select(x=>x.LOANREFERENCENUMBER).FirstOrDefault() : "",
                           operationId = op.OPERATIONTYPEID,
                           operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == op.OPERATIONTYPEID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                           reviewDetails = op.REVIEWDETAILS,
                           proposedEffectiveDate = op.EFFECTIVEDATE,
                           approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == op.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           operationCompleted = op.OPERATIONCOMPLETED,
                           //loanApplicationId = 0,
                           loanReviewOperationsId = op.LOANREVIEWOPERATIONID,
                           loanReviewApplicationId = op.LOANREVIEWAPPLICATIONID,
                           rebookAmount = op.CONTINGENTOUTSTANDINGPRINCIPAL,
                           dateRebook = op.DATECREATED,
                           rebookDate = op.REBOOKDATE,
                           loanId = op.LOANID,
                           bondAmount = context.TBL_LOAN_CONTINGENT.Where(c => c.CONTINGENTLOANID == op.LOANID).Select(c => c.CONTINGENTAMOUNT).FirstOrDefault(),
                           contingentOutstandingPrincipal = op.CONTINGENTOUTSTANDINGPRINCIPAL,
                       }).ToList();
            foreach(var p in ops)
            {
                var staff = context.TBL_LOAN_REVIEW_OPERATION.Where(o => DbFunctions.TruncateTime(o.DATECREATED) != p.dateTimeCreated && o.LOANID == p.loanId && o.OPERATIONTYPEID == p.operationId).Select(o => o.CREATEDBY).FirstOrDefault();
                p.previousOperator = context.TBL_STAFF.Where(s => s.STAFFID == staff).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                p.exposureBeforeRebook = context.TBL_LOAN_REVIEW_OPERATION.Where(o => o.LOANID == p.loanId && o.OPERATIONTYPEID == p.operationId && o.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Select(o => o.LOANID).Count() < 1 ? (decimal)0.01 : (context.TBL_LOAN_REVIEW_OPERATION.Where(o => o.LOANID == p.loanId && o.OPERATIONTYPEID == p.operationId  && o.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook).Sum(o => o.CONTINGENTOUTSTANDINGPRINCIPAL) - context.TBL_LOAN_REVIEW_OPERATION.Where(o => o.LOANID == p.loanId && o.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityAmountReduction && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Sum(o => o.PREPAYMENT));
                p.reviewOperationId = (from a in context.TBL_LMSR_APPLICATION_DETAIL join b in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID where a.LOANREVIEWAPPLICATIONID == p.loanReviewApplicationId select b.OPERATIONID).FirstOrDefault();
                p.loanApplicationId = (from a in context.TBL_LMSR_APPLICATION_DETAIL join b in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID where a.LOANREVIEWAPPLICATIONID == p.loanReviewApplicationId select b.LOANAPPLICATIONID).FirstOrDefault();

            }

            return ops.ToList();
        }

        public bool AddLoanCollateralMapping(int loanApplicationId)
        {
            LoanApplicationViewModel appl;
            List<int> existingCollateralIds;
            List<TBL_LOAN_APPLICATION_COLLATERL> recommendedCollaterals;

            var details = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false).Where(x => x.LOANAPPLICATIONID == loanApplicationId).ToList();
            
            foreach(var d in details)
            {
                appl = GetLoanApplicationByLoanSystemType(d.LOANSYSTEMTYPEID, d.LOANID);

                existingCollateralIds = context.TBL_LOAN_COLLATERAL_MAPPING
                    .Where(x => x.LOANID == d.LOANID && x.ISRELEASED == false)
                    .Select(x => x.COLLATERALCUSTOMERID)
                    .ToList();

                recommendedCollaterals = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == appl.loanApplicationId).ToList();

                foreach (var recommended in recommendedCollaterals)
                {
                    if (existingCollateralIds.Contains(recommended.COLLATERALCUSTOMERID)) continue;
                    context.TBL_LOAN_COLLATERAL_MAPPING.Add(new TBL_LOAN_COLLATERAL_MAPPING
                    {
                        COLLATERALCUSTOMERID = recommended.COLLATERALCUSTOMERID,
                        LOANAPPCOLLATERALID = recommended.LOANAPPCOLLATERALID,
                        LOANID = d.LOANID,
                        LOANSYSTEMTYPEID = d.LOANSYSTEMTYPEID,
                        ISRELEASED = false,
                    });
                }
            }

            return context.SaveChanges() > 0;
        }

        private LoanApplicationViewModel GetLoanApplicationByLoanSystemType(int loanSystemTypeId, int loanId)
        {
            var result = new LoanApplicationViewModel();

            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                result = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, l => l.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (l, d) => new { l, d })
                    .Select(x => new LoanApplicationViewModel { loanApplicationId = x.d.LOANAPPLICATIONID })
                    .FirstOrDefault();
            }
            else
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                result = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, l => l.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (l, d) => new { l, d })
                    .Select(x => new LoanApplicationViewModel { loanApplicationId = x.d.LOANAPPLICATIONID })
                    .FirstOrDefault();
            }
            else
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                result = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, l => l.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (l, d) => new { l, d })
                    .Select(x => new LoanApplicationViewModel { loanApplicationId = x.d.LOANAPPLICATIONID })
                    .FirstOrDefault();
            }
            else
            {
                throw new SecureException("Collateral Failed To Map. Loan System Type could not be resolved!");
            }

            if (result.loanApplicationId < 1) throw new SecureException("Collateral Failed To Map. Error resolving Loan Application Information.");

            return result;
        }

        private Dictionary<string, int> GetRepresentStepdownItems(int applicationId, int action, int operationId)
        {
            int levelId;
            int staffId;

            var trails = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    && x.TARGETID == applicationId
                    && x.FROMAPPROVALLEVELID != null
                    && x.TOAPPROVALLEVELID != null
                ).OrderBy(x => x.APPROVALTRAILID);

            if (action == (int)ApprovalStatusEnum.RePresent)
            {
                var traill = trails.Join(context.TBL_APPROVAL_LEVEL.Where(x => x.LEVELTYPEID == 2)
                        , t => t.FROMAPPROVALLEVELID, l => l.APPROVALLEVELID, (t, l) => new { t, l })
                        .Select(x => new { x.t }).First();
                staffId = traill.t.REQUESTSTAFFID;
                levelId = (int)traill.t.FROMAPPROVALLEVELID;
            }
            else
            {
                var trail = trails.FirstOrDefault();
                staffId = trail.REQUESTSTAFFID;
                levelId = (int)trail.FROMAPPROVALLEVELID;
            }

            if (levelId < 1 || staffId < 1) throw new SecureException("Error while resolving receiving staff.");

            var data = new Dictionary<string, int>();
            data.Add("levelId", levelId);
            data.Add("staffId", staffId);

            return data;
        }

        public decimal? GetWrittenOffAccrualAmount(int loanId, short loanSystemTypeId)
        {
            decimal? amount = null;
            var camsol = context.TBL_LOAN_CAMSOL.FirstOrDefault(x => x.LOANID == loanId && x.LOANSYSTEMTYPEID == loanSystemTypeId);
            if (camsol != null) amount = camsol.WRITTENOFFACCRUALAMOUNT;
            return amount;
        }

        public decimal GetMaximumApplicationOutstandingBalance(int applicationId)
        {
            decimal amount = 0;
            var appl = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => 
                x.LOANAPPLICATIONID == applicationId &&
                x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved &&
                x.DELETED != true
                )
                .OrderByDescending(x => x.APPROVEDAMOUNT)
                .FirstOrDefault();
            if (appl != null) amount = appl.APPROVEDAMOUNT;
            return amount;
        }

        public ContingentLoansViewModel GetContingentTotoalUsed(int contingetLoanId)
        {
            ContingentLoansViewModel data = new ContingentLoansViewModel();
            var appl = (from a in context.TBL_LOAN_CONTINGENT
                        where a.CONTINGENTLOANID == contingetLoanId
                        select new ContingentLoansViewModel
                        {
                            usedAmount = context.TBL_LOAN_CONTINGENT_USAGE.Where(x => x.CONTINGENTLOANID == a.CONTINGENTLOANID && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Select(x => x.AMOUNTREQUESTED).FirstOrDefault(),
                            facilityAmount = a.CONTINGENTAMOUNT
                        }).ToList();

            if (appl.Count > 0)
            {
                data.usedAmount = appl.Sum(x => x.usedAmount);
                data.facilityAmount = appl.FirstOrDefault().facilityAmount;
            }
            return data;
        }
    }
}
