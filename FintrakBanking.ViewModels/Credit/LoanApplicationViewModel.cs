using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanApplicationViewModel : GeneralEntity
    {
        public int loanApplicationId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int? customerId { get; set; }
        public short branchId { get; set; }
        public short productClassId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public short currencyId { get; set; }
        public short loanStatusId { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal principalAmount { get; set; }
        public double interestRate { get; set; }
        public int tenor { get; set; }
        public short tenorModeId { get; set; }
        public string loanInformation { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public bool submittedForAppraisal { get; set; }
        public bool isRealatedParty { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public int approvalStatusId { get; set; }

        public string customerName { get; set; }
        public string branchName { get; set; }
        public string productClassName { get; set; }
        public string customerGroupName { get; set; }
        public string loanTypeName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string tenorModeName { get; set; }


        public string amount { get { return this.principalAmount.ToString("#,#.00#"); } }
        public string applicantName { get { return this.customerName + "" + this.customerGroupName; } }

        public int? loanPreliminaryEvaluationId { get; set; } 
        public double exchangeRate { get; set; }
        public string nearestLandMark { get; set; }
        public string nearestBusStop { get; set; }
        public decimal? longitude { get; set; }
        public decimal? latitude { get; set; } 
    }

    public class ProductClassViewModel
    {
        public short productClassId { get; set; }
        public string productClassName { get; set; }
        public short productClassTypeId { get; set; }
    }

}
