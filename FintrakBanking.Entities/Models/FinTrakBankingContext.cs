namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FinTrakBankingContext : DbContext
    {
        public FinTrakBankingContext()
            : base("name=FinTrakBankingContext")
        {
        }

        public virtual DbSet<TBL_ACCOUNTING_STANDARD> TBL_ACCOUNTING_STANDARD { get; set; }
        public virtual DbSet<TBL_ACCREDITEDCONSULTANT> TBL_ACCREDITEDCONSULTANT { get; set; }
        public virtual DbSet<TBL_ACCREDITEDCONSULTANT_STATE> TBL_ACCREDITEDCONSULTANT_STATE { get; set; }
        public virtual DbSet<TBL_ACCREDITEDCONSULTANT_TYPE> TBL_ACCREDITEDCONSULTANT_TYPE { get; set; }
        public virtual DbSet<TBL_APPLICATION_SETUP> TBL_APPLICATION_SETUP { get; set; }
        public virtual DbSet<TBL_APPROVAL_GROUP> TBL_APPROVAL_GROUP { get; set; }
        public virtual DbSet<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }
        public virtual DbSet<TBL_APPROVAL_LEVEL> TBL_APPROVAL_LEVEL { get; set; }
        public virtual DbSet<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }
        public virtual DbSet<TBL_APPROVAL_STATE> TBL_APPROVAL_STATE { get; set; }
        public virtual DbSet<TBL_APPROVAL_STATUS> TBL_APPROVAL_STATUS { get; set; }
        public virtual DbSet<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }
        public virtual DbSet<TBL_AUDIT> TBL_AUDIT { get; set; }
        public virtual DbSet<TBL_AUDIT_TYPE> TBL_AUDIT_TYPE { get; set; }
        public virtual DbSet<TBL_BRANCH> TBL_BRANCH { get; set; }
        public virtual DbSet<TBL_CASA> TBL_CASA { get; set; }
        public virtual DbSet<TBL_CASA_ACCOUNTSTATUS> TBL_CASA_ACCOUNTSTATUS { get; set; }
        public virtual DbSet<TBL_CASA_LIEN> TBL_CASA_LIEN { get; set; }
        public virtual DbSet<TBL_CASA_LIEN_TYPE> TBL_CASA_LIEN_TYPE { get; set; }
        public virtual DbSet<TBL_CASA_OVERDRAFT> TBL_CASA_OVERDRAFT { get; set; }
        public virtual DbSet<TBL_CASA_POSTNOSTATUS> TBL_CASA_POSTNOSTATUS { get; set; }
        public virtual DbSet<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }
        public virtual DbSet<TBL_CHECKLIST_DETAIL> TBL_CHECKLIST_DETAIL { get; set; }
        public virtual DbSet<TBL_CHECKLIST_ITEM> TBL_CHECKLIST_ITEM { get; set; }
        public virtual DbSet<TBL_CHECKLIST_STATUS> TBL_CHECKLIST_STATUS { get; set; }
        public virtual DbSet<TBL_CHECKLIST_TARGETTYPE> TBL_CHECKLIST_TARGETTYPE { get; set; }
        public virtual DbSet<TBL_CITY> TBL_CITY { get; set; }
        public virtual DbSet<TBL_CITY_CLASS> TBL_CITY_CLASS { get; set; }
        public virtual DbSet<TBL_COMPANY> TBL_COMPANY { get; set; }
        public virtual DbSet<TBL_COMPANY_CLASS> TBL_COMPANY_CLASS { get; set; }
        public virtual DbSet<TBL_COMPANY_TYPE> TBL_COMPANY_TYPE { get; set; }
        public virtual DbSet<TBL_CONTENT_PLACEHOLDER> TBL_CONTENT_PLACEHOLDER { get; set; }
        public virtual DbSet<TBL_COUNTRY> TBL_COUNTRY { get; set; }
        public virtual DbSet<TBL_CURRENCY> TBL_CURRENCY { get; set; }
        public virtual DbSet<TBL_CURRENCY_RATE> TBL_CURRENCY_RATE { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELD_DATA_UPLOAD> TBL_CUSTOM_FIELD_DATA_UPLOAD { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELD_OPTION> TBL_CUSTOM_FIELD_OPTION { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELDS> TBL_CUSTOM_FIELDS { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELDS_DATA> TBL_CUSTOM_FIELDS_DATA { get; set; }
        public virtual DbSet<TBL_CUSTOM_HOSTPAGE> TBL_CUSTOM_HOSTPAGE { get; set; }
        public virtual DbSet<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }
        public virtual DbSet<TBL_CUSTOMER_ACCOUNT_KYC_ITEM> TBL_CUSTOMER_ACCOUNT_KYC_ITEM { get; set; }
        public virtual DbSet<TBL_CUSTOMER_ADDRESS> TBL_CUSTOMER_ADDRESS { get; set; }
        public virtual DbSet<TBL_CUSTOMER_BLACKLIST> TBL_CUSTOMER_BLACKLIST { get; set; }
        public virtual DbSet<TBL_CUSTOMER_BVN> TBL_CUSTOMER_BVN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CHILDREN> TBL_CUSTOMER_CHILDREN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CLIENT_SUPPLIER> TBL_CUSTOMER_CLIENT_SUPPLIER { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CLIENT_SUPPLIER_TYPE> TBL_CUSTOMER_CLIENT_SUPPLIER_TYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANY_DIRECTOR> TBL_CUSTOMER_COMPANY_DIRECTOR { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANY_DIRECTORTYPE> TBL_CUSTOMER_COMPANY_DIRECTORTYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANYINFOMATION> TBL_CUSTOMER_COMPANYINFOMATION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CUSTOM_FIELD> TBL_CUSTOMER_CUSTOM_FIELD { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EDIT_HISTORY> TBL_CUSTOMER_EDIT_HISTORY { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EDUCATIONLEVELTYPE> TBL_CUSTOMER_EDUCATIONLEVELTYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EMPLOYMENTHISTORY> TBL_CUSTOMER_EMPLOYMENTHISTORY { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_CAPTION> TBL_CUSTOMER_FS_CAPTION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_CAPTION_DETAIL> TBL_CUSTOMER_FS_CAPTION_DETAIL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_CAPTION_GROUP> TBL_CUSTOMER_FS_CAPTION_GROUP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_CAPTION> TBL_CUSTOMER_FS_RATIO_CAPTION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_DETAIL> TBL_CUSTOMER_FS_RATIO_DETAIL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_DIVISORTYPE> TBL_CUSTOMER_FS_RATIO_DIVISORTYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_VALUETYPE> TBL_CUSTOMER_FS_RATIO_VALUETYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP> TBL_CUSTOMER_GROUP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP_FS_CAPTION_DETAIL> TBL_CUSTOMER_GROUP_FS_CAPTION_DETAIL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP_MAPPING> TBL_CUSTOMER_GROUP_MAPPING { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP_RELATIONSHIPTYPE> TBL_CUSTOMER_GROUP_RELATIONSHIPTYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GUARDIAN> TBL_CUSTOMER_GUARDIAN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_IDENTIFICATION> TBL_CUSTOMER_IDENTIFICATION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_IDENTIFICATIONMODETYPE> TBL_CUSTOMER_IDENTIFICATIONMODETYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_NEXTOFKIN> TBL_CUSTOMER_NEXTOFKIN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_PHONECONTACT> TBL_CUSTOMER_PHONECONTACT { get; set; }
        public virtual DbSet<TBL_CUSTOMER_RISK_RATING> TBL_CUSTOMER_RISK_RATING { get; set; }
        public virtual DbSet<TBL_CUSTOMER_SENSITIVITY_LEVEL> TBL_CUSTOMER_SENSITIVITY_LEVEL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_TYPE> TBL_CUSTOMER_TYPE { get; set; }
        public virtual DbSet<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }
        public virtual DbSet<TBL_DAILY_ACCRUAL_CATEGORY> TBL_DAILY_ACCRUAL_CATEGORY { get; set; }
        public virtual DbSet<TBL_DAY_COUNT_CONVENTION> TBL_DAY_COUNT_CONVENTION { get; set; }
        public virtual DbSet<TBL_DAY_INTEREST_TYPE> TBL_DAY_INTEREST_TYPE { get; set; }
        public virtual DbSet<TBL_DEPARTMENT> TBL_DEPARTMENT { get; set; }
        public virtual DbSet<TBL_DEPARTMENT_UNIT> TBL_DEPARTMENT_UNIT { get; set; }
        public virtual DbSet<TBL_ERRORLOG> TBL_ERRORLOG { get; set; }
        public virtual DbSet<TBL_FEE> TBL_FEE { get; set; }
        public virtual DbSet<TBL_FEE_AMORTISATION_TYPE> TBL_FEE_AMORTISATION_TYPE { get; set; }
        public virtual DbSet<TBL_FEE_INTERVAL> TBL_FEE_INTERVAL { get; set; }
        public virtual DbSet<TBL_FEE_TARGET> TBL_FEE_TARGET { get; set; }
        public virtual DbSet<TBL_FEE_TYPE> TBL_FEE_TYPE { get; set; }
        public virtual DbSet<TBL_FINANCE_ENDOFDAY> TBL_FINANCE_ENDOFDAY { get; set; }
        public virtual DbSet<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }
        public virtual DbSet<TBL_FINANCECURRENTDATE> TBL_FINANCECURRENTDATE { get; set; }
        public virtual DbSet<TBL_FREQUENCY_TYPE> TBL_FREQUENCY_TYPE { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_DOCUMENT_MAPPING> TBL_JOB_REQUEST_DOCUMENT_MAPPING { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_STATUS> TBL_JOB_REQUEST_STATUS { get; set; }
        public virtual DbSet<TBL_JOB_TYPE> TBL_JOB_TYPE { get; set; }
        public virtual DbSet<TBL_KYC_DOCUMENTTYPE> TBL_KYC_DOCUMENTTYPE { get; set; }
        public virtual DbSet<TBL_KYC_ITEM> TBL_KYC_ITEM { get; set; }
        public virtual DbSet<TBL_MANAGEMENT_TYPE> TBL_MANAGEMENT_TYPE { get; set; }
        public virtual DbSet<TBL_MESSAGE_LOG> TBL_MESSAGE_LOG { get; set; }
        public virtual DbSet<TBL_MESSAGE_LOG_STATUS> TBL_MESSAGE_LOG_STATUS { get; set; }
        public virtual DbSet<TBL_MESSAGE_LOG_TYPE> TBL_MESSAGE_LOG_TYPE { get; set; }
        public virtual DbSet<TBL_MIS_INFO> TBL_MIS_INFO { get; set; }
        public virtual DbSet<TBL_MIS_TYPE> TBL_MIS_TYPE { get; set; }
        public virtual DbSet<TBL_NATURE_OF_BUSINESS> TBL_NATURE_OF_BUSINESS { get; set; }
        public virtual DbSet<TBL_NOTIFICATION_LOG> TBL_NOTIFICATION_LOG { get; set; }
        public virtual DbSet<TBL_OPERATIONS> TBL_OPERATIONS { get; set; }
        public virtual DbSet<TBL_OPERATIONS_TYPE> TBL_OPERATIONS_TYPE { get; set; }
        public virtual DbSet<TBL_PRODUCT> TBL_PRODUCT { get; set; }
        public virtual DbSet<TBL_PRODUCT_BEHAVIOUR> TBL_PRODUCT_BEHAVIOUR { get; set; }
        public virtual DbSet<TBL_PRODUCT_CATEGORY> TBL_PRODUCT_CATEGORY { get; set; }
        public virtual DbSet<TBL_PRODUCT_CHARGE_FEE> TBL_PRODUCT_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_PRODUCT_CLASS> TBL_PRODUCT_CLASS { get; set; }
        public virtual DbSet<TBL_PRODUCT_CLASS_TYPE> TBL_PRODUCT_CLASS_TYPE { get; set; }
        public virtual DbSet<TBL_PRODUCT_CURRENCY> TBL_PRODUCT_CURRENCY { get; set; }
        public virtual DbSet<TBL_PRODUCT_GROUP> TBL_PRODUCT_GROUP { get; set; }
        public virtual DbSet<TBL_PRODUCT_PRICE_INDEX> TBL_PRODUCT_PRICE_INDEX { get; set; }
        public virtual DbSet<TBL_PRODUCT_TYPE> TBL_PRODUCT_TYPE { get; set; }
        public virtual DbSet<TBL_PROFILE_ACTIVITY> TBL_PROFILE_ACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_ACTIVITY_PARENT> TBL_PROFILE_ACTIVITY_PARENT { get; set; }
        public virtual DbSet<TBL_PROFILE_ADDITIONALACTIVITY> TBL_PROFILE_ADDITIONALACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_GROUP> TBL_PROFILE_GROUP { get; set; }
        public virtual DbSet<TBL_PROFILE_GROUP_ACTIVITY> TBL_PROFILE_GROUP_ACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_PRIVILEDGE> TBL_PROFILE_PRIVILEDGE { get; set; }
        public virtual DbSet<TBL_PROFILE_PRIVILEDGE_ACTIVITY> TBL_PROFILE_PRIVILEDGE_ACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_USER> TBL_PROFILE_USER { get; set; }
        public virtual DbSet<TBL_PROFILE_USERGROUP> TBL_PROFILE_USERGROUP { get; set; }
        public virtual DbSet<TBL_PUBLIC_HOLIDAY> TBL_PUBLIC_HOLIDAY { get; set; }
        public virtual DbSet<TBL_REGION> TBL_REGION { get; set; }
        public virtual DbSet<TBL_SECTOR> TBL_SECTOR { get; set; }
        public virtual DbSet<TBL_SETUP_GLOBAL> TBL_SETUP_GLOBAL { get; set; }
        public virtual DbSet<TBL_SOURCE_APPLICATION> TBL_SOURCE_APPLICATION { get; set; }
        public virtual DbSet<TBL_STAFF> TBL_STAFF { get; set; }
        public virtual DbSet<TBL_STAFF_JOBTITLE> TBL_STAFF_JOBTITLE { get; set; }
        public virtual DbSet<TBL_STAFF_ORGANOGRAM> TBL_STAFF_ORGANOGRAM { get; set; }
        public virtual DbSet<TBL_STAFF_RANK> TBL_STAFF_RANK { get; set; }
        public virtual DbSet<TBL_STATE> TBL_STATE { get; set; }
        public virtual DbSet<TBL_SUB_SECTOR> TBL_SUB_SECTOR { get; set; }
        public virtual DbSet<TBL_TAX> TBL_TAX { get; set; }
        public virtual DbSet<TBL_TENOR_MODE> TBL_TENOR_MODE { get; set; }
        public virtual DbSet<TBL_CALL_MEMO> TBL_CALL_MEMO { get; set; }
        public virtual DbSet<TBL_CALL_MEMO_LIMIT> TBL_CALL_MEMO_LIMIT { get; set; }
        public virtual DbSet<TBL_CALL_MEMO_TYPE> TBL_CALL_MEMO_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_CASA> TBL_COLLATERAL_CASA { get; set; }
        public virtual DbSet<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }
        public virtual DbSet<TBL_COLLATERAL_DEPOSIT> TBL_COLLATERAL_DEPOSIT { get; set; }
        public virtual DbSet<TBL_COLLATERAL_DOCUMENTS> TBL_COLLATERAL_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_COLLATERAL_GAURANTEE> TBL_COLLATERAL_GAURANTEE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_IMMOVABLE_PROPERTY> TBL_COLLATERAL_IMMOVABLE_PROPERTY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_ITEM_POLICY> TBL_COLLATERAL_ITEM_POLICY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_MARKETABLE_SECURITY> TBL_COLLATERAL_MARKETABLE_SECURITY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_MISCELLANEOUS> TBL_COLLATERAL_MISCELLANEOUS { get; set; }
        public virtual DbSet<TBL_COLLATERAL_MISCELLANEOUS_NOTES> TBL_COLLATERAL_MISCELLANEOUS_NOTES { get; set; }
        public virtual DbSet<TBL_COLLATERAL_PLANT_AND_EQUIPMENT> TBL_COLLATERAL_PLANT_AND_EQUIPMENT { get; set; }
        public virtual DbSet<TBL_COLLATERAL_POLICY> TBL_COLLATERAL_POLICY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_PRECIOUSMETAL> TBL_COLLATERAL_PRECIOUSMETAL { get; set; }
        public virtual DbSet<TBL_COLLATERAL_PRINCIPALS> TBL_COLLATERAL_PRINCIPALS { get; set; }
        public virtual DbSet<TBL_COLLATERAL_SENIORITYOFCLAIMS> TBL_COLLATERAL_SENIORITYOFCLAIMS { get; set; }
        public virtual DbSet<TBL_COLLATERAL_STOCK> TBL_COLLATERAL_STOCK { get; set; }
        public virtual DbSet<TBL_COLLATERAL_TYPE> TBL_COLLATERAL_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_TYPE_SUB> TBL_COLLATERAL_TYPE_SUB { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VALUEBASE_TYPE> TBL_COLLATERAL_VALUEBASE_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VALUER> TBL_COLLATERAL_VALUER { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VALUER_TYPE> TBL_COLLATERAL_VALUER_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VEHICLE> TBL_COLLATERAL_VEHICLE { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMORANDUM> TBL_CREDIT_APPRAISAL_MEMORANDUM { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT> TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL> TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL { get; set; }
        public virtual DbSet<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }
        public virtual DbSet<TBL_LIMIT> TBL_LIMIT { get; set; }
        public virtual DbSet<TBL_LIMIT_DETAIL> TBL_LIMIT_DETAIL { get; set; }
        public virtual DbSet<TBL_LIMIT_METRIC> TBL_LIMIT_METRIC { get; set; }
        public virtual DbSet<TBL_LIMIT_TYPE> TBL_LIMIT_TYPE { get; set; }
        public virtual DbSet<TBL_LIMIT_VALUE_TYPE> TBL_LIMIT_VALUE_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN> TBL_LOAN { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_COLLATERAL> TBL_LOAN_APPLICATION_COLLATERAL { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETAIL_STATUS> TBL_LOAN_APPLICATION_DETAIL_STATUS { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_STATUS> TBL_LOAN_APPLICATION_STATUS { get; set; }
        public virtual DbSet<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_BULK_INTEREST_REVIEW> TBL_LOAN_BULK_INTEREST_REVIEW { get; set; }
        public virtual DbSet<TBL_LOAN_CAMSOL> TBL_LOAN_CAMSOL { get; set; }
        public virtual DbSet<TBL_LOAN_COLLATERAL_MAPPING> TBL_LOAN_COLLATERAL_MAPPING { get; set; }
        public virtual DbSet<TBL_LOAN_COMMENT> TBL_LOAN_COMMENT { get; set; }
        public virtual DbSet<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }
        public virtual DbSet<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }
        public virtual DbSet<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }
        public virtual DbSet<TBL_LOAN_COVENANT_TYPE> TBL_LOAN_COVENANT_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_FEE> TBL_LOAN_FEE { get; set; }
        public virtual DbSet<TBL_LOAN_FEE_SCHEDULE> TBL_LOAN_FEE_SCHEDULE { get; set; }
        public virtual DbSet<TBL_LOAN_FORCE_DEBIT> TBL_LOAN_FORCE_DEBIT { get; set; }
        public virtual DbSet<TBL_LOAN_GUARANTOR> TBL_LOAN_GUARANTOR { get; set; }
        public virtual DbSet<TBL_LOAN_OPERATION> TBL_LOAN_OPERATION { get; set; }
        public virtual DbSet<TBL_LOAN_PAST_DUE> TBL_LOAN_PAST_DUE { get; set; }
        public virtual DbSet<TBL_LOAN_PRELIMINARY_EVALUATION> TBL_LOAN_PRELIMINARY_EVALUATION { get; set; }
        public virtual DbSet<TBL_LOAN_PRICEINDEX_EXCEPTION> TBL_LOAN_PRICEINDEX_EXCEPTION { get; set; }
        public virtual DbSet<TBL_LOAN_PRUDENTIALGUIDELINE> TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }
        public virtual DbSet<TBL_LOAN_RELATIONSHIP_OFFICER_HISTORY> TBL_LOAN_RELATIONSHIP_OFFICER_HISTORY { get; set; }
        public virtual DbSet<TBL_LOAN_REVIEW_OPERATION> TBL_LOAN_REVIEW_OPERATION { get; set; }
        public virtual DbSet<TBL_LOAN_REVIEW_OPERATION_IRREGULAR_SCHEDULE> TBL_LOAN_REVIEW_OPERATION_IRREGULAR_SCHEDULE { get; set; }
        public virtual DbSet<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_CATEGORY> TBL_LOAN_SCHEDULE_CATEGORY { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_DAILY> TBL_LOAN_SCHEDULE_DAILY { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE> TBL_LOAN_SCHEDULE_DAILY_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_DAILY_TEMP> TBL_LOAN_SCHEDULE_DAILY_TEMP { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_IRREGULAR_INPUT> TBL_LOAN_SCHEDULE_IRREGULAR_INPUT { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_PERIODIC> TBL_LOAN_SCHEDULE_PERIODIC { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE> TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_PERIODIC_TEMP> TBL_LOAN_SCHEDULE_PERIODIC_TEMP { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_TYPE> TBL_LOAN_SCHEDULE_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_TYPE_PRODUCT_TYPE_MAPPING> TBL_LOAN_SCHEDULE_TYPE_PRODUCT_TYPE_MAPPING { get; set; }
        public virtual DbSet<TBL_LOAN_STATUS> TBL_LOAN_STATUS { get; set; }
        public virtual DbSet<TBL_LOAN_TRANSACTION_TYPE> TBL_LOAN_TRANSACTION_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_TYPE> TBL_LOAN_TYPE { get; set; }
        public virtual DbSet<TBL_LOANAPPLICATION_COLLATERAL_MAPPING> TBL_LOANAPPLICATION_COLLATERAL_MAPPING { get; set; }
        public virtual DbSet<TBL_MACHINEVALUE_BASE> TBL_MACHINEVALUE_BASE { get; set; }
        public virtual DbSet<TBL_PRODUCT_COLLATERALTYPE> TBL_PRODUCT_COLLATERALTYPE { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT> TBL_RISK_ASSESSMENT { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_INDEX> TBL_RISK_ASSESSMENT_INDEX { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_INDEX_TYPE> TBL_RISK_ASSESSMENT_INDEX_TYPE { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_RESULT> TBL_RISK_ASSESSMENT_RESULT { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_TITLE> TBL_RISK_ASSESSMENT_TITLE { get; set; }
        public virtual DbSet<TBL_RISK_RATING> TBL_RISK_RATING { get; set; }
        public virtual DbSet<TBL_SOLICITOR> TBL_SOLICITOR { get; set; }
        public virtual DbSet<TBL_SOLICITOR_STATE_MAPPING> TBL_SOLICITOR_STATE_MAPPING { get; set; }
        public virtual DbSet<SYSDIAGRAMS> SYSDIAGRAMS { get; set; }
        public virtual DbSet<TBL_CHARGES_VALUESOURCE> TBL_CHARGES_VALUESOURCE { get; set; }
        public virtual DbSet<TBL_COT> TBL_COT { get; set; }
        public virtual DbSet<TBL_LOAN_DOCUMENT_TYPE> TBL_LOAN_DOCUMENT_TYPE { get; set; }
        public virtual DbSet<TBL_ACCOUNT_CATEGORY> TBL_ACCOUNT_CATEGORY { get; set; }
        public virtual DbSet<TBL_ACCOUNT_TYPE> TBL_ACCOUNT_TYPE { get; set; }
        public virtual DbSet<TBL_CHARGE_RANGE> TBL_CHARGE_RANGE { get; set; }
        public virtual DbSet<TBL_CHARGES> TBL_CHARGES { get; set; }
        public virtual DbSet<TBL_CHART_OF_ACCOUNT> TBL_CHART_OF_ACCOUNT { get; set; }
        public virtual DbSet<TBL_CHART_OF_ACCOUNT_CLASS> TBL_CHART_OF_ACCOUNT_CLASS { get; set; }
        public virtual DbSet<TBL_CHART_OF_ACCOUNT_CURRENCY> TBL_CHART_OF_ACCOUNT_CURRENCY { get; set; }
        public virtual DbSet<TBL_FINANCIAL_STATEMENT_CAPTION> TBL_FINANCIAL_STATEMENT_CAPTION { get; set; }
        public virtual DbSet<TBL_FINANCIAL_STATEMENT_TYPE> TBL_FINANCIAL_STATEMENT_TYPE { get; set; }
        public virtual DbSet<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_TEMP_CHART_OF_ACCOUNT> TBL_TEMP_CHART_OF_ACCOUNT { get; set; }
        public virtual DbSet<TBL_TEMP_CHART_OF_ACCOUNT_CURRENCY> TBL_TEMP_CHART_OF_ACCOUNT_CURRENCY { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_CASA> TBL_TEMP_COLLATERAL_CASA { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_DEPOSIT> TBL_TEMP_COLLATERAL_DEPOSIT { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_DOCUMENTS> TBL_TEMP_COLLATERAL_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_GAURANTEE> TBL_TEMP_COLLATERAL_GAURANTEE { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY> TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY> TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_MISCELLANEOUS> TBL_TEMP_COLLATERAL_MISCELLANEOUS { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_MISCELLANEOUS_NOTES> TBL_TEMP_COLLATERAL_MISCELLANEOUS_NOTES { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT> TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_POLICY> TBL_TEMP_COLLATERAL_POLICY { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_PRECIOUSMETAL> TBL_TEMP_COLLATERAL_PRECIOUSMETAL { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_VEHICLE> TBL_TEMP_COLLATERAL_VEHICLE { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_GROUP> TBL_TEMP_CUSTOMER_GROUP { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_GROUP_MAPPING> TBL_TEMP_CUSTOMER_GROUP_MAPPING { get; set; }
        public virtual DbSet<TBL_TEMP_FEE> TBL_TEMP_FEE { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_CHARGE_FEE> TBL_TEMP_PRODUCT_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_COLLATERALTYPE> TBL_TEMP_PRODUCT_COLLATERALTYPE { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_CURRENCY> TBL_TEMP_PRODUCT_CURRENCY { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_FEE> TBL_TEMP_PRODUCT_FEE { get; set; }
        public virtual DbSet<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
        public virtual DbSet<TBL_DEAL_CLASSIFICATION> TBL_DEAL_CLASSIFICATION { get; set; }
        public virtual DbSet<TBL_DEAL_TYPE> TBL_DEAL_TYPE { get; set; }
        public virtual DbSet<TBL_STOCK> TBL_STOCK { get; set; }
        public virtual DbSet<DEV_CHECKLIST> DEV_CHECKLIST { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_STOCK> TBL_TEMP_COLLATERAL_STOCK { get; set; }
        public virtual DbSet<view_Approval_Setup> view_Approval_Setup { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TBL_ACCREDITEDCONSULTANT>()
                .HasMany(e => e.TBL_ACCREDITEDCONSULTANT_STATE)
                .WithRequired(e => e.TBL_ACCREDITEDCONSULTANT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCREDITEDCONSULTANT_TYPE>()
                .HasMany(e => e.TBL_ACCREDITEDCONSULTANT)
                .WithOptional(e => e.TBL_ACCREDITEDCONSULTANT_TYPE)
                .HasForeignKey(e => e.ACCREDITEDCONSULTANTTYPEID);

            modelBuilder.Entity<TBL_APPROVAL_GROUP>()
                .HasMany(e => e.TBL_APPROVAL_GROUP_MAPPING)
                .WithRequired(e => e.TBL_APPROVAL_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_GROUP>()
                .HasMany(e => e.TBL_APPROVAL_LEVEL)
                .WithRequired(e => e.TBL_APPROVAL_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .Property(e => e.MAXIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .Property(e => e.INVESTMENTGRADEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_APPROVAL_LEVEL_STAFF)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithOptional(e => e.TBL_APPROVAL_LEVEL)
                .HasForeignKey(e => e.FROMAPPROVALLEVELID);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL1)
                .WithOptional(e => e.TBL_APPROVAL_LEVEL1)
                .HasForeignKey(e => e.TOAPPROVALLEVELID);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_CREDIT_TEMPLATE)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL_STAFF>()
                .Property(e => e.MAXIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_APPROVAL_STATE>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithRequired(e => e.TBL_APPROVAL_STATE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .HasForeignKey(e => e.STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_COLLATERAL_MAPPING)
                .WithOptional(e => e.TBL_APPROVAL_STATUS)
                .HasForeignKey(e => e.RELEASEAPPROVALSTATUSID);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_FEE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_AUDIT)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_CASA_LIEN)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_CUSTOMER)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION)
                .WithRequired(e => e.TBL_BRANCH)
                .HasForeignKey(e => e.SOURCEBRANCHID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION1)
                .WithRequired(e => e.TBL_BRANCH1)
                .HasForeignKey(e => e.DESTINATIONBRANCHID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .Property(e => e.AVAILABLEBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA>()
                .Property(e => e.LEDGERBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CASA>()
                .Property(e => e.OVERDRAFTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA>()
                .Property(e => e.OVERDRAFTINTERESTRATE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA>()
                .Property(e => e.LIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_CASA_OVERDRAFT)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_CASA)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE1)
                .WithRequired(e => e.TBL_CASA1)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_CASA)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN1)
                .WithRequired(e => e.TBL_CASA1)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA_ACCOUNTSTATUS>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_CASA_ACCOUNTSTATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA_LIEN>()
                .Property(e => e.LIENCREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA_LIEN>()
                .Property(e => e.LIENDEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA_LIEN_TYPE>()
                .HasMany(e => e.TBL_CASA_LIEN)
                .WithRequired(e => e.TBL_CASA_LIEN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA_OVERDRAFT>()
                .Property(e => e.CREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA_OVERDRAFT>()
                .Property(e => e.DEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CASA_POSTNOSTATUS>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_CASA_POSTNOSTATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_CHARGE_RANGE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_LOAN_FEE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_PRODUCT_CHARGE_FEE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_CHARGE_FEE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_DEFINITION>()
                .HasMany(e => e.TBL_CHECKLIST_DETAIL)
                .WithRequired(e => e.TBL_CHECKLIST_DEFINITION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_ITEM>()
                .HasMany(e => e.TBL_CHECKLIST_DEFINITION)
                .WithRequired(e => e.TBL_CHECKLIST_ITEM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_STATUS>()
                .HasMany(e => e.TBL_CHECKLIST_DETAIL)
                .WithRequired(e => e.TBL_CHECKLIST_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_TARGETTYPE>()
                .HasMany(e => e.TBL_CHECKLIST_DETAIL)
                .WithRequired(e => e.TBL_CHECKLIST_TARGETTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_COLLATERAL_IMMOVABLE_PROPERTY)
                .WithRequired(e => e.TBL_CITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY)
                .WithRequired(e => e.TBL_CITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY_CLASS>()
                .HasMany(e => e.TBL_CITY)
                .WithRequired(e => e.TBL_CITY_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .Property(e => e.SHAREHOLDERSFUND)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COMPANY>()
                .Property(e => e.PRELIMINARYEVALUATION_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COMPANY>()
                .Property(e => e.AUTHORISEDSHARECAPITAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_APPROVAL_GROUP)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_BRANCH)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CASA_LIEN)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CHECKLIST_DEFINITION)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_PRODUCT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CALL_MEMO_LIMIT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_COMPANY1)
                .WithOptional(e => e.TBL_COMPANY2)
                .HasForeignKey(e => e.PARENTID);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CREDIT_TEMPLATE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION_GROUP)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_CAPTION)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER_RISK_RATING)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_FINANCE_ENDOFDAY)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_FINANCECURRENTDATE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LIMIT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_BULK_INTEREST_REVIEW)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_CAMSOL)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_PRODUCT_COLLATERALTYPE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYPE)
                .WithRequired(e => e.TBL_COMPANY)
                .HasForeignKey(e => e.COMPANYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_PRODUCT_CHARGE_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_PRODUCT_PRICE_INDEX)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_SETUP_GLOBAL)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_SOLICITOR)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_STOCK)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TAX)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_CHARGE_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYPE1)
                .WithRequired(e => e.TBL_COMPANY1)
                .HasForeignKey(e => e.COMPANYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_FEE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CONTENT_PLACEHOLDER>()
                .Property(e => e.CONTENTPLACEHOLDER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CONTENT_PLACEHOLDER>()
                .Property(e => e.COLLUMNNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COUNTRY>()
                .HasMany(e => e.TBL_COMPANY)
                .WithRequired(e => e.TBL_COUNTRY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COUNTRY>()
                .HasMany(e => e.TBL_STATE)
                .WithRequired(e => e.TBL_COUNTRY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COUNTRY>()
                .HasMany(e => e.TBL_PUBLIC_HOLIDAY)
                .WithRequired(e => e.TBL_COUNTRY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COUNTRY>()
                .HasMany(e => e.TBL_REGION)
                .WithRequired(e => e.TBL_COUNTRY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_COMPANY)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT_CURRENCY)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT_CURRENCY)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_CURRENCY_RATE)
                .WithRequired(e => e.TBL_CURRENCY)
                .HasForeignKey(e => e.CURRENCYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_CURRENCY_RATE1)
                .WithRequired(e => e.TBL_CURRENCY1)
                .HasForeignKey(e => e.BASECURRENCYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_PRODUCT_CURRENCY)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_CURRENCY)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOM_FIELDS>()
                .HasMany(e => e.TBL_CUSTOM_FIELDS_DATA)
                .WithRequired(e => e.TBL_CUSTOM_FIELDS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOM_FIELDS_DATA>()
                .HasMany(e => e.TBL_CUSTOM_FIELD_DATA_UPLOAD)
                .WithRequired(e => e.TBL_CUSTOM_FIELDS_DATA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOM_HOSTPAGE>()
                .HasOptional(e => e.TBL_CUSTOM_HOSTPAGE1)
                .WithRequired(e => e.TBL_CUSTOM_HOSTPAGE2);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOM_FIELDS)
                .WithOptional(e => e.TBL_CUSTOMER)
                .HasForeignKey(e => e.ACTEDONBY);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOM_FIELDS_DATA)
                .WithOptional(e => e.TBL_CUSTOMER)
                .HasForeignKey(e => e.ACTEDONBY);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOM_FIELDS_DATA1)
                .WithRequired(e => e.TBL_CUSTOMER1)
                .HasForeignKey(e => e.OWNERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_ACCOUNT_KYC_ITEM)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_BVN)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_CHILDREN)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_COMPANYINFOMATION)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_EMPLOYMENTHISTORY)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_GUARDIAN)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_IDENTIFICATION)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_NEXTOFKIN)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_PHONECONTACT)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_ADDRESS)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_CLIENT_SUPPLIER)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_CLIENT_SUPPLIER_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_CLIENT_SUPPLIER)
                .WithRequired(e => e.TBL_CUSTOMER_CLIENT_SUPPLIER_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_COMPANY_DIRECTORTYPE>()
                .HasMany(e => e.TBL_CUSTOMER_COMPANY_DIRECTOR)
                .WithRequired(e => e.TBL_CUSTOMER_COMPANY_DIRECTORTYPE)
                .HasForeignKey(e => e.COMPANYDIRECTORTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION1)
                .WithOptional(e => e.TBL_CUSTOMER_FS_CAPTION2)
                .HasForeignKey(e => e.PARENTIDFSCAPTIONID);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_FS_CAPTION_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION_DETAIL>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION_GROUP>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_RATIO_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_DETAIL>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_DIVISORTYPE>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_RATIO_DIVISORTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_VALUETYPE>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_RATIO_VALUETYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_FS_CAPTION_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP_FS_CAPTION_DETAIL>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP_RELATIONSHIPTYPE>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP_RELATIONSHIPTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP_RELATIONSHIPTYPE>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP_RELATIONSHIPTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_IDENTIFICATIONMODETYPE>()
                .Property(e => e.IDENTIFICATIONMODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_PHONECONTACT>()
                .Property(e => e.PHONE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAILY_ACCRUAL>()
                .Property(e => e.MAINAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_DAILY_ACCRUAL>()
                .Property(e => e.DAILYACCURALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_DAILY_ACCRUAL>()
                .Property(e => e.SYSTEMDATETIME)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_DAILY_ACCRUAL_CATEGORY>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_DAILY_ACCRUAL_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAY_COUNT_CONVENTION>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_DAY_COUNT_CONVENTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAY_COUNT_CONVENTION>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_DAY_COUNT_CONVENTION)
                .HasForeignKey(e => e.SCHEDULEDAYCOUNTCONVENTIONID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAY_COUNT_CONVENTION>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_DAY_COUNT_CONVENTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAY_COUNT_CONVENTION>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_DAY_COUNT_CONVENTION)
                .HasForeignKey(e => e.SCHEDULEDAYCOUNTCONVENTIONID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAY_INTEREST_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_DAY_INTEREST_TYPE)
                .HasForeignKey(e => e.SCHEDULEDAYINTERESTTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DAY_INTEREST_TYPE>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_DAY_INTEREST_TYPE)
                .HasForeignKey(e => e.SCHEDULEDAYINTERESTTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DEPARTMENT>()
                .HasMany(e => e.TBL_DEPARTMENT_UNIT)
                .WithRequired(e => e.TBL_DEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DEPARTMENT>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_DEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DEPARTMENT_UNIT>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_DEPARTMENT_UNIT)
                .HasForeignKey(e => e.DEPARTMENTUNITID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_FEE)
                .WithRequired(e => e.TBL_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_INTERVAL>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_INTERVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_INTERVAL>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_FEE_INTERVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_INTERVAL>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_INTERVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_INTERVAL>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_FEE_INTERVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TARGET>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_TARGET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TARGET>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_FEE_TARGET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TARGET>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_TARGET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TARGET>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_FEE_TARGET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TYPE>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TYPE>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_FEE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TYPE>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_FEE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FINANCE_TRANSACTION>()
                .Property(e => e.DEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_FINANCE_TRANSACTION>()
                .Property(e => e.CREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_CALL_MEMO_LIMIT)
                .WithRequired(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.FREQUENCYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_POLICY)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.RENEWALFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LIMIT_DETAIL)
                .WithRequired(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.LIMITFREQUENCYTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.INTERESTFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE1)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE1)
                .HasForeignKey(e => e.PRINCIPALFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE2)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE2)
                .HasForeignKey(e => e.SCHEDULEDPREPAYMENTFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LOAN)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.INTERESTFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LOAN1)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE1)
                .HasForeignKey(e => e.PRINCIPALFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_LOAN2)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE2)
                .HasForeignKey(e => e.SCHEDULEDPREPAYMENTFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_POLICY)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.RENEWALFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_JOB_REQUEST>()
                .HasMany(e => e.TBL_JOB_REQUEST_DOCUMENT_MAPPING)
                .WithRequired(e => e.TBL_JOB_REQUEST)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_REQUEST_STATUS>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_JOB_REQUEST_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_TYPE>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_JOB_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_MESSAGE_LOG>()
                .Property(e => e.MESSAGEBODY)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_MESSAGE_LOG_STATUS>()
                .HasMany(e => e.TBL_MESSAGE_LOG)
                .WithRequired(e => e.TBL_MESSAGE_LOG_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_MESSAGE_LOG_TYPE>()
                .HasMany(e => e.TBL_MESSAGE_LOG)
                .WithRequired(e => e.TBL_MESSAGE_LOG_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_MIS_INFO>()
                .HasMany(e => e.TBL_MIS_INFO1)
                .WithOptional(e => e.TBL_MIS_INFO2)
                .HasForeignKey(e => e.PARENTMISINFOID);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_APPROVAL_GROUP_MAPPING)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_OPERATIONS)
                .HasForeignKey(e => e.OPERATIONSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS_TYPE>()
                .HasMany(e => e.TBL_OPERATIONS)
                .WithRequired(e => e.TBL_OPERATIONS_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_PRODUCT_COLLATERALTYPE)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_PRODUCT)
                .HasForeignKey(e => e.PROPOSEDPRODUCTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL1)
                .WithRequired(e => e.TBL_PRODUCT1)
                .HasForeignKey(e => e.APPROVEDPRODUCTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_PRODUCT_CURRENCY)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_PRODUCT_CHARGE_FEE)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_RISK_RATING)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CATEGORY>()
                .HasMany(e => e.TBL_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CATEGORY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CHARGE_FEE>()
                .Property(e => e.RATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_PRODUCT_CHARGE_FEE>()
                .Property(e => e.DEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_PRODUCT_CLASS>()
                .HasMany(e => e.TBL_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CLASS>()
                .HasMany(e => e.TBL_CREDIT_TEMPLATE)
                .WithRequired(e => e.TBL_PRODUCT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CLASS>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION)
                .WithRequired(e => e.TBL_PRODUCT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CLASS>()
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CLASS_TYPE>()
                .HasMany(e => e.TBL_PRODUCT_CLASS)
                .WithRequired(e => e.TBL_PRODUCT_CLASS_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_GROUP>()
                .HasMany(e => e.TBL_PRODUCT_TYPE)
                .WithRequired(e => e.TBL_PRODUCT_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_PRICE_INDEX>()
                .HasMany(e => e.TBL_LOAN_BULK_INTEREST_REVIEW)
                .WithRequired(e => e.TBL_PRODUCT_PRICE_INDEX)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_COVENANT_DETAIL)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_FEE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_FORCE_DEBIT)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_GUARANTOR)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_PAST_DUE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_SCHEDULE_TYPE_PRODUCT_TYPE_MAPPING)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_ADDITIONALACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_GROUP_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_PRIVILEDGE_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY_PARENT>()
                .HasMany(e => e.TBL_PROFILE_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY_PARENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_GROUP>()
                .HasMany(e => e.TBL_PROFILE_GROUP_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_GROUP>()
                .HasMany(e => e.TBL_PROFILE_USERGROUP)
                .WithRequired(e => e.TBL_PROFILE_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_PRIVILEDGE>()
                .HasMany(e => e.TBL_PROFILE_PRIVILEDGE_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_PRIVILEDGE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_USER>()
                .HasMany(e => e.TBL_PROFILE_ADDITIONALACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_USER>()
                .HasMany(e => e.TBL_PROFILE_PRIVILEDGE_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_USER>()
                .HasMany(e => e.TBL_PROFILE_USERGROUP)
                .WithRequired(e => e.TBL_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.GENDER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.GENDEROFNOK)
                .IsFixedLength();

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_APPROVAL_LEVEL_STAFF)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.REQUESTSTAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL1)
                .WithOptional(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RESPONSESTAFFID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_AUDIT)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_CASA)
                .WithOptional(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_CASA1)
                .WithOptional(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_CUSTOMER)
                .WithOptional(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.POSTEDBY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.APPROVEDBY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.SENDERSTAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_JOB_REQUEST1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RECEIVERSTAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_JOB_REQUEST2)
                .WithOptional(e => e.TBL_STAFF2)
                .HasForeignKey(e => e.REASSIGNEDTO);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_NOTIFICATION_LOG)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_PROFILE_USER)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_CALL_MEMO)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_APPLICATION1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_RELATIONSHIP_OFFICER_HISTORY)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_REVOLVING1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_JOBTITLE>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_STAFF_JOBTITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_JOBTITLE>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithRequired(e => e.TBL_STAFF_JOBTITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_RANK>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_STAFF_RANK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_RANK>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithRequired(e => e.TBL_STAFF_RANK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STATE>()
                .Property(e => e.COLLATERALSEARCHCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .Property(e => e.CHARTINGAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .Property(e => e.VERIFICATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .HasMany(e => e.TBL_CITY)
                .WithRequired(e => e.TBL_STATE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STATE>()
                .HasMany(e => e.TBL_SOLICITOR_STATE_MAPPING)
                .WithRequired(e => e.TBL_STATE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TAX>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TAX>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithOptional(e => e.TBL_TAX)
                .HasForeignKey(e => e.PRIMARYTAXID);

            modelBuilder.Entity<TBL_TAX>()
                .HasMany(e => e.TBL_CHARGE_FEE1)
                .WithOptional(e => e.TBL_TAX1)
                .HasForeignKey(e => e.SECONDARYTAXID);

            modelBuilder.Entity<TBL_TAX>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithOptional(e => e.TBL_TAX)
                .HasForeignKey(e => e.PRIMARYTAXID);

            modelBuilder.Entity<TBL_TAX>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE1)
                .WithOptional(e => e.TBL_TAX1)
                .HasForeignKey(e => e.SECONDARYTAXID);

            modelBuilder.Entity<TBL_CALL_MEMO_LIMIT>()
                .Property(e => e.MINIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CALL_MEMO_LIMIT>()
                .Property(e => e.MAXIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CALL_MEMO_TYPE>()
                .HasMany(e => e.TBL_CALL_MEMO)
                .WithRequired(e => e.TBL_CALL_MEMO_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CASA>()
                .Property(e => e.AVAILABLEBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_CASA>()
                .Property(e => e.EXISTINGLIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_CASA>()
                .Property(e => e.LIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_CASA>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_CASA>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .Property(e => e.COLLATERALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .Property(e => e.CAMREFNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_CASA)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_DOCUMENTS)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_ITEM_POLICY)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_DEPOSIT)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_GAURANTEE)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_POLICY)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_PLANT_AND_EQUIPMENT)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_MARKETABLE_SECURITY)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_MISCELLANEOUS)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_PRECIOUSMETAL)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_IMMOVABLE_PROPERTY)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_STOCK)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_VEHICLE)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_COLLATERAL_MAPPING)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_POLICY)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_PRECIOUSMETAL)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_STOCK)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_DEPOSIT>()
                .Property(e => e.EXISTINGLIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_DEPOSIT>()
                .Property(e => e.LIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_DEPOSIT>()
                .Property(e => e.AVAILABLEBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_DEPOSIT>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_DEPOSIT>()
                .Property(e => e.MATURITYAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_DEPOSIT>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_DOCUMENTS>()
                .Property(e => e.DOCUMENTCATEGORY)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_DOCUMENTS>()
                .Property(e => e.DOCUMENTTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_DOCUMENTS>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_GAURANTEE>()
                .Property(e => e.INSTITUTIONNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_GAURANTEE>()
                .Property(e => e.GUARANTORADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_GAURANTEE>()
                .Property(e => e.GUARANTEEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_GAURANTEE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.PROPERTYADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.OPENMARKETVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.COLLATERALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.FORCEDSALEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.STAMPTOCOVER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.ORIGINALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.AVAILABLEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.COLLATERALUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.LONGITUDE)
                .HasPrecision(12, 9);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.LATITUDE)
                .HasPrecision(12, 9);

            modelBuilder.Entity<TBL_COLLATERAL_ITEM_POLICY>()
                .Property(e => e.SUMINSURED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.DEALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.LIENUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.ISSUERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.ISSUERREFERENCENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.UNITVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MISCELLANEOUS>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MISCELLANEOUS>()
                .Property(e => e.NOTE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MISCELLANEOUS>()
                .HasMany(e => e.TBL_COLLATERAL_MISCELLANEOUS_NOTES)
                .WithOptional(e => e.TBL_COLLATERAL_MISCELLANEOUS)
                .HasForeignKey(e => e.MISCELLANEOUSID);

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.MACHINENAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.YEAROFMANUFACTURE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.YEAROFPURCHASE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.REPLACEMENTVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_POLICY>()
                .Property(e => e.PREMIUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_POLICY>()
                .Property(e => e.POLICYAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_POLICY>()
                .Property(e => e.INSURANCECOMPANYNAME)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_POLICY>()
                .Property(e => e.INSURERADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_POLICY>()
                .Property(e => e.INSURERDETAILS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_POLICY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.PRECIOUSMETALNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.VALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.PRECIOUSMETALFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_SENIORITYOFCLAIMS>()
                .Property(e => e.SENIORITYOFCLAIMS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_SENIORITYOFCLAIMS>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_STOCK>()
                .Property(e => e.MARKETPRICE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_STOCK>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_STOCK>()
                .Property(e => e.SHARESSECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_STOCK>()
                .Property(e => e.SHAREVALUEAMOUNTTOUSE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_TYPE_SUB)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_VALUEBASE_TYPE)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_PRODUCT_COLLATERALTYPE)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYPE)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .HasForeignKey(e => e.COLLATERALTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYPE1)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE1)
                .HasForeignKey(e => e.COLLATERALTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE_SUB>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE_SUB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE_SUB>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE_SUB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUEBASE_TYPE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUEBASE_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_PLANT_AND_EQUIPMENT)
                .WithRequired(e => e.TBL_COLLATERAL_VALUEBASE_TYPE)
                .HasForeignKey(e => e.VALUEBASETYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUEBASE_TYPE>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT)
                .WithRequired(e => e.TBL_COLLATERAL_VALUEBASE_TYPE)
                .HasForeignKey(e => e.VALUEBASETYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUER_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_VALUER)
                .WithOptional(e => e.TBL_COLLATERAL_VALUER_TYPE)
                .HasForeignKey(e => e.VALUERTYPEID);

            modelBuilder.Entity<TBL_COLLATERAL_VEHICLE>()
                .Property(e => e.RESALEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_VEHICLE>()
                .Property(e => e.LASTVALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_VEHICLE>()
                .Property(e => e.INVOICEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_VEHICLE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMORANDUM>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT)
                .WithRequired(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMORANDUM>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL)
                .WithRequired(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LIMIT>()
                .HasMany(e => e.TBL_LIMIT_DETAIL)
                .WithRequired(e => e.TBL_LIMIT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LIMIT_DETAIL>()
                .Property(e => e.MINIMUMVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LIMIT_DETAIL>()
                .Property(e => e.MAXIMUMVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LIMIT_METRIC>()
                .HasMany(e => e.TBL_LIMIT)
                .WithRequired(e => e.TBL_LIMIT_METRIC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LIMIT_TYPE>()
                .HasMany(e => e.TBL_LIMIT_DETAIL)
                .WithRequired(e => e.TBL_LIMIT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LIMIT_VALUE_TYPE>()
                .HasMany(e => e.TBL_LIMIT)
                .WithRequired(e => e.TBL_LIMIT_VALUE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.EQUITYCONTRIBUTION)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.OUTSTANDINGPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.OUTSTANDINGINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.SCHEDULEDPREPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .HasMany(e => e.TBL_LOAN_SCHEDULE_DAILY)
                .WithRequired(e => e.TBL_LOAN)
                .HasForeignKey(e => e.LOANID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN>()
                .HasMany(e => e.TBL_LOAN_SCHEDULE_PERIODIC)
                .WithRequired(e => e.TBL_LOAN)
                .HasForeignKey(e => e.LOANID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .Property(e => e.APPLICATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .Property(e => e.APPROVEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDUM)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_CONDITION_PRECEDENT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_COLLATERAL_MAPPING)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_GUARANTOR)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_RISK_ASSESSMENT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_COLLATERAL>()
                .Property(e => e.COLLATERALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .Property(e => e.PROPOSEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .Property(e => e.APPROVEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_STATUS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.EQUITYCONTRIBUTION)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.OUTSTANDINGPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.OUTSTANDINGINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.SCHEDULEDPREPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_CAMSOL>()
                .Property(e => e.AMOUNTAFFECTED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_CONTINGENT>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN_CONTINGENT>()
                .Property(e => e.CONTINGENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_COVENANT_DETAIL>()
                .Property(e => e.COVENANTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_COVENANT_TYPE>()
                .HasMany(e => e.TBL_LOAN_COVENANT_DETAIL)
                .WithRequired(e => e.TBL_LOAN_COVENANT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .Property(e => e.FEERATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .Property(e => e.FEEDEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .Property(e => e.FEEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .HasMany(e => e.TBL_LOAN_FEE_SCHEDULE)
                .WithRequired(e => e.TBL_LOAN_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_FEE_SCHEDULE>()
                .Property(e => e.FEEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FORCE_DEBIT>()
                .Property(e => e.DEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FORCE_DEBIT>()
                .Property(e => e.CREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PAST_DUE>()
                .Property(e => e.DEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PAST_DUE>()
                .Property(e => e.CREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PRELIMINARY_EVALUATION>()
                .Property(e => e.LOANAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN)
                .WithOptional(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.INTERNALPRUDENTIALGUIDELINESTATUSID);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN1)
                .WithOptional(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.EXTERNALPRUDENTIALGUIDELINESTATUSID);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithOptional(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.INTERNALPRUDENTIALGUIDELINESTATUSID);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE1)
                .WithOptional(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.EXTERNALPRUDENTIALGUIDELINESTATUSID);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithOptional(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.EXTERNALPRUDENTIALGUIDELINESTATUSID);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING1)
                .WithOptional(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.INTERNALPRUDENTIALGUIDELINESTATUSID);

            modelBuilder.Entity<TBL_LOAN_REVIEW_OPERATION>()
                .Property(e => e.PREPAYMENT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVIEW_OPERATION>()
                .Property(e => e.OVERDRAFTTOPUP)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVIEW_OPERATION>()
                .Property(e => e.FEE_CHARGES)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVIEW_OPERATION>()
                .HasMany(e => e.TBL_LOAN_REVIEW_OPERATION_IRREGULAR_SCHEDULE)
                .WithRequired(e => e.TBL_LOAN_REVIEW_OPERATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_REVIEW_OPERATION_IRREGULAR_SCHEDULE>()
                .Property(e => e.PAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.OVERDRAFTLIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_CATEGORY>()
                .HasMany(e => e.TBL_LOAN_SCHEDULE_TYPE)
                .WithRequired(e => e.TBL_LOAN_SCHEDULE_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.OPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.DAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.DAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.DAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.CLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.ACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.PREVIOUSINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.PREVIOUSPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDOPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDDAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDDAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDDAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDCLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISEDACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.AMORTISED_AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.DISCOUNTPREMIUM)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.UNEARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.EARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY>()
                .Property(e => e.BALLONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.OPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.DAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.DAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.DAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.CLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.ACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDOPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDDAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDDAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDDAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDCLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISEDACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.AMORTISED_AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.DISCOUNTPREMIUM)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.UNEARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.EARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIVE>()
                .Property(e => e.BALLONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.OPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.DAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.DAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.DAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.CLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.ACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDOPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDDAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDDAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDDAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDCLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISEDACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.AMORTISED_AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.DISCOUNTPREMIUM)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.UNEARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.EARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_TEMP>()
                .Property(e => e.BALLONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_IRREGULAR_INPUT>()
                .Property(e => e.PAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.PERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.PERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.PERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.PREVIOUSINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.PREVIOUSPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.AMORTISEDPERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.AMORTISEDPERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.AMORTISEDPERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.PERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.PERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.PERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.AMORTISEDPERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.AMORTISEDPERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.AMORTISEDPERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARCHIVE>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.PERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.PERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.PERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.AMORTISEDPERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.AMORTISEDPERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.AMORTISEDPERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TEMP>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_TYPE>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_SCHEDULE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_SCHEDULE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_TYPE>()
                .HasMany(e => e.TBL_LOAN_SCHEDULE_TYPE_PRODUCT_TYPE_MAPPING)
                .WithRequired(e => e.TBL_LOAN_SCHEDULE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_STATUS>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_STATUS>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_STATUS>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_LOAN_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_STATUS>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_LOAN_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TRANSACTION_TYPE>()
                .HasMany(e => e.TBL_DAILY_ACCRUAL)
                .WithRequired(e => e.TBL_LOAN_TRANSACTION_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TRANSACTION_TYPE>()
                .HasMany(e => e.TBL_LOAN_FORCE_DEBIT)
                .WithRequired(e => e.TBL_LOAN_TRANSACTION_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TRANSACTION_TYPE>()
                .HasMany(e => e.TBL_LOAN_PAST_DUE)
                .WithRequired(e => e.TBL_LOAN_TRANSACTION_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TYPE>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TYPE>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_LOAN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TYPE>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_LOAN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TYPE>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATION)
                .WithRequired(e => e.TBL_LOAN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_TYPE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_LOAN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOANAPPLICATION_COLLATERAL_MAPPING>()
                .HasOptional(e => e.TBL_LOANAPPLICATION_COLLATERAL_MAPPING1)
                .WithRequired(e => e.TBL_LOANAPPLICATION_COLLATERAL_MAPPING2);

            modelBuilder.Entity<TBL_MACHINEVALUE_BASE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_RISK_ASSESSMENT_INDEX>()
                .Property(e => e.WEIGHT)
                .HasPrecision(18, 4);

            modelBuilder.Entity<TBL_RISK_ASSESSMENT_INDEX_TYPE>()
                .HasMany(e => e.TBL_RISK_ASSESSMENT_INDEX)
                .WithRequired(e => e.TBL_RISK_ASSESSMENT_INDEX_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_RISK_ASSESSMENT_TITLE>()
                .HasMany(e => e.TBL_RISK_ASSESSMENT)
                .WithRequired(e => e.TBL_RISK_ASSESSMENT_TITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_RISK_ASSESSMENT_TITLE>()
                .HasMany(e => e.TBL_RISK_ASSESSMENT_INDEX)
                .WithRequired(e => e.TBL_RISK_ASSESSMENT_TITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_RISK_RATING>()
                .Property(e => e.RATESDESCRIPTION)
                .IsFixedLength();

            modelBuilder.Entity<TBL_SOLICITOR>()
                .HasMany(e => e.TBL_SOLICITOR_STATE_MAPPING)
                .WithRequired(e => e.TBL_SOLICITOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SOLICITOR_STATE_MAPPING>()
                .Property(e => e.COLLATERALSEARCHCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COT>()
                .Property(e => e.COTACCOUNTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COT>()
                .Property(e => e.COTCREATEDBY)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_ACCOUNT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_ACCOUNT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_ACCOUNT_TYPE)
                .WithRequired(e => e.TBL_ACCOUNT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_ACCOUNT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_ACCOUNT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_TYPE>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_ACCOUNT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_TYPE>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_ACCOUNT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_RANGE>()
                .Property(e => e.MINIMUM)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CHARGE_RANGE>()
                .Property(e => e.MAXIMUM)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CHARGE_RANGE>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_FEE)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_FINANCE_TRANSACTION)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_PRODUCT)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.PRINCIPALBALANCEGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_PRODUCT1)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT1)
                .HasForeignKey(e => e.INTERESTRECEIVABLEPAYABLEGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_PRODUCT2)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT2)
                .HasForeignKey(e => e.INTERESTINCOMEEXPENSEGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_PRODUCT3)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT3)
                .HasForeignKey(e => e.PREMIUMDISCOUNTGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_PRODUCT4)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT4)
                .HasForeignKey(e => e.DORMANTGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_PRODUCT5)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT5)
                .HasForeignKey(e => e.OVERDRAWNGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TAX)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_COLLATERAL_TYPE)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.CHARGEGLACCOUNTID);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT_CURRENCY)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.PRINCIPALBALANCEGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT1)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT1)
                .HasForeignKey(e => e.INTERESTRECEIVABLEPAYABLEGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT2)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT2)
                .HasForeignKey(e => e.INTERESTINCOMEEXPENSEGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT3)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT3)
                .HasForeignKey(e => e.PREMIUMDISCOUNTGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT4)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT4)
                .HasForeignKey(e => e.DORMANTGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT5)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT5)
                .HasForeignKey(e => e.OVERDRAWNGL);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_FEE)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT_CLASS>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FINANCIAL_STATEMENT_CAPTION>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_FINANCIAL_STATEMENT_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FINANCIAL_STATEMENT_CAPTION>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_FINANCIAL_STATEMENT_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FINANCIAL_STATEMENT_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WithRequired(e => e.TBL_FINANCIAL_STATEMENT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_CHARGE_FEE>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT_CURRENCY)
                .WithRequired(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CASA>()
                .Property(e => e.AVAILABLEBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CASA>()
                .Property(e => e.EXISTINGLIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CASA>()
                .Property(e => e.LIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CASA>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CASA>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .Property(e => e.CAMREFNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CASA)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_DOCUMENTS)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_DEPOSIT)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_GAURANTEE)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_MISCELLANEOUS)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DEPOSIT>()
                .Property(e => e.EXISTINGLIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DEPOSIT>()
                .Property(e => e.LIENAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DEPOSIT>()
                .Property(e => e.AVAILABLEBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DEPOSIT>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DEPOSIT>()
                .Property(e => e.MATURITYAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DEPOSIT>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DOCUMENTS>()
                .Property(e => e.DOCUMENTCATEGORY)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DOCUMENTS>()
                .Property(e => e.DOCUMENTTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_DOCUMENTS>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_GAURANTEE>()
                .Property(e => e.INSTITUTIONNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_GAURANTEE>()
                .Property(e => e.GUARANTORADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_GAURANTEE>()
                .Property(e => e.GUARANTEEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_GAURANTEE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.PROPERTYADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.OPENMARKETVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.COLLATERALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.FORCEDSALEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.STAMPTOCOVER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.ORIGINALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.AVAILABLEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.COLLATERALUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.LONGITUDE)
                .HasPrecision(12, 9);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .Property(e => e.LATITUDE)
                .HasPrecision(12, 9);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>()
                .HasOptional(e => e.TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY1)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY2);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.DEALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.LIENUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.ISSUERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.ISSUERREFERENCENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.UNITVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MISCELLANEOUS>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MISCELLANEOUS>()
                .Property(e => e.NOTE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MISCELLANEOUS>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_MISCELLANEOUS_NOTES)
                .WithOptional(e => e.TBL_TEMP_COLLATERAL_MISCELLANEOUS)
                .HasForeignKey(e => e.MISCELLANEOUSID);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.MACHINENAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.YEAROFMANUFACTURE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.YEAROFPURCHASE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_AND_EQUIPMENT>()
                .Property(e => e.REPLACEMENTVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_POLICY>()
                .Property(e => e.PREMIUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_POLICY>()
                .Property(e => e.POLICYAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_POLICY>()
                .Property(e => e.INSURANCECOMPANYNAME)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_POLICY>()
                .Property(e => e.INSURERADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_POLICY>()
                .Property(e => e.INSURERDETAILS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_POLICY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.PRECIOUSMETALNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.METALTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.VALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.PRECIOUSMETALFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PRECIOUSMETAL>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_VEHICLE>()
                .Property(e => e.RESALEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_VEHICLE>()
                .Property(e => e.LASTVALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_VEHICLE>()
                .Property(e => e.INVOICEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_VEHICLE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_PRODUCT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYPE)
                .WithRequired(e => e.TBL_TEMP_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_PRODUCT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_CURRENCY)
                .WithRequired(e => e.TBL_TEMP_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_PRODUCT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_CHARGE_FEE)
                .WithRequired(e => e.TBL_TEMP_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_PRODUCT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_FEE)
                .WithRequired(e => e.TBL_TEMP_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_PRODUCT_CHARGE_FEE>()
                .Property(e => e.RATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PRODUCT_CHARGE_FEE>()
                .Property(e => e.DEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PRODUCT_FEE>()
                .Property(e => e.RATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PRODUCT_FEE>()
                .Property(e => e.DEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_STAFF>()
                .Property(e => e.GENDER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_STAFF>()
                .Property(e => e.GENDEROFNOK)
                .IsFixedLength();

            modelBuilder.Entity<TBL_DEAL_CLASSIFICATION>()
                .HasMany(e => e.TBL_PRODUCT_TYPE)
                .WithRequired(e => e.TBL_DEAL_CLASSIFICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_STOCK>()
                .Property(e => e.MARKETPRICE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_STOCK>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_STOCK>()
                .Property(e => e.SHARESSECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_STOCK>()
                .Property(e => e.SHAREVALUEAMOUNTTOUSE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<view_Approval_Setup>()
                .Property(e => e.LEVELMAXIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<view_Approval_Setup>()
                .Property(e => e.INVESTMENTGRADEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<view_Approval_Setup>()
                .Property(e => e.STAFFMAXIMUMAMOUNT)
                .HasPrecision(19, 4);
        }
    }
}
