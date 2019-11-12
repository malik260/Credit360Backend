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

    public class CustomerTransactionViewModels 
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
        public string webRequestDate { get; set; }
        public string webRequestStatus { get; set; }
        public string message { get; set; }
       
        public InterestRateDetails interestRateDetails { get; set; }
    }


    public class InterestRateDetails 
    {
        public string accountNumber { get; set; }
        public string accountType { get; set; }
        public string interestSerialNumber { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string lastChangedDate { get; set; }
        public string interestRateAmount { get; set; }
        public string interestTableCode { get; set; }
    }

    public class CustomerTurnoverGroupViewModel
    {
        public List<CustomerTurnoverViewModelAPI> Account { get; set; }
    }
    public class CustomerTurnoverViewModelAPI // TEMPORARY LOCATION
    {
        public string foracid { get; set; } // ": "2022072744",
        public string cust_Id { get; set; } // ": "483008974",
        public string schm_Type { get; set; } // ": "ODA|OVERDRAFT A/C",
        public string period { get; set; } // ": "Apr-15",
        public decimal? min_Debit_Balance { get; set; } // ": "",
        public decimal? max_Debit_Balance { get; set; } // ": "",
        public decimal? min_Credit_Balance { get; set; } // ": "34218.39",
        public decimal? max_Credit_Balance { get; set; } // ": "1050843.73",
        public decimal? debit_Turnover { get; set; } // ": "1159360.11",
        public decimal? credit_Turnover { get; set; } // ": "1207425.56",
        public string sms_Alert { get; set; } // ": "-176",
        public string amc { get; set; } // ": "",
        public string vat { get; set; } // ": "-92.50",
        public string management_Fee { get; set; } // ": "",
        public string commitment_Fees { get; set; } // ": "",
        public string com_Contigent_Liab { get; set; } // ": "",
        public string lc_Commission { get; set; } // ": 
        public string float_Charge { get; set; } // "2081981.94",
        public string interest { get; set; } // "2909416.54",
        public int? month { get; set; } // "0",
        public int? year { get; set; } // "0",

        //public string foracid { get; set; }
        //public string cusT_ID { get; set; }
        //public string schM_TYPE { get; set; }
        //public string period { get; set; }
        //public decimal? miN_DEBIT_BALANCE { get; set; }
        //public decimal? maX_DEBIT_BALANCE { get; set; }
        //public decimal? miN_CREDIT_BALANCE { get; set; }
        //public decimal? maX_CREDIT_BALANCE { get; set; }
        //public decimal? debiT_TURNOVER { get; set; }
        //public decimal? crediT_TURNOVER { get; set; }
        //public string smS_ALERT { get; set; }
        //public string amc { get; set; }
        //public string vat { get; set; }
        //public string managemenT_FEE { get; set; }
        //public string commitmenT_FEES { get; set; }
        //public string coM_CONTINGENT_LIAB { get; set; }
        //public string lC_COMMISION { get; set; }
    }

    public class CustomerTurnoverViewModel // TEMPORARY LOCATION
    {
        public string accountNumber { get; set; } // ": "2022072744",
        public string customerCode { get; set; } // ": "483008974",
        public string productName { get; set; } // ": "ODA|OVERDRAFT A/C",
        public string period { get; set; } // ": "Apr-15",
        public decimal? min_Debit_Balance { get; set; } // ": "",
        public decimal? max_Debit_Balance { get; set; } // ": "",
        public decimal? min_Credit_Balance { get; set; } // ": "34218.39",
        public decimal? max_Credit_Balance { get; set; } // ": "1050843.73",
        public decimal? debit_Turnover { get; set; } // ": "1159360.11",
        public decimal? credit_Turnover { get; set; } // ": "1207425.56",
        public decimal? sms_Alert { get; set; } // ": "-176",
        public decimal? amc { get; set; } // ": "",
        public decimal? vat { get; set; } // ": "-92.50",
        public decimal? management_Fee { get; set; } // ": "",
        public decimal? commitment_Fees { get; set; } // ": "",
        public decimal? com_Contigent_Liab { get; set; } // ": "",
        public decimal? lc_Commission { get; set; } // ": 
        public decimal? float_Charge { get; set; } // "2081981.94",
        public decimal? interest { get; set; } // "2909416.54",
        public int? month { get; set; } // "0",
        public int? year { get; set; } // "0",


    }

    //public class CustomerTurnoverInterestViewModels
    //{
    //    public string as_Of_Date { get; set; } // "1/31/2018 12:00:00 AM",
    //    public string account_Number { get; set; } // "2013995959",
    //    public string acct_Type { get; set; } // "Loan",
    //    public string float_Charge { get; set; } // "2081981.94",
    //    public string interest { get; set; } // "2909416.54",
    //    public string account_Name { get; set; } // "FORTE OIL PLC",
    //    public string cif_Id { get; set; } // "230009868"
    //}

    //public class InputVM
    //{
    //    public int cifid { get; set; }
    //    public DateTime fromdate { get; set; }
    //    public DateTime todate { get; set; }
    //}
}
