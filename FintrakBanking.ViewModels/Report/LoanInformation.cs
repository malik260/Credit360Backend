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
        public int? stateId { get; set; }
        public string stateName { get; set; }
        public short? sectorId { get; set; }
        public string sectorName { get; set; }
        public short? subSectorId { get; set; }
        public string subSectorName { get; set; }
        public int employerId { get; set; }
        public dynamic employer { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
    }

    public class DisburstLoanViewModel
    {
        public string loanStatus;
        // public decimal approvedTenor { get; set; }
        public decimal outstandingInterest { get; set; }
        public double approvedInterestRate { get; set; }
        public string disbursedBy { get; set; }
        public string tenor { get; set; }
        public int tenornum { get; set; }
        // private int tenor { get { return (maturitydate - effectiveDate).Days; } }
        public int tenorToDate { get; set; }
        public int approvedTenor { get { return (int)(Math.Round(tenornum * (decimal)(12.0 / 365.0))); } }
        public double exchangeValue { get; set; }
        public decimal productId { get; set; }
        public string companyName { get; set; }
        public string customerName { get; set; }
        public string productName { get; set; }
        public decimal approvedAmount { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string solId { get; set; }
        public string loanrefnum { get; set; }
        public decimal amountDisbursed { get; set; }

        public string accountNumber { get; set; }
        public string disbursementAccount { get; set; }
        public string disbursedUser { get; set; }
        public string repaymentAccount { get; set; }
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
        public short branchId { get; set; }
        public string branchName { get; set; }
        public string productClassName { get; set; }
        public decimal interest { get; set; }
        public DateTime dealDate { get; set; }
        public string interestType { get; set; }
        public string interestRepaymentFreq { get; set; }
        public string principalRepaymentFreq { get; set; }
        public decimal pricipalAmount { get; set; }
        public double rate { get; set; }
        public decimal interestRateChange { get; set; }
        public decimal interestToDate { get; set; }
        public string accountPayTo { get; set; }
        public string accountReceiveFrom { get; set; }
        public string naration { get; set; }
        public string remark { get; set; }
        public string current { get; set; }
        public string businessGroup { get; set; }
        public int loanTenor { get; set; }
        public string cRMSCode { get; set; }
        public int productClassID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string staffName { get; set; }
        public string misCode { get; set; }
        public string BU { get; set; }
    }

    public class CorporateLoansDeptViewModel
    {
        public int? bookingrequestid { get; set; }
        public string loanReferenceNumber { get; set; }
        public int loanapplicationid { get; set; }
        public string applicationreferencenumber { get; set; }
        public string loanApplicationId { get; set; }
        public string TermLoanId { get; set; }
        public string solId { get; set; }
        public string BranchName { get; set; }
        public DateTime DateTimeInitiated { get; set; }
        public string status { get; set; }
        public string PreviousStage { get; set; }
        public string DisburseOfficerName { get; set; }
        public DateTime? disburseDateTime { get; set; }
        public string verificationOfficer { get; set; }
        public DateTime? verificationDateTime { get; set; }
        public string loanOfficerName { get; set; }
        public DateTime loanOfficerDateTime { get; set; }
        public string productType { get; set; }
        public string RM { get; set; }
        public DateTime RMDate { get; set; }
        public DateTime? availmentDate { get; set; }
        public DateTime? crmsDate { get; set; }
        public string customerOperativeAccount { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public decimal? customerLoanAmount { get; set; }
        public int loanTenure { get; set; }
        public decimal? loanAmountDisbursed { get; set; }
        public int lonaTenor { get; set; }
        public decimal? disbursementAmount { get; set; }
        public string refferBackComment { get; set; }
        public string refferBackUser { get; set; }
        public string completedComment { get; set; }
    }

    public class fullname
    {
        public string name { get; set; }
    }

    public class DateRange
    {
        public DateTime startDate { get; set; }
        public DateTime date { get; set; }
        public DateTime endDate { get; set; }
        public short branchId { get; set; }
        public string loanRefNo { get; set; }
        public int productClassId { get; set; }
        public string username { get; set; }
        public string branchCode { get; set; }
        public int classification { get; set; }
        public int approvalStatus { get; set; }
        public int reportAllType { get; set; }
        public int operationId { get; set; }
        public int auditTypeId { get; set; }
        public int companyId { get; set; }
        public string searchInfo { get; set; }
        public string crmSCode { get; set; }
        public string loginStatus { get; set; }
        public short loanStatusId { get; set; }
        public short waivedOrDeferred { get; set; }
        public string searchParameter { get; set; }
        public string status { get; set; }
        public string loanAcct { get; set; }
        public string referenceNumber { get; set; }
        public int psrReportTypeId { get; set; }
        public int projectSiteReportId { get; set; }
        public string ReportType { get; set; }
        public string productId { get; set; }
        public int documentTypeId { get; set; }
    }

    public class ReportSearchEntity
    {
        public int branchId { get; set; }

        public int? typeId { get; set; }
        public int? staffId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool excludeSystem { get; set; }
        public string loanReference { get; set; }
        public string obligor { get; set; }
        public string customerName { get; set; }
        public int pageNo { get; set; }
        public int pageSize { get; set; }
        public string searchParamemter { get; set; }
        public int? productClassId { get; set; }
        public string customerCode { get; set; }
        public int categoryId { get; set; }
        public int transactionTypeId { get; set; }
        public int operationId { get; set; }
        public string valueCode { get; set; }
        public int companyId { get; set; }
        public int PostedByStaffId { get; set; }
        public int glAccountId { get; set; }
        public int customChartOfAccountId { get; set; }
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

    public class LoanAnniverseryViewModel
    {
        public string phoneNumber { get; set; }
        public int customerId { get; set; }
        public DateTime maturityDate { get; set; }
        public double intrestrate { get; set; }
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
       
       // public decimal totalOutstanding { get { return outstandingPrincipal + outstandingIntrestAmt; } }

        public decimal totalOutstanding { get; set; }

       
      //  public decimal recoveredAmount { get { return grantedAmount - outstandingPrincipal; } }

        public decimal recoveredAmount { get; set; }
    }

    public class LoanDocumentWaivedViewModel
    {
        public DateTime deferralExpiryDate { get; set; }
        public string buDescription { get; set; }
        public string checkListStatusName { get; set; }
        public int cummulativeDays { get; set; }
        public int deferralDurration { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public int customerId { get; set; }
        public string waivedDocument { get; set; }
        public decimal? facilityAmount { get; set; }
        public DateTime? facilityExpirationDate { get; set; }
        public DateTime? facilityGrantedDate { get; set; }
        public DateTime waveredDate { get; set; }
        public string facilityType { get; set; }
        public string applicationRefrenceNumber { get; set; }
        public string customerName { get; set; }
        public string branchName { get; set; }
        public int loanApplicationId { get; set; }
        public decimal proposedAmount { get; set; }
        public string companyName { get; set; }
        public string name { get; set; }
        public string  facilityProduct { get; set; }
        public int staffId { get; set; }
        public decimal currentExposure { get; set; }
        public string defferalDocument { get; set; }
        public DateTime initialDefferalDate { get; set; }
        public DateTime? currentDefferalDate { get; set; }
        public DateTime? defferalExpiryDate { get; set; }
        public string staffCode { get; set; }
        
        public string customerAcct { get; set; }
        public string nameOfRM { get; set; }
        public string nameOfBM { get; set; }
        public string customerCode { get; set; }
        public DateTime dateCreated { get; set; }
        public string businessUnit { get; set; }
       
        public int deferralDuration { get { return (this.initialDefferalDate - this.dateCreated).Days; } }
        public int cumulativeDays { get { return (DateTime.Now - this.dateCreated).Days; } }

        public decimal loanBalanceForeignCurrency { get; set; }
        public string groupDescription { get; set; }
        public string teamDescription { get; set; }
        public string deskDescription { get; set; }
        //public string buDescription { get; set; }
        public string teamCode { get; set; }
        public string groupCode { get; set; }
        public string deskCode { get; set; }
        public string reasonForDeferral { get; set; }
        public string currency { get; set; }
        public DateTime? finalApprovalDate { get; set; }
        public string perfectionRelated { get; set; }
        public string accountOfficer { get; set; }
        public string relationshipManager { get; set; }
        public int createdBy { get; set; }
        public string groupHead { get; set; }
        public int? businessUnitId { get; set; }
        public string sbu { get; set; }
    }

    public class FCYScheuledLoanViewModel
    {
        public decimal loanGlBalance { set; get; }
        public string loanRefrenceNumber { set; get; }
        public string accountNumber { set; get; }
        public string loanTypeName { set; get; }
        public string loanCurrency { set; get; }
        public double interestRate { set; get; }
        public DateTime valueDate { set; get; }
        public DateTime maturityDate { set; get; }
        public decimal facilityLimit { set; get; }
        public double facilityRate { set; get; }
        public double exchangeRate { set; get; }
        public string firstName { set; get; }
        public string lastName { set; get; }
        public string middleName { set; get; }
        public string customerName { get { return firstName + " " + middleName + " " + lastName; } }
        public int tenorDays { get; set; }
        public int tenorToDate { get; set; }
        public int tenorToMaturity { get; set; }
        public string companyName { get; set; }
        public string logoPath { get; set; }
        public string applicationRefrenceNumber { get; set; }
        public decimal loanFigure { get; set; }
        public string currency { set; get; }

    }

    public class RiskAssets
    {
        public DateTime runDate { get; set; }
        public string level { get; set; }
        public string misCode { get; set; }
        public string exposureType { get; set; }
        public string divisionName { get; set; }
        public string groupName { get; set; }
        public string branchName { get; set; }
        public string regionName { get; set; }
    }

    public class InterestIncome
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }

    public class RemedialAssetReport
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int dpd { get; set; }
        public decimal amount { get; set; }

    }

    public class Overline
    {
        public DateTime runDate { get; set; }
        public string level { get; set; }
        public string misCode { get; set; }
        public string exposureType { get; set; }
        public string divisionName { get; set; }
        public string groupName { get; set; }
        public string branchName { get; set; }
        public string regionName { get; set; }
    }

    public class DrawdownViewModel
    {
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public string branch { get; set; }
        public string misCode { get; set; }
        public string facilityType { get; set; }
        public double interestRate { get; set; }
        public decimal drawdownAmount { get; set; }
        public double processingFee { get; set; }
        public int tenor { get; set; }
        public double mgtFee { get; set; }
        public int? moratorium { get; set; }
        public double commitmentFee { get; set; }
        public decimal principalRepayment { get; set; }
        public double otherFeeSpecify { get; set; }
        public string interestRepayment { get; set; }
        public DateTime? effectiveDate { get; set; }
        public decimal approvedAmount { get; set; }
        public double amountUtilized { get; set; }
        public decimal newRequest { get; set; }
        public string inPlace { get; set; }
        public string perfected { get; set; }
        public string deffered { get; set; }
        public string relationshipOfficer { get; set; }
        public string relationshipManager { get; set; }
        public string riskManagement { get; set; }
        public string legal { get; set; }
        public string treasury { get; set; }
        public string coo { get; set; }
        public string crmInternation { get; set; }
        public string otherInPlace { get; set; }
        public string otherPerfected { get; set; }
        public string otherDeffered { get; set; }

    }

    public class DeferralWaiverViewModel
    {
        public string customerName { get; set; }
        public string facilityType { get; set; }
        public decimal approvedAmount { get; set; }
        public string preparedBy { get; set; }
        public string condition { get; set; }
        public double loanInformation { get; set; }
        public decimal reason { get; set; }
        public double cummulativeDays { get; set; }
        public int deferralDuration { get; set; }
        public double fromApprovalLevelName { get; set; }
        public int? fromStaffName { get; set; }
        public string branchName { get; set; }
        public DateTime currentDate { get; set; }

    }

    public class AnalystReportViewModel
    {
        public string customer_name { get; set; }
        public string applicationreferencenumber { get; set; }
        public decimal proposedamount { get; set; }
        public double proposedinterestrate { get; set; }
        public string loanpurpose { get; set; }
        public string analystname { get; set; }
        public string branch { get; set; }

    }

    public class RelatedPartyLoansViewModel
    {
        public string loanReferenceNumber { get; set; }
        public string solId { get; set; }
        public string customerName { get; set; }
        public string productName { get; set; }
        public int approvedTenor { get; set; }
        public decimal principalAmount { get; set; }
        public string accountStatus { get; set; }
        public string relatedParty { get; set; }
        public string relationshipType { get; set; }
    }

    public class MarturedLoansViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public string solId { get; set; }
        public string customerName { get; set; }
        public string staffName { get; set; }
        public int approvedTenor { get; set; }
        public decimal principalAmount { get; set; }
        public DateTime? disbursedDate { get; set; }
        public DateTime? maturityDate { get; set; }
        public decimal approvedAmount { get; set; }
        public double exchangeRate { get; set; }
        public string misCode { get; set; }
        public string BU { get; set; }
        public string status { get; set; }
    }

    public class ApprovedLoansViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public string solId { get; set; }
        public string customerName { get; set; }
        public string staffName { get; set; }
        public string branchName { get; set; }
        public string GLCode { get; set; }
        public decimal? manageFee { get; set; }
        public double interestRate { get; set; }
        public string product { get; set; }
        public decimal? utilizedAmount { get; set; }
        public string disbursedStatus { get; set; }
        public int approvedTenor { get; set; }
        public decimal approvedAmount { get; set; }
        public string currency { get; set; }
        public double exchangeRate { get; set; }
        public decimal applicationAmount { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime? approvedDate { get; set; }
        public string misCode { get; set; }
        public string BU { get; set; }
        public string account { get; set; }

    }

    public class InitiatedLoansViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public string solId { get; set; }
        public string customerName { get; set; }
        public string staffName { get; set; }
        public int approvedTenor { get; set; }
        public string product { get; set; }
        public decimal applicationAmount { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public double exchangeRate { get; set; }
        public decimal proposedAmount { get; set; }
        public string misCode { get; set; }
        public string BU { get; set; }
    }

    public class TerminatedLoansViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public string solId { get; set; }
        public string customerName { get; set; }
        public string staffName { get; set; }
        public int approvedTenor { get; set; }
        public decimal principalAmount { get; set; }
        public DateTime? disbursedDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string misCode { get; set; }
        public string BU { get; set; }

    }

    public class CustomerViewModel
    {
        public string customerCode { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string solId { get; set; }
        public string staffname { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public string misCode { get; set; }
        public string BU { get; set; }
    }

}
