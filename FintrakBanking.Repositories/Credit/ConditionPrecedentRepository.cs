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
                ISSUBSEQUENT = model.isSubsequent,
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

        //public bool UpdateConditionPrecedent(ConditionPrecedentViewModel model, int conditionPrecedentId)
        //{
        //}

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
                        isSubsequent = c.ISSUBSEQUENT,
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

        #region CP Template

        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentTemplate()
        {
            return this.context.TBL_CONDITION_PRECEDENT
            .Select(c => new ConditionPrecedentViewModel
            {
                conditionId = c.CONDITIONID,
                condition = c.CONDITION,
                isExternal = c.ISEXTERNAL,
                isSubsequent = c.ISSUBSEQUENT,
                corporate = c.CORPORATE,
                retail = c.RETAIL,
                productId = c.PRODUCTID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
            });
        }

        public bool AddConditionPrecedentTemplate(ConditionPrecedentViewModel model)
        {
            var data = new TBL_CONDITION_PRECEDENT
            {
                CONDITION = model.condition,
                ISEXTERNAL = model.isExternal,
                ISSUBSEQUENT = model.isSubsequent,
                PRODUCTID = model.productId,
                CORPORATE = model.corporate,
                RETAIL = model.retail,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_CONDITION_PRECEDENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Condition Precedent template '{ model.conditionId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateConditionPrecedentTemplate(ConditionPrecedentViewModel model, int conditionId)
        {
            var data = this.context.TBL_CONDITION_PRECEDENT.Find(conditionId);
            if (data == null)
            {
                return false;
            }

            data.CONDITION = model.condition;
            data.ISEXTERNAL = model.isExternal;
            data.ISSUBSEQUENT = model.isSubsequent;
            data.PRODUCTID = model.productId;
            data.CORPORATE = model.corporate;
            data.RETAIL = model.retail;
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
                DETAIL = $"Updated Condition Precedent template '{ model.conditionId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool RemoveLoanConditionPrecedent(int id, UserInfo model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }
            context.TBL_LOAN_CONDITION_PRECEDENT.Remove(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Remove Condition Precedent '{ data.CONDITION }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditLoanConditionPrecedent(int id, ConditionPrecedentViewModel model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }

            data.CONDITION = model.condition;
            data.ISEXTERNAL = (bool)model.isExternal;
            data.ISSUBSEQUENT = (bool)model.isSubsequent;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

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

        #endregion CP Template

    }
}
