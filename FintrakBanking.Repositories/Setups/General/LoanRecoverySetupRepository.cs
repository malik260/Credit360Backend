using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Validation;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    public class LoanRecoverySetupRepository : ILoanRecoverySetupRepository
    {
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private FinTrakBankingContext context;

        public LoanRecoverySetupRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup, 
            FinTrakBankingContext _context
            )
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this._genSetup = genSetup;
        }

        private bool SaveAll()
        {
            try
            {
                return this.context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
        }

        public bool AddLoanRecoverySetup(LoanRecoverySetupViewModel entity)
        {
            int loanId = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == entity.loanId).TERMLOANID;
            try
            {
                var LoanRecoverySetup = new TBL_LOAN_RECOVERY_PLAN
                {
                
                    LOANID = loanId,
                    PRODUCTTYPEID = entity.productTypeId,
                    CASAACCOUNTID = entity.casaAccountId,
                    AGENTID = entity.agentId,
                    AMOUNTOWED = entity.amountOwed,
                    WRITEOFFAMOUNT = entity.writeOffAmount,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    LASTUPDATEDBY = entity.createdBy,
                    DELETED = false,
                };

                this.context.TBL_LOAN_RECOVERY_PLAN.Add(LoanRecoverySetup);
                context.SaveChanges();

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanRecoverySetupAdded,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = "Added new tbl_LoanRecoverySetup ",
                    IPADDRESS = entity.userIPAddress,
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
                return SaveAll();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
           
        public IEnumerable<LoanRecoverySetupViewModel> GetAllLoanRecoverySetup()
        {
            var LoanRecoverySetup = (from d in context.TBL_LOAN_RECOVERY_PLAN
                                     select new LoanRecoverySetupViewModel()
                              {
                                  recoveryPlanId = d.RECOVERYPLANID,
                                  loanId = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  productTypeId = d.PRODUCTTYPEID,
                                  productTypeName = d.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                  casaAccountId = d.CASAACCOUNTID,
                                  casaAccountName = d.TBL_CASA.PRODUCTACCOUNTNAME,
                                  agentId = (int)d.AGENTID,
                                  agentName = context.TBL_ACCREDITEDCONSULTANT.FirstOrDefault(x => x.ACCREDITEDCONSULTANTID == (int)d.AGENTID).NAME,
                                  amountOwed = (decimal)d.AMOUNTOWED,
                                  writeOffAmount = d.WRITEOFFAMOUNT,

                                   }).ToList();
            return LoanRecoverySetup;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllCasa() 
        {
            var LoanRecoverySetup = (from d in context.TBL_CASA
                                   select new LoanRecoverySetupViewModel()
                                   {
                                       casaAccountId = d.CASAACCOUNTID,
                                       casaAccountName = d.PRODUCTACCOUNTNAME,

                                   }).ToList();
            return LoanRecoverySetup;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllAgent()
        {
            var LoanRecoverySetup = (from d in context.TBL_ACCREDITEDCONSULTANT
                                     select new LoanRecoverySetupViewModel()
                                     {
                                         agentId = d.ACCREDITEDCONSULTANTID,
                                         agentName = d.NAME + d.FIRMNAME,

                                     }).ToList();
            return LoanRecoverySetup;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllProductType()
        {
            var LoanRecoverySetup = (from d in context.TBL_PRODUCT_TYPE 
                                   select new LoanRecoverySetupViewModel()
                                   {
                                       productTypeId = d.PRODUCTTYPEID,
                                       productTypeName = d.PRODUCTTYPENAME,

                                   }).ToList();
            return LoanRecoverySetup;
        }

        public LoanRecoverySetupViewModel GetLoanRecoverySetup(int recoveryPlanId )
        {
            var LoanRecoverySetup = (from d in context.TBL_LOAN_RECOVERY_PLAN
                                     where d.RECOVERYPLANID == recoveryPlanId
                                     select new LoanRecoverySetupViewModel()
                              {
                                  recoveryPlanId = d.RECOVERYPLANID,
                                  loanId = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  productTypeId = d.PRODUCTTYPEID,
                                  casaAccountId = d.CASAACCOUNTID,
                                  agentId = (int)d.AGENTID,
                                  agentName = context.TBL_ACCREDITEDCONSULTANT.FirstOrDefault(x => x.ACCREDITEDCONSULTANTID == (int)d.AGENTID).NAME,
                                  amountOwed = (decimal)d.AMOUNTOWED,
                                  writeOffAmount = d.WRITEOFFAMOUNT,
                              }).SingleOrDefault();
            return LoanRecoverySetup;
        }

        public bool UpdateLoanRecoverySetup(int LoanRecoverySetupId, LoanRecoverySetupViewModel entity)
        {
            var LoanRecoverySetup = context.TBL_LOAN_RECOVERY_PLAN.Find(LoanRecoverySetupId);
            int loanId = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == entity.loanId).TERMLOANID;

            LoanRecoverySetup.LOANID = loanId;
            LoanRecoverySetup.PRODUCTTYPEID = entity.productTypeId;
            LoanRecoverySetup.CASAACCOUNTID = entity.casaAccountId;
            LoanRecoverySetup.AGENTID = entity.agentId;
            LoanRecoverySetup.AMOUNTOWED = entity.amountOwed;
            LoanRecoverySetup.WRITEOFFAMOUNT = entity.writeOffAmount;
            LoanRecoverySetup.CREATEDBY = entity.createdBy;
            LoanRecoverySetup.DATETIMECREATED = DateTime.Now;
            LoanRecoverySetup.LASTUPDATEDBY = entity.createdBy;
            LoanRecoverySetup.DELETED = false;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanRecoverySetupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_LoanRecoverySetup with Id: {entity.recoveryPlanId} ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }
    }
}