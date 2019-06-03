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
        private TBL_RISK_MATRIX matrix;
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

        private void Init(string username)
        {
            if (frequencySetup == null) frequencySetup = context.TBL_CORR_FREQUENCY_SETUP.FirstOrDefault();
            if (officer == null) officer = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE == username);
            if (currentRating == null) currentRating = context.TBL_CORR_OFFICER_RATING.Where(x => x.STAFFID == officer.STAFFID).OrderByDescending(x => x.OFFICERRATINGID).FirstOrDefault();
            if (matrix == null) matrix = context.TBL_RISK_MATRIX.FirstOrDefault(x => x.RISKMATRIXID == officer.CORRMATRIXGRIDRATINGID);
        }

        private void ComputeCreditOfficerRiskRating()
        {
            // init
            totalExposure = GetTotalExposure();
            totalBorrowingCustomers = GetTotalBorrowingCustomers();

            // rating
            var rating = context.TBL_CORR_OFFICER_RATING.Add(new TBL_CORR_OFFICER_RATING
            {
                STAFFID = officer.STAFFID,
                BRANCHID = (int)officer.BRANCHID,
                BUSINESSUNITID = (int)officer.BUSINESSUNITID,
                BORROWINGCUSTOMERS = totalBorrowingCustomers,
                EXPOSURE = totalExposure,
                // METRICS
                COMMENT = String.Empty,
                DATERATED = DateTime.Now,
                FROMDATE = frequencySetup.NEXTRATINGDATE.AddMonths((-1) * frequencySetup.RATINGPERIOD),
                TODATE = frequencySetup.NEXTRATINGDATE,
            });

            bool ratingSaved = context.SaveChanges() != 0;
            if (!ratingSaved) return;

            // indexes
            var definedIndexes = context.TBL_CORR_RATING_INDEX_SETUP.Where(x => x.ISACTIVE == true);

            foreach (var setup in definedIndexes)
            {
                context.TBL_CORR_RATING_INDEX_DETAIL.Add(new TBL_CORR_RATING_INDEX_DETAIL
                {
                    OFFICERRATINGID = rating.OFFICERRATINGID,
                    RATINGINDEXSETUPID = setup.RATINGINDEXSETUPID,
                    PERCENTAGEWEIGHT = setup.PERCENTAGEWEIGHT,
                    SCORE = ComputeScoreFromMetrics(setup.DEFINEDFUNCTIONID),
                });
            }

            bool ratingIndexesSaved = context.SaveChanges() != 0;
            if (!ratingIndexesSaved) return; // TODO rollback

            // update rating comment
            rating.COMMENT = GetMatrixDescription(rating.OFFICERRATINGID).description;
            context.SaveChanges();
        }


        private MatrixGrid GetMatrixDescription(int officerRatingId)
        {
            var score = context.TBL_CORR_RATING_INDEX_DETAIL.Where(x => x.OFFICERRATINGID == officerRatingId).Sum(x => x.SCORE);
            var matrix = context.TBL_RISK_MATRIX.FirstOrDefault(x => x.GRADINGMINIMUM <= score && score <= x.GRADINGMAXIMUM);
            return new MatrixGrid
            {
                id = matrix.RISKMATRIXID,
                rating = matrix.RATING,
                description = matrix.DESCRIPTION,
            };
        }

        public MatrixGrid GetCreditOfficerRiskRating(string username)
        {
            Init(username);
            if (!IsCreditOfficer()) return new MatrixGrid();
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
            officer.CORRMATRIXGRIDRATINGID = GetMatrixDescription(currentRating.OFFICERRATINGID).id;
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

        private int ComputeScoreFromMetrics(int? functionId)
        {
            if (functionId == null) return 0;
            throw new NotImplementedException();
        }
    }
}
