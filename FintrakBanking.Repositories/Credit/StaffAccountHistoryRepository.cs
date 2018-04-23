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

        public StaffAccountHistoryRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository genSetup,
            IAuditTrailRepository auditTrail,
            IGeneralSetupRepository generalSetup,
            IWorkflow workflow)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
            this.generalSetup = generalSetup;
        }

        public bool AddStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
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
            workflow.TargetId = entity.targetId;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "Initiation: " + entity.reasonForChange;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();
            // ----------------Drop into CAM ends-------------------

            return context.SaveChanges() > 0;


        }



        public bool ApproveStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StaffAccountHistoryViewModel> GetStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(entity.staffId, (int)OperationsEnum.TermLoanBooking).ToList();

            
                var data = (from ln in context.TBL_STAFF_ACCOUNT_HISTORY

                            join atrail in context.TBL_APPROVAL_TRAIL on ln.STAFFACCOUNTHISTORYID equals atrail.TARGETID
                            where (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                   && atrail.OPERATIONID == (int)OperationsEnum.TermLoanBooking
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)// == staffApprovalLevelId
                                  && atrail.RESPONSESTAFFID == null
                            orderby ln.STAFFACCOUNTHISTORYID descending
                            select new StaffAccountHistoryViewModel
                            {
                                reasonForChange = ln.REASONFORCHANGE,
                                startDate = ln.STARTDATE,
                                endDate = ln.STARTDATE,
                                currentRMStaffName = GetStaff(ln.STAFFID),
                                newRMStaffName = GetStaff(ln.NEWSTAFFID)

                            });
            return data.ToList();
        }

            
        private string  GetStaff(int staffId)
        {
            return context.TBL_STAFF.Where(s => s.STAFFID == staffId).Select(s => new { staffName = s.LASTNAME + " " + s.FIRSTNAME + " " + s.MIDDLENAME }).FirstOrDefault().staffName;
        }

        public bool UpdateStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
