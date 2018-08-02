using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{
    public class CasaIntegrationViewModel : GeneralEntity
    {
        public string accountNumber  { get; set; }
        public string accountName { get; set; }
        public string product { get; set; }
        public string productType { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string accountDetail { get { return (this.accountNumber + "(" + this.accountName + ")"); } }
        public string currencyType { get; set; }
        public decimal balance { get; set; }
        public string branch { get; set; }
        public string accountStatus  { get; set; }
        public string lastTransactionDate { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public string error { get; set; }
        public string field { get; set; }
        public string errorDescription { get; set; }
        public int currencyId { get; set; }
        public string customerCode { get; set; }

        public string freezeStatus { get; set; }
        public string freezeReason { get; set; }


    }

    public class CustomerIntegrationViewModels 
    {
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public string branchCode { get; set; }
        public string contactAddress { get; set; }
        public string lastContactAddress  { get; set; }
        //public string lastName { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string customerTypeName { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.lastName; } }
        public string fullName { get; set; }
        public string searchItem { get { return this.firstName + " " + this.middleName + " " + this.lastName + " " + this.customerCode; } }
        public string lastName { get; set; }
        public string gender { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string placeOfBirth { get; set; }
        public string nationality { get; set; }
        public string maritalStatus { get; set; }
        public string emailAddress { get; set; }
        public string maidenName { get; set; }
        public string spouse { get; set; }
        public string firstChildName { get; set; }
        public DateTime childDateOfBirth { get; set; }
        public string occupation { get; set; }
        public string customerType { get; set; }
        public string relationshipOfficerCode { get; set; }
        public string relationshipOfficerName { get; set; }
        public string politicallyExposedPerson  { get; set; }
        public string misCode { get; set; }
        public string staffCode { get; set; }
        public string fsCaptionGroupCode { get; set; }
        public DateTime dateofIncorporation { get; set; }
        public string actedOnBy { get; set; }
        public bool accountCreationComplete { get; set; }
        public bool creationMailSent { get; set; }
        public string customerSensitivityLevel { get; set; }
        public string taxIdNumber  { get; set; }
        public string electricMeterNumber { get; set; }
        public string businessTaxIdNumber { get; set; }
        public string bankVerificationNumber { get; set; }
        public string taxNumber { get; set; }
        public string officeAddress { get; set; }
        public string nearestLandmark { get; set; }
        public string paidUpCapital { get; set; }
        public string authorizedCapital { get; set; }
        public string employerDetails { get; set; }


        public string rcNumber { get; set; }
        //public DateTime dateOfBirth { get; set; }
        public string subSectorCode { get; set; }
        public string sectorCode { get; set; }
        //public string maritalStatus { get; set; }
        //public string emailAddress { get; set; }
        //public string maidenName { get; set; }
        //public string spouse { get; set; }
        //public string firstChildName { get; set; }
        //public DateTime childDateOfBirth { get; set; }
        //public string occupation { get; set; }
        //public string customerType { get; set; }
        //public string relationshipOfficerCode { get; set; }
        //public string relationshipOfficerName { get; set; }
        //public bool politicallyExposedPerson { get; set; }
    }

    public class CurrencyExchangeRateIntegrationViewModel 
    {

        public short currencyId { get; set; }

        public string currencyCode { get; set; }

        public string fromCurrencyCode { get; set; }

        public string toCurrencyCode { get; set; }

        public string rateCode  { get; set; }

        public DateTime webRequestDate  { get; set; }

        public string webRequestStatus { get; set; }

        public double buyingRate { get; set; }

        public double sellingRate { get; set; }

        public double exchangeRate  { get; set; }

        public short baseCurrencyId { get; set; }

        public bool isBaseCurrency { get; set; }

        



    }


    public class InterestRateInquiryIntegrationViewModel : GeneralEntity
    {
        public string accountNumber { get; set; }
        public string accountType { get; set; }
        public string interestSerialNumber { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string lastChangedDate { get; set; }
        public decimal interestRateAmount { get; set; }
        public string interestTableCode { get; set; }


    }

}
