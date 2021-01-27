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

    }
}
