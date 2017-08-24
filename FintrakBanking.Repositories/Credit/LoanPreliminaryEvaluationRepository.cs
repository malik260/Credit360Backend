using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(ILoanPreliminaryEvaluationRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanPreliminaryEvaluationRepository : ILoanPreliminaryEvaluationRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkFlowRepository workFlow;
        private IApprovalLevelStaffRepository level;


        public LoanPreliminaryEvaluationRepository(IAuditTrailRepository _auditTrail,
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

        public bool AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model)
        {
            if (model == null)
            {
                return false;
            }



            bool output = false;

            var penRecord = new tbl_Loan_Preliminary_Evaluation()
            {
                PreliminaryEvaluationCode = GeneratePENCode(),
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
                AuditTypeId = (short)AuditTypeEnum.LoanPreliminaryEvaluationAdded,
                BranchId = model.userBranchId,
                StaffId = model.createdBy,
                Detail = $"Created Prelimenary Evaluation with code ({model.preliminaryEvaluationCode}) for customer {customerRecord.FirstName} {customerRecord.LastName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = genSetup.GetApplicationDate(),
            };

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.LoanPreliminaryEvaluation, penRecord.CompanyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.tbl_Loan_Preliminary_Evaluation.Add(penRecord);
                        context.tbl_Audit.Add(auditRecord);

                        output = SaveAll();


                        if (model.sendForApproval == true)
                        {
                            var entity = new ApprovalViewModel
                            {
                                staffId = penRecord.CreatedBy,
                                companyId = penRecord.CompanyId,
                                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                                targetId = penRecord.LoanPreliminaryEvaluationId,
                                operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
                                BranchId = model.userBranchId
                            };
                            var response = workFlow.LogForApproval(entity);
                        }

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


        public bool AddPreliminaryEvaluationForApproval(LoanPreliminaryEvaluationViewModel model)
        {
            if (model == null)
            {
                return false;
            }

            bool output = false;

            var penRecord = new tbl_Loan_Preliminary_Evaluation()
            {
                PreliminaryEvaluationCode = GeneratePENCode(),
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
                AuditTypeId = (short)AuditTypeEnum.LoanPreliminaryEvaluationAdded,
                BranchId = model.userBranchId,
                StaffId = model.createdBy,
                Detail = $"Created Prelimenary Evaluation with code ({model.preliminaryEvaluationCode}) for customer {customerRecord.FirstName} {customerRecord.LastName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = genSetup.GetApplicationDate(),
            };

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.LoanPreliminaryEvaluation, penRecord.CompanyId))
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
                            operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
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

        private string GeneratePENCode()
        {
            var data = this.context.tbl_Loan_Preliminary_Evaluation.Count();
            int counter = data + 1;
            var penCode = string.Empty;

            penCode = string.Format("PEN -- {0}", counter.ToString().PadLeft(4, '0'));

            return penCode;
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanPreliminaryEvaluation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from pen in context.tbl_Loan_Preliminary_Evaluation
                        join coy in context.tbl_Company on pen.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on pen.BranchId equals br.BranchId
                        join atrail in context.tbl_Approval_Trail on pen.LoanPreliminaryEvaluationId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && pen.IsCurrent == true
                              && atrail.OperationId == (int)OperationsEnum.LoanPreliminaryEvaluation && atrail.ToApprovalLevelId == staffApprovalLevelId
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = pen.CompanyId,
                            companyName = pen.tbl_Company.Name,
                            loanPreliminaryEvaluationId = pen.LoanPreliminaryEvaluationId,
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
                            exisitingExposure = (int)pen.ExistingExposure,
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
                            taxIdentificationNumber = pen.TaxIdentificationNumber,
                            registrationNumber = pen.RegistrationNumber,
                            operationId = atrail.OperationId,
                            dateTimeCreated = pen.DateTimeCreated
                        });
            return data;
        }

        public async Task<bool> GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.LoanPreliminaryEvaluation;

            var response = await workFlow.GoForApproval(entity);

            if (response.Item1)
            {
                return ApprovePreliminaryEvaluation(entity.targetId, response.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }
        }

        private bool ApprovePreliminaryEvaluation(int loanPenId, short approvalStatusId, UserInfo user)
        {
            var penRecord = context.tbl_Loan_Preliminary_Evaluation.Find(loanPenId);

            penRecord.IsCurrent = false;
            penRecord.ApprovalStatusId = approvalStatusId;
            penRecord.DateApproved = DateTime.Now;
            penRecord.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanPreliminaryEvaluationAdded,
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

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllLoanPreliminaryEvaluations()
        {
            var data = (from p in context.tbl_Loan_Preliminary_Evaluation
                        join coy in context.tbl_Company on p.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on p.BranchId equals br.BranchId
                        where p.IsCurrent == false && p.ApprovalStatusId == (int)ApprovalStatusEnum.Approved ||
                        p.ApprovalStatusId == (int)ApprovalStatusEnum.Pending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = p.CompanyId,
                            companyName = p.tbl_Company.Name,
                            preliminaryEvaluationCode = p.PreliminaryEvaluationCode,
                            bankParticipationJustification = p.BankParticipationJustification,
                            bankRole = p.BankRole,
                            branchId = br.BranchId,
                            branchName = br.BranchName,
                            businessProfile = p.BusinessProfile,
                            clientDescription = p.ClientDescription,
                            collateralArrangement = p.CollateralArrangement,
                            commercialViabilityAssessment = p.CommercialViabilityAssessment,
                            customerId = p.CustomerId,
                            customerName = p.tbl_Customer.FirstName + " " + p.tbl_Customer.LastName,
                            environmentalImpact = p.EnvironmentalImpact,
                            exisitingExposure = (int)p.ExistingExposure,
                            implementationArrangements = p.ImplementationArrangements,
                            marketDemand = p.MarketDemand,
                            ownershipStructure = p.OwnershipStructure,
                            portfolioStrategicAlignment = p.PortfolioStrategicAlignment,
                            projectDescription = p.ProjectDescription,
                            projectFinancingPlan = p.ProjectFinancingPlan,
                            proposedTermsAndConditions = p.ProposedTermsAndConditions,
                            riskMitigants = p.RiskMitigants,
                            risksAndConcerns = p.RisksAndConcerns,
                            sustainableBankingImplications = p.SustainableBankingImplications,
                            prudentialExposureLimitImplications = p.PrudentialExposureLimitImplications,
                            relationshipManagerId = p.RelationshipManagerId,
                            relationshipOfficerId = p.RelationshipOfficerId,
                            taxIdentificationNumber = p.TaxIdentificationNumber,
                            registrationNumber = p.RegistrationNumber,
                        }).ToList();

            return data;
        }


        public bool UpdatePreliminaryEvaluation(int loanPenId, LoanPreliminaryEvaluationViewModel model)
        {
            var penRecord = context.tbl_Loan_Preliminary_Evaluation.Find(loanPenId);

            bool output = false;

            penRecord.PreliminaryEvaluationCode = model.preliminaryEvaluationCode;
            penRecord.BankParticipationJustification = model.bankParticipationJustification;
            penRecord.BankRole = model.bankRole;
            penRecord.BranchId = model.userBranchId;
            penRecord.BusinessProfile = model.businessProfile;
            penRecord.ClientDescription = model.clientDescription;
            penRecord.CollateralArrangement = model.collateralArrangement;
            penRecord.CommercialViabilityAssessment = model.commercialViabilityAssessment;
            penRecord.CompanyId = model.companyId;
            penRecord.CustomerId = model.customerId;
            penRecord.EnvironmentalImpact = model.environmentalImpact;
            penRecord.ExistingExposure = model.exisitingExposure;
            penRecord.ImplementationArrangements = model.implementationArrangements;
            penRecord.MarketDemand = model.marketDemand;
            penRecord.OwnershipStructure = model.ownershipStructure;
            penRecord.PortfolioStrategicAlignment = model.portfolioStrategicAlignment;
            penRecord.ProjectDescription = model.projectDescription;
            penRecord.ProjectFinancingPlan = model.projectFinancingPlan;
            penRecord.ProposedTermsAndConditions = model.proposedTermsAndConditions;
            penRecord.RiskMitigants = model.riskMitigants;
            penRecord.RisksAndConcerns = model.risksAndConcerns;
            penRecord.SustainableBankingImplications = model.sustainableBankingImplications;
            penRecord.PrudentialExposureLimitImplications = model.prudentialExposureLimitImplications;
            penRecord.RelationshipManagerId = model.relationshipManagerId;
            penRecord.RelationshipOfficerId = model.relationshipOfficerId;
            penRecord.ApprovalStatusId = (short)ApprovalStatusEnum.Pending;
            penRecord.IsCurrent = true;
            penRecord.DateTimeUpdated = DateTime.Now;
            penRecord.CreatedBy = model.createdBy;


            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanPreliminaryEvaluationUpdated,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Prelimenary Evaluation with code ({penRecord.PreliminaryEvaluationCode}) updated",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = loanPenId

            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    this.auditTrail.AddAuditTrail(audit);
                    // Audit Section ---------------------------

                    output = this.SaveAll();

                    if (model.sendForApproval == true)
                    {
                        var entity = new ApprovalViewModel
                        {
                            staffId = penRecord.CreatedBy,
                            companyId = penRecord.CompanyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = loanPenId,
                            operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
                            BranchId = model.userBranchId
                        };

                        var response = workFlow.LogForApproval(entity);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    throw new Exception(ex.Message);
                }

            }

            return output;

        }
    }
}
