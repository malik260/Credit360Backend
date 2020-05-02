using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.credit
{
    public class LcConditionRepository : ILcConditionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public LcConditionRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }

        public IEnumerable<LcConditionViewModel> GetLcConditions()
        {
            return context.TBL_LC_CONDITION.Where(x => x.DELETED == false)
                .Select(x => new LcConditionViewModel
                {
                    lcConditionId = x.LCCONDITIONID,
                    lcIssuanceId = x.LCISSUANCEID,
                    condition = x.CONDITION,
                    isSatisfied = x.ISSATISFIED,
                    isTransactionDynamics = x.ISTRANSACTIONDYNAMICS
                })
                .ToList();
        }

        public IEnumerable<LcConditionViewModel> GetLcConditionsBylcIssuanceId(int lcIssuanceId)
        {
            return context.TBL_LC_CONDITION.Where(x => x.LCISSUANCEID == lcIssuanceId && x.DELETED == false)
                .Select(x => new LcConditionViewModel
                {
                    lcConditionId = x.LCCONDITIONID,
                    lcIssuanceId = x.LCISSUANCEID,
                    condition = x.CONDITION,
                    isSatisfied = x.ISSATISFIED,
                    isTransactionDynamics = x.ISTRANSACTIONDYNAMICS
                })
                .ToList();
        }

        public LcConditionViewModel GetLcCondition(int id)
        {
            var entity = context.TBL_LC_CONDITION.FirstOrDefault(x => x.LCCONDITIONID == id && x.DELETED == false);

            return new LcConditionViewModel
            {
                lcConditionId = entity.LCCONDITIONID,
                lcIssuanceId = entity.LCISSUANCEID,
                condition = entity.CONDITION,
                isSatisfied = entity.ISSATISFIED,
                isTransactionDynamics = entity.ISTRANSACTIONDYNAMICS
            };
        }

        public bool AddLcCondition(LcConditionViewModel model)
        {
            var entity = new TBL_LC_CONDITION
            {
                LCISSUANCEID = model.lcIssuanceId,
                CONDITION = model.condition,
                ISSATISFIED = model.isSatisfied,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                ISTRANSACTIONDYNAMICS = model.isTransactionDynamics,
            };

            context.TBL_LC_CONDITION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcConditionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc Condition '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),  
                APPLICATIONDATE = general.GetApplicationDate(),
                 URL = model.applicationUrl,      
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcCondition(LcConditionViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_CONDITION.Find(id);
            entity.LCISSUANCEID = model.lcIssuanceId;
            entity.CONDITION = model.condition;
            entity.ISSATISFIED = model.isSatisfied;
            entity.ISTRANSACTIONDYNAMICS = model.isTransactionDynamics;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcConditionUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Condition '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),            
                TARGETID = entity.LCCONDITIONID,
                 URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()

            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcCondition(int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_CONDITION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcConditionDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Condition '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCCONDITIONID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
               
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<ILcConditionRepository>().To<LcConditionRepository>();
           // LcConditionAdded = ???, LcConditionUpdated = ???, LcConditionDeleted = ???,
