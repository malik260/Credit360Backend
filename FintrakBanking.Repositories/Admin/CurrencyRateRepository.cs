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
            var data = (from a in context.TBL_CURRENCY
                        where a.INUSE == true
                        select new CurrencyViewModel
                        {
                            currencyId = a.CURRENCYID,
                            currencyCode = a.CURRENCYCODE,
                            currencyName = a.CURRENCYNAME
                        }).ToList();
            return data;
        }

        public CurrencyViewModel GetBaseCurrency(int companyId)
        {
          var baseCurrency = (from a in context.TBL_COMPANY
             join c in context.TBL_CURRENCY on a.CURRENCYID equals c.CURRENCYID
             where a.COMPANYID == companyId
             select  new CurrencyViewModel
             {
                 currencyId = a.CURRENCYID,
                 currencyCode = c.CURRENCYCODE,
                 currencyName = c.CURRENCYNAME
             });

            return baseCurrency.FirstOrDefault();
    }



        public IEnumerable<CurrencyRateViewModel> GetCurrencyRate()
        {
            var data = (from a in context.TBL_CURRENCY_EXCHANGERATE
                        where a.DELETED == false 
                        select new CurrencyRateViewModel
                        {

                            currencyRateId = a.CURRENCYRATEID,
                            currencyId = a.CURRENCYID,
                            baseCurrencyId = a.BASECURRENCYID,
                            currency = a.TBL_CURRENCY .CURRENCYNAME,
                            baseCurrency = a.TBL_CURRENCY1 .CURRENCYNAME,
                            rateCodeId = a.RATECODEID,
                            rateCodeName = a.TBL_CURRENCY_RATECODE.RATECODEDESCRIPTION,
                            buyingRate = a.EXCHANGERATE,
                            sellingRate = a.EXCHANGERATE,
                            date = a.DATE,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy=a.CREATEDBY
                            
                        }).ToList();
            return data;
        }

        public double GetCurrentCurrencyExchangeRate(short currencyId)
        {
            return (from a in context.TBL_CURRENCY_EXCHANGERATE.Where(x=>x.CURRENCYID == currencyId && x.RATECODEID == 1
                        && x.DELETED == false) select a.EXCHANGERATE).FirstOrDefault();
        }
        public List<CurrencyRateViewModel> GetCurrencyRateById(short currencyRateId)
        {
            var data = (from a in context.TBL_CURRENCY_EXCHANGERATE
                        where a.CURRENCYID == currencyRateId && a.DELETED == false
                        
                        select new CurrencyRateViewModel
                        {
                            currencyRateId = a.CURRENCYRATEID,
                            currencyId = a.CURRENCYID,
                            baseCurrencyId = a.BASECURRENCYID,
                            currency = a.TBL_CURRENCY.CURRENCYNAME,
                            baseCurrency = a.TBL_CURRENCY1.CURRENCYNAME,
                            buyingRate = a.EXCHANGERATE,
                            sellingRate = a.EXCHANGERATE,
                            date = a.DATE,
                            rateCodeId = a.RATECODEID,
                            rateCodeName = a.TBL_CURRENCY_RATECODE.RATECODEDESCRIPTION,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).OrderByDescending(x=>x.dateTimeCreated).ToList();
            return data;
        }





        public bool AddCurrencyRate(CurrencyRateViewModel model)
        {
            var data = new TBL_CURRENCY_EXCHANGERATE
            {
                 CURRENCYID = model.currencyId,
                 BASECURRENCYID = model.baseCurrencyId,
                 EXCHANGERATE = model.buyingRate,
                 //SELLINGRATE = model.sellingRate ,
                 RATECODEID = model.rateCodeId,
                 DATE = model.date,
                 CREATEDBY = (int)model.createdBy,
                 DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            context.TBL_CURRENCY_EXCHANGERATE.Add(data);

            // Audit Section ---------------------------
            var audit_Currency = (context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.currencyId)).CURRENCYNAME;
            var audit_BaseCurrency = (context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.baseCurrencyId)).CURRENCYNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CurrencyRateAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Currency Rate :  { data.EXCHANGERATE } for date : '{data.DATE} ' on {audit_BaseCurrency} to: {audit_Currency} conversion",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool UpdateCurrencyRate(short currencyRateId, CurrencyRateViewModel model)
        {
            var data = this.context.TBL_CURRENCY_EXCHANGERATE.Find(currencyRateId);
            if (data == null) return false;

            data.CURRENCYID = model.currencyId;
            data.BASECURRENCYID = model.baseCurrencyId;
           data. EXCHANGERATE = model.buyingRate;
            data.RATECODEID = model.rateCodeId;
           //data. SELLINGRATE = model.sellingRate;
            data.DATE = model.date;

            data.LASTUPDATEDBY = (int)model.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit_Currency = (context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.currencyId)).CURRENCYNAME;
            var audit_BaseCurrency = (context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.baseCurrencyId)).CURRENCYNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CurrencyRateUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Currency Rate : { data.EXCHANGERATE } for date: '{data.DATE}' on {audit_BaseCurrency} to: {audit_Currency} conversion",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        
    }
}
