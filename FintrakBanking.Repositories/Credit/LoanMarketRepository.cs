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
    public class LoanMarketRepository : ILoanMarketRepository
    {
        private readonly FinTrakBankingContext _context;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly IAuditTrailRepository _auditTrail;

        public LoanMarketRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                IAuditTrailRepository auditTrail)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
        }

        public string AddLoanMarket(LoanMarketViewModel loanMarket)
        {
            if (loanMarket != null)
            {
                var value = new TBL_LOAN_MARKET
                {
                    ACCOUNTNUMBER = loanMarket.accountNumber,
                    CITYID = loanMarket.cityId,
                    COMPANYID = loanMarket.companyId,
                    EMAILADDRESS = loanMarket.emailAddress,
                    MARKETNAME = loanMarket.marketName,
                    PHONENUMBER =loanMarket.phoneNumber,
                    ADDRESS = loanMarket.address,

                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                };

                _context.TBL_LOAN_MARKET.Add(value);
                _context.SaveChanges();

                return $"Market  name '{loanMarket.marketName}' has been added successful";

            }
            return $"Could not add market with name '{loanMarket.marketName}' ";
        }

        public string DeleteLoanMarket(LoanMarketViewModel loanMarket)
        {
            TBL_LOAN_MARKET data = _context.TBL_LOAN_MARKET.Find(loanMarket.marketId);
            if (data != null)
            {
                data.DATETIMEDELETED = loanMarket.dateTimeDeleted;
                data.DELETED = true;
                data.DELETEDBY = loanMarket.staffId;
                _context.SaveChanges();
                return $"Market  name '{loanMarket.marketName}' has been deleted successful";
            }
            return $"Could not delete market with name '{loanMarket.marketName}' ";
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffDeleted,
                STAFFID = loanMarket.staffId,
                BRANCHID = (short)loanMarket.userBranchId,
                DETAIL = $"Deleted market with {data.MARKETID} id",
                IPADDRESS = loanMarket.userIPAddress,
                URL = loanMarket.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this._auditTrail.AddAuditTrail(audit);
        }

        public LoanMarketViewModel GetLoanMarket(int markeetId, int companyId)
        {
            LoanMarketViewModel val = new LoanMarketViewModel();

            var data = (from o in _context.TBL_LOAN_MARKET
                        where o.COMPANYID == companyId & o.MARKETID==markeetId
                        select o).FirstOrDefault();
           
                val.accountNumber = data.ACCOUNTNUMBER;
                val.address = data.ADDRESS;
                val.emailAddress = data.EMAILADDRESS;
                val.marketName = data.MARKETNAME;
                val.phoneNumber = data.PHONENUMBER;
 
           
            return val;
        }

        public IEnumerable<LoanMarketViewModel> GetLoanMarket(int companyId)
        {
            List<LoanMarketViewModel> list = new List<LoanMarketViewModel>();

            var data = (from o in _context.TBL_LOAN_MARKET
                        where o.COMPANYID == companyId
                        select o).ToList();
            foreach (var o in data)
            {
                LoanMarketViewModel val = new LoanMarketViewModel();
                val.accountNumber = o.ACCOUNTNUMBER;
                val.address = o.ADDRESS;
                val.emailAddress = o.EMAILADDRESS;
                val.marketName = o.MARKETNAME;
                val.phoneNumber = o.PHONENUMBER;

                list.Add(val);
            }
            return list;
        }

        public string UpdateLoanMarket(LoanMarketViewModel loanMarket)
        {
            TBL_LOAN_MARKET val = _context.TBL_LOAN_MARKET.Find(loanMarket.marketId);
            if (val != null)
            {
                val.ACCOUNTNUMBER = loanMarket.accountNumber;
                val.ADDRESS = loanMarket.address;
                val.EMAILADDRESS = loanMarket.emailAddress;
                val.MARKETNAME = loanMarket.marketName;
                val.PHONENUMBER = loanMarket.phoneNumber;
                val.CITYID = loanMarket.cityId;

                val.DATETIMEUPDATED = _genSetup.GetApplicationDate();
                val.LASTUPDATEDBY = loanMarket.createdBy;


                _context.SaveChanges();
                return $"Market with name '{loanMarket.marketId}' has been updated successful";
            }
            return $"Record not found, could not update market with name '{loanMarket.marketId}'";
        }
    }
}
