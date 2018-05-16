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
        public virtual DbSet<TBL_APPROVAL_GROUP> TBL_APPROVAL_GROUP { get; set; }
        public virtual DbSet<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }
        public virtual DbSet<TBL_APPROVAL_LEVEL> TBL_APPROVAL_LEVEL { get; set; }
        public virtual DbSet<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }
        public virtual DbSet<TBL_APPROVAL_STATE> TBL_APPROVAL_STATE { get; set; }
        public virtual DbSet<TBL_APPROVAL_STATUS> TBL_APPROVAL_STATUS { get; set; }
        public virtual DbSet<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }
        public virtual DbSet<TBL_APPROVAL_VOTE_OPTION> TBL_APPROVAL_VOTE_OPTION { get; set; }
        public virtual DbSet<TBL_AUDIT> TBL_AUDIT { get; set; }
        public virtual DbSet<TBL_AUDIT_TYPE> TBL_AUDIT_TYPE { get; set; }
        public virtual DbSet<TBL_BRANCH> TBL_BRANCH { get; set; }
        public virtual DbSet<TBL_BRANCH_REGION> TBL_BRANCH_REGION { get; set; }
        public virtual DbSet<TBL_CASA> TBL_CASA { get; set; }
        public virtual DbSet<TBL_CASA_ACCOUNTSTATUS> TBL_CASA_ACCOUNTSTATUS { get; set; }
        public virtual DbSet<TBL_CASA_LIEN> TBL_CASA_LIEN { get; set; }
        public virtual DbSet<TBL_CASA_LIEN_TYPE> TBL_CASA_LIEN_TYPE { get; set; }
        public virtual DbSet<TBL_CASA_OVERDRAFT> TBL_CASA_OVERDRAFT { get; set; }
        public virtual DbSet<TBL_CASA_POSTNOSTATUS> TBL_CASA_POSTNOSTATUS { get; set; }
        public virtual DbSet<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL { get; set; }
        public virtual DbSet<TBL_CHARGE_FEE_DETAIL_TYPE> TBL_CHARGE_FEE_DETAIL_TYPE { get; set; }
        public virtual DbSet<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }
        public virtual DbSet<TBL_CHECKLIST_DETAIL> TBL_CHECKLIST_DETAIL { get; set; }
        public virtual DbSet<TBL_CHECKLIST_ITEM> TBL_CHECKLIST_ITEM { get; set; }
        public virtual DbSet<TBL_CHECKLIST_RESPONSE_TYPE> TBL_CHECKLIST_RESPONSE_TYPE { get; set; }
        public virtual DbSet<TBL_CHECKLIST_STATUS> TBL_CHECKLIST_STATUS { get; set; }
        public virtual DbSet<TBL_CHECKLIST_TARGETTYPE> TBL_CHECKLIST_TARGETTYPE { get; set; }
        public virtual DbSet<TBL_CHECKLIST_TYPE> TBL_CHECKLIST_TYPE { get; set; }
        public virtual DbSet<TBL_CHECKLIST_TYPE_APROV_LEVL> TBL_CHECKLIST_TYPE_APROV_LEVL { get; set; }
        public virtual DbSet<TBL_CITY> TBL_CITY { get; set; }
        public virtual DbSet<TBL_CITY_CLASS> TBL_CITY_CLASS { get; set; }
        public virtual DbSet<TBL_COMPANY> TBL_COMPANY { get; set; }
        public virtual DbSet<TBL_COMPANY_CLASS> TBL_COMPANY_CLASS { get; set; }
        public virtual DbSet<TBL_COMPANY_TYPE> TBL_COMPANY_TYPE { get; set; }
        public virtual DbSet<TBL_CONTENT_PLACEHOLDER> TBL_CONTENT_PLACEHOLDER { get; set; }
        public virtual DbSet<TBL_COUNTRY> TBL_COUNTRY { get; set; }
        public virtual DbSet<TBL_CURRENCY> TBL_CURRENCY { get; set; }
        public virtual DbSet<TBL_CURRENCY_EXCHANGERATE> TBL_CURRENCY_EXCHANGERATE { get; set; }
        public virtual DbSet<TBL_CURRENCY_RATECODE> TBL_CURRENCY_RATECODE { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELD_DATA_UPLOAD> TBL_CUSTOM_FIELD_DATA_UPLOAD { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELD_OPTION> TBL_CUSTOM_FIELD_OPTION { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELDS> TBL_CUSTOM_FIELDS { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIELDS_DATA> TBL_CUSTOM_FIELDS_DATA { get; set; }
        public virtual DbSet<TBL_CUSTOM_HOSTPAGE> TBL_CUSTOM_HOSTPAGE { get; set; }
        public virtual DbSet<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }
        public virtual DbSet<TBL_CUSTOMER_ACCOUNT_KYC_ITEM> TBL_CUSTOMER_ACCOUNT_KYC_ITEM { get; set; }
        public virtual DbSet<TBL_CUSTOMER_ADDRESS> TBL_CUSTOMER_ADDRESS { get; set; }
        public virtual DbSet<TBL_CUSTOMER_ADDRESS_TYPE> TBL_CUSTOMER_ADDRESS_TYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_BLACKLIST> TBL_CUSTOMER_BLACKLIST { get; set; }
        public virtual DbSet<TBL_CUSTOMER_BVN> TBL_CUSTOMER_BVN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CHILDREN> TBL_CUSTOMER_CHILDREN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CLIENT_SUPPLIER> TBL_CUSTOMER_CLIENT_SUPPLIER { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CLIENT_SUPPLR_TYP> TBL_CUSTOMER_CLIENT_SUPPLR_TYP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANY_BENEFICIA> TBL_CUSTOMER_COMPANY_BENEFICIA { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANY_DIREC_TYP> TBL_CUSTOMER_COMPANY_DIREC_TYP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANY_DIRECTOR> TBL_CUSTOMER_COMPANY_DIRECTOR { get; set; }
        public virtual DbSet<TBL_CUSTOMER_COMPANYINFOMATION> TBL_CUSTOMER_COMPANYINFOMATION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CUSTOM_FIELD> TBL_CUSTOMER_CUSTOM_FIELD { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EDIT_HISTORY> TBL_CUSTOMER_EDIT_HISTORY { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EDUCATIONLEVELTYP> TBL_CUSTOMER_EDUCATIONLEVELTYP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EMPLOYER> TBL_CUSTOMER_EMPLOYER { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EMPLOYER_TYPE> TBL_CUSTOMER_EMPLOYER_TYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EMPLOYER_TYPE_SUB> TBL_CUSTOMER_EMPLOYER_TYPE_SUB { get; set; }
        public virtual DbSet<TBL_CUSTOMER_EMPLOYMENTHISTORY> TBL_CUSTOMER_EMPLOYMENTHISTORY { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_CAPTION> TBL_CUSTOMER_FS_CAPTION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_CAPTION_DETAIL> TBL_CUSTOMER_FS_CAPTION_DETAIL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_CAPTION_GROUP> TBL_CUSTOMER_FS_CAPTION_GROUP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_DETAIL> TBL_CUSTOMER_FS_RATIO_DETAIL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_DIVI_TYP> TBL_CUSTOMER_FS_RATIO_DIVI_TYP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_FS_RATIO_VALUETYP> TBL_CUSTOMER_FS_RATIO_VALUETYP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP> TBL_CUSTOMER_GROUP { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP_MAPPING> TBL_CUSTOMER_GROUP_MAPPING { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GROUP_RELATN_TYPE> TBL_CUSTOMER_GROUP_RELATN_TYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GRP_FS_CAPTN_DET> TBL_CUSTOMER_GRP_FS_CAPTN_DET { get; set; }
        public virtual DbSet<TBL_CUSTOMER_GUARDIAN> TBL_CUSTOMER_GUARDIAN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_IDENTI_MODE_TYPE> TBL_CUSTOMER_IDENTI_MODE_TYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_IDENTIFICATION> TBL_CUSTOMER_IDENTIFICATION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_MODIFICATION> TBL_CUSTOMER_MODIFICATION { get; set; }
        public virtual DbSet<TBL_CUSTOMER_MODIFICATN_TYPE> TBL_CUSTOMER_MODIFICATN_TYPE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_NEXTOFKIN> TBL_CUSTOMER_NEXTOFKIN { get; set; }
        public virtual DbSet<TBL_CUSTOMER_PHONECONTACT> TBL_CUSTOMER_PHONECONTACT { get; set; }
        public virtual DbSet<TBL_CUSTOMER_PRODUCT_FEE> TBL_CUSTOMER_PRODUCT_FEE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_RISK_RATING> TBL_CUSTOMER_RISK_RATING { get; set; }
        public virtual DbSet<TBL_CUSTOMER_SENSITIVITY_LEVEL> TBL_CUSTOMER_SENSITIVITY_LEVEL { get; set; }
        public virtual DbSet<TBL_CUSTOMER_SIGNATORY> TBL_CUSTOMER_SIGNATORY { get; set; }
        public virtual DbSet<TBL_CUSTOMER_TYPE> TBL_CUSTOMER_TYPE { get; set; }
        public virtual DbSet<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }
        public virtual DbSet<TBL_DAILY_ACCRUAL_CATEGORY> TBL_DAILY_ACCRUAL_CATEGORY { get; set; }
        public virtual DbSet<TBL_DAY_COUNT_CONVENTION> TBL_DAY_COUNT_CONVENTION { get; set; }
        public virtual DbSet<TBL_DAY_INTEREST_TYPE> TBL_DAY_INTEREST_TYPE { get; set; }
        public virtual DbSet<TBL_DEPARTMENT> TBL_DEPARTMENT { get; set; }
        public virtual DbSet<TBL_DEPARTMENT_UNIT> TBL_DEPARTMENT_UNIT { get; set; }
        public virtual DbSet<TBL_ERRORLOG> TBL_ERRORLOG { get; set; }
        public virtual DbSet<TBL_FEE_AMORTISATION_TYPE> TBL_FEE_AMORTISATION_TYPE { get; set; }
        public virtual DbSet<TBL_FEE_INTERVAL> TBL_FEE_INTERVAL { get; set; }
        public virtual DbSet<TBL_FEE_TARGET> TBL_FEE_TARGET { get; set; }
        public virtual DbSet<TBL_FEE_TYPE> TBL_FEE_TYPE { get; set; }
        public virtual DbSet<TBL_FINANCE_ENDOFDAY> TBL_FINANCE_ENDOFDAY { get; set; }
        public virtual DbSet<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }
        public virtual DbSet<TBL_FINANCECURRENTDATE> TBL_FINANCECURRENTDATE { get; set; }
        public virtual DbSet<TBL_FREQUENCY_TYPE> TBL_FREQUENCY_TYPE { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_DETAIL> TBL_JOB_REQUEST_DETAIL { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_DOCUMENT_MAPPN> TBL_JOB_REQUEST_DOCUMENT_MAPPN { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_MESSAGE> TBL_JOB_REQUEST_MESSAGE { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_STATUS> TBL_JOB_REQUEST_STATUS { get; set; }
        public virtual DbSet<TBL_JOB_REQUEST_STATUS_FEEDBAK> TBL_JOB_REQUEST_STATUS_FEEDBAK { get; set; }
        public virtual DbSet<TBL_JOB_TYPE> TBL_JOB_TYPE { get; set; }
        public virtual DbSet<TBL_JOB_TYPE_DEPARTMENT> TBL_JOB_TYPE_DEPARTMENT { get; set; }
        public virtual DbSet<TBL_JOB_TYPE_SUB> TBL_JOB_TYPE_SUB { get; set; }
        public virtual DbSet<TBL_KYC_DOCUMENTTYPE> TBL_KYC_DOCUMENTTYPE { get; set; }
        public virtual DbSet<TBL_KYC_ITEM> TBL_KYC_ITEM { get; set; }
        public virtual DbSet<TBL_LANGUAGE> TBL_LANGUAGE { get; set; }
        public virtual DbSet<TBL_LOCALGOVERNMENT> TBL_LOCALGOVERNMENT { get; set; }
        public virtual DbSet<TBL_MANAGEMENT_TYPE> TBL_MANAGEMENT_TYPE { get; set; }
        public virtual DbSet<TBL_MESSAGE_LOG> TBL_MESSAGE_LOG { get; set; }
        public virtual DbSet<TBL_MESSAGE_LOG_STATUS> TBL_MESSAGE_LOG_STATUS { get; set; }
        public virtual DbSet<TBL_MESSAGE_LOG_TYPE> TBL_MESSAGE_LOG_TYPE { get; set; }
        public virtual DbSet<TBL_MIS_INFO> TBL_MIS_INFO { get; set; }
        public virtual DbSet<TBL_MIS_TYPE> TBL_MIS_TYPE { get; set; }
        public virtual DbSet<TBL_MONITORING_ALERT_SETUP> TBL_MONITORING_ALERT_SETUP { get; set; }
        public virtual DbSet<TBL_NATURE_OF_BUSINESS> TBL_NATURE_OF_BUSINESS { get; set; }
        public virtual DbSet<TBL_NOTIFICATION_LOG> TBL_NOTIFICATION_LOG { get; set; }
        public virtual DbSet<TBL_OPERATIONS> TBL_OPERATIONS { get; set; }
        public virtual DbSet<TBL_OPERATIONS_TYPE> TBL_OPERATIONS_TYPE { get; set; }
        public virtual DbSet<TBL_OVERRIDE_DETAIL> TBL_OVERRIDE_DETAIL { get; set; }
        public virtual DbSet<TBL_OVERRIDE_ITEM> TBL_OVERRIDE_ITEM { get; set; }
        public virtual DbSet<TBL_POSTING_TYPE> TBL_POSTING_TYPE { get; set; }
        public virtual DbSet<TBL_PRODUCT> TBL_PRODUCT { get; set; }
        public virtual DbSet<TBL_PRODUCT_BEHAVIOUR> TBL_PRODUCT_BEHAVIOUR { get; set; }
        public virtual DbSet<TBL_PRODUCT_CATEGORY> TBL_PRODUCT_CATEGORY { get; set; }
        public virtual DbSet<TBL_PRODUCT_CHARGE_FEE> TBL_PRODUCT_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_PRODUCT_CLASS> TBL_PRODUCT_CLASS { get; set; }
        public virtual DbSet<TBL_PRODUCT_CLASS_PROCESS> TBL_PRODUCT_CLASS_PROCESS { get; set; }
        public virtual DbSet<TBL_PRODUCT_CLASS_TYPE> TBL_PRODUCT_CLASS_TYPE { get; set; }
        public virtual DbSet<TBL_PRODUCT_CURRENCY> TBL_PRODUCT_CURRENCY { get; set; }
        public virtual DbSet<TBL_PRODUCT_GROUP> TBL_PRODUCT_GROUP { get; set; }
        public virtual DbSet<TBL_PRODUCT_PRICE_INDEX> TBL_PRODUCT_PRICE_INDEX { get; set; }
        public virtual DbSet<TBL_PRODUCT_PRICE_INDEX_DAILY> TBL_PRODUCT_PRICE_INDEX_DAILY { get; set; }
        public virtual DbSet<TBL_PRODUCT_TYPE> TBL_PRODUCT_TYPE { get; set; }
        public virtual DbSet<TBL_PROFILE_ACTIVITY> TBL_PROFILE_ACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_ACTIVITY_PARENT> TBL_PROFILE_ACTIVITY_PARENT { get; set; }
        public virtual DbSet<TBL_PROFILE_ADDITIONALACTIVITY> TBL_PROFILE_ADDITIONALACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_GROUP> TBL_PROFILE_GROUP { get; set; }
        public virtual DbSet<TBL_PROFILE_GROUP_ACTIVITY> TBL_PROFILE_GROUP_ACTIVITY { get; set; }
        public virtual DbSet<TBL_PROFILE_PRIVILEDGE> TBL_PROFILE_PRIVILEDGE { get; set; }
        public virtual DbSet<TBL_PROFILE_PRIVILEDGE_ACTIVIT> TBL_PROFILE_PRIVILEDGE_ACTIVIT { get; set; }
        public virtual DbSet<TBL_PROFILE_STAFF_ROLE_ADT_ACT> TBL_PROFILE_STAFF_ROLE_ADT_ACT { get; set; }
        public virtual DbSet<TBL_PROFILE_STAFF_ROLE_GROUP> TBL_PROFILE_STAFF_ROLE_GROUP { get; set; }
        public virtual DbSet<TBL_PROFILE_USER> TBL_PROFILE_USER { get; set; }
        public virtual DbSet<TBL_PROFILE_USERGROUP> TBL_PROFILE_USERGROUP { get; set; }
        public virtual DbSet<TBL_PUBLIC_HOLIDAY> TBL_PUBLIC_HOLIDAY { get; set; }
        public virtual DbSet<TBL_REGION> TBL_REGION { get; set; }
        public virtual DbSet<TBL_SECTOR> TBL_SECTOR { get; set; }
        public virtual DbSet<TBL_SETUP_COMPANY> TBL_SETUP_COMPANY { get; set; }
        public virtual DbSet<TBL_SETUP_GLOBAL> TBL_SETUP_GLOBAL { get; set; }
        public virtual DbSet<TBL_SIGNATURE_DOCUMENT_STAFF> TBL_SIGNATURE_DOCUMENT_STAFF { get; set; }
        public virtual DbSet<TBL_SIGNATURE_DOCUMENT_TYPE> TBL_SIGNATURE_DOCUMENT_TYPE { get; set; }
        public virtual DbSet<TBL_SOURCE_APPLICATION> TBL_SOURCE_APPLICATION { get; set; }
        public virtual DbSet<TBL_STAFF> TBL_STAFF { get; set; }
        public virtual DbSet<TBL_STAFF_ACCOUNT_HISTORY> TBL_STAFF_ACCOUNT_HISTORY { get; set; }
        public virtual DbSet<TBL_STAFF_JOBTITLE> TBL_STAFF_JOBTITLE { get; set; }
        public virtual DbSet<TBL_STAFF_RELIEF> TBL_STAFF_RELIEF { get; set; }
        public virtual DbSet<TBL_STAFF_ROLE> TBL_STAFF_ROLE { get; set; }
        public virtual DbSet<TBL_STATE> TBL_STATE { get; set; }
        public virtual DbSet<TBL_SUB_SECTOR> TBL_SUB_SECTOR { get; set; }
        public virtual DbSet<TBL_TAX> TBL_TAX { get; set; }
        public virtual DbSet<TBL_TENOR_MODE> TBL_TENOR_MODE { get; set; }
        public virtual DbSet<TBL_CALL_MEMO> TBL_CALL_MEMO { get; set; }
        public virtual DbSet<TBL_CALL_MEMO_LIMIT> TBL_CALL_MEMO_LIMIT { get; set; }
        public virtual DbSet<TBL_CALL_MEMO_TYPE> TBL_CALL_MEMO_TYPE { get; set; }
        public virtual DbSet<TBL_CHECKLIST_DEFERRAL> TBL_CHECKLIST_DEFERRAL { get; set; }
        public virtual DbSet<TBL_COLLATERAL_CASA> TBL_COLLATERAL_CASA { get; set; }
        public virtual DbSet<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }
        public virtual DbSet<TBL_COLLATERAL_DEPOSIT> TBL_COLLATERAL_DEPOSIT { get; set; }
        public virtual DbSet<TBL_COLLATERAL_GAURANTEE> TBL_COLLATERAL_GAURANTEE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_IMMOVE_PROPERTY> TBL_COLLATERAL_IMMOVE_PROPERTY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_ITEM_POLICY> TBL_COLLATERAL_ITEM_POLICY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_MISC_NOTES> TBL_COLLATERAL_MISC_NOTES { get; set; }
        public virtual DbSet<TBL_COLLATERAL_MISCELLANEOUS> TBL_COLLATERAL_MISCELLANEOUS { get; set; }
        public virtual DbSet<TBL_COLLATERAL_MKT_SECURITY> TBL_COLLATERAL_MKT_SECURITY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_PERFECTN_STAT> TBL_COLLATERAL_PERFECTN_STAT { get; set; }
        public virtual DbSet<TBL_COLLATERAL_PLANT_AND_EQUIP> TBL_COLLATERAL_PLANT_AND_EQUIP { get; set; }
        public virtual DbSet<TBL_COLLATERAL_POLICY> TBL_COLLATERAL_POLICY { get; set; }
        public virtual DbSet<TBL_COLLATERAL_PRECIOUSMETAL> TBL_COLLATERAL_PRECIOUSMETAL { get; set; }
        public virtual DbSet<TBL_COLLATERAL_SENIORITY_CLAIM> TBL_COLLATERAL_SENIORITY_CLAIM { get; set; }
        public virtual DbSet<TBL_COLLATERAL_STOCK> TBL_COLLATERAL_STOCK { get; set; }
        public virtual DbSet<TBL_COLLATERAL_TYPE> TBL_COLLATERAL_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_TYPE_SUB> TBL_COLLATERAL_TYPE_SUB { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VALUEBASE_TYPE> TBL_COLLATERAL_VALUEBASE_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VALUER> TBL_COLLATERAL_VALUER { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VALUER_TYPE> TBL_COLLATERAL_VALUER_TYPE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VEHICLE> TBL_COLLATERAL_VEHICLE { get; set; }
        public virtual DbSet<TBL_COLLATERAL_VISITATION> TBL_COLLATERAL_VISITATION { get; set; }
        public virtual DbSet<TBL_COMPLIANCE_TIMELINE> TBL_COMPLIANCE_TIMELINE { get; set; }
        public virtual DbSet<TBL_CONDITION_PRECEDENT> TBL_CONDITION_PRECEDENT { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMO_DETL> TBL_CREDIT_APPRAISAL_MEMO_DETL { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMO_DOCU> TBL_CREDIT_APPRAISAL_MEMO_DOCU { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMO_LOG> TBL_CREDIT_APPRAISAL_MEMO_LOG { get; set; }
        public virtual DbSet<TBL_CREDIT_APPRAISAL_MEMORANDM> TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }
        public virtual DbSet<TBL_CREDIT_BUREAU> TBL_CREDIT_BUREAU { get; set; }
        public virtual DbSet<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }
        public virtual DbSet<TBL_LOAN> TBL_LOAN { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_COLLATERL> TBL_LOAN_APPLICATION_COLLATERL { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_COLLATRL2> TBL_LOAN_APPLICATION_COLLATRL2 { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_BG> TBL_LOAN_APPLICATION_DETL_BG { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_EDU> TBL_LOAN_APPLICATION_DETL_EDU { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_FEE> TBL_LOAN_APPLICATION_DETL_FEE { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_INV> TBL_LOAN_APPLICATION_DETL_INV { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_LOG> TBL_LOAN_APPLICATION_DETL_LOG { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_STA> TBL_LOAN_APPLICATION_DETL_STA { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_DETL_TRA> TBL_LOAN_APPLICATION_DETL_TRA { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_STATUS> TBL_LOAN_APPLICATION_STATUS { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATION_TYPE> TBL_LOAN_APPLICATION_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_APPLICATN_DETL_MTRIG> TBL_LOAN_APPLICATN_DETL_MTRIG { get; set; }
        public virtual DbSet<TBL_LOAN_APPLTN_CREDIT_BUREAU> TBL_LOAN_APPLTN_CREDIT_BUREAU { get; set; }
        public virtual DbSet<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_BOOKING_REQUEST> TBL_LOAN_BOOKING_REQUEST { get; set; }
        public virtual DbSet<TBL_LOAN_BULK_INTEREST_REVIEW> TBL_LOAN_BULK_INTEREST_REVIEW { get; set; }
        public virtual DbSet<TBL_LOAN_CAMSOL> TBL_LOAN_CAMSOL { get; set; }
        public virtual DbSet<TBL_LOAN_COLLATERAL_MAPPING> TBL_LOAN_COLLATERAL_MAPPING { get; set; }
        public virtual DbSet<TBL_LOAN_COMMENT> TBL_LOAN_COMMENT { get; set; }
        public virtual DbSet<TBL_LOAN_CONCESSION_TYPE> TBL_LOAN_CONCESSION_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_CONDITION_DEFERRAL> TBL_LOAN_CONDITION_DEFERRAL { get; set; }
        public virtual DbSet<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }
        public virtual DbSet<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }
        public virtual DbSet<TBL_LOAN_CONTINGENT_USAGE> TBL_LOAN_CONTINGENT_USAGE { get; set; }
        public virtual DbSet<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }
        public virtual DbSet<TBL_LOAN_COVENANT_TYPE> TBL_LOAN_COVENANT_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_FEE> TBL_LOAN_FEE { get; set; }
        public virtual DbSet<TBL_LOAN_FEE_ARCHIVE> TBL_LOAN_FEE_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_FEE_SCHEDULE> TBL_LOAN_FEE_SCHEDULE { get; set; }
        public virtual DbSet<TBL_LOAN_FORCE_DEBIT> TBL_LOAN_FORCE_DEBIT { get; set; }
        public virtual DbSet<TBL_LOAN_MARKET> TBL_LOAN_MARKET { get; set; }
        public virtual DbSet<TBL_LOAN_MATURITY_INSTRU_TYPE> TBL_LOAN_MATURITY_INSTRU_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_MATURITY_INSTRUCTION> TBL_LOAN_MATURITY_INSTRUCTION { get; set; }
        public virtual DbSet<TBL_LOAN_MONITORING_TRIG_SETUP> TBL_LOAN_MONITORING_TRIG_SETUP { get; set; }
        public virtual DbSet<TBL_LOAN_MONITORING_TRIGGER> TBL_LOAN_MONITORING_TRIGGER { get; set; }
        public virtual DbSet<TBL_LOAN_OPERATION> TBL_LOAN_OPERATION { get; set; }
        public virtual DbSet<TBL_LOAN_PAST_DUE> TBL_LOAN_PAST_DUE { get; set; }
        public virtual DbSet<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }
        public virtual DbSet<TBL_LOAN_PRICEINDEX_EXCEPTION> TBL_LOAN_PRICEINDEX_EXCEPTION { get; set; }
        public virtual DbSet<TBL_LOAN_PRINCIPAL> TBL_LOAN_PRINCIPAL { get; set; }
        public virtual DbSet<TBL_LOAN_PRUDENT_GUIDE_TYPE> TBL_LOAN_PRUDENT_GUIDE_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_PRUDENTIALGUIDELINE> TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }
        public virtual DbSet<TBL_LOAN_RATE_FEE_CONCESSION> TBL_LOAN_RATE_FEE_CONCESSION { get; set; }
        public virtual DbSet<TBL_LOAN_RECOVERY_PLAN> TBL_LOAN_RECOVERY_PLAN { get; set; }
        public virtual DbSet<TBL_LOAN_RECOVERY_PLAN_PAYMNT> TBL_LOAN_RECOVERY_PLAN_PAYMNT { get; set; }
        public virtual DbSet<TBL_LOAN_RELATIONSHIP_OFF_HIST> TBL_LOAN_RELATIONSHIP_OFF_HIST { get; set; }
        public virtual DbSet<TBL_LOAN_REVIEW_APPLICATION> TBL_LOAN_REVIEW_APPLICATION { get; set; }
        public virtual DbSet<TBL_LOAN_REVIEW_APPLICATN_CAM> TBL_LOAN_REVIEW_APPLICATN_CAM { get; set; }
        public virtual DbSet<TBL_LOAN_REVIEW_OPERATION> TBL_LOAN_REVIEW_OPERATION { get; set; }
        public virtual DbSet<TBL_LOAN_REVIEW_OPRATN_IREG_SC> TBL_LOAN_REVIEW_OPRATN_IREG_SC { get; set; }
        public virtual DbSet<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }
        public virtual DbSet<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }
        public virtual DbSet<TBL_LOAN_REVOLVING_TYPE> TBL_LOAN_REVOLVING_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_CATEGORY> TBL_LOAN_SCHEDULE_CATEGORY { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_DAILY> TBL_LOAN_SCHEDULE_DAILY { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_DAILY_ARCHIV> TBL_LOAN_SCHEDULE_DAILY_ARCHIV { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_DAILY_TEMP> TBL_LOAN_SCHEDULE_DAILY_TEMP { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_IREGUL_INPUT> TBL_LOAN_SCHEDULE_IREGUL_INPUT { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_PERIODIC> TBL_LOAN_SCHEDULE_PERIODIC { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_PERIODIC_ARC> TBL_LOAN_SCHEDULE_PERIODIC_ARC { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_PERIODIC_TMP> TBL_LOAN_SCHEDULE_PERIODIC_TMP { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_TYPE> TBL_LOAN_SCHEDULE_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_SCHEDULE_TYPE_PRODUCT> TBL_LOAN_SCHEDULE_TYPE_PRODUCT { get; set; }
        public virtual DbSet<TBL_LOAN_STATUS> TBL_LOAN_STATUS { get; set; }
        public virtual DbSet<TBL_LOAN_SYSTEM_TYPE> TBL_LOAN_SYSTEM_TYPE { get; set; }
        public virtual DbSet<TBL_LOAN_TRANSACTION_DYNAMICS> TBL_LOAN_TRANSACTION_DYNAMICS { get; set; }
        public virtual DbSet<TBL_LOAN_TRANSACTION_TYPE> TBL_LOAN_TRANSACTION_TYPE { get; set; }
        public virtual DbSet<TBL_LOANAPPLICATION_COLTRL_MAP> TBL_LOANAPPLICATION_COLTRL_MAP { get; set; }
        public virtual DbSet<TBL_MACHINEVALUE_BASE> TBL_MACHINEVALUE_BASE { get; set; }
        public virtual DbSet<TBL_OFFERLETTER> TBL_OFFERLETTER { get; set; }
        public virtual DbSet<TBL_PRODUCT_COLLATERALTYPE> TBL_PRODUCT_COLLATERALTYPE { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT> TBL_RISK_ASSESSMENT { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_INDEX> TBL_RISK_ASSESSMENT_INDEX { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_INDEX_TYPE> TBL_RISK_ASSESSMENT_INDEX_TYPE { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_RESULT> TBL_RISK_ASSESSMENT_RESULT { get; set; }
        public virtual DbSet<TBL_RISK_ASSESSMENT_TITLE> TBL_RISK_ASSESSMENT_TITLE { get; set; }
        public virtual DbSet<TBL_RISK_RATING> TBL_RISK_RATING { get; set; }
        public virtual DbSet<TBL_TRANSACTION_DYNAMICS> TBL_TRANSACTION_DYNAMICS { get; set; }
        public virtual DbSet<TBL_CUSTOM_CRCBUREAU_PRODUCT> TBL_CUSTOM_CRCBUREAU_PRODUCT { get; set; }
        public virtual DbSet<TBL_CUSTOM_FIANCE_TRANSACTION> TBL_CUSTOM_FIANCE_TRANSACTION { get; set; }
        public virtual DbSet<TBL_CUSTOM_LIEN_PROCESS> TBL_CUSTOM_LIEN_PROCESS { get; set; }
        public virtual DbSet<TBL_CUSTOM_OVERDRAFTEXTEND> TBL_CUSTOM_OVERDRAFTEXTEND { get; set; }
        public virtual DbSet<TBL_CUSTOM_OVERDRAFTNORMAL> TBL_CUSTOM_OVERDRAFTNORMAL { get; set; }
        public virtual DbSet<TBL_CUSTOM_OVERDRAFTTOPUP> TBL_CUSTOM_OVERDRAFTTOPUP { get; set; }
        public virtual DbSet<TBL_CUSTOM_TEMPORARYOVERDRAFT> TBL_CUSTOM_TEMPORARYOVERDRAFT { get; set; }
        public virtual DbSet<ELMAH_Error> ELMAH_Error { get; set; }
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
        public virtual DbSet<TBL_FINANCIAL_STATEMENT_CAPTN> TBL_FINANCIAL_STATEMENT_CAPTN { get; set; }
        public virtual DbSet<TBL_FINANCIAL_STATEMENT_TYPE> TBL_FINANCIAL_STATEMENT_TYPE { get; set; }
        public virtual DbSet<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_TEMP_CHARGE_FEE_DETAIL> TBL_TEMP_CHARGE_FEE_DETAIL { get; set; }
        public virtual DbSet<TBL_TEMP_CHART_OF_ACCOUNT> TBL_TEMP_CHART_OF_ACCOUNT { get; set; }
        public virtual DbSet<TBL_TEMP_CHART_OF_ACCOUNT_CUR> TBL_TEMP_CHART_OF_ACCOUNT_CUR { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_CASA> TBL_TEMP_COLLATERAL_CASA { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_DEPOSIT> TBL_TEMP_COLLATERAL_DEPOSIT { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_GAURANTEE> TBL_TEMP_COLLATERAL_GAURANTEE { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_IMMOV_PROP> TBL_TEMP_COLLATERAL_IMMOV_PROP { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_ITEM_POLI> TBL_TEMP_COLLATERAL_ITEM_POLI { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_MISCELLAN> TBL_TEMP_COLLATERAL_MISCELLAN { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_MKT_SEC> TBL_TEMP_COLLATERAL_MKT_SEC { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_PLANT_EQUP> TBL_TEMP_COLLATERAL_PLANT_EQUP { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_POLICY> TBL_TEMP_COLLATERAL_POLICY { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_PREC_METAL> TBL_TEMP_COLLATERAL_PREC_METAL { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_STOCK> TBL_TEMP_COLLATERAL_STOCK { get; set; }
        public virtual DbSet<TBL_TEMP_COLLATERAL_VEHICLE> TBL_TEMP_COLLATERAL_VEHICLE { get; set; }
        public virtual DbSet<TBL_TEMP_COMPANY_BENEFICIA> TBL_TEMP_COMPANY_BENEFICIA { get; set; }
        public virtual DbSet<TBL_TEMP_CUST_CLIENT_SUPPLIER> TBL_TEMP_CUST_CLIENT_SUPPLIER { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER> TBL_TEMP_CUSTOMER { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_ADDRESS> TBL_TEMP_CUSTOMER_ADDRESS { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_COMPANYINFO> TBL_TEMP_CUSTOMER_COMPANYINFO { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_DIRECTOR> TBL_TEMP_CUSTOMER_DIRECTOR { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_EMPLOYER> TBL_TEMP_CUSTOMER_EMPLOYER { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_GROUP> TBL_TEMP_CUSTOMER_GROUP { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_GROUP_MAPPNG> TBL_TEMP_CUSTOMER_GROUP_MAPPNG { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_NEXTOFKIN> TBL_TEMP_CUSTOMER_NEXTOFKIN { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMER_PHONCONTACT> TBL_TEMP_CUSTOMER_PHONCONTACT { get; set; }
        public virtual DbSet<TBL_TEMP_CUSTOMEREMPLOYMENT> TBL_TEMP_CUSTOMEREMPLOYMENT { get; set; }
        public virtual DbSet<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }
        public virtual DbSet<TBL_TEMP_OFFERLETTER> TBL_TEMP_OFFERLETTER { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_BEHAVIOUR> TBL_TEMP_PRODUCT_BEHAVIOUR { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_CHARGE_FEE> TBL_TEMP_PRODUCT_CHARGE_FEE { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_COLLATERALTYP> TBL_TEMP_PRODUCT_COLLATERALTYP { get; set; }
        public virtual DbSet<TBL_TEMP_PRODUCT_CURRENCY> TBL_TEMP_PRODUCT_CURRENCY { get; set; }
        public virtual DbSet<TBL_TEMP_PROFILE_ADTN_ACTIVITY> TBL_TEMP_PROFILE_ADTN_ACTIVITY { get; set; }
        public virtual DbSet<TBL_TEMP_PROFILE_STAFF_ROL_GRP> TBL_TEMP_PROFILE_STAFF_ROL_GRP { get; set; }
        public virtual DbSet<TBL_TEMP_PROFILE_STAFF_ROLE_AA> TBL_TEMP_PROFILE_STAFF_ROLE_AA { get; set; }
        public virtual DbSet<TBL_TEMP_PROFILE_USER> TBL_TEMP_PROFILE_USER { get; set; }
        public virtual DbSet<TBL_TEMP_PROFILE_USERGROUP> TBL_TEMP_PROFILE_USERGROUP { get; set; }
        public virtual DbSet<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
        public virtual DbSet<TBL_DEAL_CLASSIFICATION> TBL_DEAL_CLASSIFICATION { get; set; }
        public virtual DbSet<TBL_DEAL_TYPE> TBL_DEAL_TYPE { get; set; }
        public virtual DbSet<TBL_STOCK_COMPANY> TBL_STOCK_COMPANY { get; set; }
        public virtual DbSet<TBL_STOCK_PRICE> TBL_STOCK_PRICE { get; set; }
        public virtual DbSet<TBL_PROFILE_SETTING> TBL_PROFILE_SETTING { get; set; }
        public virtual DbSet<DEV_CHECKLIST> DEV_CHECKLIST { get; set; }

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
                .HasMany(e => e.TBL_CHECKLIST_TYPE_APROV_LEVL)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMO_DOCU)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_CREDIT_TEMPLATE)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithOptional(e => e.TBL_APPROVAL_LEVEL)
                .HasForeignKey(e => e.FINALAPPROVAL_LEVELID);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_LOAN_REVIEW_APPLICATN_CAM)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .HasForeignKey(e => e.STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_FEE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_BOOKING_REQUEST)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_COLLATERAL_MAPPING)
                .WithOptional(e => e.TBL_APPROVAL_STATUS)
                .HasForeignKey(e => e.RELEASEAPPROVALSTATUSID);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_CONDITION_DEFERRAL)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_CONDITION_PRECEDENT)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT_USAGE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_FEE_ARCHIVE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_FEE)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATN)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_RATE_FEE_CONCESSION)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_LOAN_REVIEW_APPLICATION)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_OVERRIDE_DETAIL)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_STAFF_ACCOUNT_HISTORY)
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
                .HasMany(e => e.TBL_TEMP_CUSTOMER_ADDRESS)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPNG)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_PHONCONTACT)
                .WithRequired(e => e.TBL_APPROVAL_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_STATUS>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER)
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

            modelBuilder.Entity<TBL_APPROVAL_VOTE_OPTION>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithOptional(e => e.TBL_APPROVAL_VOTE_OPTION)
                .HasForeignKey(e => e.VOTE);

            modelBuilder.Entity<TBL_AUDIT_TYPE>()
                .HasMany(e => e.TBL_AUDIT)
                .WithRequired(e => e.TBL_AUDIT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .Property(e => e.NPL_LIMIT)
                .HasPrecision(19, 4);

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
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_BRANCH)
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
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATN)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_REVIEW_APPLICATION)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
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

            modelBuilder.Entity<TBL_BRANCH>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_BRANCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_BRANCH_REGION>()
                .HasMany(e => e.TBL_BRANCH)
                .WithRequired(e => e.TBL_BRANCH_REGION)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithOptional(e => e.TBL_CASA)
                .HasForeignKey(e => e.EQUITYCASAACCOUNTID);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_CASA)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE1)
                .WithOptional(e => e.TBL_CASA1)
                .HasForeignKey(e => e.CASAACCOUNTID2);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_RECOVERY_PLAN)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_CASA)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_LOAN1)
                .WithOptional(e => e.TBL_CASA1)
                .HasForeignKey(e => e.CASAACCOUNTID2);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_CASA)
                .HasForeignKey(e => e.CASAACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA>()
                .HasMany(e => e.TBL_TEMP_LOAN1)
                .WithOptional(e => e.TBL_CASA1)
                .HasForeignKey(e => e.CASAACCOUNTID2);

            modelBuilder.Entity<TBL_CASA_ACCOUNTSTATUS>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_CASA_ACCOUNTSTATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CASA_LIEN>()
                .Property(e => e.LIENAMOUNT)
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
                .HasMany(e => e.TBL_CHARGE_FEE_DETAIL)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_CHARGE_RANGE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_CUSTOMER_PRODUCT_FEE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_FEE)
                .WithRequired(e => e.TBL_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHARGE_FEE>()
                .HasMany(e => e.TBL_LOAN_FEE_ARCHIVE)
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

            modelBuilder.Entity<TBL_CHARGE_FEE_DETAIL_TYPE>()
                .HasMany(e => e.TBL_CHARGE_FEE_DETAIL)
                .WithRequired(e => e.TBL_CHARGE_FEE_DETAIL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_DEFINITION>()
                .HasMany(e => e.TBL_CHECKLIST_DETAIL)
                .WithRequired(e => e.TBL_CHECKLIST_DEFINITION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_ITEM>()
                .HasMany(e => e.TBL_CHECKLIST_DEFINITION)
                .WithRequired(e => e.TBL_CHECKLIST_ITEM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_RESPONSE_TYPE>()
                .HasMany(e => e.TBL_CHECKLIST_ITEM)
                .WithRequired(e => e.TBL_CHECKLIST_RESPONSE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_RESPONSE_TYPE>()
                .HasMany(e => e.TBL_CHECKLIST_STATUS)
                .WithRequired(e => e.TBL_CHECKLIST_RESPONSE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_RESPONSE_TYPE>()
                .HasMany(e => e.TBL_CONDITION_PRECEDENT)
                .WithRequired(e => e.TBL_CHECKLIST_RESPONSE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_RESPONSE_TYPE>()
                .HasMany(e => e.TBL_LOAN_CONDITION_PRECEDENT)
                .WithRequired(e => e.TBL_CHECKLIST_RESPONSE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_STATUS>()
                .HasMany(e => e.TBL_CHECKLIST_DETAIL)
                .WithRequired(e => e.TBL_CHECKLIST_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_TARGETTYPE>()
                .HasMany(e => e.TBL_CHECKLIST_DETAIL)
                .WithRequired(e => e.TBL_CHECKLIST_TARGETTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_TYPE>()
                .HasMany(e => e.TBL_CHECKLIST_DEFINITION)
                .WithRequired(e => e.TBL_CHECKLIST_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHECKLIST_TYPE>()
                .HasMany(e => e.TBL_CHECKLIST_TYPE_APROV_LEVL)
                .WithRequired(e => e.TBL_CHECKLIST_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_COLLATERAL_IMMOVE_PROPERTY)
                .WithRequired(e => e.TBL_CITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_CITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_LOAN_MARKET)
                .WithRequired(e => e.TBL_CITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_CITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CITY>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_IMMOV_PROP)
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
                .HasMany(e => e.TBL_BRANCH_REGION)
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
                .HasMany(e => e.TBL_COMPANY1)
                .WithOptional(e => e.TBL_COMPANY2)
                .HasForeignKey(e => e.PARENTID);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDM)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CREDIT_TEMPLATE)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER_BLACKLIST)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_CUSTOMER_PRODUCT_FEE)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
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
                .HasMany(e => e.TBL_LOAN_MARKET)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATN)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_PRINCIPAL)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_REVIEW_APPLICATN_CAM)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
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
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYP)
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
                .HasMany(e => e.TBL_SETUP_COMPANY)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_SIGNATURE_DOCUMENT_STAFF)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_STAFF)
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
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPNG)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COMPANY>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_CHARGE_FEE)
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
                .HasMany(e => e.TBL_STOCK_COMPANY)
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
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT_CUR)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_CURRENCY_EXCHANGERATE)
                .WithRequired(e => e.TBL_CURRENCY)
                .HasForeignKey(e => e.CURRENCYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_CURRENCY_EXCHANGERATE1)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_INV)
                .WithRequired(e => e.TBL_CURRENCY)
                .HasForeignKey(e => e.INVOICE_CURRENCYID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_BG)
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
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
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

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_CURRENCY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CURRENCY_RATECODE>()
                .HasMany(e => e.TBL_CURRENCY_EXCHANGERATE)
                .WithRequired(e => e.TBL_CURRENCY_RATECODE)
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
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPNG)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_LOAN)
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
                .HasMany(e => e.TBL_CUSTOMER_MODIFICATION)
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
                .HasMany(e => e.TBL_CUSTOMER_PRODUCT_FEE)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_ADDRESS)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_ADDRESS)
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

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_CUSTOMER_CREDIT_BUREAU)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_COMPANYINFO)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_PHONCONTACT)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER)
                .WithRequired(e => e.TBL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_ADDRESS_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_ADDRESS)
                .WithRequired(e => e.TBL_CUSTOMER_ADDRESS_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_ADDRESS_TYPE>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_ADDRESS)
                .WithRequired(e => e.TBL_CUSTOMER_ADDRESS_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_CLIENT_SUPPLR_TYP>()
                .HasMany(e => e.TBL_CUSTOMER_CLIENT_SUPPLIER)
                .WithRequired(e => e.TBL_CUSTOMER_CLIENT_SUPPLR_TYP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_COMPANY_DIREC_TYP>()
                .HasMany(e => e.TBL_CUSTOMER_COMPANY_DIRECTOR)
                .WithRequired(e => e.TBL_CUSTOMER_COMPANY_DIREC_TYP)
                .HasForeignKey(e => e.COMPANYDIRECTORTYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_COMPANY_DIRECTOR>()
                .HasMany(e => e.TBL_CUSTOMER_COMPANY_BENEFICIA)
                .WithRequired(e => e.TBL_CUSTOMER_COMPANY_DIRECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_COMPANYINFOMATION>()
                .Property(e => e.SHAREHOLDER_FUND)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_EMPLOYER_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_EMPLOYER_TYPE_SUB)
                .WithRequired(e => e.TBL_CUSTOMER_EMPLOYER_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_EMPLOYER_TYPE_SUB>()
                .HasMany(e => e.TBL_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_CUSTOMER_EMPLOYER_TYPE_SUB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_EMPLOYER_TYPE_SUB>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_EMPLOYER)
                .WithRequired(e => e.TBL_CUSTOMER_EMPLOYER_TYPE_SUB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION)
                .HasForeignKey(e => e.FSCAPTIONID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL1)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION1)
                .HasForeignKey(e => e.RATIOCAPTIONID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION>()
                .HasMany(e => e.TBL_CUSTOMER_GRP_FS_CAPTN_DET)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION_DETAIL>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_FS_CAPTION_GROUP>()
                .HasMany(e => e.TBL_CUSTOMER_FS_CAPTION)
                .WithRequired(e => e.TBL_CUSTOMER_FS_CAPTION_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_DETAIL>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_DIVI_TYP>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_RATIO_DIVI_TYP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_FS_RATIO_VALUETYP>()
                .HasMany(e => e.TBL_CUSTOMER_FS_RATIO_DETAIL)
                .WithRequired(e => e.TBL_CUSTOMER_FS_RATIO_VALUETYP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP>()
                .HasMany(e => e.TBL_CUSTOMER_GRP_FS_CAPTN_DET)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP_RELATN_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_GROUP_MAPPING)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP_RELATN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GROUP_RELATN_TYPE>()
                .HasMany(e => e.TBL_TEMP_CUSTOMER_GROUP_MAPPNG)
                .WithRequired(e => e.TBL_CUSTOMER_GROUP_RELATN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_GRP_FS_CAPTN_DET>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_IDENTI_MODE_TYPE>()
                .Property(e => e.IDENTIFICATIONMODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_MODIFICATN_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_MODIFICATION)
                .WithRequired(e => e.TBL_CUSTOMER_MODIFICATN_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_PHONECONTACT>()
                .Property(e => e.PHONE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_PRODUCT_FEE>()
                .Property(e => e.RATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_PRODUCT_FEE>()
                .Property(e => e.DEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_CUSTOMER)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_SENSITIVITY_LEVEL>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_CUSTOMER_SENSITIVITY_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_TYPE>()
                .HasMany(e => e.TBL_CUSTOMER_COMPANY_DIRECTOR)
                .WithRequired(e => e.TBL_CUSTOMER_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_TYPE>()
                .HasMany(e => e.TBL_PRODUCT_CLASS)
                .WithRequired(e => e.TBL_CUSTOMER_TYPE)
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
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_DAY_COUNT_CONVENTION)
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

            modelBuilder.Entity<TBL_DAY_COUNT_CONVENTION>()
                .HasMany(e => e.TBL_TEMP_LOAN)
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

            modelBuilder.Entity<TBL_DAY_INTEREST_TYPE>()
                .HasMany(e => e.TBL_TEMP_LOAN)
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

            modelBuilder.Entity<TBL_DEPARTMENT>()
                .HasMany(e => e.TBL_JOB_TYPE_DEPARTMENT)
                .WithRequired(e => e.TBL_DEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DEPARTMENT_UNIT>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_DEPARTMENT_UNIT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_INTERVAL>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_INTERVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_INTERVAL>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_INTERVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TARGET>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_TARGET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TARGET>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_TARGET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TYPE>()
                .HasMany(e => e.TBL_CHARGE_FEE)
                .WithRequired(e => e.TBL_FEE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TYPE>()
                .HasMany(e => e.TBL_CHARGE_FEE_DETAIL)
                .WithRequired(e => e.TBL_FEE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FEE_TYPE>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE_DETAIL)
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
                .HasForeignKey(e => e.SCH_PREPAYMENT_FREQUENCY_TYPID);

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
                .HasForeignKey(e => e.SCH_PREPAYMENT_FREQUENCY_TYPID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_POLICY)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.RENEWALFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE)
                .HasForeignKey(e => e.INTERESTFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_TEMP_LOAN1)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE1)
                .HasForeignKey(e => e.PRINCIPALFREQUENCYTYPEID);

            modelBuilder.Entity<TBL_FREQUENCY_TYPE>()
                .HasMany(e => e.TBL_TEMP_LOAN2)
                .WithOptional(e => e.TBL_FREQUENCY_TYPE2)
                .HasForeignKey(e => e.SCH_PREPAYMENT_FREQUENCY_TYPID);

            modelBuilder.Entity<TBL_JOB_REQUEST>()
                .HasMany(e => e.TBL_JOB_REQUEST_DETAIL)
                .WithRequired(e => e.TBL_JOB_REQUEST)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_REQUEST>()
                .HasMany(e => e.TBL_JOB_REQUEST_DOCUMENT_MAPPN)
                .WithRequired(e => e.TBL_JOB_REQUEST)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_REQUEST>()
                .HasMany(e => e.TBL_JOB_REQUEST_MESSAGE)
                .WithRequired(e => e.TBL_JOB_REQUEST)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_REQUEST_DETAIL>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_JOB_REQUEST_STATUS>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_JOB_REQUEST_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_REQUEST_STATUS>()
                .HasMany(e => e.TBL_JOB_REQUEST_STATUS_FEEDBAK)
                .WithRequired(e => e.TBL_JOB_REQUEST_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_TYPE>()
                .HasMany(e => e.TBL_JOB_REQUEST)
                .WithRequired(e => e.TBL_JOB_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_TYPE>()
                .HasMany(e => e.TBL_JOB_REQUEST_STATUS_FEEDBAK)
                .WithRequired(e => e.TBL_JOB_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_TYPE>()
                .HasMany(e => e.TBL_JOB_TYPE_DEPARTMENT)
                .WithRequired(e => e.TBL_JOB_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_TYPE>()
                .HasMany(e => e.TBL_JOB_TYPE_SUB)
                .WithRequired(e => e.TBL_JOB_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_JOB_TYPE_SUB>()
                .HasMany(e => e.TBL_JOB_REQUEST_DETAIL)
                .WithRequired(e => e.TBL_JOB_TYPE_SUB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LANGUAGE>()
                .HasMany(e => e.TBL_COMPANY)
                .WithRequired(e => e.TBL_LANGUAGE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOCALGOVERNMENT>()
                .HasMany(e => e.TBL_CITY)
                .WithRequired(e => e.TBL_LOCALGOVERNMENT)
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

            modelBuilder.Entity<TBL_MESSAGE_LOG_TYPE>()
                .HasMany(e => e.TBL_MONITORING_ALERT_SETUP)
                .WithRequired(e => e.TBL_MESSAGE_LOG_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_MIS_INFO>()
                .HasMany(e => e.TBL_MIS_INFO1)
                .WithOptional(e => e.TBL_MIS_INFO2)
                .HasForeignKey(e => e.PARENTMISINFOID);

            modelBuilder.Entity<TBL_MONITORING_ALERT_SETUP>()
                .Property(e => e.MESSAGE_TEMPLATE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_APPROVAL_GROUP_MAPPING)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_CHECKLIST_DEFINITION)
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
                .HasMany(e => e.TBL_CUSTOM_FIANCE_TRANSACTION)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_FEE_ARCHIVE)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS>()
                .HasMany(e => e.TBL_LOAN_REVIEW_APPLICATION)
                .WithRequired(e => e.TBL_OPERATIONS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OPERATIONS_TYPE>()
                .HasMany(e => e.TBL_OPERATIONS)
                .WithRequired(e => e.TBL_OPERATIONS_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_OVERRIDE_ITEM>()
                .HasMany(e => e.TBL_OVERRIDE_DETAIL)
                .WithRequired(e => e.TBL_OVERRIDE_ITEM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_POSTING_TYPE>()
                .HasMany(e => e.TBL_CHARGE_FEE_DETAIL)
                .WithRequired(e => e.TBL_POSTING_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_POSTING_TYPE>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE_DETAIL)
                .WithRequired(e => e.TBL_POSTING_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_CASA)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_CUSTOMER_PRODUCT_FEE)
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
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_TEMP_LOAN)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
                .WithRequired(e => e.TBL_PRODUCT)
                .HasForeignKey(e => e.PROPOSEDPRODUCTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH1)
                .WithRequired(e => e.TBL_PRODUCT1)
                .HasForeignKey(e => e.APPROVEDPRODUCTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_LOG)
                .WithRequired(e => e.TBL_PRODUCT)
                .HasForeignKey(e => e.APPROVEDPRODUCTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_PRODUCT_BEHAVIOUR)
                .WithRequired(e => e.TBL_PRODUCT)
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

            modelBuilder.Entity<TBL_PRODUCT>()
                .HasMany(e => e.TBL_TRANSACTION_DYNAMICS)
                .WithRequired(e => e.TBL_PRODUCT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_BEHAVIOUR>()
                .Property(e => e.CUSTOMER_LIMIT)
                .HasPrecision(19, 4);

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
                .HasMany(e => e.TBL_TEMP_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CLASS_PROCESS>()
                .Property(e => e.MAXIMUM_AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_PRODUCT_CLASS_PROCESS>()
                .HasMany(e => e.TBL_PRODUCT_CLASS)
                .WithRequired(e => e.TBL_PRODUCT_CLASS_PROCESS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_CLASS_PROCESS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_PRODUCT_CLASS_PROCESS)
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

            modelBuilder.Entity<TBL_PRODUCT_PRICE_INDEX>()
                .HasMany(e => e.TBL_PRODUCT_PRICE_INDEX_DAILY)
                .WithRequired(e => e.TBL_PRODUCT_PRICE_INDEX)
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
                .HasMany(e => e.TBL_LOAN_FORCE_DEBIT)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_PAST_DUE)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_RECOVERY_PLAN)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_LOAN_SCHEDULE_TYPE_PRODUCT)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PRODUCT_TYPE>()
                .HasMany(e => e.TBL_STAFF_ACCOUNT_HISTORY)
                .WithRequired(e => e.TBL_PRODUCT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_ADDITIONALACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_TEMP_PROFILE_ADTN_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_GROUP_ACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_PRIVILEDGE_ACTIVIT)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_PROFILE_STAFF_ROLE_ADT_ACT)
                .WithRequired(e => e.TBL_PROFILE_ACTIVITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_ACTIVITY>()
                .HasMany(e => e.TBL_TEMP_PROFILE_STAFF_ROLE_AA)
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
                .HasMany(e => e.TBL_PROFILE_STAFF_ROLE_GROUP)
                .WithRequired(e => e.TBL_PROFILE_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_GROUP>()
                .HasMany(e => e.TBL_PROFILE_USERGROUP)
                .WithRequired(e => e.TBL_PROFILE_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_GROUP>()
                .HasMany(e => e.TBL_TEMP_PROFILE_STAFF_ROL_GRP)
                .WithRequired(e => e.TBL_PROFILE_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_GROUP>()
                .HasMany(e => e.TBL_TEMP_PROFILE_USERGROUP)
                .WithRequired(e => e.TBL_PROFILE_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_PRIVILEDGE>()
                .HasMany(e => e.TBL_PROFILE_PRIVILEDGE_ACTIVIT)
                .WithRequired(e => e.TBL_PROFILE_PRIVILEDGE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_USER>()
                .HasMany(e => e.TBL_PROFILE_ADDITIONALACTIVITY)
                .WithRequired(e => e.TBL_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_USER>()
                .HasMany(e => e.TBL_PROFILE_PRIVILEDGE_ACTIVIT)
                .WithRequired(e => e.TBL_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PROFILE_USER>()
                .HasMany(e => e.TBL_PROFILE_USERGROUP)
                .WithRequired(e => e.TBL_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SECTOR>()
                .Property(e => e.LOAN_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_SECTOR>()
                .HasMany(e => e.TBL_STOCK_COMPANY)
                .WithRequired(e => e.TBL_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SIGNATURE_DOCUMENT_TYPE>()
                .HasMany(e => e.TBL_SIGNATURE_DOCUMENT_STAFF)
                .WithRequired(e => e.TBL_SIGNATURE_DOCUMENT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.GENDER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.GENDEROFNOK)
                .IsFixedLength();

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.NPL_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.LOAN_LIMIT)
                .HasPrecision(19, 4);

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
                .HasMany(e => e.TBL_APPROVAL_TRAIL2)
                .WithOptional(e => e.TBL_STAFF2)
                .HasForeignKey(e => e.TOSTAFFID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_APPROVAL_TRAIL3)
                .WithOptional(e => e.TBL_STAFF3)
                .HasForeignKey(e => e.RELIEVEDSTAFFID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_AUDIT)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_BRANCH_REGION)
                .WithOptional(e => e.TBL_STAFF)
                .HasForeignKey(e => e.CAM_HOU_STAFFID);

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
                .WithOptional(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RECEIVERSTAFFID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_JOB_REQUEST2)
                .WithOptional(e => e.TBL_STAFF2)
                .HasForeignKey(e => e.REASSIGNEDTO);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_JOB_REQUEST_MESSAGE)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_NOTIFICATION_LOG)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_OVERRIDE_DETAIL)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.CREATEDBY)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
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
                .HasMany(e => e.TBL_LOAN_BOOKING_REQUEST)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.CREATEDBY)
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
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATN)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATN1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_RELATIONSHIP_OFF_HIST)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
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

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF_ACCOUNT_HISTORY)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.STAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF_ACCOUNT_HISTORY1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.NEWSTAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF_RELIEF)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.STAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF_RELIEF1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELIEFSTAFFID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF1)
                .WithOptional(e => e.TBL_STAFF2)
                .HasForeignKey(e => e.SUPERVISOR_STAFFID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_STAFF)
                .HasForeignKey(e => e.RELATIONSHIPOFFICERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_TEMP_LOAN1)
                .WithRequired(e => e.TBL_STAFF1)
                .HasForeignKey(e => e.RELATIONSHIPMANAGERID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithOptional(e => e.TBL_STAFF)
                .HasForeignKey(e => e.SUPERVISOR_STAFFID);

            modelBuilder.Entity<TBL_STAFF_JOBTITLE>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_STAFF_JOBTITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_JOBTITLE>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithRequired(e => e.TBL_STAFF_JOBTITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_ROLE>()
                .HasMany(e => e.TBL_PROFILE_STAFF_ROLE_ADT_ACT)
                .WithRequired(e => e.TBL_STAFF_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_ROLE>()
                .HasMany(e => e.TBL_PROFILE_STAFF_ROLE_GROUP)
                .WithRequired(e => e.TBL_STAFF_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_ROLE>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_STAFF_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_ROLE>()
                .HasMany(e => e.TBL_TEMP_PROFILE_STAFF_ROL_GRP)
                .WithRequired(e => e.TBL_STAFF_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_ROLE>()
                .HasMany(e => e.TBL_TEMP_PROFILE_STAFF_ROLE_AA)
                .WithRequired(e => e.TBL_STAFF_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_ROLE>()
                .HasMany(e => e.TBL_TEMP_STAFF)
                .WithRequired(e => e.TBL_STAFF_ROLE)
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
                .HasMany(e => e.TBL_LOCALGOVERNMENT)
                .WithRequired(e => e.TBL_STATE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
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
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
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

            modelBuilder.Entity<TBL_SUB_SECTOR>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_SUB_SECTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TAX>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

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
                .HasMany(e => e.TBL_COLLATERAL_PLANT_AND_EQUIP)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_COLLATERAL_MKT_SECURITY)
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
                .HasMany(e => e.TBL_COLLATERAL_IMMOVE_PROPERTY)
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
                .HasMany(e => e.TBL_LOAN_APPLICATION_COLLATERL)
                .WithRequired(e => e.TBL_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_LOAN_COLLATERAL_MAPPING)
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

            modelBuilder.Entity<TBL_COLLATERAL_GAURANTEE>()
                .Property(e => e.GUARANTEEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.PROPERTYADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.OPENMARKETVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.FORCEDSALEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.STAMPTOCOVER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.COLLATERALUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.VALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_IMMOVE_PROPERTY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_ITEM_POLICY>()
                .Property(e => e.SUMINSURED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MISCELLANEOUS>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MISCELLANEOUS>()
                .Property(e => e.NOTE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MISCELLANEOUS>()
                .HasMany(e => e.TBL_COLLATERAL_MISC_NOTES)
                .WithOptional(e => e.TBL_COLLATERAL_MISCELLANEOUS)
                .HasForeignKey(e => e.MISCELLANEOUSID);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.DEALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.LIENUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.ISSUERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.ISSUERREFERENCENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.UNITVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COLLATERAL_MKT_SECURITY>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PERFECTN_STAT>()
                .HasMany(e => e.TBL_COLLATERAL_IMMOVE_PROPERTY)
                .WithRequired(e => e.TBL_COLLATERAL_PERFECTN_STAT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_PERFECTN_STAT>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_IMMOV_PROP)
                .WithRequired(e => e.TBL_COLLATERAL_PERFECTN_STAT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIP>()
                .Property(e => e.MACHINENAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIP>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIP>()
                .Property(e => e.YEAROFMANUFACTURE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIP>()
                .Property(e => e.YEAROFPURCHASE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_COLLATERAL_PLANT_AND_EQUIP>()
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

            modelBuilder.Entity<TBL_COLLATERAL_SENIORITY_CLAIM>()
                .Property(e => e.SENIORITYOFCLAIMS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_SENIORITY_CLAIM>()
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
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYP)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_TYPE>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WithRequired(e => e.TBL_COLLATERAL_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUEBASE_TYPE>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUEBASE_TYPE>()
                .HasMany(e => e.TBL_COLLATERAL_PLANT_AND_EQUIP)
                .WithRequired(e => e.TBL_COLLATERAL_VALUEBASE_TYPE)
                .HasForeignKey(e => e.VALUEBASETYPEID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_COLLATERAL_VALUEBASE_TYPE>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_PLANT_EQUP)
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

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMO_DETL>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMORANDM>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMO_DETL)
                .WithRequired(e => e.TBL_CREDIT_APPRAISAL_MEMORANDM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMORANDM>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMO_DOCU)
                .WithRequired(e => e.TBL_CREDIT_APPRAISAL_MEMORANDM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CREDIT_APPRAISAL_MEMORANDM>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMO_LOG)
                .WithRequired(e => e.TBL_CREDIT_APPRAISAL_MEMORANDM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CREDIT_BUREAU>()
                .Property(e => e.CORPORATE_CHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CREDIT_BUREAU>()
                .Property(e => e.INDIVIDUAL_CHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CREDIT_BUREAU>()
                .HasMany(e => e.TBL_CUSTOMER_CREDIT_BUREAU)
                .WithRequired(e => e.TBL_CREDIT_BUREAU)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CUSTOMER_CREDIT_BUREAU>()
                .Property(e => e.CHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CUSTOMER_CREDIT_BUREAU>()
                .HasMany(e => e.TBL_LOAN_APPLTN_CREDIT_BUREAU)
                .WithRequired(e => e.TBL_CUSTOMER_CREDIT_BUREAU)
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
                .Property(e => e.PASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.PASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.INTERESTONPASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.INTERESTONPASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN>()
                .Property(e => e.PENALCHARGEAMOUNT)
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
                .Property(e => e.TOTALEXPOSUREAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_CREDIT_APPRAISAL_MEMORANDM)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_COLLATERL)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_COLLATRL2)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_APPLTN_CREDIT_BUREAU)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION>()
                .HasMany(e => e.TBL_RISK_ASSESSMENT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_ARCHIVE>()
                .Property(e => e.APPLICATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_ARCHIVE>()
                .Property(e => e.APPROVEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_COLLATERL>()
                .Property(e => e.LEGAL_FEE_AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_COLLATRL2>()
                .Property(e => e.COLLATERALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_COLLATRL2>()
                .Property(e => e.STAMPEDTOCOVERAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_COVENANT>()
                .Property(e => e.COVENANTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .Property(e => e.PROPOSEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .Property(e => e.APPROVEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .Property(e => e.EQUITYAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_COVENANT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_TRANSACTION_DYNAMICS)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_EDU)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_INV)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_TRA)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_ARCH)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_BG)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_FEE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_LOG)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_APPLICATN_DETL_MTRIG)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_BOOKING_REQUEST)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_CONDITION_PRECEDENT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_RATE_FEE_CONCESSION)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETAIL>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_DETAIL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_ARCH>()
                .Property(e => e.PROPOSEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_ARCH>()
                .Property(e => e.APPROVEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_BG>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_EDU>()
                .Property(e => e.AVERAGE_SCHOOL_FEES)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_EDU>()
                .Property(e => e.TOTAL_PREVIOUS_TERM_SCHOL_FEES)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_FEE>()
                .Property(e => e.DEFAULT_FEERATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_FEE>()
                .Property(e => e.RECOMMENDED_FEERATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_INV>()
                .Property(e => e.INVOICE_AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_LOG>()
                .Property(e => e.APPROVEDAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_STA>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_BG)
                .WithOptional(e => e.TBL_LOAN_APPLICATION_DETL_STA)
                .HasForeignKey(e => e.APPROVALSTATUSID);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_STA>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_INV)
                .WithOptional(e => e.TBL_LOAN_APPLICATION_DETL_STA)
                .HasForeignKey(e => e.APPROVALSTATUSID);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_DETL_TRA>()
                .Property(e => e.AVERAGE_MONTHLY_TURNOVER)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_STATUS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_STATUS)
                .HasForeignKey(e => e.APPLICATIONSTATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_STATUS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION1)
                .WithOptional(e => e.TBL_LOAN_APPLICATION_STATUS1)
                .HasForeignKey(e => e.NEXTAPPLICATIONSTATUSID);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_STATUS>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_TYPE>()
                .HasMany(e => e.TBL_LOAN_APPLICATION)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_TYPE>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_APPLICATION_TYPE>()
                .HasMany(e => e.TBL_LOAN_PRELIMINARY_EVALUATN)
                .WithRequired(e => e.TBL_LOAN_APPLICATION_TYPE)
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
                .Property(e => e.PASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.PASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.INTERESTONPASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.INTERESTONPASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.PENALCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_ARCHIVE>()
                .Property(e => e.SCHEDULEDPREPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_BOOKING_REQUEST>()
                .Property(e => e.AMOUNT_REQUESTED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_CAMSOL>()
                .Property(e => e.AMOUNTAFFECTED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_CONCESSION_TYPE>()
                .HasMany(e => e.TBL_LOAN_RATE_FEE_CONCESSION)
                .WithRequired(e => e.TBL_LOAN_CONCESSION_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_CONDITION_PRECEDENT>()
                .HasMany(e => e.TBL_LOAN_CONDITION_DEFERRAL)
                .WithRequired(e => e.TBL_LOAN_CONDITION_PRECEDENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_CONTINGENT>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN_CONTINGENT>()
                .Property(e => e.CONTINGENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_CONTINGENT>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT_USAGE)
                .WithRequired(e => e.TBL_LOAN_CONTINGENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_CONTINGENT_USAGE>()
                .Property(e => e.AMOUNTREQUESTED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_COVENANT_DETAIL>()
                .Property(e => e.COVENANTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_COVENANT_TYPE>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_COVENANT)
                .WithRequired(e => e.TBL_LOAN_COVENANT_TYPE)
                .WillCascadeOnDelete(false);

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
                .Property(e => e.EARNEDFEEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .Property(e => e.TAXAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .Property(e => e.EARNEDTAXAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE>()
                .HasMany(e => e.TBL_LOAN_FEE_SCHEDULE)
                .WithRequired(e => e.TBL_LOAN_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_FEE_ARCHIVE>()
                .Property(e => e.FEERATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE_ARCHIVE>()
                .Property(e => e.FEEDEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE_ARCHIVE>()
                .Property(e => e.FEEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE_ARCHIVE>()
                .Property(e => e.EARNEDFEEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE_ARCHIVE>()
                .Property(e => e.TAXAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE_ARCHIVE>()
                .Property(e => e.EARNEDTAXAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FEE_SCHEDULE>()
                .Property(e => e.FEEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FORCE_DEBIT>()
                .Property(e => e.DEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_FORCE_DEBIT>()
                .Property(e => e.CREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_MARKET>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_TRA)
                .WithRequired(e => e.TBL_LOAN_MARKET)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_MATURITY_INSTRU_TYPE>()
                .HasMany(e => e.TBL_LOAN_MATURITY_INSTRUCTION)
                .WithRequired(e => e.TBL_LOAN_MATURITY_INSTRU_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PAST_DUE>()
                .Property(e => e.DEBITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PAST_DUE>()
                .Property(e => e.CREDITAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PRELIMINARY_EVALUATN>()
                .Property(e => e.LOANAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_PRINCIPAL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_BG)
                .WithRequired(e => e.TBL_LOAN_PRINCIPAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRINCIPAL>()
                .HasMany(e => e.TBL_LOAN_APPLICATION_DETL_INV)
                .WithRequired(e => e.TBL_LOAN_PRINCIPAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENT_GUIDE_TYPE>()
                .HasMany(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .WithRequired(e => e.TBL_LOAN_PRUDENT_GUIDE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.INT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN1)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.EXT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN2)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE2)
                .HasForeignKey(e => e.USER_PRUDENTIAL_GUIDE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.INT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE1)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.EXT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE2)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE2)
                .HasForeignKey(e => e.USER_PRUDENTIAL_GUIDE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.EXT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE1)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.INT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE2)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE2)
                .HasForeignKey(e => e.USER_PRUDENTIAL_GUIDE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.EXT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING1)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.INT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING2)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE2)
                .HasForeignKey(e => e.USER_PRUDENTIAL_GUIDE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE)
                .HasForeignKey(e => e.INT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_PRUDENTIALGUIDELINE>()
                .HasMany(e => e.TBL_TEMP_LOAN1)
                .WithRequired(e => e.TBL_LOAN_PRUDENTIALGUIDELINE1)
                .HasForeignKey(e => e.EXT_PRUDENT_GUIDELINE_STATUSID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_RECOVERY_PLAN>()
                .Property(e => e.AMOUNTOWED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_RECOVERY_PLAN>()
                .Property(e => e.WRITEOFFAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_RECOVERY_PLAN>()
                .HasMany(e => e.TBL_LOAN_RECOVERY_PLAN_PAYMNT)
                .WithRequired(e => e.TBL_LOAN_RECOVERY_PLAN)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_RECOVERY_PLAN_PAYMNT>()
                .Property(e => e.PAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVIEW_APPLICATION>()
                .HasMany(e => e.TBL_LOAN_REVIEW_APPLICATN_CAM)
                .WithRequired(e => e.TBL_LOAN_REVIEW_APPLICATION)
                .WillCascadeOnDelete(false);

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
                .HasMany(e => e.TBL_LOAN_REVIEW_OPRATN_IREG_SC)
                .WithRequired(e => e.TBL_LOAN_REVIEW_OPERATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_REVIEW_OPRATN_IREG_SC>()
                .Property(e => e.PAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.OVERDRAFTLIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.PASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.PASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.INTERESTONPASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.INTERESTONPASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .Property(e => e.PENALCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_REVOLVING)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.OVERDRAFTLIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.PASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.PASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.INTERESTONPASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.INTERESTONPASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_ARCHIVE>()
                .Property(e => e.PENALCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_REVOLVING_TYPE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_REVOLVING_TYPE)
                .WillCascadeOnDelete(false);

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

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.OPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.DAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.DAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.DAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.CLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.ACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDOPENINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDDAILYPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDDAILYINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDDAILYPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDCLOSINGBALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISEDACCRUEDINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.AMORTISED_AMORTISEDCOST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.DISCOUNTPREMIUM)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.UNEARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
                .Property(e => e.EARNEDFEE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>()
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

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_IREGUL_INPUT>()
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

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.PERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.PERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.PERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.AMORTISEDPERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.AMORTISEDPERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.AMORTISEDPERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_ARC>()
                .Property(e => e.AMORTISEDENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.STARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.PERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.PERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.PERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.ENDPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.AMORTISEDSTARTPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.AMORTISEDPERIODPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.AMORTISEDPERIODINTERESTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
                .Property(e => e.AMORTISEDPERIODPRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_PERIODIC_TMP>()
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
                .HasMany(e => e.TBL_LOAN_SCHEDULE_TYPE_PRODUCT)
                .WithRequired(e => e.TBL_LOAN_SCHEDULE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SCHEDULE_TYPE>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_LOAN_SCHEDULE_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_STATUS>()
                .HasMany(e => e.TBL_LOAN)
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

            modelBuilder.Entity<TBL_LOAN_STATUS>()
                .HasMany(e => e.TBL_TEMP_LOAN)
                .WithRequired(e => e.TBL_LOAN_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_COLLATERAL_MAPPING)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_CONTINGENT)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_COVENANT_DETAIL)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_FEE)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_FEE_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_MATURITY_INSTRUCTION)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_MONITORING_TRIGGER)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_LOAN_SYSTEM_TYPE>()
                .HasMany(e => e.TBL_LOAN_REVOLVING_ARCHIVE)
                .WithRequired(e => e.TBL_LOAN_SYSTEM_TYPE)
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

            modelBuilder.Entity<TBL_LOANAPPLICATION_COLTRL_MAP>()
                .HasOptional(e => e.TBL_LOANAPPLICATION_COLTRL_MAP1)
                .WithRequired(e => e.TBL_LOANAPPLICATION_COLTRL_MAP2);

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

            modelBuilder.Entity<TBL_CUSTOM_LIEN_PROCESS>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COT>()
                .Property(e => e.COTACCOUNTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_COT>()
                .Property(e => e.COTCREATEDBY)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_ACCOUNT_TYPE)
                .WithRequired(e => e.TBL_ACCOUNT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_ACCOUNT_CATEGORY>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
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
                .HasMany(e => e.TBL_CHARGE_FEE_DETAIL)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.GLACCOUNTID1);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_CHARGE_FEE_DETAIL1)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT1)
                .HasForeignKey(e => e.GLACCOUNTID2);

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
                .HasMany(e => e.TBL_PRODUCT6)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT6)
                .HasForeignKey(e => e.PRINCIPALBALANCEGL2);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_SETUP_COMPANY)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.LEGAL_CHARGE_GLACCOUNTID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TAX)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_COLLATERAL_TYPE)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.CHARGEGLACCOUNTID);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_CREDIT_BUREAU)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

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
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE_DETAIL)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT)
                .HasForeignKey(e => e.GLACCOUNTID1);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE_DETAIL1)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT1)
                .HasForeignKey(e => e.GLACCOUNTID2);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT6)
                .WithOptional(e => e.TBL_CHART_OF_ACCOUNT6)
                .HasForeignKey(e => e.PRINCIPALBALANCEGL2);

            modelBuilder.Entity<TBL_CHART_OF_ACCOUNT_CLASS>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_CHART_OF_ACCOUNT_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FINANCIAL_STATEMENT_CAPTN>()
                .HasMany(e => e.TBL_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_FINANCIAL_STATEMENT_CAPTN)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_FINANCIAL_STATEMENT_CAPTN>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT)
                .WithRequired(e => e.TBL_FINANCIAL_STATEMENT_CAPTN)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_CHARGE_FEE>()
                .Property(e => e.AMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_CHARGE_FEE>()
                .HasMany(e => e.TBL_TEMP_CHARGE_FEE_DETAIL)
                .WithRequired(e => e.TBL_TEMP_CHARGE_FEE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_CHART_OF_ACCOUNT>()
                .HasMany(e => e.TBL_TEMP_CHART_OF_ACCOUNT_CUR)
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
                .Property(e => e.COLLATERALVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .Property(e => e.CAMREFNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_MKT_SEC)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_GAURANTEE)
                .WithRequired(e => e.TBL_TEMP_COLLATERAL_CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_CUSTOMER>()
                .HasMany(e => e.TBL_TEMP_COLLATERAL_MISCELLAN)
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

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_GAURANTEE>()
                .Property(e => e.GUARANTEEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.PROPERTYADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.OPENMARKETVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.FORCEDSALEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.STAMPTOCOVER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.COLLATERALUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.VALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_IMMOV_PROP>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_ITEM_POLI>()
                .Property(e => e.SUMINSURED)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MISCELLAN>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MISCELLAN>()
                .Property(e => e.NOTE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MKT_SEC>()
                .Property(e => e.DEALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MKT_SEC>()
                .Property(e => e.SECURITYVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MKT_SEC>()
                .Property(e => e.LIENUSABLEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MKT_SEC>()
                .Property(e => e.UNITVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_MKT_SEC>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_EQUP>()
                .Property(e => e.MACHINENAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_EQUP>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_EQUP>()
                .Property(e => e.YEAROFMANUFACTURE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_EQUP>()
                .Property(e => e.YEAROFPURCHASE)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PLANT_EQUP>()
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

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PREC_METAL>()
                .Property(e => e.PRECIOUSMETALNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PREC_METAL>()
                .Property(e => e.METALTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PREC_METAL>()
                .Property(e => e.VALUATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PREC_METAL>()
                .Property(e => e.PRECIOUSMETALFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_COLLATERAL_PREC_METAL>()
                .Property(e => e.REMARK)
                .IsUnicode(false);

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

            modelBuilder.Entity<TBL_TEMP_CUSTOMER_COMPANYINFO>()
                .Property(e => e.SHAREHOLDER_FUND)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_CUSTOMER_PHONCONTACT>()
                .Property(e => e.PHONE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.TEAMMISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.EQUITYCONTRIBUTION)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.OUTSTANDINGPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.OUTSTANDINGINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.PASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.PASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.INTERESTONPASTDUEPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.INTERESTONPASTDUEINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.PENALCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_LOAN>()
                .Property(e => e.SCHEDULEDPREPAYMENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PRODUCT>()
                .HasMany(e => e.TBL_TEMP_PRODUCT_COLLATERALTYP)
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

            modelBuilder.Entity<TBL_TEMP_PRODUCT_BEHAVIOUR>()
                .Property(e => e.CUSTOMER_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PRODUCT_CHARGE_FEE>()
                .Property(e => e.RATEVALUE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PRODUCT_CHARGE_FEE>()
                .Property(e => e.DEPENDENTAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_TEMP_PROFILE_USER>()
                .HasMany(e => e.TBL_TEMP_PROFILE_ADTN_ACTIVITY)
                .WithRequired(e => e.TBL_TEMP_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_PROFILE_USER>()
                .HasMany(e => e.TBL_TEMP_PROFILE_USERGROUP)
                .WithRequired(e => e.TBL_TEMP_PROFILE_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_TEMP_STAFF>()
                .Property(e => e.GENDER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_STAFF>()
                .Property(e => e.GENDEROFNOK)
                .IsFixedLength();

            modelBuilder.Entity<TBL_TEMP_STAFF>()
                .HasMany(e => e.TBL_TEMP_PROFILE_USER)
                .WithRequired(e => e.TBL_TEMP_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DEAL_CLASSIFICATION>()
                .HasMany(e => e.TBL_PRODUCT_TYPE)
                .WithRequired(e => e.TBL_DEAL_CLASSIFICATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STOCK_COMPANY>()
                .HasMany(e => e.TBL_STOCK_PRICE)
                .WithRequired(e => e.TBL_STOCK_COMPANY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STOCK_PRICE>()
                .Property(e => e.STOCKPRICE)
                .HasPrecision(19, 4);
        }
    }
}
