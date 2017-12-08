using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanPrincipalRepository : ILoanPrincipalRepository
    {
        private readonly FinTrakBankingContext _context;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly IAuditTrailRepository _auditTrail;
     //   private TokenDecryptionHelper token = new TokenDecryptionHelper();



        public LoanPrincipalRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                IAuditTrailRepository auditTrail)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
        }

        public string AddLoanPrincipal( LoanPrincipalViewModel loanP)
        {
            if (loanP != null)
            {
                var value = new TBL_LOAN_PRINCIPAL
                {
                    PRINCIPALSREGNUMBER = loanP.principalsRegNumber,
                    NAME = loanP.name,
                    ACCOUNTNUMBER = loanP.accountNumber,
                    EMAILADDRESS = loanP.emailAddress,
                    PHONENUMBER = loanP.phoneNumber,
                    COMPANYID = loanP.companyId,
                    ADDRESS = loanP.address,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                };

                _context.TBL_LOAN_PRINCIPAL.Add(value);
                _context.SaveChanges();

                return $"Principal with {loanP.principalsRegNumber} registration number has been added successful";

            }
            return $"Could not add Principal with {loanP.principalsRegNumber}  registration number";
        }

        public string DeleteLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            TBL_LOAN_PRINCIPAL data = _context.TBL_LOAN_PRINCIPAL.Find(loanPrincipal.principalId);
            if (data != null)
            {
                data.DATETIMEDELETED = loanPrincipal.dateTimeDeleted;
                data.DELETED = true;
                data.DELETEDBY =loanPrincipal.staffId;
                _context.SaveChanges();
                return $"Principal with {loanPrincipal.principalId} id has been deleted successful";
            }
            return $"Record not found, could not delete Principal with {loanPrincipal.principalId} id";
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffDeleted,
                STAFFID = loanPrincipal.staffId,
                BRANCHID = (short)loanPrincipal.userBranchId,
                DETAIL = $"Deleted loan principal with {data.PRINCIPALID} id",
                IPADDRESS = loanPrincipal.userIPAddress,
                URL = loanPrincipal.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this._auditTrail.AddAuditTrail(audit);
        }

        public IEnumerable<LoanPrincipalViewModel> GetLoanPrincipal(int conpanyId)
        {
            List<LoanPrincipalViewModel> list = new List<LoanPrincipalViewModel>();

            var data = (from o in _context.TBL_LOAN_PRINCIPAL
                       where o.COMPANYID == conpanyId
                       select o).ToList();
            foreach (var o in data)
            {
                LoanPrincipalViewModel val = new LoanPrincipalViewModel();
                val.accountNumber = o.ACCOUNTNUMBER;
                val.address = o.ADDRESS;
                val.emailAddress = o.EMAILADDRESS;
                val.name = o.NAME;
                val.phoneNumber = o.PHONENUMBER;
                val.principalsRegNumber = o.PRINCIPALSREGNUMBER;

                list.Add(val);
            }
            return list;
        }

        public LoanPrincipalViewModel GetLoanPrincipal(int principalId,int companyId)
        {
            LoanPrincipalViewModel val = new LoanPrincipalViewModel();

            if (principalId!=0)
            {
                var data = (from a in _context.TBL_LOAN_PRINCIPAL
                            where a.PRINCIPALID == principalId & a.COMPANYID==companyId
                            select a).FirstOrDefault();

                val.accountNumber = data.ACCOUNTNUMBER;
                val.address = data.ADDRESS;
                val.emailAddress = data.EMAILADDRESS;
                val.name = data.NAME;
                val.phoneNumber = data.PHONENUMBER;
                val.principalsRegNumber = data.PRINCIPALSREGNUMBER;
            }

            return val;
        }


        public string UpdateLoanPrincipal(LoanPrincipalViewModel model)
        {
            TBL_LOAN_PRINCIPAL val = _context.TBL_LOAN_PRINCIPAL.Find(model.principalId);
            if (val != null)
            {
                val.ACCOUNTNUMBER = model.accountNumber;
                val.ADDRESS = model.address;
                val.EMAILADDRESS = model.emailAddress;
                val.NAME = model.name;
                val.PHONENUMBER = model.phoneNumber;
                val.PRINCIPALSREGNUMBER = model.principalsRegNumber;

                val.DATETIMEUPDATED = _genSetup.GetApplicationDate();
                val.LASTUPDATEDBY = model.createdBy;


                _context.SaveChanges();
                return $"Principal with {model.principalId} id has been updated successful";
            }
            return $"Record not found, could not update Principal with {model.principalId} id";
        }
    }
}
