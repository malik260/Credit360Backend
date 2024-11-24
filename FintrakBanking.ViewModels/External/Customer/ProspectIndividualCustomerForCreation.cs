using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class ProspectIndividualCustomerForCreation
    { 
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

        //public string registrationNumber { get; set; }
        //public string companyWebsite { get; set; }
        //public string companyEmail { get; set; }
        //public string registeredOffice { get; set; }
        //public string annualTurnOver { get; set; }
        //public string corporateBusinessCategory { get; set; }
        //public string creditRating { get; set; }
        //public string previousCreditRating { get; set; }
        //public decimal? paidUpCapital { get; set; }
        //public decimal? authorizedCapital { get; set; }
        //public decimal? shareholderFund { get; set; }
        //public bool startLoan { get; set; }
        public ContactAddressForCustomer contactAddress { get; set; }
        public ContactPhoneNoForCustomer contactPhone { get; set; }
        public NextOfKin nextOfKin { get; set; }
        public bool isInsiderRelated { get; set; } = false;
        public InsiderRelated insiderRelated { get; set; }
    }
}
