using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Credit
{
    public class RepaymentTermsRepository : IRepaymentTermsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public RepaymentTermsRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail
                                        )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
        }

        public IEnumerable<RepaymentScheduleTermSetupViewModel> GetAllRepaymentTerms()
        {
            var terms = context.TBL_REPAYMENT_TERM.Select(t =>
                new RepaymentScheduleTermSetupViewModel
                {
                    repaymentScheduleId = t.REPAYMENTSCHEDULEID,
                    repaymentScheduleDetail = t.REPAYMENTTERMDETAIL,
                }).ToList();
            // todo code
            return terms;
        }

        public RepaymentScheduleTermSetupViewModel GetRepaymentTerm(int id)
        {
            var term = context.TBL_REPAYMENT_TERM.Find(id);
            if (term != null)
            {
                return new RepaymentScheduleTermSetupViewModel
                {
                    repaymentScheduleId = term.REPAYMENTSCHEDULEID,
                    repaymentScheduleDetail = term.REPAYMENTTERMDETAIL,
                };
            }
            return null;
        }

        public bool AddRepaymentTerm(RepaymentScheduleTermSetupViewModel model)
        {
            context.TBL_REPAYMENT_TERM.Add(
                new TBL_REPAYMENT_TERM
                {
                    REPAYMENTTERMDETAIL = model.repaymentScheduleDetail,
                    DELETED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now
                });

            auditTrail.AddAuditTrail(
                new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.RepaymentTermAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_REPAYMENT_TERM with {model.ToString()} is added",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                });
            return context.SaveChanges() != 0;
        }

        public bool UpdateRepaymentTerm(RepaymentScheduleTermSetupViewModel model)
        {

            var term = context.TBL_REPAYMENT_TERM.Find(model.repaymentScheduleId);
            if (term != null)
            {
                term.REPAYMENTTERMDETAIL = model.repaymentScheduleDetail;
                term.LASTUPDATEDBY = model.createdBy;
                term.DATETIMEUPDATED = DateTime.Now;
            }

            context.Entry(term).State = EntityState.Modified;
            auditTrail.AddAuditTrail(
                new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.RepaymentTermUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_REPAYMENT_TERM with {model.ToString()} is updated",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                });
            return context.SaveChanges() != 0;
        }

        public bool DeleteRepaymentTerm(int id, UserInfo user)
        {
            var term = context.TBL_REPAYMENT_TERM.Find(id);
            if (term != null)
            {
                term.DELETED = true;
                term.DATETIMEDELETED = DateTime.Now;
            }

            context.Entry(term).State = EntityState.Modified;
            auditTrail.AddAuditTrail(
                new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.RepaymentTermDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"TBL_REPAYMENT_TERM with {term.ToString()} is updated",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                });
            return context.SaveChanges() != 0;
        }
    }
}
