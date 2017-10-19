using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Admin
{
    public class CurrencyRateRepository : ICurrencyRateRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CurrencyRateRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        public IEnumerable<CurrencyViewModel> GetCurrency()
        {
            var data = (from a in context.tbl_Currency
                        select new CurrencyViewModel
                        {
                            currencyId = a.CurrencyId,
                            currencyCode = a.CurrencyCode,
                            currencyName = a.CurrencyName
                        }).ToList();
            return data;
        }
        public CurrencyViewModel GetBaseCurrency(int companyId)
        {
            var data = (from a in context.tbl_Company where a.CompanyId == companyId 
                        select new CurrencyViewModel
                        {
                            currencyId = a.CurrencyId,
                            currencyCode = a.tbl_Currency.CurrencyCode,
                            currencyName = a.tbl_Currency.CurrencyCode +" - " + a.tbl_Currency.CurrencyName
                        }).FirstOrDefault();
            return data;
        }
        public IEnumerable<CurrencyRateViewModel> GetCurrencyRate()
        {
            var data = (from a in context.tbl_Currency_Rate
                        where a.Deleted == false
                        select new CurrencyRateViewModel
                        {

                            currencyRateId = a.CurrencyRateId,
                            currencyId = a.CurrencyId,
                            baseCurrencyId = a.BaseCurrencyId,
                            currency = a.tbl_Currency .CurrencyName,
                            baseCurrency = a.tbl_Currency1 .CurrencyName,
                          
                            buyingRate = a.BuyingRate,
                            sellingRate = a.SellingRate,
                            date = a.Date,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy=a.CreatedBy
                            
                        }).ToList();
            return data;
        }
        public List<CurrencyRateViewModel> GetCurrencyRateById(short currencyRateId)
        {
            var data = (from a in context.tbl_Currency_Rate
                        where a.CurrencyId == currencyRateId && a.Deleted == false
                        
                        select new CurrencyRateViewModel
                        {
                            currencyRateId = a.CurrencyRateId,
                            currencyId = a.CurrencyId,
                            baseCurrencyId = a.BaseCurrencyId,
                            currency = a.tbl_Currency.CurrencyName,
                            baseCurrency = a.tbl_Currency1.CurrencyName,
                            buyingRate = a.BuyingRate,
                            sellingRate = a.SellingRate,
                            date = a.Date,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).OrderByDescending(x=>x.dateTimeCreated).ToList();
            return data;
        }
        public bool AddCurrencyRate(CurrencyRateViewModel model)
        {
            var data = new tbl_Currency_Rate
            {
                 CurrencyId = model.currencyId,
                 BaseCurrencyId = model.baseCurrencyId,
                 BuyingRate = model.buyingRate,
                 SellingRate = model.sellingRate ,
                 Date = model.date,
                 CreatedBy = (int)model.createdBy,
                 DateTimeCreated = _genSetup.GetApplicationDate()
            };

            context.tbl_Currency_Rate.Add(data);

            // Audit Section ---------------------------
            var audit_Currency = (context.tbl_Currency.FirstOrDefault(x => x.CurrencyId == model.currencyId)).CurrencyName;
            var audit_BaseCurrency = (context.tbl_Currency.FirstOrDefault(x => x.CurrencyId == model.baseCurrencyId)).CurrencyName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CurrencyRateAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Currency Rate :  { data.BuyingRate } for date: '{data.Date}' on {audit_BaseCurrency} to: {audit_Currency} conversion",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool UpdateCurrencyRate(short currencyRateId, CurrencyRateViewModel model)
        {
            var data = this.context.tbl_Currency_Rate.Find(currencyRateId);
            if (data == null) return false;

            data.CurrencyId = model.currencyId;
            data.BaseCurrencyId = model.baseCurrencyId;
           data. BuyingRate = model.buyingRate;
           data. SellingRate = model.sellingRate;
            data.Date = model.date;

            data.LastUpdatedBy = (int)model.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit_Currency = (context.tbl_Currency.FirstOrDefault(x => x.CurrencyId == model.currencyId)).CurrencyName;
            var audit_BaseCurrency = (context.tbl_Currency.FirstOrDefault(x => x.CurrencyId == model.baseCurrencyId)).CurrencyName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CurrencyRateUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Currency Rate : { data.BuyingRate } for date: '{data.Date}' on {audit_BaseCurrency} to: {audit_Currency} conversion",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
        
    }
}
