using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CreditBereauViewModel : GeneralEntity
    {
        public bool appliedsearchForLoan;
        public bool appliedSearchForLoan;
        public bool isMandatory;
        public bool useIntegration;
        public bool hasFile;
        public string fileName;

        public int customerId { get; set; }
        public string accountNumber { get; set; }
        public string accountStatus { get; set; }
        public double outstandingBalance { get; set; }
        public double installmentAmount { get; set; }
        public double currentAccount { get; set; }
        public double savingsAccount { get; set; }

        public short creditBureauId { get; set; }
        public string creditBureauName { get; set; }
        public decimal corporateChargeAmount { get; set; }
        public decimal retailChargeAmount { get; set; }
        public bool inUse { get; set; }
        public LoanCreditBereauViewModel LoanCreditBereauReport { get; set; }

    }

    public class CRCBureauFacilityViewModel
    {
        public string productCode { get; set; }
        public string productName { get; set; }
    }

    public class LoanCreditBereauViewModel : GeneralEntity
    {
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
        public DateTime dateCompleted { get; set; }
    }
     
}
