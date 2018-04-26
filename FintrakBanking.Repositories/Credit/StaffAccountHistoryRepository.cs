using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Entities.Models;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.WorkFlow;
using System.Data.Entity.Validation;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Repositories.Credit
{
    public class StaffAccountHistoryRepository : IStaffAccountHistoryRepository
    {
        private IWorkflow workflow;
        private IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository generalSetup;
        private ILoanOperationsRepository loanOp;

        public StaffAccountHistoryRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository genSetup,
            IAuditTrailRepository auditTrail,
            IGeneralSetupRepository generalSetup,
            ILoanOperationsRepository loanOp,
            IWorkflow workflow)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
            this.generalSetup = generalSetup;
            this.loanOp = loanOp;
        }

        public bool AddStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
            var checkStartDate = context.TBL_STAFF_ACCOUNT_HISTORY.Where(c => c.NEWSTAFFID == entity.currentRMStaffId
            && c.PRODUCTTYPEID == entity.productTypeId
            && c.TARGETID == entity.targetId);
            if (checkStartDate.Any())
            {
                entity.startDate = checkStartDate.FirstOrDefault().ENDDATE;
            }
            else
            {
                var getLoan = GetloanDetails(entity.targetId, entity.staffId, entity.productTypeId);
                entity.startDate = getLoan.effectiveDate;
            }

            var data = new TBL_STAFF_ACCOUNT_HISTORY
            {
                TARGETID = entity.targetId,
                STAFFID = entity.currentRMStaffId,
                NEWSTAFFID = entity.newRMStaffId,
                REASONFORCHANGE = entity.reasonForChange,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                ENDDATE = entity.endDate,
                STARTDATE = entity.startDate,
                PRODUCTTYPEID = entity.productTypeId

            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.InitiatAccountReassigning,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Initial account reassigned",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.STAFFACCOUNTHISTORYID
            };


            bool output = false;
            context.TBL_STAFF_ACCOUNT_HISTORY.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            try
            {
                output = context.SaveChanges() > 0;
                //  response = context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {

                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }

            // ----------------Drop into CAM-------------------
            workflow.StaffId = entity.staffId;
            workflow.OperationId = (int)OperationsEnum.ReassigningOfAccount;
            workflow.TargetId = data.STAFFACCOUNTHISTORYID;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "Initiation: " + entity.reasonForChange;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();
            // ----------------Drop into CAM ends-------------------

            return context.SaveChanges() > 0;


        }

        private loanDetailsViewModel GetloanDetails(int loanId, int staffId, int productTypeId)
        {
            loanDetailsViewModel loanDetails = null;
            switch (productTypeId)
            {
                case ((int)LoanProductTypeEnum.TermLoan):
                    loanDetails = context.TBL_LOAN.Where(l => l.RELATIONSHIPOFFICERID == staffId && l.TERMLOANID == loanId).Select(l => new loanDetailsViewModel
                    {
                        loanId = l.TERMLOANID,
                        relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                        effectiveDate = l.EFFECTIVEDATE,
                        productTypeId = productTypeId
                    }).FirstOrDefault(); break;
                case ((int)LoanProductTypeEnum.RevolvingLoan):
                    loanDetails = context.TBL_LOAN_REVOLVING.Where(r => r.RELATIONSHIPMANAGERID == staffId && r.REVOLVINGLOANID == loanId).Select(r => new loanDetailsViewModel
                    {
                        loanId = r.REVOLVINGLOANID,
                        relationshipOfficerId = r.RELATIONSHIPOFFICERID,
                        effectiveDate = r.EFFECTIVEDATE,
                        productTypeId = productTypeId
                    }).FirstOrDefault(); break;
                case ((int)LoanProductTypeEnum.ContingentLiability):
                    loanDetails = context.TBL_LOAN_CONTINGENT.Where(c => c.RELATIONSHIPMANAGERID == staffId && c.CONTINGENTLOANID == loanId).Select(c => new loanDetailsViewModel
                    {
                        loanId = c.CONTINGENTLOANID,
                        relationshipOfficerId = c.RELATIONSHIPOFFICERID,
                        effectiveDate = c.EFFECTIVEDATE,
                        productTypeId = productTypeId
                    }).FirstOrDefault(); break;
            }
            return loanDetails;
        }

        public IEnumerable<StaffAccountHistoryViewModel> GetStaffAccountHistory(int staffId)
        {
            try
            {
                List<StaffAccountHistoryViewModel> accountLst = new List<StaffAccountHistoryViewModel>();
                var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ReassigningOfAccount).ToList();


                var data = (from ln in context.TBL_STAFF_ACCOUNT_HISTORY

                            join atrail in context.TBL_APPROVAL_TRAIL on ln.STAFFACCOUNTHISTORYID equals atrail.TARGETID
                            where
                            (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                   && atrail.OPERATIONID == (int)OperationsEnum.ReassigningOfAccount
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)// == staffApprovalLevelId
                                  && atrail.RESPONSESTAFFID == null
                            orderby ln.STAFFACCOUNTHISTORYID descending
                            select new StaffAccountHistoryViewModel
                            {
                                reasonForChange = ln.REASONFORCHANGE,
                                startDate = ln.STARTDATE,
                                endDate = ln.ENDDATE,
                                newRMStaffId = ln.NEWSTAFFID,
                                currentRMStaffId = ln.STAFFID

                            }).ToList();


                foreach (var d in data)
                {
                    d.newRMStaffName = GetStaff(d.newRMStaffId);
                    d.currentRMStaffName = GetStaff(d.currentRMStaffId);

                    accountLst.Add(d);
                }
                return accountLst;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        
        private string GetStaff(int staffId)
        {
            return context.TBL_STAFF.Where(s => s.STAFFID == staffId).Select(s => new { staffName = s.LASTNAME + " " + s.FIRSTNAME + " " + s.MIDDLENAME }).FirstOrDefault().staffName;
        }

        public bool ApproveStaffAccountHistory(ApprovalViewModel  entity)
        {
            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
            workflow.TargetId = entity.targetId;
            workflow.Comment = entity.comment;
            workflow.OperationId = entity.operationId;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = false;
            workflow.LogActivity();

            context.SaveChanges();

            if(workflow.NewState == (int)ApprovalState.Ended)
            {

            }




            //switch (entity.productTypeId)
            //{
            //    case ((int)LoanProductTypeEnum.TermLoan): TeamLoan(entity); break;

            //    case ((int)LoanProductTypeEnum.RevolvingLoan): RevolvingLoan(entity); break;
            //}

            return true;
        }


        private void TeamLoan(StaffAccountHistoryViewModel entity )
        {
            loanOp.ArchiveLoan(entity.targetId, (int)OperationsEnum.ReassigningOfAccount);
        }
        private void RevolvingLoan(StaffAccountHistoryViewModel entity)
        {
            loanOp.ArchiveLoan(entity.targetId, (int)OperationsEnum.ReassigningOfAccount);
        }
        private void ContingentLiability(StaffAccountHistoryViewModel entity)
        {

        }

      
    }
    internal class loanDetailsViewModel
    {
        public int loanId { get; set; }
        public int relationshipOfficerId { get; set; }
        public DateTime effectiveDate { get; set; }
        public int productTypeId { get; set; }
    }
}
