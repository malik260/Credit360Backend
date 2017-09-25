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
            var data = new tbl_Loan_Condition_Precedent
            {
                Condition = model.condition,
                IsExternal = model.isExternal,
                CreatedBy = model.createdBy,
                LoanApplicationId = model.loanApplicationId,
                DateTimeCreated = general.GetApplicationDate(),
            };

            context.tbl_Loan_Condition_Precedent.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ConditionPrecedentAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Condition Precedent '{ model.conditionId }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateConditionPrecedent(ConditionPrecedentViewModel model, int conditionPrecedentId)
        {
            var data = this.context.tbl_Loan_Condition_Precedent.Find(conditionPrecedentId);
            if (data == null)
            {
                return false;
            }

            data.Condition = model.condition;
            data.IsExternal = model.isExternal;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = DateTime.Now;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Condition Precedent '{ model.conditionId }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedent()
        {
            return this.context.tbl_Loan_Condition_Precedent
                .Join(
                    context.tbl_Staff,
                    c => c.CreatedBy,
                    s => s.StaffId,
                    (c, s) => new ConditionPrecedentViewModel
                    {
                        conditionId = c.ConditionId,
                        condition = c.Condition,
                        isExternal = c.IsExternal,
                        staffName = s.FirstName + " " + s.MiddleName + " " + s.LastName,
                        loanApplicationId = c.LoanApplicationId,
                        dateTimeCreated = c.DateTimeCreated,
                        dateTimeUpdated = c.DateTimeUpdated,
                    });
        }

        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByApplicationId(int applicationId)
        {
            return this.GetAllConditionPrecedent().Where(x => x.loanApplicationId == applicationId);
        }

    }
}
