using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels.Risk;
using FintrakBanking.ViewModels.Setups;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Web;


namespace FintrakBanking.Repositories.Risk
{
    public class RiskImplementation : IRiskImplementation
    {
        private FinTrakBankingContext context;

        public RiskImplementation(FinTrakBankingContext _context)
        {
            context = _context;
        }

        public IEnumerable<AssessmentFormViewModel> GetRiskFormElements(int companyId, int titleId, int? targetId) // GET FORM OR EXISTING
        {
            // assessmentId tobe changed to targetId
            // refactor: .Where(x => x.RISKASSESSMENTTITLEID == titleId && x.TARGETID == assessmentId),
            bool assessmentExist = context.TBL_RISK_ASSESSMENT_TITLE
                            .Where(x => x.RISKASSESSMENTTITLEID == titleId)
                            .SelectMany(x => x.TBL_RISK_ASSESSMENT)
                           // .Where(x => x.TARGETID == targetId || (targetId == null))
                            .Any();

            IEnumerable<AssessmentFormViewModel> results;
            IEnumerable<TBL_RISK_ASSESSMENT_INDEX> indexes;
            IEnumerable<AssessmentFormViewModel> assessments;

            if (assessmentExist)
            {
                indexes = context.TBL_RISK_ASSESSMENT_INDEX.Where(x => x.COMPANYID == companyId && x.DELETED == false && x.RISKASSESSMENTTITLEID == titleId);

                // build results
                int n = 0;
                List<AssessmentFormViewModel> merge = new List<AssessmentFormViewModel>();
                foreach (var x in indexes.Where(x => x.PARENTID == null))
                {
                    merge.Add(new AssessmentFormViewModel
                    {
                        id = n + 1,
                        riskId = x.RISKID,
                        name = x.NAME,
                        description = x.DESCRIPTION,
                        weight = x.WEIGHT,
                        parentId = x.PARENTID,
                        level = x.ITEMLEVEL,
                        indexTypeId = x.INDEXTYPEID,
                        titleName = x.TBL_RISK_ASSESSMENT_TITLE.RISKTITLE,
                        titleId = x.RISKASSESSMENTTITLEID,
                        assessmentId = 0,
                        score = 0,
                        selected = false
                    });
                }

                //assessments = indexes.Join(context.TBL_RISK_ASSESSMENT
                //        .Where(x => x.RISKASSESSMENTTITLEID == titleId && x.TARGETID == targetId),
                //        a => a.RISKID, b => b.RISKINDEXID, (a, b) => new { a, b })
                //    .ToList()
                //    .Select((x, index) => new AssessmentFormViewModel
                //    {
                //        id = index + n + 1,
                //        riskId = x.a.RISKID,
                //        name = x.a.NAME,
                //        description = x.a.DESCRIPTION,
                //        weight = x.a.WEIGHT,
                //        parentId = x.a.PARENTID,
                //        level = x.a.ITEMLEVEL,
                //        indexTypeId = x.a.INDEXTYPEID,
                //        titleName = x.a.TBL_RISK_ASSESSMENT_TITLE.RISKTITLE,
                //        titleId = x.a.RISKASSESSMENTTITLEID,
                //        assessmentId = x.b.RISKASSESSMENTID,
                //        score = Math.Round(x.b.INDEXSCORE, 2),
                //        selected = x.b.SELECTED
                //    });

                //foreach (var x in assessments)
                //{
                //    merge.Add(new AssessmentFormViewModel
                //    {
                //        id = n + 1,
                //        riskId = x.riskId,
                //        name = x.name,
                //        description = x.description,
                //        weight = x.weight,
                //        parentId = x.parentId,
                //        level = x.level,
                //        indexTypeId = x.indexTypeId,
                //        titleName = x.titleName,
                //        titleId = x.titleId,
                //        assessmentId = x.assessmentId,
                //        score = x.score,
                //        selected = x.selected
                //    });
                //}

                return merge.AsEnumerable();
            }

            results = context.TBL_RISK_ASSESSMENT_TITLE.Where(x =>
                    x.COMPANYID == companyId
                    && x.DELETED == false
                    && x.RISKASSESSMENTTITLEID == titleId)
                .SelectMany(x => x.TBL_RISK_ASSESSMENT_INDEX)
                .ToList()
                .Select((x, index) => new AssessmentFormViewModel
                {
                    id = index + 1,
                    riskId = x.RISKID,
                    name = x.NAME,
                    description = x.DESCRIPTION,
                    weight = x.WEIGHT,
                    parentId = x.PARENTID,
                    level = x.ITEMLEVEL,
                    indexTypeId = x.INDEXTYPEID,
                    titleName = x.TBL_RISK_ASSESSMENT_TITLE.RISKTITLE,
                    titleId = x.RISKASSESSMENTTITLEID,
                    assessmentId = 0,
                    score = 0,
                    selected = false
                });

            return results;
        }

        public IEnumerable<AssessmentFormViewModel> SaveFormElements(AssessmentFormSaveViewModel entity) // SAVING AND FINISHING
        {
            var data = context.TBL_RISK_ASSESSMENT.Where(x => x.COMPANYID == entity.companyId
                    //&& x.LOANAPPLICATIONID == entity.loanApplicationId
                    && x.RISKASSESSMENTTITLEID == entity.riskAssessmentTitleId
                );

            TBL_RISK_ASSESSMENT assessment;
            foreach (var item in entity.indexFields)
            {
                assessment = data.FirstOrDefault(x => x.RISKINDEXID == item.riskId);
                if (assessment == null)
                {
                    context.TBL_RISK_ASSESSMENT.Add(new TBL_RISK_ASSESSMENT
                    {
                        RISKINDEXID = item.riskId,
                        PARENTID = item.parentId,
                        REFCODE = "n/a",
                       // TARGETID = entity.targetId,
                        RISKASSESSMENTTITLEID = entity.riskAssessmentTitleId,
                        SELECTED = item.selected,
                        INDEXSCORE = ComputeScore(entity.indexFields, item.riskId, item.weight),
                        COMPANYID = entity.companyId,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now,
                    });
                }
                else
                {
                    assessment = data.FirstOrDefault(x => x.RISKINDEXID == item.riskId);
                    assessment.DATETIMEUPDATED = DateTime.Now;
                    assessment.LASTUPDATEDBY = entity.lastUpdatedBy;
                    assessment.SELECTED = item.selected;
                    assessment.INDEXSCORE = ComputeScore(entity.indexFields, item.riskId, item.weight);
                }
            }
            context.SaveChanges();

            if (entity.command == "finish")
            {
                FinishAssessment(entity);
            }

            IEnumerable<AssessmentFormViewModel> results;

            results = context.TBL_RISK_ASSESSMENT_TITLE.Where(x =>
                    x.COMPANYID == entity.companyId
                    && x.DELETED == false
                    && x.RISKASSESSMENTTITLEID == entity.riskAssessmentTitleId
                 )
                .SelectMany(x => x.TBL_RISK_ASSESSMENT_INDEX)
                .Join(context.TBL_RISK_ASSESSMENT,//.Where(x => x.LOANAPPLICATIONID == entity.loanApplicationId),
                    a => a.RISKID, b => b.RISKINDEXID, (a, b) => new { a, b })
                .ToList()
                .Select((x, index) => new AssessmentFormViewModel
                {
                    id = index + 1,
                    riskId = x.a.RISKID,
                    name = x.a.NAME,
                    description = x.a.DESCRIPTION,
                    weight = x.a.WEIGHT,
                    parentId = x.a.PARENTID,
                    level = x.a.ITEMLEVEL,
                    indexTypeId = x.a.INDEXTYPEID,
                    titleName = x.a.TBL_RISK_ASSESSMENT_TITLE.RISKTITLE,
                    titleId = x.a.RISKASSESSMENTTITLEID,
                    assessmentId = x.b.RISKASSESSMENTID,
                    score = Math.Round(x.b.INDEXSCORE, 2),
                    selected = x.b.SELECTED
                });

            return results;
        }

        private void FinishAssessment(AssessmentFormSaveViewModel entity) // TODO: recursive compute
        {
            var result = context.TBL_RISK_ASSESSMENT_RESULT.Where(o =>
                o.DELETED == false
               // && o.TARGETID == entity.targetId
                && o.RISKASSESSMENTTITLEID == entity.riskAssessmentTitleId
            );

            var totalScore = ComputeTotalScore(entity);

            if (result.Any())
            {
                var change = result.First(); // exception!!!
                change.TOTALSCORE = totalScore;
                change.CREDITRATING = GetCreditRating(totalScore);
                change.DATETIMEUPDATED = DateTime.Now;
                change.LASTUPDATEDBY = entity.lastUpdatedBy;
            }
            else
            {
                context.TBL_RISK_ASSESSMENT_RESULT.Add(new TBL_RISK_ASSESSMENT_RESULT
                {
                  //  TARGETID = entity.targetId,
                    TOTALSCORE = totalScore,
                    CREDITRATING = GetCreditRating(totalScore),
                    COMPANYID = (short)entity.companyId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    RISKASSESSMENTTITLEID = entity.riskAssessmentTitleId,
                });
            }

            context.SaveChanges();
        }

        private string GetCreditRating(decimal totalScore)
        {
            if (totalScore > 100) { return "total > 100"; }
            var rating = context.TBL_RISK_RATING.FirstOrDefault(x => x.MINRANGE <= totalScore && totalScore <= x.MAXRANGE);
            if (rating == null) { return "Undefined"; }
            return rating.RATES;
        }

        private decimal ComputeTotalScore(AssessmentFormSaveViewModel entity)
        {
            var scores = context.TBL_RISK_ASSESSMENT.Where(x => x.INDEXSCORE > 0
                                                            //&& x.LOANAPPLICATIONID == entity.loanApplicationId
                                                            && x.RISKASSESSMENTTITLEID == entity.riskAssessmentTitleId
                                                    ).ToList();
            if (scores.Any() == false) { return 0; }
            return scores.Sum(x => x.INDEXSCORE);
        }

        private decimal ComputeScore(List<AssessmentFormViewModel> indexFields, int id, decimal weight)
        {
            decimal score;

            var children = indexFields.Where(x => x.parentId == id);
            if (children.Any() == false)
            {
                return 0;
            }

            var checkedChild = children.Where(x => x.selected == true).FirstOrDefault();
            if (checkedChild == null)
            {
                return 0;
            }
            else
            {
                score = checkedChild.weight;
            }

            var sum = children.Sum(x => x.weight);

            return (sum == 0) ? 0 : (weight * (score / sum)); // parentScore = parentWeight * (scoreOfCheckedChild / sumOfChildren)
        }
        
        public IEnumerable<AssessmentResultViewModel> GetAllAssessmentResult(int companyId)
        {
            var titles = context.TBL_RISK_ASSESSMENT_TITLE.Where(x => x.DELETED == false && x.COMPANYID == companyId);

            return this.context.TBL_RISK_ASSESSMENT_RESULT.Where(x => x.DELETED == false)
                //.Join(context.TBL_LOAN_APPLICATION, a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Select(x => new AssessmentResultViewModel
                {
                    assessmentResultId = x.ASSESSMENTRESULTID,
                    //targetId = x.TARGETID,
                    riskAssessmentTitleId = x.RISKASSESSMENTTITLEID,
                    assessmentTitle = titles.FirstOrDefault(t => t.RISKASSESSMENTTITLEID == x.RISKASSESSMENTTITLEID) == null ? string.Empty : titles.FirstOrDefault(t => t.RISKASSESSMENTTITLEID == x.RISKASSESSMENTTITLEID).RISKTITLE,
                    creditRating = x.CREDITRATING,
                    totalScore = x.TOTALSCORE,
                    createdBy = x.CREATEDBY,
                    dateTimeCreated = x.DATETIMECREATED,
                    dateTimeUpdated = x.DATETIMEUPDATED,
                });
        }
    }
}
