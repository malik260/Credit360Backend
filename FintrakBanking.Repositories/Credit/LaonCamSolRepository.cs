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

        public List<LoanCAMSOLViewModel> GetCamSol()
        {

            var data = (from camsol in context.TBL_LOAN_CAMSOL
                        join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                        join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
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
                            camsolType = c.CAMSOLTYPENAME,
                            loansystemtype = a.LOANSYSTEMTYPENAME,
                            interestinsuspense = camsol.INTERESTINSUSPENSE,
                            loancamsolid = camsol.LOAN_CAMSOLID,
                            loanid = camsol.LOANID,
                            principal = camsol.PRINCIPAL,
                            remark = camsol.REMARK,
                        });

           
            return data.ToList();
        }

        public List<LoanCAMSOLViewModel> GetCamSol(string loancamsolid)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       where camsol.CUSTOMERNAME.StartsWith(loancamsolid.ToUpper()) || camsol.CUSTOMERCODE ==loancamsolid
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           camsolType = c.CAMSOLTYPENAME,
                           loansystemtype = a.LOANSYSTEMTYPENAME,
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,

                       };
            return data.ToList();
        }
        public List<LoanCAMSOLViewModel> GetCamSolByCustomerCode(string customerCode)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       where camsol.CUSTOMERCODE == customerCode
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
            return data.ToList();
        }
        public List<LoanCAMSOLViewModel> GetCamSolByType(int camsolTyepId)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                        join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                        join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
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
                            loansystemtype = a.LOANSYSTEMTYPENAME,
                            camsolType = c.CAMSOLTYPENAME,
                            interestinsuspense = camsol.INTERESTINSUSPENSE,
                            loancamsolid = camsol.LOAN_CAMSOLID,
                            loanid = camsol.LOANID,
                            principal = camsol.PRINCIPAL,
                            remark = camsol.REMARK,


                        };

            
            return data.ToList();
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

        public LoanCAMSOLViewModel ViewCamSolByType(int id)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       where camsol.LOAN_CAMSOLID == id 
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           loansystemtype = a.LOANSYSTEMTYPENAME,
                           camsolType = c.CAMSOLTYPENAME,
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,

                       };
            return data.FirstOrDefault();
        }
        public bool ApproveCamsol(LoanCAMSOLViewModel option)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       where camsol.CUSTOMERCODE == option.customercode
                       select camsol;
            if (data!=null)
            {
                foreach (var x in data)
                {
                    x.CANTAKELOAN = option.updateOption;
                }
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
