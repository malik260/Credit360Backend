using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Credit
{
    public class ContingentLoanViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
        public short branchId { get; set; }
        public short currencyId { get; set; }
        public double exchangeRate { get; set; }
        public int loanApplicationId { get; set; }
        public string loanReferenceNumber { get; set; }
        public short subSectorId { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal contingentAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public int approvalStatusId { get; set; }
        public string approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public bool isDisbursed { get; set; }
        public string disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public int? operationId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public string trancheBatchCode { get; set; }
        public bool dischargeLetter { get; set; }
        public short customerSensitivityLevelId { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; }
        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string SectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string loanStatusName { get; set; }

        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }
        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        //......End f Loan Relational Table View Mapping Models......//

    }

}
