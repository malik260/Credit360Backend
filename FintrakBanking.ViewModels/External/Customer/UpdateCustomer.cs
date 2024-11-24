using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class UpdateCustomer
    {
        public string customerCode { get; set; }
        public int branchId { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }   
        public string middleName { get; set; }   
        public string lastName { get; set; }   
        public int? maritalStatus { get; set; }
        public string emailAddress { get; set; }
        public string spouse { get; set; }
        public string occupation { get; set; }
        public int? customerTypeId { get; set; }
        public int isPoliticallyExposed { get; set; }
        public int isInvestmentGrade { get; set; }
        public string actedOnBy { get; set; }
        public string customerNin { get; set; }
        public int? lastUpdatedBy { get; set; }
        public DateTime? datetimeUpdated { get; set; }
        public DateTime? dateOfEmployment { get; set; }
        public decimal monthlyIncome { get; set; }
        public string otherBankSortCode { get; set; }
        public string otherBankAccount { get; set; }
        public string customerBVN { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string mobilePhoneNo { get; set; }
        public string officeLandNo { get; set; }
        public int? subSectorId { get; set; }
        public string employerNumber { get; set; }
        public bool? isBvnValidated { get; set; }
        public string gender { get; set; }
    }
}
