using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.Repositories.Risk
{
    public class CreditOfficerRiskRepository : ICreditOfficerRiskRepository
    {
        private FinTrakBankingContext context;
        private TBL_STAFF officer;
        private TBL_CORR_FREQUENCY_SETUP frequencySetup;
        private TBL_CORR_RISK_MATRIX matrix;
        private TBL_CORR_OFFICER_RATING currentRating;
        private decimal totalExposure;
        private int totalBorrowingCustomers;
        private List<int> creditOfficerRoleIds = new List<int> { 6, 7, 9 };

        public CreditOfficerRiskRepository(
                FinTrakBankingContext _context
            )
        {
            this.context = _context;
        }

        private bool Init(string username)
        {
            if (officer == null) officer = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == username.ToLower());
            if (officer == null) return false;
            if (frequencySetup == null) frequencySetup = context.TBL_CORR_FREQUENCY_SETUP.FirstOrDefault();
            if (currentRating == null) currentRating = context.TBL_CORR_OFFICER_RATING
                    .Where(x => x.STAFFID == officer.STAFFID)
                    .OrderByDescending(x => x.OFFICERRATINGID)
                    .FirstOrDefault();
            if (matrix == null) matrix = context.TBL_CORR_RISK_MATRIX.FirstOrDefault(x => x.RISKMATRIXID == officer.CORRMATRIXGRIDRATINGID);
            return true;
        }

        private void ComputeCreditOfficerRiskRating()
        {
            // init
            totalExposure = GetTotalExposure();
            totalBorrowingCustomers = GetTotalBorrowingCustomers();
            
            RiskIndexMetrics riskIndexMetrics = new RiskIndexMetrics();

            riskIndexMetrics.keyRiskDrivers = GetKeyRiskDrivers();
            riskIndexMetrics.borrowingCustomersCount = totalBorrowingCustomers;
            riskIndexMetrics.borrowingCustomersExposure = totalExposure;
            // todo..

            // update rating comment
            var comment = GetMatrixDescription(riskIndexMetrics.creditOfficerRiskRating).description;

            // rating
            var rating = context.TBL_CORR_OFFICER_RATING.Add(new TBL_CORR_OFFICER_RATING
            {
                STAFFID = officer.STAFFID,
                BRANCHID = (int)officer.BRANCHID,
                BUSINESSUNITID = (int)officer.BUSINESSUNITID,
                BORROWINGCUSTOMERS = totalBorrowingCustomers,
                EXPOSURE = totalExposure,
                // METRICS...
                CORRSCORE = riskIndexMetrics.creditOfficerRiskRating,
                CORRCOMMENT = comment,
                DATERATED = DateTime.Now,
                FROMDATE = frequencySetup.NEXTRATINGDATE.AddMonths((-1) * frequencySetup.RATINGPERIOD),
                TODATE = frequencySetup.NEXTRATINGDATE,
            });

            context.SaveChanges();
        }
        
        private MatrixGrid GetMatrixDescription(int score)
        {
            var matrix = context.TBL_CORR_RISK_MATRIX.FirstOrDefault(x => x.GRADINGMINIMUM <= score && score <= x.GRADINGMAXIMUM);
            return new MatrixGrid
            {
                id = matrix.RISKMATRIXID,
                rating = matrix.RATING,
                description = matrix.DESCRIPTION,
            };
        }

        public MatrixGrid GetCreditOfficerRiskRating(string username)
        {
            if (Init(username) == false || !IsCreditOfficer()) return new MatrixGrid();
            if (RatingExpired()) ComputeAndUpdateRating();
            return GetCurrentRiskRating();
        }

        private void ComputeAndUpdateRating()
        {
            ComputeCreditOfficerRiskRating();
            UpdateCreditOfficerRating();
        }

        private void UpdateCreditOfficerRating()
        {
            var lastRating = context.TBL_CORR_OFFICER_RATING
                .Where(x => x.STAFFID == officer.STAFFID)
                .OrderByDescending(x => x.OFFICERRATINGID)
                .FirstOrDefault();
            officer.CORRMATRIXGRIDRATINGID = GetMatrixDescription(lastRating.CORRSCORE).id;
            context.SaveChanges();
        }

        private MatrixGrid GetCurrentRiskRating()
        {
            if (matrix == null) return new MatrixGrid();
            return new MatrixGrid
            {
                id = matrix.RISKMATRIXID,
                rating = matrix.RATING,
                description = matrix.DESCRIPTION
            };
        }

        private bool RatingExpired()
        {
            if (officer.CORRMATRIXGRIDRATINGID == null) return false;
            return frequencySetup.NEXTRATINGDATE < currentRating.DATERATED 
                && DateTime.Now > frequencySetup.NEXTRATINGDATE
                ;
        }

        private bool IsCreditOfficer()
        {
            return creditOfficerRoleIds.Contains(officer.STAFFROLEID);
        }

        // COMPUTATIONS

        private int GetTotalBorrowingCustomers()
        {
            throw new NotImplementedException();
        }

        private decimal GetTotalExposure()
        {
            throw new NotImplementedException();
        }


        private KeyRiskDrivers GetKeyRiskDrivers()
        {
            return new KeyRiskDrivers // hardcoded for now!
            {
                UnpaidObligationsCount = 5,
                UnpaidObligationsVolume = 15,
                OverdraftNoLimitOverlineVolume = 5,
                OverdraftNoLimitOverlineCount = 5,
                Watchlist = 5,
                NonPerformingLoans = 15,
                Cer = 5,
                OverdraftWithAgeLastCreditDate = 5,
                DefferalExistence = 5,
                DefferalVolume = 5,
                PastDueDefferal = 2,
                RepeatedDeferral = 3,
                InternalSolLimitAdherence = 2,
                LoanDepositRatioLimitAdherence = 3,
                IncompleteDocumentationFile = 5,
                ExpiredValuation = 1,
                ExpiredInsurance = 1,
                NonPerfectedCollateral = 1,
                SiteVisitationReportAbsence = 1,
                FinancialsAbsence = 1,
                GovernmentExposure = 10,
                SolBreach = 10,
                CapitalConsumingExposure = 10,
                SectorConcentration = 10,
            };
        }

    }
}
