using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class ConditionPrecedentRepository : IConditionPrecedentRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public ConditionPrecedentRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddConditionPrecedent(ConditionPrecedentViewModel model)
        {
            var data = new TBL_LOAN_CONDITION_PRECEDENT
            {
                CONDITION = model.condition,
                ISEXTERNAL = (bool)model.isExternal,
                CREATEDBY = model.createdBy,
                LOANAPPLICATIONID = model.loanApplicationId,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_LOAN_CONDITION_PRECEDENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Condition Precedent '{ model.conditionId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateConditionPrecedent(ConditionPrecedentViewModel model, int conditionPrecedentId)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(conditionPrecedentId);
            if (data == null)
            {
                return false;
            }

            data.CONDITION = model.condition;
            data.ISEXTERNAL = (bool)model.isExternal;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Condition Precedent '{ model.conditionId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedent()
        {
            return this.context.TBL_LOAN_CONDITION_PRECEDENT
                .Join(
                    context.TBL_STAFF,
                    c => c.CREATEDBY,
                    s => s.STAFFID,
                    (c, s) => new ConditionPrecedentViewModel
                    {
                        conditionId = c.CONDITIONID,
                        condition = c.CONDITION,
                        isExternal = c.ISEXTERNAL,
                        staffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                        loanApplicationId = c.LOANAPPLICATIONID,
                        dateTimeCreated = c.DATETIMECREATED,
                        dateTimeUpdated = c.DATETIMEUPDATED,
                    });
        }

        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByApplicationId(int applicationId)
        {
            return this.GetAllConditionPrecedent().Where(x => x.loanApplicationId == applicationId);
        }

    }
}
