using FintrakBanking.Common.Enum;
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
        private IGeneralSetupRepository genSetup;

        public LaonCamSolRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup, IAuditTrailRepository _auditTrail, IWorkflow _workflow,
            IGeneralSetupRepository _genSetup )
        {
            context = _context;
            generalSetup = _generalSetup;
            auditTrail = _auditTrail;
            workflow = _workflow;
            genSetup = _genSetup;
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
        public List<LoanCAMSOLViewModel> CamSolAwaitingApproval(int companyId, int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CamsolBackbookModification).ToList();

            var data = from camsol in context.TBL_TEMP_LOAN_CAMSOL
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       join atrail in context.TBL_APPROVAL_TRAIL on camsol.TEMPLOAN_CAMSOLID equals atrail.TARGETID
                       where 
                                      atrail.RESPONSESTAFFID == null
                                     && atrail.OPERATIONID == (int)OperationsEnum.CamsolBackbookModification
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                       orderby camsol.TEMPLOAN_CAMSOLID descending
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
                           //date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                           tempLoancamsolid = (short)camsol.TEMPLOAN_CAMSOLID

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

        public LoanCAMSOLViewModel CamSolAwaitingApprovalById(int id)
        {
            var data = from camsol in context.TBL_TEMP_LOAN_CAMSOL
                       join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       where camsol.TEMPLOAN_CAMSOLID == id 
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
                          // date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                           tempLoancamsolid=(short)camsol.TEMPLOAN_CAMSOLID

                       };
            return data.FirstOrDefault();
        }

        public string goForApproval(LoanCAMSOLViewModel data)
        {
            bool response = false;
            bool action = false;
            using (var transaction = context.Database.BeginTransaction())
            {
            
                workflow.StaffId = data.createdBy;
                workflow.CompanyId = data.companyId;
                workflow.StatusId = (short)data.approvalStatusId;
                workflow.TargetId = data.tempLoancamsolid;
                workflow.Comment = data.comment;
                workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (data.approvalStatusId != (int)ApprovalStatusEnum.Disapproved)
                        {
                            action = finalCamsolApproval(data, (short)workflow.StatusId);
                        }
                    }
                    if (context.SaveChanges()>0)
                    {
                        transaction.Commit();
                        if (action==true)
                        {
                            return "Consession has been granted to access loan";
                        }
                        return "This customer has been blacklisted successfully";
                    }
                    return "Could not perform any action";
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
      
            }
     
        }
        public string ApproveCamsol(LoanCAMSOLViewModel option)
        {
           
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       where camsol.CUSTOMERCODE == option.customercode
                       select camsol;
            if (data != null)
            {
                string listOfExistingCamsols = string.Empty;
                foreach (var x in data)
                {
                    var iSCamsolExit = context.TBL_TEMP_LOAN_CAMSOL.Any(a => a.LOAN_CAMSOLID == x.LOAN_CAMSOLID);

                    bool cantakeLoanStatus = false;

                    if (x.CANTAKELOAN==true)
                    {
                        cantakeLoanStatus = false;
                    }
                    else { cantakeLoanStatus = true; }
                    if (!iSCamsolExit)
                    {
                        var temp = new TBL_TEMP_LOAN_CAMSOL
                        {
                            ACCOUNTNAME = x.ACCOUNTNAME,
                            ACCOUNTNUMBER = x.ACCOUNTNUMBER,
                            APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                            BALANCE = x.BALANCE,
                            CREATEDBY = x.CREATEDBY,
                            CAMSOLTYPEID = x.CAMSOLTYPEID,
                            CANTAKELOAN = cantakeLoanStatus,
                            COMPANYID = x.COMPANYID,
                            CUSTOMERCODE = x.CUSTOMERCODE,
                            CUSTOMERNAME = x.CUSTOMERNAME,
                            DATE = x.DATE,
                            DATETIMECREATED = DateTime.Now,
                            INTERESTINSUSPENSE = x.INTERESTINSUSPENSE,
                            ISCURRENT = true,
              
                            LOANID = x.LOANID,
                            LOANSYSTEMTYPEID = x.LOANSYSTEMTYPEID,
                            LOAN_CAMSOLID = x.LOAN_CAMSOLID,
                            PRINCIPAL = x.PRINCIPAL,
                            REMARK = x.REMARK,
                        };

                        context.TBL_TEMP_LOAN_CAMSOL.Add(temp);
                        context.SaveChanges();
                      
                        workflow.StaffId = x.CREATEDBY;
                        workflow.CompanyId = x.COMPANYID;
                        workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                        workflow.TargetId = temp.TEMPLOAN_CAMSOLID;
                        workflow.Comment = "Request for CAMSOL/Blackbook approval";
                        workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;
                        workflow.DeferredExecution = false;
                        workflow.ExternalInitialization = true;
                        workflow.LogActivity();
                    }
                    else
                    {
                        listOfExistingCamsols = listOfExistingCamsols + x.CUSTOMERCODE + " | ";
                    }
                }
            
                if (listOfExistingCamsols != null)
                {
                    return " The following Customer code is currently undergoing approval : " + listOfExistingCamsols;
                }
                else
                    return " CAMSOL approval initiated successfully! ";
            }
            return " Record not found ";
        }

        private bool finalCamsolApproval(LoanCAMSOLViewModel data, short StatusId)
        {
            var loanamSolId = (from x in context.TBL_TEMP_LOAN_CAMSOL
                             where x.TEMPLOAN_CAMSOLID == data.tempLoancamsolid
                             select new { x.LOAN_CAMSOLID, x.CANTAKELOAN }).FirstOrDefault();

            if (loanamSolId != null)
            {
                var values = (from camsol in context.TBL_LOAN_CAMSOL
                           where camsol.LOAN_CAMSOLID == loanamSolId.LOAN_CAMSOLID
                              select camsol).FirstOrDefault();
                if (values != null)
                {
                    values.CANTAKELOAN = loanamSolId.CANTAKELOAN;
                   return loanamSolId.CANTAKELOAN;
                }
                return false;
            }
            return false;
        }
    }

    
}
