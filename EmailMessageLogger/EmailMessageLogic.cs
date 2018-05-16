
using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories.AppEmail;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailMessageLogger
{


    public class EmailMessageLogic
    {
        private static FinTrakBankingContext context = new FinTrakBankingContext();
        private static IAuditTrailRepository auditTrail;
        private static EmailHelpers emailHelpers;
        private static IGeneralSetupRepository genSetup;
        private static DateTime applDate;
        private static IStaffRepository staffRepo;

        FinTrakBankingContext dbContext = new FinTrakBankingContext();

        EmailAndAlertsRepository repo = new EmailAndAlertsRepository(
                context,
                auditTrail,
                emailHelpers,
                genSetup,
                staffRepo
            );

        public string Start()
        {
            string title = string.Empty;
            string body = string.Empty;

            var alertSetups = context.TBL_MONITORING_ALERT_SETUP.ToList();

            repo.SendAlertsForCovenantsApproachingDueDate(title, body, alertSetups);

            repo.SendAlertsForCovenantsOverDue(title, body, alertSetups);

            repo.SendAlertForExpiredInsurance(title, body, alertSetups);

            repo.SendAlertOnAccountWithExeption_Overdrawn(title, body, alertSetups);

            repo.SendAlertOnAccountWithExeption_Watchist(title, body, alertSetups);

            repo.SendAlertOnAccountWithExeption_Unauthorized(title, body, alertSetups);

            repo.SendAlertOnInsuranceApprochingExpiration(title, body, alertSetups);

            repo.SendAlertOnPastDueObligationAccounts(title, body, alertSetups);

            repo.SendAlertOnTurnoverCovenant(title, body, alertSetups);

            repo.SendAlertsForCollateralPropertyApproachingRevaluation(title, body, alertSetups);

            repo.SendAlertsForCollateralPropertyDueForVisitation(title, body, alertSetups);

            repo.SendAlertsOnExpiredActiveBondAndGuarantee(title, body, alertSetups);

            repo.SendAlertsOnInActiveBondAndGuarantee(title, body, alertSetups);

          //  repo.SendAlertsOnLoanCASAwithPND(title, body, alertSetups);

            repo.SendAlertsOnOverDraftLoansAlmostDue(title, body, alertSetups);

            repo.SendAlertsOnSelfLiquidatingLoanExpiry(title, body, alertSetups);

            return "";
        }
    }
}
