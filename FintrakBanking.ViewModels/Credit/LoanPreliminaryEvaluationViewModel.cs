using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanPreliminaryEvaluationViewModel: GeneralEntity
    {
        

        public LoanPreliminaryEvaluationViewModel()
        {
            customerBvnInformation = new List<CustomerBvnViewModels>();
            customerCompanyDirectors = new List<CustomerCompanyDirectorsViewModels>();
            customerCompanyShareholders = new List<CustomerCompanyShareholdersViewModels>();
            customerClients = new List<CustomerClientOrSupplierViewModels>();
            customerSuppliers = new List<CustomerSupplierViewModels>();
        }
        public bool isHouRouting { get; set; }
        public short loanApplicationtypeId { get; set; }
        public string relatedCompanies { get; set; }
        public int loanPreliminaryEvaluationId { get; set; }
        public string preliminaryEvaluationCode { get; set; }
        public short branchId { get; set; }
        public string branchName { get; set; }
        public int? customerId { get; set; }
        public string customerName { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string projectDescription { get; set; }
        public string ownershipStructure { get; set; }
        public string clientDescription { get; set; }
        public string registrationNumber { get; set; }
        public string taxIdentificationNumber { get; set; }
        public string existingExposure { get; set; }
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
        public bool sendForEvaluation { get; set; }
        public bool sentForLoanApplication { get; set; }
        public bool isCurrent { get; set; }
        public int operationId { get; set; }
        public short approvalStatusId { get; set; }

        public List<CustomerBvnViewModels> customerBvnInformation { get; set; }
        public List<CustomerCompanyShareholdersViewModels> customerCompanyShareholders { get; set; }
        public List<CustomerCompanyDirectorsViewModels> customerCompanyDirectors { get; set; }
        public string customerCode { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public decimal loanAmount { get; set; }
        public short subSectorId { get; set; }
        public string subSectorName { get; set; }
        public short sectorId { get; set; }
        public short loanTypeId { get; set; }
        public string loanTypeName { get; set; }
        public string customerAccountNumber { get; set; }
        public short? customerTypeId { get; set; }
        public int? customerGroupId { get; set; }
        public string customerGroupCode { get; set; }
        public List<CustomerClientOrSupplierViewModels> customerClients { get; set; }
        public List<CustomerSupplierViewModels> customerSuppliers { get; set; }
        public string customerGroupName { get; set; }
        public List<CustomerGroupMappingViewModel> customerGroupMappings { get; set; }
        public int penId { get; set; }
        public int? capRegionId { get; set; }
        public int? levelTypeId { get; set; }

    }
}
