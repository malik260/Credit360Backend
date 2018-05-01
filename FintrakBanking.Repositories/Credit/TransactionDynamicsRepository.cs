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
    public class TransactionDynamicsRepository : ITransactionDynamicsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public TransactionDynamicsRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddTransactionDynamics(TransactionDynamicsViewModel model)
        {
            var data = new TBL_LOAN_TRANSACTION_DYNAMICS
            {
                DYNAMICS = model.dynamics,
                CREATEDBY = model.createdBy,
                //LOANAPPLICATIONID = model.loanApplicationId,
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_LOAN_TRANSACTION_DYNAMICS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Transaction Dynamics '{ model.dynamicsId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditLoanTransactionDynamics(int id, TransactionDynamicsViewModel model)
        {
            var data = this.context.TBL_LOAN_TRANSACTION_DYNAMICS.Find(id);
            if (data == null)
            {
                return false;
            }

            data.DYNAMICS = model.dynamics;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Transaction Dynamics '{ model.dynamicsId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<TransactionDynamicsViewModel> GetAllTransactionDynamics()
        {
            return this.context.TBL_LOAN_TRANSACTION_DYNAMICS
                .Join(
                    context.TBL_STAFF,
                    c => c.CREATEDBY,
                    s => s.STAFFID,
                    (c, s) => new TransactionDynamicsViewModel
                    {
                        loanDynamicsId = c.LOANDYNAMICSID,
                        dynamicsId = c.DYNAMICSID == null ? 0 : (int)c.DYNAMICSID,
                        dynamics = c.DYNAMICS,
                        staffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                        loanApplicationId = c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                        loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                        dateTimeCreated = c.DATETIMECREATED,
                        dateTimeUpdated = c.DATETIMEUPDATED,
                    });
        }

        public IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsByDetailId(int detailId)
        {
            return this.GetAllTransactionDynamics().Where(x => x.loanApplicationDetailId == detailId);
        }

        #region CP Template

        public IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsTemplate()
        {
            return this.context.TBL_TRANSACTION_DYNAMICS
            .Select(c => new TransactionDynamicsViewModel
            {
                dynamicsId = c.DYNAMICSID,
                dynamics = c.DYNAMICS,
                productId = c.PRODUCTID,
                productName = c.TBL_PRODUCT.PRODUCTNAME,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
            });
        }

        public bool AddTransactionDynamicsTemplate(TransactionDynamicsViewModel model)
        {
            var data = new TBL_TRANSACTION_DYNAMICS
            {
                DYNAMICS = model.dynamics,
                PRODUCTID = (short)model.productId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_TRANSACTION_DYNAMICS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Transaction Dynamics template '{ model.dynamicsId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateTransactionDynamicsTemplate(TransactionDynamicsViewModel model, int dynamicsId)
        {
            var data = this.context.TBL_TRANSACTION_DYNAMICS.Find(dynamicsId);
            if (data == null)
            {
                return false;
            }

            data.DYNAMICS = model.dynamics;
            data.PRODUCTID = (short)model.productId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Transaction Dynamics template '{ model.dynamicsId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool RemoveLoanTransactionDynamics(int id, UserInfo model)
        {
            var data = this.context.TBL_LOAN_TRANSACTION_DYNAMICS.Find(id);
            if (data == null)
            {
                return false;
            }
            context.TBL_LOAN_TRANSACTION_DYNAMICS.Remove(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Remove Transaction Dynamics '{ data.DYNAMICS }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByDetailId(int detailId)
        {
            var ids = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONDETAILID == detailId)
                .Select(x => x.APPROVEDPRODUCTID)
                .Distinct();

            var dynamics = this.context.TBL_TRANSACTION_DYNAMICS.Where(x => ids.Contains((short)x.PRODUCTID))
            .Select(c => new TransactionDynamicsViewModel
            {
                dynamicsId = c.DYNAMICSID,
                dynamics = c.DYNAMICS,
                loanApplicationDetailId = c.PRODUCTID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
            });

            return dynamics.ToList();
        }

        public List<TransactionDynamicsViewModel> AddSelectedTransactionDynamics(SelectedIdsViewModel entity)
        {
            var dynamics = context.TBL_TRANSACTION_DYNAMICS.Where(x => entity.selectedIds.Contains(x.DYNAMICSID)).ToList();

            var loandynamics = context.TBL_LOAN_TRANSACTION_DYNAMICS.Where(x =>
                x.DYNAMICSID != null
                && x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == entity.id
            );

            foreach (var c in dynamics)
            {
                if (!loandynamics.Any(x => x.DYNAMICSID == (int)c.DYNAMICSID))
                {
                    var data = new TBL_LOAN_TRANSACTION_DYNAMICS
                    {
                        DYNAMICS = c.DYNAMICS,
                        DYNAMICSID = c.DYNAMICSID,
                        CREATEDBY = c.CREATEDBY,
                        LOANAPPLICATIONDETAILID = entity.detailId,
                        DATETIMECREATED = general.GetApplicationDate(),
                    };
                    context.TBL_LOAN_TRANSACTION_DYNAMICS.Add(data);
                }
            }

            context.TBL_LOAN_TRANSACTION_DYNAMICS.RemoveRange(
                context.TBL_LOAN_TRANSACTION_DYNAMICS.Where(x => x.DYNAMICSID != null && entity.selectedIds.Contains((int)x.DYNAMICSID) == false)
            );
            context.SaveChanges();

            return GetTransactionDynamicsByDetailId(entity.detailId).ToList();
        }

        #endregion CP Template

    }
}
