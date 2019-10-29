using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public class DEFERRALWAIVER
    {
        FinTrakBankingContext context = new FinTrakBankingContext();
        private IGeneralSetupRepository _genSetup;
        public IEnumerable<DeferralWaiverViewModel> GetDeferralWaiver(int staffId, int operationId, int targetId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                StringBuilder sb = new StringBuilder();
                
                var data = from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           where a.LOANAPPLICATIONDETAILID == targetId

                           select new DeferralWaiverViewModel
                           {
                               branchName = context.TBL_BRANCH.Where(h => h.BRANCHID == b.BRANCHID).Select(h => h.BRANCHNAME).FirstOrDefault() == null ? "" : context.TBL_BRANCH.Where(h => h.BRANCHID == b.BRANCHID).Select(h => h.BRANCHNAME).FirstOrDefault(),
                               facilityType = context.TBL_PRODUCT.Where(pr => pr.PRODUCTID == b.PRODUCTID).Select(pr => pr.PRODUCTNAME).FirstOrDefault() == null ? "" : context.TBL_PRODUCT.Where(pr => pr.PRODUCTID == b.PRODUCTID).Select(pr => pr.PRODUCTNAME).FirstOrDefault(), //p.PRODUCTNAME,
                               approvedAmount = b.APPROVEDAMOUNT.ToString("#,##.00"),
                               customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               currentDate = DateTime.Now.ToShortDateString(),
                               preparedBy = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                               
                           };

                return data.ToList();

            }

        }

        public IEnumerable<ChecklistApprovalViewModel> GetChecklistAwaitingApproval(int staffId, int operationId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var dataLOS = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                           where c.ISLMS == false
                           && ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && atrail.RESPONSESTAFFID == null
                               && atrail.LOOPEDSTAFFID == null
                           orderby a.DATETIMECREATED descending
                           select new ChecklistApprovalViewModel()
                           {
                               customerName = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               customerId = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID : a.TBL_CUSTOMER.CUSTOMERID,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                               deferredDate = b.DEFEREDDATE,
                               deferralDuration = 1,
                               cummulativeDays = 1,
                               condition = b.CONDITION,
                               conditionId = b.LOANCONDITIONID,
                               loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                               applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               checklistStatus = b.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                               dateCreated = b.DATETIMECREATED,
                               operationId = atrail.OPERATIONID,
                               //Loan Information
                               relationshipOfficerName = a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME,
                               relationshipManagerName = a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME,
                               applicationAmount = a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                               applicationTenor = a.PROPOSEDTENOR,
                               applicationDate = a.TBL_LOAN_APPLICATION.APPLICATIONDATE,
                               isInvestmentGrade = a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                               isPoliticallyExposed = a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                               isRelatedParty = a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                               approvalStatusId = a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                               applicationStatusId = a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                               submittedForAppraisal = a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                               loanInformation = a.LOANPURPOSE,
                               isLMS = c.ISLMS == true,
                               reason = c.DEFERRALREASON
                           }).ToList();

            var dataLMS = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                           join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                           where c.ISLMS == true
                            && ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && atrail.RESPONSESTAFFID == null
                               && atrail.LOOPEDSTAFFID == null
                           orderby a.DATETIMECREATED descending
                           select new ChecklistApprovalViewModel()
                           {
                               customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == b.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                               deferredDate = b.DEFEREDDATE,
                               deferralDuration = 1,
                               cummulativeDays = 1,
                               condition = b.CONDITION,
                               conditionId = b.LOANCONDITIONID,
                               loanApplicationId = a.LOANAPPLICATIONID,
                               applicationReferenceNumber = a.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER,
                               checklistStatus = context.TBL_CHECKLIST_STATUS.Where(o => o.CHECKLISTSTATUSID == b.CHECKLISTSTATUSID).Select(o => o.CHECKLISTSTATUSNAME).FirstOrDefault(),
                               dateCreated = b.DATETIMECREATED,
                               relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.LASTNAME).FirstOrDefault(),
                               relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.LASTNAME).FirstOrDefault(),
                               applicationAmount = 0,//a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                               applicationTenor = 0,//a.PROPOSEDTENOR,
                               applicationDate = a.TBL_LMSR_APPLICATION.APPLICATIONDATE,
                               isInvestmentGrade = false,//a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                               isPoliticallyExposed = false,//a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                               isRelatedParty = false,//a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                               approvalStatusId = 0,//a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                               applicationStatusId = 0,//a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                               submittedForAppraisal = true,//a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                               loanInformation = "",//a.LOANPURPOSE
                               isLMS = c.ISLMS == true
                           }).ToList();


            return dataLOS.Union(dataLMS);
        }

        public List<ApprovalTrailViewModel> GetAwaitingApproval(int operationId, int targetId)
        {
            List<ApprovalTrailViewModel> approvalTrailViewModels = new List<ApprovalTrailViewModel>();
            var appId = context.TBL_LOAN_APPLICATION_DETAIL.Find(targetId);
            var precedent = GetConditionPrecedentByApplicationDetailId(appId.LOANAPPLICATIONDETAILID);
            foreach (var pre in precedent)
            {
                approvalTrailViewModels = GetDeferralnAprroval(operationId, pre.loanConditionId);
               
            }
            return approvalTrailViewModels;
        }
        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
            });
        }

        public List<ApprovalTrailViewModel> GetDeferralnAprroval(int operationId, int targetId)
        {

            var allstaff = this.GetAllStaffNames();
            var staffs = context.TBL_STAFF.ToList();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.OPERATIONID == operationId && x.TARGETID == targetId);
            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                vote = x.VOTE,
                targetId = x.TARGETID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            }).ToList();

            return data;
        }


        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByApplicationDetailId(int applicationDetailId)
        {

            var trail = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.LOANAPPLICATIONDETAILID == applicationDetailId);
            var data = trail.Select(x => new ConditionPrecedentViewModel
            {
                loanConditionId = x.LOANCONDITIONID,
                loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                condition = x.CONDITION
            }).ToList();

            return data;
        }


    }
}
