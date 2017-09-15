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

        public IEnumerable<AssessmentFormViewModel> GetRiskFormElements(int companyId, int titleId, int applicationId) // GET FORM OR EXISTING
        {
            bool assessmentExist = context.tbl_Risk_Assessment_Title
                            .Where(x => x.RiskAssessmentTitleId == titleId)
                            .SelectMany(x => x.tbl_Risk_Assessment)
                            .Where(x => x.LoanApplicationId == applicationId)
                            .Any();

            IEnumerable<AssessmentFormViewModel> results;
            IEnumerable<tbl_Risk_Assessment_Index> indexes;
            IEnumerable<AssessmentFormViewModel> assessments;
            if (assessmentExist)
            {
                indexes = context.tbl_Risk_Assessment_Index.Where(x =>
                        x.CompanyId == companyId
                        && x.Deleted == false
                        && x.RiskAssessmentTitleId == titleId
                     );

                // build results
                int n = 0;
                List<AssessmentFormViewModel> merge = new List<AssessmentFormViewModel>();
                foreach (var x in indexes.Where(x => x.ParentId == null))
                {
                    merge.Add(new AssessmentFormViewModel
                    {
                        id = n + 1,
                        riskId = x.RiskId,
                        name = x.Name,
                        description = x.Description,
                        weight = x.Weight,
                        parentId = x.ParentId,
                        level = x.ItemLevel,
                        indexTypeId = x.IndexTypeId,
                        titleName = x.tbl_Risk_Assessment_Title.RiskTitle,
                        titleId = x.RiskAssessmentTitleId,
                        assessmentId = 0,
                        score = 0,
                        selected = false
                    });
                }

                assessments = indexes.Join(context.tbl_Risk_Assessment
                        .Where(x => x.LoanApplicationId == applicationId),
                        a => a.RiskId, b => b.RiskIndexId, (a, b) => new { a, b })
                    .ToList()
                    .Select((x, index) => new AssessmentFormViewModel
                    {
                        id = index + n + 1,
                        riskId = x.a.RiskId,
                        name = x.a.Name,
                        description = x.a.Description,
                        weight = x.a.Weight,
                        parentId = x.a.ParentId,
                        level = x.a.ItemLevel,
                        indexTypeId = x.a.IndexTypeId,
                        titleName = x.a.tbl_Risk_Assessment_Title.RiskTitle,
                        titleId = x.a.RiskAssessmentTitleId,
                        assessmentId = x.b.RiskAssessmentId,
                        score = Math.Round(x.b.IndexScore, 2),
                        selected = x.b.Selected
                    });

                foreach (var x in assessments)
                {
                    merge.Add(new AssessmentFormViewModel
                    {
                        id = n + 1,
                        riskId = x.riskId,
                        name = x.name,
                        description = x.description,
                        weight = x.weight,
                        parentId = x.parentId,
                        level = x.level,
                        indexTypeId = x.indexTypeId,
                        titleName = x.titleName,
                        titleId = x.titleId,
                        assessmentId = x.assessmentId,
                        score = x.score,
                        selected = x.selected
                    });
                }

                return merge.AsEnumerable();
            }

            results = context.tbl_Risk_Assessment_Title.Where(x =>
                    x.CompanyId == companyId
                    && x.Deleted == false
                    && x.RiskAssessmentTitleId == titleId)
                .SelectMany(x => x.tbl_Risk_Assessment_Index)
                .ToList()
                .Select((x, index) => new AssessmentFormViewModel
                {
                    id = index + 1,
                    riskId = x.RiskId,
                    name = x.Name,
                    description = x.Description,
                    weight = x.Weight,
                    parentId = x.ParentId,
                    level = x.ItemLevel,
                    indexTypeId = x.IndexTypeId,
                    titleName = x.tbl_Risk_Assessment_Title.RiskTitle,
                    titleId = x.RiskAssessmentTitleId,
                    assessmentId = 0,
                    score = 0,
                    selected = false
                });

            return results;
        }

        public IEnumerable<AssessmentFormViewModel> SaveFormElements(AssessmentFormSaveViewModel entity) // SAVING AND FINISHING
        {
            var data = context.tbl_Risk_Assessment.Where(x => x.CompanyId == entity.companyId
                    && x.LoanApplicationId == entity.loanApplicationId
                    && x.RiskAssessmentTitleId == entity.riskAssessmentTitleId
                );

            tbl_Risk_Assessment assessment;
            foreach (var item in entity.indexFields)
            {
                assessment = data.FirstOrDefault(x => x.RiskIndexId == item.riskId);
                if (assessment == null)
                {
                    context.tbl_Risk_Assessment.Add(new tbl_Risk_Assessment
                    {
                        RiskIndexId = item.riskId,
                        ParentId = item.parentId,
                        RefCode = "n/a",
                        LoanApplicationId = entity.loanApplicationId,
                        RiskAssessmentTitleId = entity.riskAssessmentTitleId,
                        Selected = item.selected,
                        IndexScore = ComputeScore(entity.indexFields, item.riskId, item.weight),
                        CompanyId = entity.companyId,
                        CreatedBy = entity.createdBy,
                        DateTimeCreated = DateTime.Now,
                    });
                }
                else
                {
                    assessment = data.FirstOrDefault(x => x.RiskIndexId == item.riskId);
                    assessment.DateTimeUpdated = DateTime.Now;
                    assessment.LastUpdatedBy = entity.lastUpdatedBy;
                    assessment.Selected = item.selected;
                    assessment.IndexScore = ComputeScore(entity.indexFields, item.riskId, item.weight);
                }
            }
            context.SaveChanges();

            if (entity.command == "finish")
            {
                FinishAssessment(entity);
            }

            IEnumerable<AssessmentFormViewModel> results;

            results = context.tbl_Risk_Assessment_Title.Where(x =>
                    x.CompanyId == entity.companyId
                    && x.Deleted == false
                    && x.RiskAssessmentTitleId == entity.riskAssessmentTitleId
                 )
                .SelectMany(x => x.tbl_Risk_Assessment_Index)
                .Join(context.tbl_Risk_Assessment.Where(x => x.LoanApplicationId == entity.loanApplicationId),
                    a => a.RiskId, b => b.RiskIndexId, (a, b) => new { a, b })
                .ToList()
                .Select((x, index) => new AssessmentFormViewModel
                {
                    id = index + 1,
                    riskId = x.a.RiskId,
                    name = x.a.Name,
                    description = x.a.Description,
                    weight = x.a.Weight,
                    parentId = x.a.ParentId,
                    level = x.a.ItemLevel,
                    indexTypeId = x.a.IndexTypeId,
                    titleName = x.a.tbl_Risk_Assessment_Title.RiskTitle,
                    titleId = x.a.RiskAssessmentTitleId,
                    assessmentId = x.b.RiskAssessmentId,
                    score = Math.Round(x.b.IndexScore, 2),
                    selected = x.b.Selected
                });

            return results;
        }

        private void FinishAssessment(AssessmentFormSaveViewModel entity) // TODO: recursive compute
        {
            var result = context.tbl_Risk_Assessment_Result.Where(o =>
                o.Deleted == false
                && o.LoanApplicationId == entity.loanApplicationId
                && o.RiskAssessmentTitleId == entity.riskAssessmentTitleId
            );

            var totalScore = ComputeTotalScore(entity);

            if (result.Any())
            {
                var change = result.First(); // exception!!!
                change.TotalScore = totalScore;
                change.CreditRating = GetCreditRating(totalScore);
                change.DateTimeUpdated = DateTime.Now;
                change.LastUpdatedBy = entity.lastUpdatedBy;
            }
            else
            {
                context.tbl_Risk_Assessment_Result.Add(new tbl_Risk_Assessment_Result
                {
                    LoanApplicationId = entity.loanApplicationId,
                    TotalScore = totalScore,
                    CreditRating = GetCreditRating(totalScore),
                    CompanyId = (short)entity.companyId,
                    CreatedBy = entity.createdBy,
                    DateTimeCreated = DateTime.Now,
                    RiskAssessmentTitleId = entity.riskAssessmentTitleId,
                });
            }

            context.SaveChanges();
        }

        private string GetCreditRating(decimal totalScore)
        {
            if (totalScore > 100) { return "total > 100"; }
            var rating = context.tbl_Risk_Rating.FirstOrDefault(x => x.MinRange <= totalScore && totalScore <= x.MaxRange);
            if (rating == null) { return "Undefined"; }
            return rating.Rates;
        }

        private decimal ComputeTotalScore(AssessmentFormSaveViewModel entity)
        {
            var scores = context.tbl_Risk_Assessment.Where(x => x.IndexScore > 0
                                                            && x.LoanApplicationId == entity.loanApplicationId
                                                            && x.RiskAssessmentTitleId == entity.riskAssessmentTitleId
                                                    ).ToList();
            if (scores.Any() == false) { return 0; }
            return scores.Sum(x => x.IndexScore);
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

        public IEnumerable<AssessmentResultViewModel> GetAllAssessmentResultByApplicationId(int companyId, int applicationId) // RESULTS
        {
            return this.GetAllAssessmentResult(companyId).Where(x => x.loanApplicationId == applicationId);
        }

        public IEnumerable<AssessmentResultViewModel> GetAllAssessmentResult(int companyId)
        {
            var titles = context.tbl_Risk_Assessment_Title.Where(x => x.Deleted == false && x.CompanyId == companyId);

            return this.context.tbl_Risk_Assessment_Result.Where(x => x.Deleted == false)
                .Join(context.tbl_Loan_Application, a => a.LoanApplicationId, b => b.LoanApplicationId, (a, b) => new { a, b })
                .Select(x => new AssessmentResultViewModel
                {
                    assessmentResultId = x.a.AssessmentResultId,
                    loanApplicationId = x.a.LoanApplicationId,
                    refrenceNumber = x.b.ApplicationReferenceNumber,
                    customerName = x.b.tbl_Customer.FirstName + " " + x.b.tbl_Customer.MiddleName + " " + x.b.tbl_Customer.LastName,
                    riskAssessmentTitleId = x.a.RiskAssessmentTitleId,
                    assessmentTitle = titles.FirstOrDefault(t => t.RiskAssessmentTitleId == x.a.RiskAssessmentTitleId) == null ? string.Empty : titles.FirstOrDefault(t => t.RiskAssessmentTitleId == x.a.RiskAssessmentTitleId).RiskTitle,
                    creditRating = x.a.CreditRating,
                    totalScore = x.a.TotalScore,
                    createdBy = x.a.CreatedBy,
                    dateTimeCreated = x.a.DateTimeCreated,
                    dateTimeUpdated = x.a.DateTimeUpdated,
                });
        }
    }
}
