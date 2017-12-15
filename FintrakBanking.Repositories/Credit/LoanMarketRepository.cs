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
                    PHONENUMBER = loanMarket.phoneNumber,
                    ADDRESS = loanMarket.address,
                    CREATEDBY = loanMarket.staffId,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    DELETED = false,
                };

                _context.TBL_LOAN_MARKET.Add(value);
                _context.SaveChanges();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovedMarketInserted,
                    STAFFID = loanMarket.staffId,
                    BRANCHID = (short)loanMarket.userBranchId,
                    DETAIL = $"Approved Loan market with {loanMarket.companyId} company id is added",
                    IPADDRESS = loanMarket.userIPAddress,
                    URL = loanMarket.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this._auditTrail.AddAuditTrail(audit);

                return "The record has been added successful";

            }
            return "The record has not been added";
        }

        public string DeleteLoanMarket(int marketId, LoanMarketViewModel loanMarket)
        {
            TBL_LOAN_MARKET data = _context.TBL_LOAN_MARKET.Find(marketId);
            if (data != null)
            {
                data.DATETIMEDELETED = loanMarket.dateTimeDeleted;
                data.DELETED = true;
                data.DELETEDBY = loanMarket.staffId;
                _context.SaveChanges();
                return "The record has been deleted successful";
            }
            return "The record has not been deleted";
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovedMarketDeleted,
                STAFFID = loanMarket.staffId,
                BRANCHID = (short)loanMarket.userBranchId,
                DETAIL = $"Deleted market with {data.MARKETID} id",
                IPADDRESS = loanMarket.userIPAddress,
                URL = loanMarket.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            };

            this._auditTrail.AddAuditTrail(audit);
        }

        public LoanMarketViewModel GetLoanMarket(int markeetId, int companyId)
        {
            LoanMarketViewModel val = new LoanMarketViewModel();

            var data = (from o in _context.TBL_LOAN_MARKET
                        where o.COMPANYID == companyId & o.MARKETID == markeetId
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
            var data = (from o in _context.TBL_LOAN_MARKET
                        join c in _context.TBL_CITY on o.CITYID equals c.CITYID

                        where o.COMPANYID == companyId
                        select new LoanMarketViewModel
                        {
                            accountNumber = o.ACCOUNTNUMBER,
                            address = o.ADDRESS,
                            cityId = o.CITYID,
                            companyId = o.COMPANYID,
                            createdBy = o.CREATEDBY,
                            emailAddress = o.EMAILADDRESS,
                            marketId = o.MARKETID,
                            marketName = o.MARKETNAME,
                            phoneNumber = o.PHONENUMBER,
                            stateId = c.STATEID,
                            cityName=c.CITYNAME,
                            
                        }).ToList();

            return data;

        }

        public string UpdateLoanMarket(int marketId, LoanMarketViewModel loanMarket)
        {

            TBL_LOAN_MARKET val = _context.TBL_LOAN_MARKET.Find(marketId);
            if (val != null)
            {
                val.ACCOUNTNUMBER = loanMarket.accountNumber;
                val.CITYID = loanMarket.cityId;
                val.COMPANYID = loanMarket.companyId;
                val.EMAILADDRESS = loanMarket.emailAddress;
                val.MARKETNAME = loanMarket.marketName;
                val.PHONENUMBER = loanMarket.phoneNumber;
                val.ADDRESS = loanMarket.address;
                val.CREATEDBY = loanMarket.staffId;
                //val.DATETIMECREATED = _genSetup.GetApplicationDate();
                val.DELETED = false;
                val.DATETIMEUPDATED = DateTime.Now;
                val.LASTUPDATEDBY = loanMarket.staffId;

                _context.SaveChanges();

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovedMarketUpdated,
                    STAFFID = loanMarket.staffId,
                    BRANCHID = (short)loanMarket.userBranchId,
                    DETAIL = $"Update market with {loanMarket.marketId} id",
                    IPADDRESS = loanMarket.userIPAddress,
                    URL = loanMarket.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                this._auditTrail.AddAuditTrail(audit);

                return "The record has been updated successful";
            }
            return "The record has not been updated";
        }
    }
}
