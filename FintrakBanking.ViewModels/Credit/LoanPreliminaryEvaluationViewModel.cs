namespace FintrakBanking.ViewModels.Credit
{
    public class LoanPreliminaryEvaluationViewModel: GeneralEntity
    {
        public int loanPreliminaryEvaluationId { get; set; }
        public string preliminaryEvaluationCode { get; set; }
        public short branchId { get; set; }
        public string branchName { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string projectDescription { get; set; }
        public string ownershipStructure { get; set; }
        public string clientDescription { get; set; }
        public string registrationNumber { get; set; }
        public string taxIdentificationNumber { get; set; }
        public string exisitingExposure { get; set; }
        public string projectFinancingPlan { get; set; }
        public string bankRole { get; set; }
        public string proposedTermsAndConditions { get; set; }
        public string collateralArrangement { get; set; }
        public string implementationArrangements { get; set; }
        public string marketDemand { get; set; }
        public string businessProfile { get; set; }
        public string risksAndConcerns { get; set; }
        public string riskMitigants { get; set; }
        public string prudentialExposureLimitImplications { get; set; }
        public string environmentalImpact { get; set;}
        public string sustainableBankingImplications { get; set; }
        public string bankParticipationJustification { get; set; }
        public string portfolioStrategicAlignment { get; set; }
        public string commercialViabilityAssessment { get; set; }

        public int operationId { get; set; }
        public short approvalStatusId { get; set; }

    }
}
