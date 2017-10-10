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
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Finance
{
    public class EndOfDayRepository : IEndOfDayRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanOperationsRepository loanOperation;
        private IPublicHolidayRepository publicHoliday;

        public bool RunEndOfDay(EndOfDayViewModel model)
        {
            var applicationDate = generalSetup.GetApplicationDate();

            ProcessEndOfDay(model.date);

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RanEndOfDay,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Ran end of day for {model.date.ToString("dd/mmm/yyyy")} successfully",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = applicationDate,
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges();

            return true;            
        }


        public void ProcessEndOfDay(DateTime date)
        {
            loanOperation.GetDailyTeamLoansInterestAccrual(date);

            loanOperation.GetDailyUnauthorisedOverdraftInterestAccrual(date);

            loanOperation.GetDailyUnauthorisedOverdraftInterestAccrual(date);

            loanOperation.GetDailyPastDueInterestAccrual(date);

            loanOperation.GetDailyPastDuePrincipalAccrual(date);
        }

    }
}
 