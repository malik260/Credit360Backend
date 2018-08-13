using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CRMSRegulatories : ICRMSRegulatories
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;

        public CRMSRegulatories(FinTrakBankingContext _context, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        IAuditTrailRepository _audit)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;

        }

        public bool AddCRMSCode(CRMSViewModel code)
        {
          var loan=  context.TBL_LOAN.Where(x => x.TERMLOANID == code.loanApplId).Select(x => x).FirstOrDefault();
            if (loan == null)
                throw new ConditionNotMetException("This loan does not exist");

            loan.CRMSCODE = code.crmsCode;
            loan.CRMSDATE = code.crmsDate;

            if (context.SaveChanges()>0)
            {
                return true;
            }
            return false;

        }


    }
}
