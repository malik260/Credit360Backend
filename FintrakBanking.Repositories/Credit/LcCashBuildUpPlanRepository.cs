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

namespace FintrakBanking.Repositories.credit
{
    public class LcCashBuildUpPlanRepository : ILcCashBuildUpPlanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public LcCashBuildUpPlanRepository(
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

        public IEnumerable<LcCashBuildUpPlanViewModel> GetLcCashBuildUpPlansByLcIssuanceId(int id)
        {
            return context.TBL_LC_CASHBUILDUPPLAN.Where(x => x.DELETED == false && x.LCISSUANCEID == id)
                .Select(x => new LcCashBuildUpPlanViewModel
                {
                    lcCashBuildUpPlanId = x.LCCASHBUILDUPPLANID,
                    cashBuildUpReferenceTypeName = context.TBL_CASHBUILDUPREFERENCETYPE.FirstOrDefault(b => b.CASHBUILDUPREFERENCETYPEID == x.CASHBUILDUPREFERENCETYPEID).NAME,
                    lcIssuanceId = x.LCISSUANCEID,
                    lcReferenceNumber = context.TBL_LC_ISSUANCE.FirstOrDefault(l => l.LCISSUANCEID == x.LCISSUANCEID).LCREFERENCENUMBER,
                    amount = x.AMOUNT,
                    currencyId = x.CURRENCYID,
                    cashBuildUpReferenceTypeId = x.CASHBUILDUPREFERENCETYPEID,
                    collectionCasaAccountId = x.COLLECTIONCASAACCOUNTID,
                    daysInterval = x.DAYSINTERVAL,
                    planDate = x.PLANDATE,
                })
                .ToList();
        }

        public IEnumerable<LcCashBuildUpPlanViewModel> GetLcCashBuildUpReferenceTypes()
        {
            var cashBuildUp = context.TBL_CASHBUILDUPREFERENCETYPE.ToList();
            return context.TBL_CASHBUILDUPREFERENCETYPE
                .Select(x => new LcCashBuildUpPlanViewModel
                {
                    cashBuildUpReferenceTypeName = x.NAME,
                    cashBuildUpReferenceTypeId = x.CASHBUILDUPREFERENCETYPEID,
                }).ToList();
        }

        public LcCashBuildUpPlanViewModel GetLcCashBuildUpPlan(int id)
        {
            var entity = context.TBL_LC_CASHBUILDUPPLAN.FirstOrDefault(x => x.LCCASHBUILDUPPLANID == id && x.DELETED == false);
            return new LcCashBuildUpPlanViewModel
            {
                lcCashBuildUpPlanId = entity.LCCASHBUILDUPPLANID,
                lcIssuanceId = entity.LCISSUANCEID,
                amount = entity.AMOUNT,
                currencyId = entity.CURRENCYID,
                cashBuildUpReferenceTypeId = entity.CASHBUILDUPREFERENCETYPEID,
                collectionCasaAccountId = entity.COLLECTIONCASAACCOUNTID,
                daysInterval = entity.DAYSINTERVAL,
                planDate = entity.PLANDATE,
            };
        }

        public bool AddLcCashBuildUpPlan(LcCashBuildUpPlanViewModel model)
        {
            var entity = new TBL_LC_CASHBUILDUPPLAN
            {
                LCISSUANCEID = model.lcIssuanceId,
                AMOUNT = model.amount,
                CURRENCYID = model.currencyId,
                CASHBUILDUPREFERENCETYPEID = model.cashBuildUpReferenceTypeId,
                COLLECTIONCASAACCOUNTID = model.collectionCasaAccountId,
                DAYSINTERVAL = model.daysInterval,
                PLANDATE = model.planDate,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            context.TBL_LC_CASHBUILDUPPLAN.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcCashBuildUpPlanAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc CashBuildUpPlan '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcCashBuildUpPlan(LcCashBuildUpPlanViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_CASHBUILDUPPLAN.Find(id);
            entity.LCISSUANCEID = model.lcIssuanceId;
            entity.AMOUNT = model.amount;
            entity.CURRENCYID = model.currencyId;
            entity.CASHBUILDUPREFERENCETYPEID = model.cashBuildUpReferenceTypeId;
            entity.COLLECTIONCASAACCOUNTID = model.collectionCasaAccountId;
            entity.DAYSINTERVAL = model.daysInterval;
            //entity.PLANDATE = model.planDate;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcCashBuildUpPlanUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc CashBuildUpPlan '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCCASHBUILDUPPLANID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcCashBuildUpPlan(int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_CASHBUILDUPPLAN.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcCashBuildUpPlanDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc CashBuildUpPlan '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCCASHBUILDUPPLANID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}