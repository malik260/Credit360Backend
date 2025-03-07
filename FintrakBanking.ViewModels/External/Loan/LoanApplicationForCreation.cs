using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.External.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanApplicationForCreation
    {
        //public string branchCode { get; set; }
        public int? loanId { get; set; }
        public string customerCode { get; set; }
        //public string loanInformation { get; set; }
        public int? loanApplicationSourceId { get; set; }
        public LoanApplicationDetailForCreation loanApplicationDetail { get; set; }
        public List<LoanApplicationDetailForCreation> loanApplicationDetails { get; set; }
        public AffordabilityViewModel affordabilityDetails { get; set; }

        // PROPERTIES THAT WILL BE AUTO GENERATED/GOTTEN
        public string applicationReferenceNumber { get; set; }
        public int branchId { get; set; }
        public int customerId { get; set; }
        public int? regionId { get; set; } = null;
        public short productClassId { get; set; }
        public decimal applicationAmount { get; set; }
        public int proposedTenor { get; set; }
        public int requireCollateral { get; set; } = 0;
        public short loanTypeId { get; set; } = (int)LoanTypeEnum.Single;
        public string misCode { get; set; }
        public int relationshipOfficerId { get; set; }
        public int isPoliticallyExposed { get; set; }
        public int isRelatedParty { get; set; }
        public int companyId { get; set; } = 1;
        public int createdBy { get { return this.relationshipOfficerId; } }
        public int LenderId { get; set; }

    }
}
