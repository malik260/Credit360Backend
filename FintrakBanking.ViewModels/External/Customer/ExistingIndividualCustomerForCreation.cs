using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class ExistingIndividualCustomerForCreation
    {
        public string customerCode { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.lastName; } }
        public string emailAddress { get; set; }

        public int crmsLegalStatusId { get; set; }
        public int crmsRelationshipTypeId { get; set; }
        public string taxNumber { get; set; } 
        public string branchCode { get; set; }


        public bool isEmailValidated { get; set; }
        public bool isBvnValidated { get; set; }
        public bool isPhoneValidated { get; set; }

        public string gender { get; set; }
        public string placeOfBirth { get; set; }
        public string nationality { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public string occupation { get; set; }
        public string customerBVN { get; set; }
        public int subSectorId { get; set; }
        public AccountCreationDetails accountDetails { get; set; }

        public ContactAddressForCustomer contactAddress { get; set; }
        public ContactPhoneNoForCustomer contactPhone { get; set; }
        public int? customerTypeId { get; set; } 
        public string employerNumber { get; set; }
        public int? profileSourceId { get; set; }
        //public NextOfKin nextOfKin { get; set; }
        //public bool isInsiderRelated { get; set; } = false;
        //public InsiderRelated insiderRelated { get; set; }
    }

    public class AccountCreationDetails
    {
        public string accountNumber { get; set; }
        public string productAccountName { get; set; }
        public string accountStatusName { get; set; }
        public DateTime dateOfEmployment { get; set; }
        public decimal monthlyIncome { get; set; }
        public string otherBankAccountNumber { get; set; }
        public string otherBankSortCode { get; set; }
        public string pmbNhfAccount { get; set; }
    }
}
