using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralPerfectionyettoCommenceViewModel
    {
        public int loanId { get; set; }

      //  public short loanSystemTypeId { get; set; }
        public string subHead { get; set; }
        public string customername { get; set; }

        public decimal outstandingBalance { get; set; }

        public decimal outstandingInterest { get; set; }

        public string collateralType { get; set; }
        public DateTime facilityGrantDate { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string staffCode { get; set; }
        public decimal total { get; set; }
        public string collateralCode { get; set; }
        public short colaterallSubType { get; set; }
        public DateTime captureDate { get; set; }
        public string collateralSubType { get; set; }
    }


    public class SubHead
    {
        public string subHead { get; set; }
        public string staffCode { get; set; }
        public string teamUnit { get; set; }
        public string region { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string businessDevelopmentManger { get; set; }
        public string businessUnit { get; set; }
        public string deptName { get; set; }
    }



    public class StaffMis
    {
        public int staffId { get; set; }
        public string staffCode { get; set; }
        public string subhead { get; set; }
    }

    public class CollateralPerfectionViewModel
    {
        public int loanId { get; set; }

        //  public short loanSystemTypeId { get; set; }
        public string subHead { get; set; }

        public int tenor { get; set; }
        public string customername { get; set; }
        public string loanReferenceNumber { get; set; }
        public string branchName { get; set; }
        public short solId { get; set; }
        public Decimal sanctionLimit { get; set; }
        public DateTime facilityGrantDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string collateralType { get; set; }
        public string perfectionStatus { get; set; }
        public string remarks { get; set; }
        public decimal outstandingBalance { get; set; }
        public decimal outstandingInterest { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string staffCode { get; set; }
        public decimal total { get; set; }

        public string buCode { get; set; }
        public string deskCode { get; set; }
        public string groupCode { get; set; }
        public string TeamCode { get; set; }

        public string misCode { get; set; }

        public string buDescription { set; get; }
        public string teamDescription { set; get; }
        public string deskDescription { set; get; }

        public string businessUnit { get; set; }
        public string collateralSubType { get; set; }
        public string collateralCode { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime captureDate { get; set; }
    }

    public class DisbursedFacilityCollateralViewModel
    {
        public int loanId { get; set; }

        //  public short loanSystemTypeId { get; set; }
        public string subHead { get; set; }

        public int tenor { get; set; }
        public string customerName { get; set; }
        public string loanReferenceNumber { get; set; }
        public string branchName { get; set; }
        public short solId { get; set; }
        public Decimal sanctionLimit { get; set; }
        public DateTime facilityGrantDate { get; set; }
        public DateTime expiryDate { get; set; }
        public string collateralType { get; set; }
        public string perfectionStatus { get; set; }
        public string remarks { get; set; }
        public decimal outstandingBalance { get; set; }
        public decimal outstandingInterest { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string staffCode { get; set; }
        public decimal total { get; set; }

        public string buCode { get; set; }
        public string deskCode { get; set; }
        public string groupCode { get; set; }
        public string TeamCode { get; set; }

        public string misCode { get; set; }

        public string buDescription { set; get; }
        public string teamDescription { set; get; }
        public string deskDescription { set; get; }

        public string businessUnit { get; set; }
        public string collateralSubType { get; set; }
        public string collateralCode { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime captureDate { get; set; }
        public string customerID { get; set; }
        public string settlementAccount { get; set; }
        public string refNo { get; set; }
        public string productName { get; set; }
        public string adjFacilityType { get; set; }
        public string currencyType { get; set; }
        public DateTime bookingDate { get; set; }
        public DateTime? valueDate { get; set; }
        public DateTime maturityDate { get; set; }
        public double rate { get; set; }
        public string collateralDetails { get; set; }
        public string guarantorName { get; set; }
        public string guarantorPhoneNo { get; set; }
        public string guarantorEmailAddress { get; set; }
        public string guarantorBVN { get; set; }
        public string guarantorHomeAddress { get; set; }
        public string guarantorOfficeAddress { get; set; }
        public string accountOfficerName { get; set; }
        public string divisionName { get; set; }
        public string groupName { get; set; }
        public decimal totalExposureLCY { get; set; }
        public decimal loanAmountLCY { get; set; }
        public string teamName { get; set; }
        public string regionName { get; set; }
    }

    public class CollateralRegisterViewModel : CollateralPerfectionViewModel
    {
        public string glSubheadCode { get; set; }
        public string accountNumber { get; set; }
        public Decimal grossBalance { get; set; }
        public Decimal collateralValueOmv { get; set; }
        public Decimal collateralValueEfsv { get; set; }
        public Decimal approvedAmount { get; set; }
        public Decimal? securityValue { get; set; }
        public Decimal? collateralCoverage { get; set; }
        public string collateralLocation { get; set; }
        public int customerID { get; set; }
        public DateTime dateOfValuation { get; set; }
        public string nameOfValuer { get; set; }
        public short? valuerId { get; set; }
        public int? collateralCustomerID { get; set; }

        public DateTime? dateOfCollateralInspection { get; set; }
        public string collateralDescription { get; set; }
        public DateTime dateOfInsurance { get; set; }
        public string insuranceCompany { get; set; }
        public string rmCode { get; set; }
        public string rmName { get; set; }
        public DateTime? dateOfExpiration { get; set; }
        public int days { get; set; }
        public string stc { get; set; }
        public string groupDescription { get; set; }
        public int loanApplicationId { get; set; }
        public decimal? exposure { get; set; }
        public string collateralForm { get; set; }
        public string collateralSummary { get; set; }
        public string guarantorName { get; set; }
        public int collateralTypeId { get; set; }
        public string customerCode { get; set; }
        public string insurancePolicyType { get; set; }
        public int? businessUnitId { get; set; }
    }


    public class CollateralAdequacyViewModel 
    {
        public string glSubheadCode { get; set; }
        public string accountNumber { get; set; }
        public Decimal grossBalance { get; set; }
        public Decimal collateralValueOmv { get; set; }
        public Decimal collateralValueEfsv { get; set; }
        public Decimal approvedAmount { get; set; }
        public Decimal? securityValue { get; set; }
        public Decimal? collateralCoverage { get; set; }
        public string collateralLocation { get; set; }
        public string customerId { get; set; }
        public DateTime dateOfValuation { get; set; }
        public string nameOfValuer { get; set; }
        public short? valuerId { get; set; }
        public int? collateralCustomerID { get; set; }

        public DateTime? dateOfCollateralInspection { get; set; }
        public DateTime collateralDescription { get; set; }
        public DateTime dateOfInsurance { get; set; }
        public string insuranceCompany { get; set; }
        public string rmCode { get; set; }
        public string rmName { get; set; }
        public DateTime? dateOfExpiration { get; set; }
        public int days { get; set; }
        public string stc { get; set; }
        public string groupDescription { get; set; }
        public int loanApplicationId { get; set; }
        public decimal? exposure { get; set; }
        public string collateralForm { get; set; }
        public string collateralSummary { get; set; }
        public string guarantorName { get; set; }
        public string customerName { get; set; }
        public decimal totalDirectExposure { get; set; }
        public string collateralType { get; set; }
        public string currency { get; set; }
        public decimal? collateralValue { get; set; }
        public string accountOfficer { get; set; }
        public int createdBy { get; set; }
        public string relationshipManager { get; set; }
        public int? businessUnitId { get; set; }
        public string groupHead { get; set; }
        public string sbu { get; set; }
        public short productId { get; set; }
        public int cCustomerId { get; set; }
        public int collateralTypeId { get; set; }
        public decimal totalTangibleCollateral { get; set; }
        public decimal totalIntangibleCollateral { get; set; }
        public decimal percentageTangibleCoverage { get; set; }
        public decimal percentageIntangibleCoverage { get; set; }
        public decimal totalPercentageCoverage { get; set; }
        public short currencyId { get; set; }
        public string collateralCurrency { get; set; }
        public decimal collateralVal { get; set; }
    }


    public class CollateralRegisterReportViewModel : CollateralPerfectionViewModel
    {
        public string glSubheadCode { get; set; }
        public string accountNumber { get; set; }
        public Decimal grossBalance { get; set; }
        public Decimal collateralValueOmv { get; set; }
        public Decimal collateralValueEfsv { get; set; }
        public Decimal approvedAmount { get; set; }
        public Decimal? securityValue { get; set; }
        public Decimal? collateralCoverage { get; set; }
        public string collateralLocation { get; set; }
        public string customerID { get; set; }
        public DateTime dateOfValuation { get; set; }
        public string nameOfValuer { get; set; }
        public short? valuerId { get; set; }
        public int? collateralCustomerID { get; set; }

        public DateTime? dateOfCollateralInspection { get; set; }
        public string collateralDescription { get; set; }
        public DateTime? dateOfInsurance { get; set; }
        public string insuranceCompany { get; set; }
        public string rmCode { get; set; }
        public string rmName { get; set; }
        public DateTime? dateOfExpiration { get; set; }
        public int days { get; set; }
        public string stc { get; set; }
        public string groupDescription { get; set; }
        public int loanApplicationId { get; set; }
        public decimal? exposure { get; set; }
        public string collateralForm { get; set; }
        public string collateralSummary { get; set; }
        public string guarantorName { get; set; }
        public int collateralTypeId { get; set; }
        public string customerCode { get; set; }
        public string insurancePolicyType { get; set; }
        public int? businessUnitId { get; set; }
        public decimal collateralValue { get; set; }
    }
}
