using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.Repositories.Risk
{
    public class CreditOfficerRiskRepository : ICreditOfficerRiskRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        // private IWorkflow workflow;

        public CreditOfficerRiskRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin
                // IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            // this.workflow = _workflow;
        }

        public IEnumerable<CreditOfficerRiskViewModel> GetCreditOfficerRisks()
        {
            return context.TBL_CREDIT_OFFICER_RISK//.Where(x => x.DELETED == false)
                .Select(x => new CreditOfficerRiskViewModel
                {
                    creditOfficerRiskId = x.CREDITOFFICERRISKID,
                })
                .ToList();
        }

        public CreditOfficerRiskViewModel GetCreditOfficerRisk(int id)
        {
            var entity = context.TBL_CREDIT_OFFICER_RISK;//.FirstOrDefault(x => x.CREDITOFFICERRISKID == id && x.DELETED == false);

            return new CreditOfficerRiskViewModel
            {
                //creditOfficerRiskId = entity.CREDITOFFICERRISKID,
            };
        }

        public bool AddCreditOfficerRisk(CreditOfficerRiskViewModel model)
        {
            var entity = new TBL_CREDIT_OFFICER_RISK
            {
                CREDITOFFICERRISKID = model.creditOfficerRiskId,
                // COMPANYID = model.companyId,
            };

            context.TBL_CREDIT_OFFICER_RISK.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
       
            return context.SaveChanges() != 0;
        }

        public bool UpdateCreditOfficerRisk(CreditOfficerRiskViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_CREDIT_OFFICER_RISK.Find(id);
            entity.CREDITOFFICERRISKID = model.creditOfficerRiskId;
            
            return context.SaveChanges() != 0;
        }

        public bool DeleteCreditOfficerRisk(int id, UserInfo user)
        {
            var entity = this.context.TBL_CREDIT_OFFICER_RISK.Find(id);

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<ICreditOfficerRiskRepository>().To<CreditOfficerRiskRepository>();
           // CreditOfficerRiskAdded = ???, CreditOfficerRiskUpdated = ???, CreditOfficerRiskDeleted = ???,
