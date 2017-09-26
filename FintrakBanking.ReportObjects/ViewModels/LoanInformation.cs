using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ViewModels
{
    public class LoanInformation : LoanScheduleViewModel
    {
        public string accountNumber { get; set; }
        public string productName { get; set; }
        public int customerId { get; set; }
        public int tearmLoanId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string branchCode { get; set; }
        public string branchName { get; set; }
        // public DateTime effectiveDate { get; set; }
        public string frequencyType { get; set; }
        public int frequancy { get; set; }
        public string customerName { get { return $"{lastName } {firstName}  {lastName}"; } }
        public string customerCode { get; set; }
        public string loanTypeName { get; set; }
        public string companyName { get; set; }
        public string loanRefrenceNumber { get; set; }
        public decimal principalAmount { get; set; }
        public double interestRate { get; set; }
        public double tenor { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime reportDateTime { get { return DateTime.Now; } }
        public decimal outstandingPrincipal { get; set; }
        public decimal outstandingInterest { get; set; }
        public decimal totalOutstanding { get { return (outstandingInterest + outstandingPrincipal); } }
    }


}
