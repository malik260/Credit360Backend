using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CreditBereauViewModel : GeneralEntity
    {
        public short currencyId { get; set; }

        public bool debitBusiness { get; set; }
        public int glAccountId { get; set; }
        public bool debitRequest { get; set; }

        public bool appliedsearchForLoan { get; set; }
        public bool appliedSearchForLoan { get; set; }
        public bool isMandatory { get; set; }
        public bool useIntegration { get; set; }
        public bool hasFile { get; set; }
        public string fileName { get; set; }
        public decimal feeAmount { get; set; }
        public decimal individualChargeAmount { get; set; }
        public decimal corporateChargeAmount { get; set; }
        public string referenceNumber { get; set; }
        public int customerId { get; set; }
        public int? casaAccountId { get; set; }
        public int creditGl { get; set; }
        public int debitGl { get; set; }
        public string accountNumber { get; set; }
        public string accountStatus { get; set; }
        public double outstandingBalance { get; set; }
        public double installmentAmount { get; set; }
        public double currentAccount { get; set; }
        public double savingsAccount { get; set; }

        public short creditBureauId { get; set; }
        public string creditBureauName { get; set; }
        public decimal retailChargeAmount { get; set; }
        public bool inUse { get; set; }
        public byte[] fileData { get; set; }
        public LoanCreditBureauViewModel LoanCreditBereauReport { get; set; }

    }

    public class CRCBureauFacilityViewModel
    {
        public string productCode { get; set; }
        public string productName { get; set; }
    }

    public class LoanCreditBureauViewModel : GeneralEntity
    {
        public int dayAgo { get; set; }

        public short creditBureauId { get; set; }
        public string creditBureauName { get; set; }
        public int customerCreditBureauId { get; set; }
        public int? companyDirectorId { get; set; }
        public string companyDirectorName { get; set; }
        public bool usedIntegration { get; set; }
        public bool isReportOkay { get; set; }
        public int searchCount { get; set; }
        public int uploadCount { get; set; }
        public int documentId { get; set; }

        public int applicationCreditBureauId { get; set; }
        public int loanApplicationId { get; set; }
        public int customerId { get; set; }
        public decimal chargeAmount { get; set; }
        public bool isComplete { get; set; }
        public bool debitBusiness { get; set; }
        public string accountNumber { get; set; }
        public DateTime? dateCompleted { get; set; }
    }


    public class CreditBureauDocument : GeneralEntity
    {
        public int documentId { get; set; }

        public int customerCreditBureauId { get; set; }

        public string documentTitle { get; set; }

        public string fileName { get; set; }

        public string fileExtension { get; set; }

        public byte[] fileData { get; set; }
    }
     
}
