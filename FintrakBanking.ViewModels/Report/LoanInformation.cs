using FintrakBanking.ReportObjects.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Reports
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
        public string companylogo { get; set; }
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

    public class DisburstLoanViewModel
    {
        public decimal  outstandingInterest { get; set; }
        public double approvedInterestRate { get; set; }
        private int da { get { return (maturitydate - effectiveDate).Days; } }
        public int approvedTenor { get { return (int)(Math.Round(da * (decimal)(12.0 / 365.0))); } }
        public double exchangeValue { get; set; }
        public decimal productId { get; set; }
        public string companyName { get; set; }
        public string customerName { get; set; }
        public string productName { get; set; }
        public decimal approvedAmount { get; set; }
        public string applicationReferenceNumber { get; set; }
        public decimal amountDisbursed { get; set; }
        public string accountNumber { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturitydate { get; set; }
        public DateTime? disburseDate { get; set; }
        public string facilityCurrency { get; set; }
        public double exchangeRate { get; set; }
        public string baseCurrency { get; set; }
        public string logoPath { get; set; }
        public string status { get; set; }
        public string bookingRef { get; set; }

    }

    public class DateRange
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }

    public class ReportSearchEntity
    {
        public int? branchId { get; set; }

        public int? typeId { get; set; }
        public int? staffId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool excludeSystem { get; set; }
    }
    public class AllLoanViewModel
    {
        public string loanStatus { get; set; }
        public string bookingNumber { get; set; }
        public DateTime bookingDate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime interestDateinView { get { return DateTime.Now.Date; } }
        public DateTime? disburseDate { get; set; }
        public int tenor { get { return (effectiveDate - maturityDate).Days; } }
        public int tenorTillDate { get { return (effectiveDate - DateTime.Now.Date).Days; } }
        public string customerName { get; set; }
        public decimal principalAmount { get; set; }
        public double? rate { get; set; }
        public double rateCharged { get; set; }
        public decimal interestToDate { get; set; }
        public string payAccountTo { get; set; }
        public string receiveAccountFrom { get; set; }
        public string remarks { get; set; }
        public string currency { get; set; }
        public string businessGroup { get; set; }
        public string requestState { get; set; }
    }

    public class InterestOnLoansViewModel
    {
        public string status { get; set; }
        public string bookingNumber { get; set; }
        public DateTime bookingDate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime interestDateinView { get { return DateTime.Now.Date; } }
        public DateTime? disburseDate { get; set; }
        public int tenor { get { return (effectiveDate - maturityDate).Days; } }
        public int tenorTillDate { get { return (effectiveDate - DateTime.Now.Date).Days; } }
        public string customerName { get; set; }
        public decimal principalAmount { get; set; }
        public double? rate { get; set; }
        public double rateCharged { get; set; }
        public decimal interestToDate { get; set; }
        public string payAccountTo { get; set; }
        public string receiveAccountFrom { get; set; }
        public string remarks { get; set; }
        public string currency { get; set; }
        public string businessGroup { get; set; }
        public string requestintState { get; set; }
    }


}
