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
        private TBL_CORR_RATING_PERIOD currentRatingPeriod;
        //private TBL_CORR_RISK_MATRIX matrix;
        private TBL_CORR_OFFICER_RATING currentRating;
        //private decimal totalExposure;
        //private int totalBorrowingCustomers;
        private List<int> creditOfficerRoleIds;
        //private List<int> creditOfficerRoleIds = new List<int> { 6, 7, 9, 48 };

        public CreditOfficerRiskRepository(
                FinTrakBankingContext _context
            )
        {
            this.context = _context;
            creditOfficerRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(s => s.STAFFROLEID).ToList();
        }
         
        private bool Init(string username)
        {
            if (officer == null) officer = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == username.ToLower());
            if (officer == null) return false;
            if (currentRatingPeriod == null) currentRatingPeriod = context.TBL_CORR_RATING_PERIOD.OrderByDescending(x => x.STARTDATE).FirstOrDefault();
            if (currentRating == null) currentRating = context.TBL_CORR_OFFICER_RATING
                    .Where(x => x.STAFFID == officer.STAFFID)
                    .OrderByDescending(x => x.OFFICERRATINGID)
                    .FirstOrDefault();
            //if (matrix == null) matrix = context.TBL_CORR_RISK_MATRIX.FirstOrDefault(x => x.RISKMATRIXID == currentRating.CORRSCORE);
            return true;
        }

        private void ComputeCreditOfficerRiskRating()
        {
            // init
            //totalExposure = GetTotalExposure();
            //totalBorrowingCustomers = GetTotalBorrowingCustomers();
            
            //RiskIndexMetrics riskIndexMetrics = new RiskIndexMetrics();

            //riskIndexMetrics.keyRiskDrivers = GetKeyRiskDrivers();
            //riskIndexMetrics.borrowingCustomersCount = totalBorrowingCustomers;
            //riskIndexMetrics.borrowingCustomersExposure = totalExposure;

            // todo..

            // update rating comment
            //var comment = GetMatrixDescription(riskIndexMetrics.creditOfficerRiskRating).description;

            // rating
            //var rating = context.TBL_CORR_OFFICER_RATING.Add(new TBL_CORR_OFFICER_RATING
            //{
            //    STAFFID = officer.STAFFID,
            //    // METRICS...
            //    CORRSCORE = riskIndexMetrics.creditOfficerRiskRating,
            //    CORRCOMMENT = comment,
            //    DATERATED = DateTime.Now,
            //    //FROMDATE = frequencySetup.NEXTRATINGDATE.AddMonths((-1) * frequencySetup.RATINGPERIOD),
            //});

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
            // if (RatingExpired()) ComputeAndUpdateRating();
            return GetCurrentRiskRating();
        }

        private void ComputeAndUpdateRating()
        {
            ComputeCreditOfficerRiskRating();
            //UpdateCreditOfficerRating();
        }

        //private void UpdateCreditOfficerRating()
        //{
        //    var lastRating = context.TBL_CORR_OFFICER_RATING
        //        .Where(x => x.STAFFID == officer.STAFFID)
        //        .OrderByDescending(x => x.OFFICERRATINGID)
        //        .FirstOrDefault();
        //    officer.CORRMATRIXGRIDRATINGID = GetMatrixDescription(lastRating.CORRSCORE).id;
        //    context.SaveChanges();
        //}

        private MatrixGrid GetCurrentRiskRating()
        {
            if (currentRating == null) return new MatrixGrid();
            return GetMatrixDescription(currentRating.CORRSCORE);
        }

        private bool RatingExpired()
        {
            return currentRatingPeriod.RATINGPERIODID != currentRating.RATINGPERIODID;
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

        public IEnumerable<RatingPeriodViewModel> GetRatingPeriods()
        {
            return context.TBL_CORR_RATING_PERIOD
                .Where(x => x.DELETED == false)
                .OrderByDescending(x => x.RATINGPERIODID)
                .Select(x => new RatingPeriodViewModel
                {
                    ratingPeriodId = x.RATINGPERIODID,
                    startDate = x.STARTDATE,
                    endDate = x.ENDDATE,
                })
                .ToList();
        }

        public bool AddRatingPeriod(RatingPeriodViewModel model)
        {
            if (model.startDate >= model.endDate) throw new SecureException("End Date must be greater than Start Date!");
            if (context.TBL_CORR_RATING_PERIOD.Where(x => x.ENDDATE > model.startDate).Any()) throw new SecureException("Rating periods can not overlap!");

            var entity = new TBL_CORR_RATING_PERIOD
            {
                STARTDATE = model.startDate,
                ENDDATE = model.endDate,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            context.TBL_CORR_RATING_PERIOD.Add(entity);
            return context.SaveChanges() != 0;
        }

        public List<CreditOfficerRatingViewModel> GetCreditOfficerSearch(CreditOfficerSearchViewModel model)
        {
            List<CreditOfficerRatingViewModel> officerRating = new List<CreditOfficerRatingViewModel>();

            if (!string.IsNullOrWhiteSpace(model.searchString))
            {
                var searchString = model.searchString.Trim().ToLower();

                officerRating = context.TBL_STAFF.Where(x => x.DELETED == false)// && x.c == companyId)
                        .Where(x => creditOfficerRoleIds.Contains(x.STAFFROLEID) &&
                        (x.FIRSTNAME.ToLower().Contains(searchString)
                        || x.MIDDLENAME.ToLower().Contains(searchString)
                        || x.LASTNAME.ToLower().Contains(searchString)
                        || x.STAFFCODE.ToLower().Contains(searchString))
                    )
                    .Select(o => new CreditOfficerRatingViewModel
                    {
                        staffId = o.STAFFID,
                        firstName = o.FIRSTNAME,
                        middleName = o.MIDDLENAME,
                        lastName = o.LASTNAME,
                        staffCode = o.STAFFCODE,
                        currentRating = context.TBL_CORR_OFFICER_RATING
                                            .Where(x => x.STAFFID == o.STAFFID)
                                            .OrderByDescending(x => x.OFFICERRATINGID)
                                            .Select(x => new ParameterScoreViewModel
                                            {
                                                score = x.CORRSCORE,
                                                comment = x.CORRCOMMENT
                                            })
                                            .FirstOrDefault(),
                    })
                    .Take(10)
                    .ToList();
            }

            return officerRating;

        }


        public bool AddOfficerRating(OfficerRatingViewModel model)
        {
            // init
            var periods = GetRatingPeriods();
            if (periods.Count() == 0) throw new SecureException("No rating period exist in setup!");
            ValidateScoreWithinWeight(model.assessment);
            int ratingPeriodId = periods.FirstOrDefault().ratingPeriodId;
            int corrScore = model.assessment.Sum(x => x.score);
            string corrComment = GetMatrixDescription(corrScore).description;

            // parent
            var rating = context.TBL_CORR_OFFICER_RATING.Add(new TBL_CORR_OFFICER_RATING
            {
                STAFFID = model.creditOfficerId,
                RATINGPERIODID = ratingPeriodId,
                CORRSCORE = corrScore,//update
                CORRCOMMENT = corrComment,//update
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
            });
            
            bool ratingSaved = context.SaveChanges() != 0;

            if (ratingSaved)
            {
                foreach(var param in model.assessment)
                {
                    context.TBL_CORR_OFFICER_RATING_DETAIL.Add(new TBL_CORR_OFFICER_RATING_DETAIL
                    {
                        OFFICERRATINGID = rating.OFFICERRATINGID,
                        ASSESSMENTPARAMETERID = param.parameterId,
                        SCORE = param.score,
                    });
                }
                bool detailSaved = context.SaveChanges() != 0;
                if (detailSaved) return true;
                else DeleteOfficerRating(rating.OFFICERRATINGID);
            }

            return false;
        }

        private void ValidateScoreWithinWeight(List<ParameterScoreViewModel> assessment)
        {
            // throw new NotImplementedException();
        }

        public bool DeleteOfficerRating(int id)
        {
            var entity = this.context.TBL_CORR_OFFICER_RATING.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = 1;
            entity.DATETIMEDELETED = DateTime.Now;

            return context.SaveChanges() != 0;
        }

        public KeyIndicatorAssessmentParametersViewModel GetKeyIndicatorAssessmentParameters()
        {
            KeyIndicatorAssessmentParametersViewModel corr = new KeyIndicatorAssessmentParametersViewModel();
            List<KeyIndicator> keyIndicators = new List<KeyIndicator>();

            List<ParameterScoreViewModel> parameters = context.TBL_CORR_ASSESSMENT_PARAMETER
                .Where(x => x.ISACTIVE == true)
                .Select(x => new ParameterScoreViewModel
                {
                    keyIndicatorId = x.KEYINDICATORID,
                    id = x.ASSESSMENTPARAMETERID,
                    weight = x.PERCENTAGEWEIGHT,
                    parameterName = x.PARAMETERNAME
                })
                .ToList();

            var indicators = context.TBL_CORR_KEY_INDICATOR.Where(x => x.ISACTIVE == true).ToList();

            foreach (var indicator in indicators)
            {
                KeyIndicator indicatorCategory = new KeyIndicator();
                indicatorCategory.keyIndicatorWeight = indicator.KEYINDICATORID;
                indicatorCategory.parameters = parameters.Where(x => x.keyIndicatorId == indicator.KEYINDICATORID).ToList();
                indicatorCategory.keyIndicatorName = indicator.INDICATORNAME;
                keyIndicators.Add(indicatorCategory);
            }

            corr.count = keyIndicators.Count();
            corr.keyIndicators = keyIndicators;

            return corr;
        }

        public CreditOfficerRiskRatingDetail GetCurrentCreditOfficerRiskRating(int id)
        {
            CreditOfficerRiskRatingDetail result = new CreditOfficerRiskRatingDetail();

            var lastRating = context.TBL_CORR_OFFICER_RATING
               .Where(x => x.STAFFID == id)
               .OrderByDescending(x => x.OFFICERRATINGID)
               .FirstOrDefault();

            if (lastRating != null)
            {
                result.score = lastRating.CORRSCORE;
                result.comment = lastRating.CORRCOMMENT;
            }

            result.parameters = context.TBL_CORR_OFFICER_RATING_DETAIL
                .Join(context.TBL_CORR_ASSESSMENT_PARAMETER, d => d.ASSESSMENTPARAMETERID, p => p.ASSESSMENTPARAMETERID, (d, p) => new { d, p })
                .Select(x => new GenericRiskScore
                {
                    id = x.d.OFFICERRATINGDETAILID,
                    name = x.p.PARAMETERNAME,
                    score = x.d.SCORE,
                    weight = x.p.PERCENTAGEWEIGHT,
                    indicatorId = x.p.KEYINDICATORID,
                    indicatorName = x.p.TBL_CORR_KEY_INDICATOR.INDICATORNAME,
                    indicatorWeight = x.p.TBL_CORR_KEY_INDICATOR.PERCENTAGEWEIGHT,
                })
                .ToList();

            var indicators = context.TBL_CORR_KEY_INDICATOR.Where(x => x.ISACTIVE == true).ToList();

            foreach (var indicator in indicators)
            {
                if (result.parameters.Where(x => x.indicatorId == indicator.KEYINDICATORID).Any())
                {
                    GenericRiskScore temp = new GenericRiskScore();
                    temp.id = indicator.KEYINDICATORID;
                    temp.name = indicator.INDICATORNAME;
                    temp.weight = indicator.PERCENTAGEWEIGHT;
                    temp.score = result.parameters.Where(x => x.indicatorId == indicator.KEYINDICATORID).Sum(x => x.score);
                    result.indicators.Add(temp);
                }
            }

            return result;
        }
    }
}
