using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;


namespace FintrakBanking.Repositories.Finance
{
    public class EndOfDayRepository : IEndOfDayRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanOperationsRepository loanOperation;
        private IPublicHolidayRepository publicHoliday;

        public EndOfDayRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup,
                                    ILoanOperationsRepository _loanOperation, IPublicHolidayRepository _publicHoliday,
                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.generalSetup=_generalSetup;
            this.publicHoliday = _publicHoliday;
            this.auditTrail = _auditTrail;
            this.loanOperation = _loanOperation;
        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public bool RunEndOfDay(EndOfDayViewModel model)
        {
            var applicationDate = generalSetup.GetApplicationDate();

            var countryId = context.tbl_Company.FirstOrDefault(x => x.CompanyId == model.companyId).CountryId;

            var nextWorkDay = publicHoliday.GetNextWorkDay(applicationDate, countryId);

            if (applicationDate.AddDays(1) == nextWorkDay)
               ProcessEndOfDay(applicationDate, model.companyId, model.createdBy);
            else
            {
                DateTime runDate = applicationDate;

                do
                {
                    ProcessEndOfDay(runDate, model.companyId, model.createdBy);
                    runDate = runDate.AddDays(1);
                }
                while (runDate < nextWorkDay);
            }

            var financeCurrentDate = context.tbl_FinanceCurrentDate.FirstOrDefault();
            financeCurrentDate.CurrentDate = nextWorkDay;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RanEndOfDay,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Ran end of day from {applicationDate.ToString("dd/mmm/yyyy")} to {nextWorkDay.AddDays(-1).ToString("dd/mmm/yyyy")} successfully",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = applicationDate,
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges();

            return true;            
        }


        public IEnumerable<FinanceEndofdayViewModel> GetFinanceEndofday(int companyId)
        {
            var financeEod = (from e in context.tbl_Finance_EndOfDay
                              where e.CompanyId == companyId // e.EndDateTime == null && e.StartDateTime == null
                              select new FinanceEndofdayViewModel()
                              {
                                  endOfDayId = e.EndOfDayId,
                                  date = e.Date,
                                  startDateTime = e.StartDateTime,
                                  endDateTime = e.EndDateTime,
                                  createdBy = e.CreatedBy,
                              });
            return financeEod;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ProcessEndOfDay(DateTime date, int companyId, int staffId)
        {
            tbl_Finance_EndOfDay endOfDay = new tbl_Finance_EndOfDay();

            endOfDay.CompanyId = companyId;
            endOfDay.Date = date;
            endOfDay.CreatedBy = staffId;
            endOfDay.StartDateTime = DateTime.Now;

            loanOperation.GetDailyTeamLoansInterestAccrual(date);

            loanOperation.GetDailyUnauthorisedOverdraftInterestAccrual(date);

            loanOperation.GetDailyUnauthorisedOverdraftInterestAccrual(date);

            loanOperation.GetDailyPastDueInterestAccrual(date);

            loanOperation.GetDailyPastDuePrincipalAccrual(date);

            endOfDay.EndDateTime = DateTime.Now;

            context.tbl_Finance_EndOfDay.Add(endOfDay);

            context.SaveChanges();
        }

    }
}
 