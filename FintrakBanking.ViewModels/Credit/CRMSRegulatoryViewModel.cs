using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CRMSRegulatoryViewModel
    {
        public string crmsCode { get; set; }
        public DateTime? crmsDate { get; set; }
        public string beneficiary { get; set; }
        public string accountNumber { get; set; }
        public string facilityType { get; set; }
        public decimal grantedAmount { get; set; }
        public double interestRate { get; set; }
        public int tenor { get; set; }
        public DateTime effectiveDate { get; set; }
        public int loanId { get; set; }
        public int loanSystemTypeId { get; set; }
    }

    public class CRMSViewModel
    {
        public string crmsCode { get; set; }
        public DateTime crmsDate { get; set; }
        public int loanId { get; set; }
        public int loanSystemTypeId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }

    public  class CRMS300TemplateViewModel
    {
        public string UNIQUE_IDENTIFICATION_TYPE { get; set; }
        public string UNIQUE_IDENTIFICATION_NO { get; set; }
        public string CREDIT_TYPE { get; set; }
        public string CREDIT_PURPOSE_BY_BUSINESSLINES { get; set; }
        public string CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR { get; set; }
        public decimal? CREDIT_LIMIT { get; set; }
        public string OUTSTANDING_AMOUNT { get; set; }
        public string FEES { get; set; }
        public DateTime? EFFECTIVE_DATE { get; set; }
        public string TENOR { get; set; }
        public string EXPIRY_DATE { get; set; }
        public string REPAYMENT_AGREEMENT_MODE { get; set; }
        public string INTEREST_RATE { get; set; }
        public string BENEFICIARY_ACCOUNT_NUMBER { get; set; }
        public string LOCATION_OF_BENEFICIARY { get; set; }
        public string RELATIONSHIP_TYPE { get; set; }
        public string COMPANY_SIZE { get; set; }
        public string FUNDING_SOURCE_CATEGORY { get; set; }
        public string ECCI_NUMBER { get; set; }
        public string FUNDING_SOURCE { get; set; }
        public string LEGAL_STATUS { get; set; }
        public string CLASSIFICATION_BY_BUSINESS_LINES { get; set; }
        public string CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR { get; set; }
        public string SPECIALISED_LOAN { get; set; }
        public string SPECIALISED_LOAN_MORATORIUM_PERIOD { get; set; }
        public string DIRECTOR_UNIQUE_IDENTIFIER { get; set; }
        public string SYNDICATION { get; set; }
        public string SYNDICATION_STATUS { get; set; }
        public string SYNDICATION_REF_NUMBER { get; set; }
        public string COLLATERAL_PRESENT { get; set; }
        public string COLLATERAL_SECURE { get; set; }
        public string SECURITY_TYPE { get; set; }
        public string ADDRESS_OF_SECURITY { get; set; }
        public string OWNER_OF_SECURITY { get; set; }
        public string UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER { get; set; }
        public string UNIQUE_IDENTIFIER_OF_SECURITY_OWNER { get; set; }
        public string GUARANTEE { get; set; }
        public string GUARANTEE_TYPE { get; set; }
        public string GUARANTOR_UNIQUE_IDENTIFICATION_TYPE { get; set; }
        public string GUARANTOR_UNIQUE_IDENTIFICATION { get; set; }
        public string AMOUNT_GUARANTEED { get; set; }
    }
}
