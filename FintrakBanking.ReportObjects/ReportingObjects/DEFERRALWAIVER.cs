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
                               facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == a.APPROVEDPRODUCTID).Select(O => O.PRODUCTNAME).FirstOrDefault(),
                               approvedAmount = b.APPROVEDAMOUNT,
                               customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               currentDate = DateTime.Now,
                               preparedBy = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                               
                           };

                return data.ToList();

            }

        }

        public IEnumerable<ChecklistApprovalViewModel> GetChecklistAwaitingApproval(int staffId, int operationId, int loanApplicationDetailId)
        {
            var ids = GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var dataLOS = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                           where c.ISLMS == false
                           //&& ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           orderby a.DATETIMECREATED descending
                           select new ChecklistApprovalViewModel()
                           {
                               customerName = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               customerId = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID : a.TBL_CUSTOMER.CUSTOMERID,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                               deferredDate = c.DEFERREDDATE,
                               dateTimeCreated = c.DATETIMECREATED,
                               condition = b.CONDITION,
                               conditionId = b.LOANCONDITIONID,
                               loanApplicationId = a.LOANAPPLICATIONID,
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
                               isLms = c.ISLMS,
                               reason = c.DEFERRALREASON,
                               approvalTrailId = atrail.APPROVALTRAILID,
                               deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                               dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                           }).ToList();

            foreach (var x in dataLOS)
            {
                x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;
                x.numberOfTimesDeferred = context.TBL_LOAN_CONDITION_DEFERRAL.Where(xx => xx.LOANCONDITIONID == x.loanConditionId).Select(xx => xx.LOANCONDITIONID).Count();
            }

            var dataLMS = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                           join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                           where c.ISLMS == true
                            //&& ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && a.LOANREVIEWAPPLICATIONID == loanApplicationDetailId
                           orderby a.DATETIMECREATED descending
                           select new ChecklistApprovalViewModel()
                           {
                               deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                               dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                               customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == b.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                               deferredDate = c.DEFERREDDATE,
                               dateTimeCreated = c.DATETIMECREATED,
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
                               isLms = c.ISLMS,
                               approvalTrailId = atrail.APPROVALTRAILID,
                           }).ToList();

            foreach (var x in dataLMS)
            {
                x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;
                x.numberOfTimesDeferred = context.TBL_LOAN_CONDITION_DEFERRAL.Where(xx => xx.LOANCONDITIONID == x.loanConditionId).Select(xx => xx.LOANCONDITIONID).Count();
            }


            var data = dataLOS.Union(dataLMS);
            var result = data.GroupBy(r => r.conditionId)
                               .Select(p => p.OrderByDescending(r => r.approvalTrailId).FirstOrDefault()).ToList();
            return result;
        }

        public IEnumerable<int> GetRelievedStaffApprovalLevelIds(int staffId, int operationId)
        {
            var now = DateTime.Now;

            var staffIds = context.TBL_STAFF_RELIEF
                .Where(x => x.DELETED == false
                    && x.RELIEFSTAFFID == staffId
                    && x.STARTDATE <= now
                    && x.ENDDATE >= now
                    && x.ISACTIVE == true
                ).Select(x => x.STAFFID).Distinct();

            var staff = context.TBL_STAFF.Where(x => staffIds.Contains(x.STAFFID));
            var roleids = staff.Select(x => x.STAFFROLEID).ToList();

            var roleLevelIds = context.TBL_APPROVAL_LEVEL
                .Where(x => x.DELETED == false && roleids.Contains((int)x.STAFFROLEID))
                .Select(x => x.APPROVALLEVELID)
                .Distinct();

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.ISACTIVE == true));

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.DELETED == false && staffIds.Contains(x.STAFFID));

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            return staffLevels.Union(roleLevelIds);
        }
        public IEnumerable<int> GetStaffApprovalLevelIds(int staffId, int operationId)
        {
            var relievedLevelids = GetRelievedStaffApprovalLevelIds(staffId, operationId); // for approval delegation

            var staff = context.TBL_STAFF.Find(staffId);

            var roleLevelIds = context.TBL_APPROVAL_LEVEL
                .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                .Select(x => x.APPROVALLEVELID)
                .Distinct().ToList();

            int scope = (int)ProcessViewScopeEnum.Level; // default 1

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.DELETED == false && l.ISACTIVE == true)).ToList();

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId).ToList();

            if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

            if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct().Union(roleLevelIds).Union(relievedLevelids);

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            if (scope == 2)
            {
                var groups = context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && staffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
                return context.TBL_APPROVAL_LEVEL
                    .Where(x => groups.Contains(x.GROUPID))
                    .Select(x => x.APPROVALLEVELID)
                    .Distinct()
                    .Union(roleLevelIds)
                    .Union(relievedLevelids);
            }

            //return staffLevels.Union(roleLevelIds); // without relief code
            return staffLevels.Union(roleLevelIds).Union(relievedLevelids);
        }

        public List<ApprovalTrailViewModel> GetAwaitingApproval(int operationId, int targetId)
        {
            List<ApprovalTrailViewModel> approvalTrailViewModels = new List<ApprovalTrailViewModel>();
            //var appId = context.TBL_LOAN_APPLICATION_DETAIL.Find(targetId);
            var precedent = GetConditionPrecedentByApplicationDetailId(targetId);
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
            //x.FROMAPPROVALLEVELID != null &&
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == targetId);
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
