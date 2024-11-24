using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class ExistingCorporateCustomerForCreation
    {  
        public string customerCode { get; set; }
        public string companyName { get; set; } 
        public int crmsCompanySizeId { get; set; }
        public int crmsLegalStatusId { get; set; }
        public int crmsRelationshipTypeId { get; set; }
        public DateTime dateOfIncorporation { get; set; }
        public string taxNumber { get; set; }
        public string emailAddress { get; set; }
        public string branchCode { get; set; }

        public bool isEmailValidated { get; set; }
        public bool isBvnValidated { get; set; }
        public bool isPhoneValidated { get; set; }


        public string registrationNumber { get; set; }
        public string companyWebsite { get; set; }
        public string companyEmail { get; set; }
        public string registeredOffice { get; set; }
        public string annualTurnOver { get; set; } 
        public decimal? paidUpCapital { get; set; }
        public decimal? authorizedCapital { get; set; }
        public decimal? shareholderFund { get; set; } 
    }
}
