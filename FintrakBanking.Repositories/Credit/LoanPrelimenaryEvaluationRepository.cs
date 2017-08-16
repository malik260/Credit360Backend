using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(ILoanPrelimenaryEvaluationRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanPrelimenaryEvaluationRepository : ILoanPrelimenaryEvaluationRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkFlowRepository workFlow;
        private IApprovalLevelStaffRepository level;


        public LoanPrelimenaryEvaluationRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository _genSetup, IWorkFlowRepository _workFlow,
        FinTrakBankingContext _context, IApprovalLevelStaffRepository _level)
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            workFlow = _workFlow;
            level = _level;
        }

        private bool SaveAll()
        {
            return context.SaveChanges() > 0;
        }

        public bool AddPrelimenaryEvaluation(LoanPrelimenaryEvaluationViewModel model)
        {
            if (model == null)
            {
                return false;
            }

            bool output = false;

            var penRecord = new tbl_Loan_Preliminary_Evaluation()
            {
                PreliminaryEvaluationCode = GeneratePENCode(model.companyId),
                AccountOfficer = model.accountOfficer,
                BankParticipationJustification = model.bankParticipationJustification,
                BankRole = model.bankRole,
                BranchId = model.userBranchId,
                BusinessProfile = model.businessProfile,
                ClientDescription = model.clientDescription,
                CollateralArrangement = model.collateralArrangement,
                CommercialViabilityAssessment = model.commercialViabilityAssessment,
                CompanyId = model.companyId,
                CustomerId = model.customerId,
                EnvironmentalImpact = model.environmentalImpact,
                ExistingExposure = model.exisitingExposure,
                ImplementationArrangements = model.implementationArrangements,
                MarketDemand = model.marketDemand,
                OwnershipStructure = model.ownershipStructure,
                PortfolioStrategicAlignment = model.portfolioStrategicAlignment,
                ProjectDescription = model.projectDescription,
                ProjectFinancingPlan = model.projectFinancingPlan,
                ProposedTermsAndConditions = model.proposedTermsAndConditions,
                RiskMitigants = model.riskMitigants,
                RisksAndConcerns = model.risksAndConcerns,
                SustainableBankingImplications = model.sustainableBankingImplications,
                PrudentialExposureLimitImplications = model.prudentialExposureLimitImplications,
                RelationshipManagerId = model.relationshipManagerId,
                RelationshipOfficerId = model.relationshipOfficerId,
                ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                IsCurrent = true,
                DateTimeCreated = genSetup.GetApplicationDate(),
                CreatedBy = model.createdBy
            };

            var customerRecord = context.tbl_Customer.FirstOrDefault(c => c.CustomerId == model.customerId);

            var auditRecord = new tbl_Audit()
            {
                AuditTypeId = (short)AuditTypeEnum.LoanPrelimenaryEvaluation,
                BranchId = model.userBranchId,
                StaffId = model.createdBy,
                Detail = $"Created Prelimenary Evaluation with code ({model.preliminaryEvaluationCode}) for customer {customerRecord.FirstName} {customerRecord.LastName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = genSetup.GetApplicationDate(),
            };

            if (workFlow.CheckRouteForOperation((int)Operations.LoanPrelimenaryEvaluation, penRecord.CompanyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.tbl_Loan_Preliminary_Evaluation.Add(penRecord);
                        context.tbl_Audit.Add(auditRecord);

                        output = SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = penRecord.CreatedBy,
                            companyId = penRecord.CompanyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = penRecord.LoanPreliminaryEvaluationId,
                            operationId = (int)Operations.LoanPrelimenaryEvaluation,
                            BranchId = model.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();
                    }

                    catch (Exception ex)
                    {
                        trans.Rollback();

                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }

            return output;
        }

        private string GeneratePENCode(int companyId)
        {
            var data = this.context.tbl_Loan_Preliminary_Evaluation.Count(x => x.CompanyId == companyId);
            int counter = data + 1;
            var penCode = string.Empty;

            penCode = string.Format("PEN -- {0}", counter.ToString().PadLeft(4, '0'));

            return penCode;
        }

        public IEnumerable<LoanPrelimenaryEvaluationViewModel> GetPrelimenaryEvaluationsAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)Operations.LoanPrelimenaryEvaluation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from pen in context.tbl_Loan_Preliminary_Evaluation
                        join coy in context.tbl_Company on pen.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on pen.BranchId equals br.BranchId
                        join atrail in context.tbl_Approval_Trail on pen.LoanPreliminaryEvaluationId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && pen.IsCurrent == true
                              && atrail.OperationId == (int)Operations.LoanPrelimenaryEvaluation && atrail.ToApprovalLevelId == staffApprovalLevelId
                        select new LoanPrelimenaryEvaluationViewModel()
                        {
                            companyId = pen.CompanyId,
                            companyName = pen.tbl_Company.Name,
                            accountOfficer = pen.AccountOfficer,
                            preliminaryEvaluationCode = pen.PreliminaryEvaluationCode,
                            bankParticipationJustification = pen.BankParticipationJustification,
                            bankRole = pen.BankRole,
                            branchId = br.BranchId,
                            branchName = br.BranchName,
                            businessProfile = pen.BusinessProfile,
                            clientDescription = pen.ClientDescription,
                            collateralArrangement = pen.CollateralArrangement,
                            commercialViabilityAssessment = pen.CommercialViabilityAssessment,
                            customerId = pen.CustomerId,
                            customerName = pen.tbl_Customer.FirstName + " " + pen.tbl_Customer.LastName,
                            environmentalImpact = pen.EnvironmentalImpact,
                            exisitingExposure = pen.ExistingExposure,
                            implementationArrangements = pen.ImplementationArrangements,
                            marketDemand = pen.MarketDemand,
                            ownershipStructure = pen.OwnershipStructure,
                            portfolioStrategicAlignment = pen.PortfolioStrategicAlignment,
                            projectDescription = pen.ProjectDescription,
                            projectFinancingPlan = pen.ProjectFinancingPlan,
                            proposedTermsAndConditions = pen.ProposedTermsAndConditions,
                            riskMitigants = pen.RiskMitigants,
                            risksAndConcerns = pen.RisksAndConcerns,
                            sustainableBankingImplications = pen.SustainableBankingImplications,
                            prudentialExposureLimitImplications = pen.PrudentialExposureLimitImplications,
                            relationshipManagerId = pen.RelationshipManagerId,
                            relationshipOfficerId = pen.RelationshipOfficerId,
                            operationId = atrail.OperationId,
                        });
            return data;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)Operations.LoanPrelimenaryEvaluation;

            var response = workFlow.GoForApproval(entity);

            if (response.Result.Item1)
            {
                return ApprovePrelimenaryEvaluation(entity.targetId, response.Result.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }
        }

        private bool ApprovePrelimenaryEvaluation(int loanPenId, short approvalStatusId, UserInfo user)
        {
            var penRecord = context.tbl_Loan_Preliminary_Evaluation.Find(loanPenId);

            penRecord.ApprovalStatusId = approvalStatusId;
            penRecord.DateApproved = DateTime.Now;
            penRecord.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanPrelimenaryEvaluation,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Prelimenary Evaluation with code ({penRecord.PreliminaryEvaluationCode})",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.context.SaveChanges() > 0;

        }
    }
}
