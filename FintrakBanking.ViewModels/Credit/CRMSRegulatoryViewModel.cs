using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CRMSRegulatoryViewModel : GeneralEntity
    {
        public string crmsCode { get; set; }
        public DateTime? crmsDate { get; set; }
        public string beneficiary { get; set; }
        public string accountNumber { get; set; }
        public string facilityType { get; set; }
        public decimal grantedAmount { get; set; }
        public double interestRate { get; set; }
        public double tenor { get; set; }
        public DateTime effectiveDate { get; set; }
        public int loanId { get; set; }
        public short loanSystemTypeId { get; set; }
        public LoansCount loansCount { get; set; }
        public int? crmsLegalStatusId { get; set; }
        public string operationName { get; set; }

    }

    public class CRMSViewModel : GeneralEntity
    {
        public string crmsCode { get; set; }
        public DateTime crmsDate { get; set; }
        public int loanId { get; set; }
        public short LOANSYSTEMTYPEID { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int templateTypeId { get; set; }
        public bool isLms { get; set; }

    }

    public class CRMSRecord
    {
        public byte[] reportData { get; set; }
        public string templateTypeName  { get; set; }
    }

    public class LoansCount
    {
        public int count { get; set; }
        public string crmsLegalStatusName { get; set; }
        public string code { get; set; }
    }

    public  class CRMSTemplateViewModel 
    {
        public string UNIQUE_IDENTIFICATION_TYPE { get; set; }
        public string UNIQUE_IDENTIFICATION_NO { get; set; }
        public string CREDIT_TYPE { get; set; }
        public string CREDIT_PURPOSE_BY_BUSINESSLINES { get; set; }
        public string CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR { get; set; }
        public decimal? CREDIT_LIMIT { get; set; }
        public decimal OUTSTANDING_AMOUNT { get; set; }
        public string FEES { get; set; }
        public DateTime EFFECTIVE_DATE { get; set; }
        public double TENOR { get; set; }
        public DateTime EXPIRY_DATE { get; set; }
        public DateTime? MATURITYDATE { get; set; }
        public string REFERENCENUMBER { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? CRMSDATE { get; set; }
        public bool? CRMSVALIDATED { get; set; }

        public string REPAYMENT_AGREEMENT_MODE { get; set; }
        public double? INTEREST_RATE { get; set; }
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
        public double SPECIALISED_LOAN_MORATORIUM_PERIOD { get; set; }
        public string DIRECTOR_UNIQUE_IDENTIFIER { get; set; }
        public string SYNDICATION { get; set; }
        public string SYNDICATION_STATUS { get; set; }
        public string SYNDICATION_REF_NUMBER { get; set; }
        public string COLLATERAL_PRESENT { get; set; }
        public string COLLATERAL_SECURE { get; set; }
        public string SECURITY_TYPE { get; set; }
        public string ADDRESS_OF_SECURITY { get; set; }
        public string OWNER_OF_SECURITY { get; set; }
        public short? UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER { get; set; }
        public string UNIQUE_IDENTIFIER_OF_SECURITY_OWNER { get; set; }
        public string GUARANTEE { get; set; }
        public short? GUARANTEE_TYPE { get; set; }
        public string GUARANTOR_UNIQUE_IDENTIFICATION_TYPE { get; set; }
        public string GUARANTOR_UNIQUE_IDENTIFICATION { get; set; }
        public decimal? AMOUNT_GUARANTEED { get; set; }
        public DateTime? FIRSTPRINCIPALPAYMENTDATE { get; set; }
        public int? MORATORIUMDURATION { get; set; }

        public int LOANID { get; set; }
        public int? CRMSLEGALSTATUSID { get; set; }
        public string ID_TTPE { get; set; }
        public string ID_DETAIL { get; set; }

        //100
        public string GOVERNMENT_CODE   { get; set; }
        public string SPECIALISED_LOAN_PERIOD { get; set; }
        public string REPAYMENT_SOURCE { get; set; }
        public string OPERATION_NAME { get; set; }


        //200
        public string GOVERNMENT_MDA_TIN { get; set; }
        public string PERFORMANCE_REPAYMENT_STATUS { get; set; }


        public decimal? TOTAL_BANK_INDUCED_DEBIT_BANK_CHARGES { get; set; }
        public decimal? TOTAL_BANK_INDUCED_CREDIT_WRITEOFF { get; set; }
        public decimal? TOTAL_BANK_INDUCED_CREDIT_DRAWDOWN { get; set; }
        public decimal? TOTAL_CUSTOMER_INDUCED_CREDIT { get; set; }
        public string TOTAL_CUSTOMER_INDUCED_CREDIT_TRN_TYPE { get; set; }
        public decimal? TOTAL_CUSTOMER_INDUCED_DEBIT_AMT { get; set; }
        public string TOTAL_CUSTOMER_INDUCED_DEBIT_TRN_TYPE { get; set; }
        public decimal? UNAMORTIZED_CREDIT_CHARGES { get; set; }
        public string LIQUIDATION { get; set; }



        public string REASON_FOR_RESTRUCTURING { get; set; }


        //600
        public string SYNDICATION_NAME { get; set; }
        public decimal? SYNDICATION_TOTAL_AMOUNT { get; set; }
        public string PARTICIPATING_BANK_CODE { get; set; }
        public string ACCOUNT { get; set; }
        public int? FEE_TYPE { get; set; }
        public string FEE_TYPE_NAME { get; set; }

        public decimal FEE_AMOUNT { get; set; }
        public int CUSTOMERID { get; set; }
        public string CONDITIONPRECIDENT { get; set; }
        public string CRMSCODE { get; set; }
        public string EMAIL { get; set; }
        public int LOANREVIEWOPERATIONID { get; set; }
        public int? LOANAPPLICATIONDETAILID { get; set; }

    }

    public class CRMSCodeGeneration : GeneralEntity
    {
        public short loanSystemTypeId { get; set; }

        public string governmentCode { get; set; }
        public string sourceReferenceNumber { get; set; }

        public int customerId { get; set; }

        public int loanApplicationDetailId { get; set; }

        public string callreport_id { get; set; }
        public string callreport_desc { get; set; }
        public string inst_code { get; set; }
        public string inst_name { get; set; }
        public string sl_no { get; set; }
        public string unique_identification_type { get; set; }
        public string unique_identification_no { get; set; }
        public string credit_type { get; set; }
        public string credit_purpose_by_businesslines { get; set; }
        public string credit_purpose_by_businesslines_sub { get; set; }
        public string credit_limit { get; set; }
        public string outstanding_amount { get; set; }
        public string fee_type { get; set; }
        public string fee_amount { get; set; }
        public string tenor { get; set; }
        public string repayment_mode { get; set; }
        public string interest_rate { get; set; }
        public string beneficiary_account_no { get; set; }
        public string beneficiary_location { get; set; }
        public string prepared_date { get; set; }
        public string relationship_types { get; set; }
        public string company_size { get; set; }
        public string funding_source_category { get; set; }
        public string funding_sources { get; set; }
        public string ecci_number { get; set; }
        public string legal_status { get; set; }
        public string classification_by_business_lines { get; set; }
        public string classification_by_business_lines_sub { get; set; }
        public string specialized_loan { get; set; }
        public string specialized_loan_moratorium { get; set; }
        public string syndication { get; set; }
        public string syndication_status { get; set; }
        public string syndication_ref_number { get; set; }
        public string collateral_present { get; set; }
        public string collateral_secure { get; set; }
        public string security_type { get; set; }
        public string security_address { get; set; }
        public string security_owner { get; set; }
        public string securityowner_uniqueid_type { get; set; }
        public string securityowner_uniqueid { get; set; }
        public string guarantee { get; set; }
        public string signatory_name { get; set; }
        public string signatory_designation { get; set; }
        public string signatory_position { get; set; }
        public string signatory_phone { get; set; }
        public string sig_extn { get; set; }
        public string contact_name { get; set; }
        public string contact_designation { get; set; }
        public string contact_phone { get; set; }
        public string contact_extn { get; set; }
        public string description { get; set; }
        public string prepared_by { get; set; }
        public string auth_by { get; set; }
        public string mlr_officer_code { get; set; }
        public string headoffice_address { get; set; }
        public string headoffice_tel { get; set; }
        public string credit_officer { get; set; }
        public string branch_manager { get; set; }
        public string checked_by { get; set; }
        public string as_at { get; set; }
        public string channel_code { get; set; }
        public string token { get; set; }
    }

}
