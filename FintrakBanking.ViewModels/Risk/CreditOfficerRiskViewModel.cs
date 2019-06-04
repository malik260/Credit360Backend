using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Risk
{
    public class CreditOfficerRiskViewModel : GeneralEntity
    {
        public int creditOfficerRiskId { get; set; }

    }

    public class MatrixGrid
    {
        public MatrixGrid()
        {
            description = "UNRATED";
        }

        public int id { get; set; }
        public string description { get; set; }
        public string rating { get; set; }
    }

    public class KeyRiskDrivers
    {
        public int UnpaidObligationsCount { get; set; }
        public int UnpaidObligationsVolume { get; set; }
        public int OverdraftNoLimitOverlineVolume { get; set; }
        public int OverdraftNoLimitOverlineCount { get; set; }
        public int Watchlist { get; set; }
        public int NonPerformingLoans { get; set; }
        public int Cer { get; set; }
        public int OverdraftWithAgeLastCreditDate { get; set; }
        public int DefferalExistence { get; set; }
        public int DefferalVolume { get; set; }
        public int PastDueDefferal { get; set; }
        public int RepeatedDeferral { get; set; }
        public int InternalSolLimitAdherence { get; set; }
        public int LoanDepositRatioLimitAdherence { get; set; }
        public int IncompleteDocumentationFile { get; set; }
        public int ExpiredValuation { get; set; }
        public int ExpiredInsurance { get; set; }
        public int NonPerfectedCollateral { get; set; }
        public int SiteVisitationReportAbsence { get; set; }
        public int FinancialsAbsence { get; set; }
        public int GovernmentExposure { get; set; }
        public int SolBreach { get; set; }
        public int CapitalConsumingExposure { get; set; }
        public int SectorConcentration { get; set; }

    }

    public class RiskIndexMetrics
    {
        public KeyRiskDrivers keyRiskDrivers { get; set; }

        public int borrowingCustomersCount { get; set; }
        public decimal borrowingCustomersExposure { get; set; }


        // DEFERRAL
        public int deferralCount { get; set; }
        public decimal deferralVolume { get; set; }
        public int deferralExistence { get; set; }
        public int percentageVolumeToTotalPortfolio { get { return (int)(deferralVolume / borrowingCustomersExposure) * keyRiskDrivers.DefferalVolume; } }

        // PAST DUE DEFERRAL
        public int pastDueDeferralCount { get; set; }
        public decimal pastDueDeferralVolume { get; set; }
        public int pastDueDeferralExistence { get; set; }

        // REPEATED DEFERRAL
        public int repeatedDeferralCount { get; set; }
        public decimal repeatedDeferralVolume { get; set; }
        public int repeatedDeferralExistence { get; set; }

        // todo...

        public int creditOfficerRiskRating { get { return 20; } } // todo..

    }
}