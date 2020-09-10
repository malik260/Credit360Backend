using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    //class LoanStatementViewModel
    //{
    //}

    public class LoanStatementViewModel
    {
        //public int loanApplicationDetailId { set; get; }
        public string firstName { set; get; }
        public string lastName { set; get; }
        //public string buCode { get; set; }
        public string budescription { set; get; }
        public string teamDescription { set; get; }
        public string deskDescription { set; get; }
        public string groupDescription { set; get; }
        public string region { set; get; }
        public string staffCode { get; set; }
        public string middleName { set; get; }
        public string customerName { get { return lastName + " " + firstName + " " + middleName; } }
        public string accountNumber { set; get; }
        public string productName { set; get; }
        public string loanRefrenceNumber { set; get; }
        public decimal grantedAmount { set; get; }
        public string loanCurrency { set; get; }
        public decimal productId { set; get; }

        public DateTime? postDate { set; get; }
        public DateTime? valueDate { set; get; }
        public decimal creditAmount { set; get; }
        public decimal debitAmount { set; get; }
        public string discription { set; get; }
        public string transactionCurrency { set; get; }
        public string companyName { get; set; }
        public string logoPath { get; set; }
        public decimal balance { get; set; }
        public string applicationRefrenceNumber { get; set; }

        public string facilityType { get; set; }

        // public string buCode { get; set; }
        //public string deskCode { get; set; }
        //public string groupCode { get; set; }
        //public string TeamCode { get; set; }

        //public string buDescription { set; get; }
        //public string teamDescription { set; get; }
        //public string misCode { set; get; }
        //public string businessUnit { set; get; }

    }
}
