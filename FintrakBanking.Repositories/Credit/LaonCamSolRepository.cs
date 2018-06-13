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

        public LaonCamSolRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup, IAuditTrailRepository _auditTrail, IWorkflow _workflow)
        {
            context = _context;
            generalSetup = _generalSetup;
            auditTrail = _auditTrail;
            workflow = _workflow;
        }

        public List<LoanCAMSOLViewModel> GetCamSol()//string customerName)
        {

            var data1 = from camsol in context.TBL_LOAN_CAMSOL
                        //join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       // join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                        //join l in context.TBL_LOAN_REVOLVING on camsol.LOANID equals l.REVOLVINGLOANID
                        //where camsol.LOANID == l.REVOLVINGLOANID
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
                            //loansystemtype = a.LOANSYSTEMTYPENAME,
                            //camsolType = c.CAMSOLTYPENAME,
                            interestinsuspense = camsol.INTERESTINSUSPENSE,
                            loancamsolid = camsol.LOAN_CAMSOLID,
                            loanid = camsol.LOANID,
                            principal = camsol.PRINCIPAL,
                            remark = camsol.REMARK,


                        };

            var data = from camsol in context.TBL_LOAN_CAMSOL
                      // join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                      // join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       //join l in context.TBL_LOAN on camsol.LOANID equals l.TERMLOANID
                      // where camsol.LOANID == l.TERMLOANID

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
                          // loansystemtype = a.LOANSYSTEMTYPENAME,
                          // camsolType = c.CAMSOLTYPENAME,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,


                       };
            return data.Union(data1).ToList();
        }

        public LoanCAMSOLViewModel GetCamSol(string loancamsolid)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       where camsol.ACCOUNTNAME == loancamsolid || camsol.ACCOUNTNAME.StartsWith(loancamsolid)
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

        public List<LoanCAMSOLViewModel> GetCamSolByType(int camsolTyepId)
        {
            var data1 = from camsol in context.TBL_LOAN_CAMSOL
                            //join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                            // join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                            //join l in context.TBL_LOAN_REVOLVING on camsol.LOANID equals l.REVOLVINGLOANID
                            where camsol.CAMSOLTYPEID  == camsolTyepId
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
                            //loansystemtype = a.LOANSYSTEMTYPENAME,
                            //camsolType = c.CAMSOLTYPENAME,
                            interestinsuspense = camsol.INTERESTINSUSPENSE,
                            loancamsolid = camsol.LOAN_CAMSOLID,
                            loanid = camsol.LOANID,
                            principal = camsol.PRINCIPAL,
                            remark = camsol.REMARK,


                        };

            var data = from camsol in context.TBL_LOAN_CAMSOL
                           // join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                           // join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                           //join l in context.TBL_LOAN on camsol.LOANID equals l.TERMLOANID
                       where camsol.CAMSOLTYPEID == camsolTyepId

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
                           // loansystemtype = a.LOANSYSTEMTYPENAME,
                           // camsolType = c.CAMSOLTYPENAME,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,


                       };
            return data.Union(data1).ToList();
        }

        public List<LoanCAMSOLViewModel> GetCamSolType()
        {
          var camsolType = (from x in context.TBL_LOAN_CAMSOL_TYPE
                           select new LoanCAMSOLViewModel {
                                camsoltypeid = x.CAMSOLTYPEID,
                                camsolType = x.CAMSOLTYPENAME
            }).ToList();
            return camsolType;
        }
    }
}
