using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
   public class LoanMonthlyRepaymentViewModel
    {
        public string phoneNumber { get; set; }
        public int customerId { get; set; }
        public DateTime maturityDate { get; set; }
        public object intrestrate { get; set; }
        public decimal grantedAmount { get; set; }
        public decimal outstandingIntrestAmt { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public DateTime paymentdate { get; set; }
        public decimal scheduledRepaymentAmt { get; set; }
        public string loanRefrenceNumber { get; set; }
        public decimal totalperiodicPaymentAmt { get; set; }
        public decimal periodicInterestAmt { get; set; }
        public decimal periodicPrincipalAmt { get; set; }
        public string accountNumber { get; set; }
        public string productName { get; set; }
        public int productId { get; set; }
        public string companyName { get; set; }
        public string logoPath { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string customerName { get { return lastName + " " + firstName + " " + middleName; } }
        public string applicationRefrenceNumber { get; set; }
        public string emailAddress { get; set; }
    }
}
