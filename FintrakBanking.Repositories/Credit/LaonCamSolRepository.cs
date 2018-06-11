using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class LaonCamSolRepository : ILaonCamSolRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workflow;

        public LaonCamSolRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup, IAuditTrailRepository _auditTrail,IWorkflow _workflow)
        {
            context = _context;
            generalSetup = _generalSetup;
            auditTrail = _auditTrail;
            workflow = _workflow;
        }

        public List<LoanCAMSOLViewModel> GetCamSol(string customerName)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       where camsol.CUSTOMERNAME.StartsWith(customerName)
                       select new LoanCAMSOLViewModel
                       {
                           accountname =camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                           
                       };
            return data.ToList();
        }

        public LoanCAMSOLViewModel GetCamSol(int loancamsolid)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       where camsol.LOAN_CAMSOLID== loancamsolid
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                       };
            return data.FirstOrDefault();
        }
    }
}
