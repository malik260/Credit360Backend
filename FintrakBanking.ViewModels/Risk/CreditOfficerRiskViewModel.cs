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








    public class CreditOfficerRatingViewModel
    {
        public string creditOfficerName { get { return firstName + " " + middleName + " " + lastName; } }

        public int staffId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string staffCode { get; set; }

        public string corrScore { get { return currentRating == null ? "NOT RATED" : currentRating.score.ToString(); } }
        public string corrComment { get { return currentRating == null ? "NOT RATED" : currentRating.comment; } }
        //public DateTime dateRated { get { return currentRating == null ? "NOT RATED" : currentRating.date; } }

        public ParameterScoreViewModel currentRating { get; set; }
    }

    public class CreditOfficerSearchViewModel
    {
        public string searchString { get; set; }

    }

    public class RatingPeriodViewModel : GeneralEntity
    {
        public int ratingPeriodId { get; set; }
        
        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

    }

    public class OfficerRatingViewModel : GeneralEntity
    {
        public OfficerRatingViewModel()
        {
            assessment = new List<ParameterScoreViewModel>();
        }
        public int creditOfficerId { get; set; }
        public  List<ParameterScoreViewModel> assessment { get; set; }
    }

    public class ParameterScoreViewModel
    {
        public int id { get; set; }
        public int weight { get; set; }
        public string parameterName { get; set; }

        public int parameterId { get; set; }
        public int score { get; set; }
        public int keyIndicatorId { get; set; }
        public string comment { get; set; }
    }

    public class KeyIndicatorAssessmentParametersViewModel
    {
        public int count { get; set; }
        public List<KeyIndicator> keyIndicators { get; set; }
    }

    public class KeyIndicator
    {
        public string keyIndicatorName { get; set; }
        public List<ParameterScoreViewModel> parameters { get; set; }
        public int keyIndicatorWeight { get; set; }
        public int score { get; set; }
        public decimal percentageWeight { get; set; }
    }

    public class CreditOfficerRiskRatingDetail
    {
        public CreditOfficerRiskRatingDetail()
        {
            indicators = new List<GenericRiskScore>();
        }

        public int score { get; set; }
        public string comment { get; set; }

        public List<GenericRiskScore> indicators { get; set; }
        public List<GenericRiskScore> parameters { get; set; }

    }

    public class GenericRiskScore
    {

        public int id { get; set; }
        public string name { get; set; }
        public int score { get; set; }
        public int weight { get; set; }
        public int indicatorId { get; set; }
        public string indicatorName { get; set; }
        public int indicatorWeight { get; set; }
    }
}