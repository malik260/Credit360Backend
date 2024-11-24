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
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Common;

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

        public string AddLoanPrincipal(LoanPrincipalViewModel loanP)
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

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = loanP.staffId,
                    BRANCHID = (short)loanP.userBranchId,
                    DETAIL = $"Loan principal with {loanP.companyId} company id is added",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = loanP.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this._auditTrail.AddAuditTrail(audit);

                return "The record has been added successful";

            }
            return "The record has not been added";
        }

        public string DeleteLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            TBL_LOAN_PRINCIPAL data = _context.TBL_LOAN_PRINCIPAL.Find(loanPrincipal.principalId);
            if (data != null)
            {
                data.DATETIMEDELETED = loanPrincipal.dateTimeDeleted;
                data.DELETED = true;
                data.DELETEDBY = loanPrincipal.staffId;
                _context.SaveChanges();
                return "The record has been deleted successful";
            }
            return "The record has not been deleted";
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalDeleted,
                STAFFID = loanPrincipal.staffId,
                BRANCHID = (short)loanPrincipal.userBranchId,
                DETAIL = $"Deleted loan principal with {data.PRINCIPALID} id",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = loanPrincipal.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this._auditTrail.AddAuditTrail(audit);
        }

        public IEnumerable<LoanPrincipalViewModel> GetLoanPrincipal(int conpanyId)
        {

            var data = (from o in _context.TBL_LOAN_PRINCIPAL
                        where o.COMPANYID == conpanyId & o.DELETED == false
                        orderby o.NAME
                        select new LoanPrincipalViewModel
                        {

                            accountNumber = o.ACCOUNTNUMBER,
                            address = o.ADDRESS,
                            emailAddress = o.EMAILADDRESS,
                            name = o.NAME,
                            phoneNumber = o.PHONENUMBER,
                            principalsRegNumber = o.PRINCIPALSREGNUMBER,
                            principalId = o.PRINCIPALID,

                        }).ToList();

            return data;


        }

        public LoanPrincipalViewModel GetLoanPrincipal(int principalId, int companyId)
        {
            LoanPrincipalViewModel val = new LoanPrincipalViewModel();

            if (principalId != 0)
            {
                var data = (from a in _context.TBL_LOAN_PRINCIPAL
                            where a.PRINCIPALID == principalId & a.COMPANYID == companyId & a.DELETED == false
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
            TBL_LOAN_PRINCIPAL val = _context.TBL_LOAN_PRINCIPAL.FirstOrDefault(x => x.PRINCIPALID == model.principalId);
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

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalUpdated,
                    STAFFID = model.staffId,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Update loan principal with {model.principalId} id",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                this._auditTrail.AddAuditTrail(audit);

                return "The record has been updated successful";
            }

            return "The record has not been updated";
        }

        public List<LoanPrincipalViewModel> GetLoanPrincipal()
        {
            using (var _context = new FinTrakBankingContext())
            {

                var data = (from o in _context.TBL_LOAN_PRINCIPAL
                            where o.DELETED == false
                            orderby o.NAME
                            select new LoanPrincipalViewModel
                            {

                                accountNumber = o.ACCOUNTNUMBER,
                                address = o.ADDRESS,
                                emailAddress = o.EMAILADDRESS,
                                name = o.NAME,
                                phoneNumber = o.PHONENUMBER,
                                principalsRegNumber = o.PRINCIPALSREGNUMBER,
                                principalId = o.PRINCIPALID,

                            }).ToList();

                return data;


            }
        }
    }
}
