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

        public virtual DbSet<tbl_Accounting_Standard> tbl_Accounting_Standard { get; set; }
        public virtual DbSet<tbl_AccreditedConsultant> tbl_AccreditedConsultant { get; set; }
        public virtual DbSet<tbl_AccreditedConsultant_State> tbl_AccreditedConsultant_State { get; set; }
        public virtual DbSet<tbl_AccreditedConsultant_Type> tbl_AccreditedConsultant_Type { get; set; }
        public virtual DbSet<tbl_Application_Setup> tbl_Application_Setup { get; set; }
        public virtual DbSet<tbl_Approval_Group> tbl_Approval_Group { get; set; }
        public virtual DbSet<tbl_Approval_Group_Mapping> tbl_Approval_Group_Mapping { get; set; }
        public virtual DbSet<tbl_Approval_Level> tbl_Approval_Level { get; set; }
        public virtual DbSet<tbl_Approval_Level_Staff> tbl_Approval_Level_Staff { get; set; }
        public virtual DbSet<tbl_Approval_State> tbl_Approval_State { get; set; }
        public virtual DbSet<tbl_Approval_Status> tbl_Approval_Status { get; set; }
        public virtual DbSet<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }
        public virtual DbSet<tbl_Audit> tbl_Audit { get; set; }
        public virtual DbSet<tbl_Audit_Type> tbl_Audit_Type { get; set; }
        public virtual DbSet<tbl_Branch> tbl_Branch { get; set; }
        public virtual DbSet<tbl_CASA> tbl_CASA { get; set; }
        public virtual DbSet<tbl_CASA_AccountStatus> tbl_CASA_AccountStatus { get; set; }
        public virtual DbSet<tbl_CASA_Lien_Type> tbl_CASA_Lien_Type { get; set; }
        public virtual DbSet<tbl_CASA_PostNoStatus> tbl_CASA_PostNoStatus { get; set; }
        public virtual DbSet<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }
        public virtual DbSet<tbl_Checklist_Definition> tbl_Checklist_Definition { get; set; }
        public virtual DbSet<tbl_Checklist_Detail> tbl_Checklist_Detail { get; set; }
        public virtual DbSet<tbl_CheckList_Item> tbl_CheckList_Item { get; set; }
        public virtual DbSet<tbl_Checklist_Status> tbl_Checklist_Status { get; set; }
        public virtual DbSet<tbl_Checklist_TargetType> tbl_Checklist_TargetType { get; set; }
        public virtual DbSet<tbl_City> tbl_City { get; set; }
        public virtual DbSet<tbl_City_Class> tbl_City_Class { get; set; }
        public virtual DbSet<tbl_Company> tbl_Company { get; set; }
        public virtual DbSet<tbl_Company_Class> tbl_Company_Class { get; set; }
        public virtual DbSet<tbl_Company_Type> tbl_Company_Type { get; set; }
        public virtual DbSet<tbl_Content_PlaceHolder> tbl_Content_PlaceHolder { get; set; }
        public virtual DbSet<tbl_Country> tbl_Country { get; set; }
        public virtual DbSet<tbl_Currency> tbl_Currency { get; set; }
        public virtual DbSet<tbl_Currency_Rate> tbl_Currency_Rate { get; set; }
        public virtual DbSet<tbl_Custom_Field_Data_Upload> tbl_Custom_Field_Data_Upload { get; set; }
        public virtual DbSet<tbl_Custom_Field_Option> tbl_Custom_Field_Option { get; set; }
        public virtual DbSet<tbl_Custom_Fields> tbl_Custom_Fields { get; set; }
        public virtual DbSet<tbl_Custom_Fields_Data> tbl_Custom_Fields_Data { get; set; }
        public virtual DbSet<tbl_Custom_HostPage> tbl_Custom_HostPage { get; set; }
        public virtual DbSet<tbl_Customer> tbl_Customer { get; set; }
        public virtual DbSet<tbl_Customer_Account_KYC_Item> tbl_Customer_Account_KYC_Item { get; set; }
        public virtual DbSet<tbl_Customer_Address> tbl_Customer_Address { get; set; }
        public virtual DbSet<tbl_Customer_Blacklist> tbl_Customer_Blacklist { get; set; }
        public virtual DbSet<tbl_Customer_BVN> tbl_Customer_BVN { get; set; }
        public virtual DbSet<tbl_Customer_Client_Supplier> tbl_Customer_Client_Supplier { get; set; }
        public virtual DbSet<tbl_Customer_Client_Supplier_Type> tbl_Customer_Client_Supplier_Type { get; set; }
        public virtual DbSet<tbl_Customer_Company_Director> tbl_Customer_Company_Director { get; set; }
        public virtual DbSet<tbl_Customer_Company_DirectorType> tbl_Customer_Company_DirectorType { get; set; }
        public virtual DbSet<tbl_Customer_CompanyInfomation> tbl_Customer_CompanyInfomation { get; set; }
        public virtual DbSet<tbl_Customer_Custom_Field> tbl_Customer_Custom_Field { get; set; }
        public virtual DbSet<tbl_Customer_Edit_History> tbl_Customer_Edit_History { get; set; }
        public virtual DbSet<tbl_Customer_EducationLevelType> tbl_Customer_EducationLevelType { get; set; }
        public virtual DbSet<tbl_Customer_EmploymentHistory> tbl_Customer_EmploymentHistory { get; set; }
        public virtual DbSet<tbl_Customer_FS_Caption> tbl_Customer_FS_Caption { get; set; }
        public virtual DbSet<tbl_Customer_FS_Caption_Detail> tbl_Customer_FS_Caption_Detail { get; set; }
        public virtual DbSet<tbl_Customer_FS_Caption_Group> tbl_Customer_FS_Caption_Group { get; set; }
        public virtual DbSet<tbl_Customer_FS_Ratio_Caption> tbl_Customer_FS_Ratio_Caption { get; set; }
        public virtual DbSet<tbl_Customer_FS_Ratio_Detail> tbl_Customer_FS_Ratio_Detail { get; set; }
        public virtual DbSet<tbl_Customer_FS_Ratio_DivisorType> tbl_Customer_FS_Ratio_DivisorType { get; set; }
        public virtual DbSet<tbl_Customer_FS_Ratio_ValueType> tbl_Customer_FS_Ratio_ValueType { get; set; }
        public virtual DbSet<tbl_Customer_Group> tbl_Customer_Group { get; set; }
        public virtual DbSet<tbl_Customer_Group_Mapping> tbl_Customer_Group_Mapping { get; set; }
        public virtual DbSet<tbl_Customer_Group_RelationshipType> tbl_Customer_Group_RelationshipType { get; set; }
        public virtual DbSet<tbl_Customer_Guardian> tbl_Customer_Guardian { get; set; }
        public virtual DbSet<tbl_Customer_Identification> tbl_Customer_Identification { get; set; }
        public virtual DbSet<tbl_Customer_IdentificationModeType> tbl_Customer_IdentificationModeType { get; set; }
        public virtual DbSet<tbl_Customer_NextOfKin> tbl_Customer_NextOfKin { get; set; }
        public virtual DbSet<tbl_Customer_PhoneContact> tbl_Customer_PhoneContact { get; set; }
        public virtual DbSet<tbl_Customer_Sensitivity_Level> tbl_Customer_Sensitivity_Level { get; set; }
        public virtual DbSet<tbl_Customer_Type> tbl_Customer_Type { get; set; }
        public virtual DbSet<tbl_Daily_Accrual> tbl_Daily_Accrual { get; set; }
        public virtual DbSet<tbl_Daily_Accrual_Category> tbl_Daily_Accrual_Category { get; set; }
        public virtual DbSet<tbl_Day_Count_Convention> tbl_Day_Count_Convention { get; set; }
        public virtual DbSet<tbl_Day_Interest_Type> tbl_Day_Interest_Type { get; set; }
        public virtual DbSet<tbl_Department> tbl_Department { get; set; }
        public virtual DbSet<tbl_ErrorLog> tbl_ErrorLog { get; set; }
        public virtual DbSet<tbl_Fee> tbl_Fee { get; set; }
        public virtual DbSet<tbl_Fee_Amortisation_Type> tbl_Fee_Amortisation_Type { get; set; }
        public virtual DbSet<tbl_Fee_Interval> tbl_Fee_Interval { get; set; }
        public virtual DbSet<tbl_Fee_Target> tbl_Fee_Target { get; set; }
        public virtual DbSet<tbl_Fee_Type> tbl_Fee_Type { get; set; }
        public virtual DbSet<tbl_Finance_EndOfDay> tbl_Finance_EndOfDay { get; set; }
        public virtual DbSet<tbl_Finance_Transaction> tbl_Finance_Transaction { get; set; }
        public virtual DbSet<tbl_FinanceCurrentDate> tbl_FinanceCurrentDate { get; set; }
        public virtual DbSet<tbl_Frequency_Type> tbl_Frequency_Type { get; set; }
        public virtual DbSet<tbl_Job_Request> tbl_Job_Request { get; set; }
        public virtual DbSet<tbl_Job_Request_Document_Mapping> tbl_Job_Request_Document_Mapping { get; set; }
        public virtual DbSet<tbl_Job_Request_Status> tbl_Job_Request_Status { get; set; }
        public virtual DbSet<tbl_Job_Type> tbl_Job_Type { get; set; }
        public virtual DbSet<tbl_KYC_Item> tbl_KYC_Item { get; set; }
        public virtual DbSet<tbl_Management_Type> tbl_Management_Type { get; set; }
        public virtual DbSet<tbl_Message_Log> tbl_Message_Log { get; set; }
        public virtual DbSet<tbl_Message_Log_Status> tbl_Message_Log_Status { get; set; }
        public virtual DbSet<tbl_Message_Log_Type> tbl_Message_Log_Type { get; set; }
        public virtual DbSet<tbl_MIS_Info> tbl_MIS_Info { get; set; }
        public virtual DbSet<tbl_MIS_Type> tbl_MIS_Type { get; set; }
        public virtual DbSet<tbl_Nature_Of_Business> tbl_Nature_Of_Business { get; set; }
        public virtual DbSet<tbl_Notification_Log> tbl_Notification_Log { get; set; }
        public virtual DbSet<tbl_Operations> tbl_Operations { get; set; }
        public virtual DbSet<tbl_Operations_Type> tbl_Operations_Type { get; set; }
        public virtual DbSet<tbl_Product> tbl_Product { get; set; }
        public virtual DbSet<tbl_Product_Category> tbl_Product_Category { get; set; }
        public virtual DbSet<tbl_Product_Charge_Fee> tbl_Product_Charge_Fee { get; set; }
        public virtual DbSet<tbl_Product_Class> tbl_Product_Class { get; set; }
        public virtual DbSet<tbl_Product_Class_Type> tbl_Product_Class_Type { get; set; }
        public virtual DbSet<tbl_Product_Currency> tbl_Product_Currency { get; set; }
        public virtual DbSet<tbl_Product_Group> tbl_Product_Group { get; set; }
        public virtual DbSet<tbl_Product_Price_Index> tbl_Product_Price_Index { get; set; }
        public virtual DbSet<tbl_Product_Type> tbl_Product_Type { get; set; }
        public virtual DbSet<tbl_Profile_Activity> tbl_Profile_Activity { get; set; }
        public virtual DbSet<tbl_Profile_Activity_Parent> tbl_Profile_Activity_Parent { get; set; }
        public virtual DbSet<tbl_Profile_AdditionalActivity> tbl_Profile_AdditionalActivity { get; set; }
        public virtual DbSet<tbl_Profile_Group> tbl_Profile_Group { get; set; }
        public virtual DbSet<tbl_Profile_Group_Activity> tbl_Profile_Group_Activity { get; set; }
        public virtual DbSet<tbl_Profile_Priviledge> tbl_Profile_Priviledge { get; set; }
        public virtual DbSet<tbl_Profile_Priviledge_Activity> tbl_Profile_Priviledge_Activity { get; set; }
        public virtual DbSet<tbl_Profile_User> tbl_Profile_User { get; set; }
        public virtual DbSet<tbl_Profile_UserGroup> tbl_Profile_UserGroup { get; set; }
        public virtual DbSet<tbl_Public_Holiday> tbl_Public_Holiday { get; set; }
        public virtual DbSet<tbl_Region> tbl_Region { get; set; }
        public virtual DbSet<tbl_Sector> tbl_Sector { get; set; }
        public virtual DbSet<tbl_Setup_Global> tbl_Setup_Global { get; set; }
        public virtual DbSet<tbl_Source_Application> tbl_Source_Application { get; set; }
        public virtual DbSet<tbl_Staff> tbl_Staff { get; set; }
        public virtual DbSet<tbl_Staff_JobTitle> tbl_Staff_JobTitle { get; set; }
        public virtual DbSet<tbl_Staff_Organogram> tbl_Staff_Organogram { get; set; }
        public virtual DbSet<tbl_Staff_Rank> tbl_Staff_Rank { get; set; }
        public virtual DbSet<tbl_State> tbl_State { get; set; }
        public virtual DbSet<tbl_Sub_Sector> tbl_Sub_Sector { get; set; }
        public virtual DbSet<tbl_Tax> tbl_Tax { get; set; }
        public virtual DbSet<tbl_Tenor_Mode> tbl_Tenor_Mode { get; set; }
        public virtual DbSet<tbl_Call_Memo> tbl_Call_Memo { get; set; }
        public virtual DbSet<tbl_Call_Memo_Limit> tbl_Call_Memo_Limit { get; set; }
        public virtual DbSet<tbl_Call_Memo_Type> tbl_Call_Memo_Type { get; set; }
        public virtual DbSet<tbl_Collateral_Casa> tbl_Collateral_Casa { get; set; }
        public virtual DbSet<tbl_Collateral_Customer> tbl_Collateral_Customer { get; set; }
        public virtual DbSet<tbl_Collateral_Deposit> tbl_Collateral_Deposit { get; set; }
        public virtual DbSet<tbl_Collateral_Documents> tbl_Collateral_Documents { get; set; }
        public virtual DbSet<tbl_Collateral_Gaurantee> tbl_Collateral_Gaurantee { get; set; }
        public virtual DbSet<tbl_Collateral_Immovable_Property> tbl_Collateral_Immovable_Property { get; set; }
        public virtual DbSet<tbl_Collateral_Item_Policy> tbl_Collateral_Item_Policy { get; set; }
        public virtual DbSet<tbl_Collateral_Marketable_Security> tbl_Collateral_Marketable_Security { get; set; }
        public virtual DbSet<tbl_Collateral_Miscellaneous> tbl_Collateral_Miscellaneous { get; set; }
        public virtual DbSet<tbl_Collateral_Miscellaneous_Notes> tbl_Collateral_Miscellaneous_Notes { get; set; }
        public virtual DbSet<tbl_Collateral_Plant_And_Equipment> tbl_Collateral_Plant_And_Equipment { get; set; }
        public virtual DbSet<tbl_Collateral_Policy> tbl_Collateral_Policy { get; set; }
        public virtual DbSet<tbl_Collateral_PreciousMetal> tbl_Collateral_PreciousMetal { get; set; }
        public virtual DbSet<tbl_Collateral_Principals> tbl_Collateral_Principals { get; set; }
        public virtual DbSet<tbl_Collateral_SeniorityOfClaims> tbl_Collateral_SeniorityOfClaims { get; set; }
        public virtual DbSet<tbl_Collateral_Stock> tbl_Collateral_Stock { get; set; }
        public virtual DbSet<tbl_Collateral_Type> tbl_Collateral_Type { get; set; }
        public virtual DbSet<tbl_Collateral_Type_Sub> tbl_Collateral_Type_Sub { get; set; }
        public virtual DbSet<tbl_Collateral_Valuebase_Type> tbl_Collateral_Valuebase_Type { get; set; }
        public virtual DbSet<tbl_Collateral_Valuer> tbl_Collateral_Valuer { get; set; }
        public virtual DbSet<tbl_Collateral_Valuer_Type> tbl_Collateral_Valuer_Type { get; set; }
        public virtual DbSet<tbl_Collateral_Vehicle> tbl_Collateral_Vehicle { get; set; }
        public virtual DbSet<tbl_Credit_Appraisal_Memorandum> tbl_Credit_Appraisal_Memorandum { get; set; }
        public virtual DbSet<tbl_Credit_Appraisal_Memorandum_Document> tbl_Credit_Appraisal_Memorandum_Document { get; set; }
        public virtual DbSet<tbl_Credit_Appraisal_Memorandum_Loan_Detail> tbl_Credit_Appraisal_Memorandum_Loan_Detail { get; set; }
        public virtual DbSet<tbl_Credit_Template> tbl_Credit_Template { get; set; }
        public virtual DbSet<tbl_Limit> tbl_Limit { get; set; }
        public virtual DbSet<tbl_Limit_Detail> tbl_Limit_Detail { get; set; }
        public virtual DbSet<tbl_Limit_Metric> tbl_Limit_Metric { get; set; }
        public virtual DbSet<tbl_Limit_Type> tbl_Limit_Type { get; set; }
        public virtual DbSet<tbl_Limit_Value_Type> tbl_Limit_Value_Type { get; set; }
        public virtual DbSet<tbl_Loan> tbl_Loan { get; set; }
        public virtual DbSet<tbl_Loan_Application> tbl_Loan_Application { get; set; }
        public virtual DbSet<tbl_Loan_Application_Collateral> tbl_Loan_Application_Collateral { get; set; }
        public virtual DbSet<tbl_Loan_Application_Detail> tbl_Loan_Application_Detail { get; set; }
        public virtual DbSet<tbl_Loan_Application_Detail_Status> tbl_Loan_Application_Detail_Status { get; set; }
        public virtual DbSet<tbl_Loan_Application_Status> tbl_Loan_Application_Status { get; set; }
        public virtual DbSet<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }
        public virtual DbSet<tbl_Loan_Camsol> tbl_Loan_Camsol { get; set; }
        public virtual DbSet<tbl_Loan_Collateral_Mapping> tbl_Loan_Collateral_Mapping { get; set; }
        public virtual DbSet<tbl_Loan_Comment> tbl_Loan_Comment { get; set; }
        public virtual DbSet<tbl_Loan_Condition_Precedent> tbl_Loan_Condition_Precedent { get; set; }
        public virtual DbSet<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }
        public virtual DbSet<tbl_Loan_Covenant_Detail> tbl_Loan_Covenant_Detail { get; set; }
        public virtual DbSet<tbl_Loan_Covenant_Type> tbl_Loan_Covenant_Type { get; set; }
        public virtual DbSet<tbl_Loan_Fee> tbl_Loan_Fee { get; set; }
        public virtual DbSet<tbl_Loan_Fee_Schedule> tbl_Loan_Fee_Schedule { get; set; }
        public virtual DbSet<tbl_Loan_Force_Debit> tbl_Loan_Force_Debit { get; set; }
        public virtual DbSet<tbl_Loan_Guarantor> tbl_Loan_Guarantor { get; set; }
        public virtual DbSet<tbl_Loan_Operation> tbl_Loan_Operation { get; set; }
        public virtual DbSet<tbl_Loan_Past_Due> tbl_Loan_Past_Due { get; set; }
        public virtual DbSet<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }
        public virtual DbSet<tbl_Loan_PriceIndex_Exception> tbl_Loan_PriceIndex_Exception { get; set; }
        public virtual DbSet<tbl_Loan_PrudentialGuideline> tbl_Loan_PrudentialGuideline { get; set; }
        public virtual DbSet<tbl_Loan_Relationship_Officer_History> tbl_Loan_Relationship_Officer_History { get; set; }
        public virtual DbSet<tbl_Loan_Review_Operation> tbl_Loan_Review_Operation { get; set; }
        public virtual DbSet<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Category> tbl_Loan_Schedule_Category { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Daily> tbl_Loan_Schedule_Daily { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Daily_Archive> tbl_Loan_Schedule_Daily_Archive { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Daily_Temp> tbl_Loan_Schedule_Daily_Temp { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Irregular_Input> tbl_Loan_Schedule_Irregular_Input { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Periodic> tbl_Loan_Schedule_Periodic { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Periodic_Archive> tbl_Loan_Schedule_Periodic_Archive { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Periodic_Temp> tbl_Loan_Schedule_Periodic_Temp { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Type> tbl_Loan_Schedule_Type { get; set; }
        public virtual DbSet<tbl_Loan_Schedule_Type_Product_Type_Mapping> tbl_Loan_Schedule_Type_Product_Type_Mapping { get; set; }
        public virtual DbSet<tbl_Loan_Status> tbl_Loan_Status { get; set; }
        public virtual DbSet<tbl_Loan_Transaction_Type> tbl_Loan_Transaction_Type { get; set; }
        public virtual DbSet<tbl_Loan_Type> tbl_Loan_Type { get; set; }
        public virtual DbSet<tbl_LoanApplication_Collateral_Mapping> tbl_LoanApplication_Collateral_Mapping { get; set; }
        public virtual DbSet<tbl_MachineValue_Base> tbl_MachineValue_Base { get; set; }
        public virtual DbSet<tbl_Product_CollateralType> tbl_Product_CollateralType { get; set; }
        public virtual DbSet<tbl_Risk_Assessment> tbl_Risk_Assessment { get; set; }
        public virtual DbSet<tbl_Risk_Assessment_Index> tbl_Risk_Assessment_Index { get; set; }
        public virtual DbSet<tbl_Risk_Assessment_Index_Type> tbl_Risk_Assessment_Index_Type { get; set; }
        public virtual DbSet<tbl_Risk_Assessment_Result> tbl_Risk_Assessment_Result { get; set; }
        public virtual DbSet<tbl_Risk_Assessment_Title> tbl_Risk_Assessment_Title { get; set; }
        public virtual DbSet<tbl_Risk_Rating> tbl_Risk_Rating { get; set; }
        public virtual DbSet<tbl_Solicitor> tbl_Solicitor { get; set; }
        public virtual DbSet<tbl_Solicitor_State_Mapping> tbl_Solicitor_State_Mapping { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<tbl_Approval> tbl_Approval { get; set; }
        public virtual DbSet<tbl_CASA_Lien> tbl_CASA_Lien { get; set; }
        public virtual DbSet<tbl_Charges_ValueSource> tbl_Charges_ValueSource { get; set; }
        public virtual DbSet<tbl_COT> tbl_COT { get; set; }
        public virtual DbSet<tbl_Loan_Document_Type> tbl_Loan_Document_Type { get; set; }
        public virtual DbSet<tbl_Account_Category> tbl_Account_Category { get; set; }
        public virtual DbSet<tbl_Account_Type> tbl_Account_Type { get; set; }
        public virtual DbSet<tbl_Charge_Range> tbl_Charge_Range { get; set; }
        public virtual DbSet<tbl_Charges> tbl_Charges { get; set; }
        public virtual DbSet<tbl_Chart_Of_Account> tbl_Chart_Of_Account { get; set; }
        public virtual DbSet<tbl_Chart_Of_Account_Class> tbl_Chart_Of_Account_Class { get; set; }
        public virtual DbSet<tbl_Chart_Of_Account_Currency> tbl_Chart_Of_Account_Currency { get; set; }
        public virtual DbSet<tbl_Financial_Statement_Caption> tbl_Financial_Statement_Caption { get; set; }
        public virtual DbSet<tbl_Financial_Statement_Type> tbl_Financial_Statement_Type { get; set; }
        public virtual DbSet<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee { get; set; }
        public virtual DbSet<tbl_Temp_Chart_Of_Account> tbl_Temp_Chart_Of_Account { get; set; }
        public virtual DbSet<tbl_Temp_Chart_Of_Account_Currency> tbl_Temp_Chart_Of_Account_Currency { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Casa> tbl_Temp_Collateral_Casa { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Customer> tbl_Temp_Collateral_Customer { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Deposit> tbl_Temp_Collateral_Deposit { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Documents> tbl_Temp_Collateral_Documents { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Gaurantee> tbl_Temp_Collateral_Gaurantee { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Immovable_Property> tbl_Temp_Collateral_Immovable_Property { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Marketable_Security> tbl_Temp_Collateral_Marketable_Security { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Miscellaneous> tbl_Temp_Collateral_Miscellaneous { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Miscellaneous_Notes> tbl_Temp_Collateral_Miscellaneous_Notes { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Plant_And_Equipment> tbl_Temp_Collateral_Plant_And_Equipment { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Policy> tbl_Temp_Collateral_Policy { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_PreciousMetal> tbl_Temp_Collateral_PreciousMetal { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Vehicle> tbl_Temp_Collateral_Vehicle { get; set; }
        public virtual DbSet<tbl_Temp_Customer_Group> tbl_Temp_Customer_Group { get; set; }
        public virtual DbSet<tbl_Temp_Customer_Group_Mapping> tbl_Temp_Customer_Group_Mapping { get; set; }
        public virtual DbSet<tbl_Temp_Fee> tbl_Temp_Fee { get; set; }
        public virtual DbSet<tbl_Temp_Product> tbl_Temp_Product { get; set; }
        public virtual DbSet<tbl_Temp_Product_Charge_Fee> tbl_Temp_Product_Charge_Fee { get; set; }
        public virtual DbSet<tbl_Temp_Product_CollateralType> tbl_Temp_Product_CollateralType { get; set; }
        public virtual DbSet<tbl_Temp_Product_Currency> tbl_Temp_Product_Currency { get; set; }
        public virtual DbSet<tbl_Temp_Product_Fee> tbl_Temp_Product_Fee { get; set; }
        public virtual DbSet<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
        public virtual DbSet<tbl_Deal_Classification> tbl_Deal_Classification { get; set; }
        public virtual DbSet<tbl_Deal_Type> tbl_Deal_Type { get; set; }
        public virtual DbSet<tbl_Stock> tbl_Stock { get; set; }
        public virtual DbSet<tbl_Temp_Collateral_Stock> tbl_Temp_Collateral_Stock { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tbl_AccreditedConsultant>()
                .HasMany(e => e.tbl_AccreditedConsultant_State)
                .WithRequired(e => e.tbl_AccreditedConsultant)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_AccreditedConsultant_Type>()
                .HasMany(e => e.tbl_AccreditedConsultant)
                .WithOptional(e => e.tbl_AccreditedConsultant_Type)
                .HasForeignKey(e => e.AccreditedConsultantTypeId);

            modelBuilder.Entity<tbl_Approval_Group>()
                .HasMany(e => e.tbl_Approval_Group_Mapping)
                .WithRequired(e => e.tbl_Approval_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Group>()
                .HasMany(e => e.tbl_Approval_Level)
                .WithRequired(e => e.tbl_Approval_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Level>()
                .Property(e => e.MaximumAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Approval_Level>()
                .Property(e => e.InvestmentGradeAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Approval_Level>()
                .HasMany(e => e.tbl_Approval_Level_Staff)
                .WithRequired(e => e.tbl_Approval_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Level>()
                .HasMany(e => e.tbl_Approval_Trail)
                .WithOptional(e => e.tbl_Approval_Level)
                .HasForeignKey(e => e.FromApprovalLevelId);

            modelBuilder.Entity<tbl_Approval_Level>()
                .HasMany(e => e.tbl_Approval_Trail1)
                .WithOptional(e => e.tbl_Approval_Level1)
                .HasForeignKey(e => e.ToApprovalLevelId);

            modelBuilder.Entity<tbl_Approval_Level>()
                .HasMany(e => e.tbl_Credit_Appraisal_Memorandum_Document)
                .WithRequired(e => e.tbl_Approval_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Level>()
                .HasMany(e => e.tbl_Credit_Template)
                .WithRequired(e => e.tbl_Approval_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Level_Staff>()
                .Property(e => e.MaximumAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Approval_State>()
                .HasMany(e => e.tbl_Approval_Trail)
                .WithRequired(e => e.tbl_Approval_State)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Approval_Trail)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Collateral_Customer)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Customer_Group)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Product)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Approval_Status>()
                .HasMany(e => e.tbl_Temp_Staff)
                .WithRequired(e => e.tbl_Approval_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Audit)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Chart_Of_Account)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_CASA_Lien)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Customer)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Finance_Transaction)
                .WithRequired(e => e.tbl_Branch)
                .HasForeignKey(e => e.SourceBranchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Finance_Transaction1)
                .WithRequired(e => e.tbl_Branch1)
                .HasForeignKey(e => e.DestinationBranchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Branch>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Branch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .Property(e => e.AvailableBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA>()
                .Property(e => e.LedgerBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA>()
                .Property(e => e.TeamMISCode)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_CASA>()
                .Property(e => e.OverdraftAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA>()
                .Property(e => e.OverdraftInterestRate)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA>()
                .Property(e => e.LienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_CASA)
                .HasForeignKey(e => e.CasaAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan_Archive1)
                .WithRequired(e => e.tbl_CASA1)
                .HasForeignKey(e => e.CasaAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_CASA)
                .HasForeignKey(e => e.CasaAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_CASA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA>()
                .HasMany(e => e.tbl_Loan1)
                .WithRequired(e => e.tbl_CASA1)
                .HasForeignKey(e => e.CasaAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA_AccountStatus>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_CASA_AccountStatus)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA_Lien_Type>()
                .HasMany(e => e.tbl_CASA_Lien)
                .WithRequired(e => e.tbl_CASA_Lien_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CASA_PostNoStatus>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_CASA_PostNoStatus)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Charge_Fee>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Charge_Fee>()
                .HasMany(e => e.tbl_Charge_Range)
                .WithRequired(e => e.tbl_Charge_Fee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Charge_Fee>()
                .HasMany(e => e.tbl_Loan_Fee)
                .WithRequired(e => e.tbl_Charge_Fee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Charge_Fee>()
                .HasMany(e => e.tbl_Product_Charge_Fee)
                .WithRequired(e => e.tbl_Charge_Fee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Charge_Fee>()
                .HasMany(e => e.tbl_Temp_Product_Charge_Fee)
                .WithRequired(e => e.tbl_Charge_Fee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Checklist_Definition>()
                .HasMany(e => e.tbl_Checklist_Detail)
                .WithRequired(e => e.tbl_Checklist_Definition)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_CheckList_Item>()
                .HasMany(e => e.tbl_Checklist_Definition)
                .WithRequired(e => e.tbl_CheckList_Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Checklist_Status>()
                .HasMany(e => e.tbl_Checklist_Detail)
                .WithRequired(e => e.tbl_Checklist_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Checklist_TargetType>()
                .HasMany(e => e.tbl_Checklist_Detail)
                .WithRequired(e => e.tbl_Checklist_TargetType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_City>()
                .HasMany(e => e.tbl_Collateral_Immovable_Property)
                .WithRequired(e => e.tbl_City)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_City>()
                .HasMany(e => e.tbl_Temp_Collateral_Immovable_Property)
                .WithRequired(e => e.tbl_City)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_City_Class>()
                .HasMany(e => e.tbl_City)
                .WithRequired(e => e.tbl_City_Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .Property(e => e.ShareHoldersFund)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Company>()
                .Property(e => e.PreliminaryEvaluation_Limit)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Company>()
                .Property(e => e.AuthorisedShareCapital)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Approval_Group)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Approval_Trail)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Branch)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Checklist_Definition)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Product)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Product)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Call_Memo_Limit)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_CASA_Lien)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Chart_Of_Account)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Collateral_Customer)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Collateral_Customer)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Company1)
                .WithOptional(e => e.tbl_Company2)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Credit_Appraisal_Memorandum)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Credit_Template)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Customer_FS_Caption_Group)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Customer_FS_Ratio_Caption)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Customer)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Finance_EndOfDay)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Finance_Transaction)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_FinanceCurrentDate)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Limit)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan_Camsol)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Product_CollateralType)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Product_CollateralType)
                .WithRequired(e => e.tbl_Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Product_Charge_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Product_Price_Index)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Setup_Global)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Solicitor)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Staff)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Stock)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Tax)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Customer_Group)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Product_Charge_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Product_CollateralType1)
                .WithRequired(e => e.tbl_Company1)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Product_Fee)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Company>()
                .HasMany(e => e.tbl_Temp_Staff)
                .WithRequired(e => e.tbl_Company)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Content_PlaceHolder>()
                .Property(e => e.ContentPlaceHolder)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Content_PlaceHolder>()
                .Property(e => e.CollumnName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Country>()
                .HasMany(e => e.tbl_Company)
                .WithRequired(e => e.tbl_Country)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Country>()
                .HasMany(e => e.tbl_State)
                .WithRequired(e => e.tbl_Country)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Country>()
                .HasMany(e => e.tbl_Public_Holiday)
                .WithRequired(e => e.tbl_Country)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Country>()
                .HasMany(e => e.tbl_Region)
                .WithRequired(e => e.tbl_Country)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Company)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Chart_Of_Account_Currency)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account_Currency)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Collateral_Customer)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Currency_Rate)
                .WithRequired(e => e.tbl_Currency)
                .HasForeignKey(e => e.CurrencyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Currency_Rate1)
                .WithRequired(e => e.tbl_Currency1)
                .HasForeignKey(e => e.BaseCurrencyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Finance_Transaction)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Loan_Application_Detail)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Product_Currency)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Currency>()
                .HasMany(e => e.tbl_Temp_Product_Currency)
                .WithRequired(e => e.tbl_Currency)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Custom_Fields>()
                .HasMany(e => e.tbl_Custom_Fields_Data)
                .WithRequired(e => e.tbl_Custom_Fields)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Custom_Fields_Data>()
                .HasMany(e => e.tbl_Custom_Field_Data_Upload)
                .WithRequired(e => e.tbl_Custom_Fields_Data)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Custom_HostPage>()
                .HasOptional(e => e.tbl_Custom_HostPage1)
                .WithRequired(e => e.tbl_Custom_HostPage2);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Custom_Fields)
                .WithOptional(e => e.tbl_Customer)
                .HasForeignKey(e => e.ActedOnBy);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Custom_Fields_Data)
                .WithOptional(e => e.tbl_Customer)
                .HasForeignKey(e => e.ActedOnBy);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Custom_Fields_Data1)
                .WithRequired(e => e.tbl_Customer1)
                .HasForeignKey(e => e.OwnerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_Account_KYC_Item)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Temp_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Collateral_Customer)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Customer)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_BVN)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_CompanyInfomation)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_EmploymentHistory)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_FS_Caption_Detail)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_Guardian)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_Identification)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_NextOfKin)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_PhoneContact)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_Address)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Customer_Client_Supplier)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer>()
                .HasMany(e => e.tbl_Loan_Application_Detail)
                .WithRequired(e => e.tbl_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Client_Supplier_Type>()
                .HasMany(e => e.tbl_Customer_Client_Supplier)
                .WithRequired(e => e.tbl_Customer_Client_Supplier_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Company_DirectorType>()
                .HasMany(e => e.tbl_Customer_Company_Director)
                .WithRequired(e => e.tbl_Customer_Company_DirectorType)
                .HasForeignKey(e => e.CompanyDirectorTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_FS_Caption>()
                .HasMany(e => e.tbl_Customer_FS_Caption_Detail)
                .WithRequired(e => e.tbl_Customer_FS_Caption)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_FS_Caption>()
                .HasMany(e => e.tbl_Customer_FS_Ratio_Detail)
                .WithRequired(e => e.tbl_Customer_FS_Caption)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_FS_Caption>()
                .HasMany(e => e.tbl_Customer_FS_Caption1)
                .WithOptional(e => e.tbl_Customer_FS_Caption2)
                .HasForeignKey(e => e.ParentIdFSCaptionId);

            modelBuilder.Entity<tbl_Customer_FS_Caption_Detail>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Customer_FS_Caption_Group>()
                .HasMany(e => e.tbl_Customer_FS_Caption)
                .WithRequired(e => e.tbl_Customer_FS_Caption_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_FS_Ratio_Caption>()
                .HasMany(e => e.tbl_Customer_FS_Ratio_Detail)
                .WithRequired(e => e.tbl_Customer_FS_Ratio_Caption)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_FS_Ratio_Detail>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Customer_FS_Ratio_DivisorType>()
                .HasMany(e => e.tbl_Customer_FS_Ratio_Detail)
                .WithRequired(e => e.tbl_Customer_FS_Ratio_DivisorType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_FS_Ratio_ValueType>()
                .HasMany(e => e.tbl_Customer_FS_Ratio_Detail)
                .WithRequired(e => e.tbl_Customer_FS_Ratio_ValueType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Group>()
                .HasMany(e => e.tbl_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Customer_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Group_RelationshipType>()
                .HasMany(e => e.tbl_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Customer_Group_RelationshipType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Group_RelationshipType>()
                .HasMany(e => e.tbl_Temp_Customer_Group_Mapping)
                .WithRequired(e => e.tbl_Customer_Group_RelationshipType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Group_RelationshipType>()
                .HasOptional(e => e.tbl_Customer_Group_RelationshipType1)
                .WithRequired(e => e.tbl_Customer_Group_RelationshipType2);

            modelBuilder.Entity<tbl_Customer_IdentificationModeType>()
                .Property(e => e.IdentificationMode)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Customer_PhoneContact>()
                .Property(e => e.Phone)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Customer_Sensitivity_Level>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_Customer_Sensitivity_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Sensitivity_Level>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Customer_Sensitivity_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Sensitivity_Level>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Customer_Sensitivity_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Sensitivity_Level>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Customer_Sensitivity_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Customer_Sensitivity_Level>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Customer_Sensitivity_Level)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Daily_Accrual>()
                .Property(e => e.MainAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Daily_Accrual>()
                .Property(e => e.DailyAccuralAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Daily_Accrual>()
                .Property(e => e.SystemDateTime)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Daily_Accrual_Category>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Daily_Accrual_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Day_Count_Convention>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Day_Count_Convention)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Day_Count_Convention>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Day_Count_Convention)
                .HasForeignKey(e => e.ScheduleDayCountConventionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Day_Count_Convention>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Day_Count_Convention)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Day_Count_Convention>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Day_Count_Convention)
                .HasForeignKey(e => e.ScheduleDayCountConventionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Day_Interest_Type>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Day_Interest_Type)
                .HasForeignKey(e => e.ScheduleDayInterestTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Day_Interest_Type>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Day_Interest_Type)
                .HasForeignKey(e => e.ScheduleDayInterestTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Department>()
                .HasMany(e => e.tbl_Job_Request)
                .WithRequired(e => e.tbl_Department)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee>()
                .HasMany(e => e.tbl_Temp_Product_Fee)
                .WithRequired(e => e.tbl_Fee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Interval>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Fee_Interval)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Interval>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Fee_Interval)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Interval>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Fee_Interval)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Interval>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Fee_Interval)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Target>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Fee_Target)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Target>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Fee_Target)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Target>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Fee_Target)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Target>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Fee_Target)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Type>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Fee_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Type>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Fee_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Fee_Type>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Fee_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Finance_Transaction>()
                .Property(e => e.DebitAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Finance_Transaction>()
                .Property(e => e.CreditAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Call_Memo_Limit)
                .WithRequired(e => e.tbl_Frequency_Type)
                .HasForeignKey(e => e.FrequencyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Collateral_Policy)
                .WithOptional(e => e.tbl_Frequency_Type)
                .HasForeignKey(e => e.RenewalFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Limit_Detail)
                .WithRequired(e => e.tbl_Frequency_Type)
                .HasForeignKey(e => e.LimitFrequencyTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithOptional(e => e.tbl_Frequency_Type)
                .HasForeignKey(e => e.InterestFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Loan_Archive1)
                .WithOptional(e => e.tbl_Frequency_Type1)
                .HasForeignKey(e => e.PrincipalFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Loan_Archive2)
                .WithOptional(e => e.tbl_Frequency_Type2)
                .HasForeignKey(e => e.ScheduledPrepaymentFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Loan)
                .WithOptional(e => e.tbl_Frequency_Type)
                .HasForeignKey(e => e.InterestFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Loan1)
                .WithOptional(e => e.tbl_Frequency_Type1)
                .HasForeignKey(e => e.PrincipalFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Loan2)
                .WithOptional(e => e.tbl_Frequency_Type2)
                .HasForeignKey(e => e.ScheduledPrepaymentFrequencyTypeId);

            modelBuilder.Entity<tbl_Frequency_Type>()
                .HasMany(e => e.tbl_Temp_Collateral_Policy)
                .WithOptional(e => e.tbl_Frequency_Type)
                .HasForeignKey(e => e.RenewalFrequencyTypeId);

            modelBuilder.Entity<tbl_Job_Request>()
                .HasMany(e => e.tbl_Job_Request_Document_Mapping)
                .WithRequired(e => e.tbl_Job_Request)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Job_Request_Status>()
                .HasMany(e => e.tbl_Job_Request)
                .WithRequired(e => e.tbl_Job_Request_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Job_Type>()
                .HasMany(e => e.tbl_Job_Request)
                .WithRequired(e => e.tbl_Job_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Message_Log>()
                .Property(e => e.MessageBody)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Message_Log_Status>()
                .HasMany(e => e.tbl_Message_Log)
                .WithRequired(e => e.tbl_Message_Log_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Message_Log_Type>()
                .HasMany(e => e.tbl_Message_Log)
                .WithRequired(e => e.tbl_Message_Log_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_MIS_Info>()
                .HasMany(e => e.tbl_MIS_Info1)
                .WithOptional(e => e.tbl_MIS_Info2)
                .HasForeignKey(e => e.ParentMISInfoId);

            modelBuilder.Entity<tbl_Operations>()
                .HasMany(e => e.tbl_Approval_Group_Mapping)
                .WithRequired(e => e.tbl_Operations)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Operations>()
                .HasMany(e => e.tbl_Approval_Trail)
                .WithRequired(e => e.tbl_Operations)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Operations>()
                .HasMany(e => e.tbl_Finance_Transaction)
                .WithRequired(e => e.tbl_Operations)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Operations>()
                .HasMany(e => e.tbl_Job_Request)
                .WithRequired(e => e.tbl_Operations)
                .HasForeignKey(e => e.OperationsId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Operations>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_Operations)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Operations>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Operations)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Operations_Type>()
                .HasMany(e => e.tbl_Operations)
                .WithRequired(e => e.tbl_Operations_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_CASA)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Product_CollateralType)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Loan_Application_Detail)
                .WithRequired(e => e.tbl_Product)
                .HasForeignKey(e => e.ProposedProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Loan_Application_Detail1)
                .WithRequired(e => e.tbl_Product1)
                .HasForeignKey(e => e.ApprovedProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Product_Currency)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Product_Charge_Fee)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product>()
                .HasMany(e => e.tbl_Risk_Rating)
                .WithRequired(e => e.tbl_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Category>()
                .HasMany(e => e.tbl_Product)
                .WithRequired(e => e.tbl_Product_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Category>()
                .HasMany(e => e.tbl_Temp_Product)
                .WithRequired(e => e.tbl_Product_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Charge_Fee>()
                .Property(e => e.RateValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Product_Charge_Fee>()
                .Property(e => e.DependentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Product_Class>()
                .HasMany(e => e.tbl_Product)
                .WithRequired(e => e.tbl_Product_Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Class>()
                .HasMany(e => e.tbl_Credit_Template)
                .WithRequired(e => e.tbl_Product_Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Class>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Product_Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Class>()
                .HasMany(e => e.tbl_Temp_Product)
                .WithRequired(e => e.tbl_Product_Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Class_Type>()
                .HasMany(e => e.tbl_Product_Class)
                .WithRequired(e => e.tbl_Product_Class_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Group>()
                .HasMany(e => e.tbl_Product_Type)
                .WithRequired(e => e.tbl_Product_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Product)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Temp_Product)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Loan_Covenant_Detail)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Loan_Fee)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Loan_Force_Debit)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Loan_Guarantor)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Loan_Past_Due)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Loan_Schedule_Type_Product_Type_Mapping)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Product_Type>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Product_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Activity>()
                .HasMany(e => e.tbl_Profile_AdditionalActivity)
                .WithRequired(e => e.tbl_Profile_Activity)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Activity>()
                .HasMany(e => e.tbl_Profile_Group_Activity)
                .WithRequired(e => e.tbl_Profile_Activity)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Activity>()
                .HasMany(e => e.tbl_Profile_Priviledge_Activity)
                .WithRequired(e => e.tbl_Profile_Activity)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Activity_Parent>()
                .HasMany(e => e.tbl_Profile_Activity)
                .WithRequired(e => e.tbl_Profile_Activity_Parent)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Group>()
                .HasMany(e => e.tbl_Profile_Group_Activity)
                .WithRequired(e => e.tbl_Profile_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Group>()
                .HasMany(e => e.tbl_Profile_UserGroup)
                .WithRequired(e => e.tbl_Profile_Group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_Priviledge>()
                .HasMany(e => e.tbl_Profile_Priviledge_Activity)
                .WithRequired(e => e.tbl_Profile_Priviledge)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_User>()
                .HasMany(e => e.tbl_Profile_AdditionalActivity)
                .WithRequired(e => e.tbl_Profile_User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_User>()
                .HasMany(e => e.tbl_Profile_Priviledge_Activity)
                .WithRequired(e => e.tbl_Profile_User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Profile_User>()
                .HasMany(e => e.tbl_Profile_UserGroup)
                .WithRequired(e => e.tbl_Profile_User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .Property(e => e.Gender)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Staff>()
                .Property(e => e.GenderOfNOK)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Approval_Level_Staff)
                .WithRequired(e => e.tbl_Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Approval_Trail)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RequestStaffId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Approval_Trail1)
                .WithOptional(e => e.tbl_Staff1)
                .HasForeignKey(e => e.ResponseStaffId);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Audit)
                .WithRequired(e => e.tbl_Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_CASA)
                .WithOptional(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_CASA1)
                .WithOptional(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Customer)
                .WithOptional(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Finance_Transaction)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.PostedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Finance_Transaction1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.ApprovedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Job_Request)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.SenderStaffId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Job_Request1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.ReceiverStaffId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Job_Request2)
                .WithOptional(e => e.tbl_Staff2)
                .HasForeignKey(e => e.ReassignedTo);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Notification_Log)
                .WithRequired(e => e.tbl_Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Profile_User)
                .WithRequired(e => e.tbl_Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Call_Memo)
                .WithRequired(e => e.tbl_Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Application1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Archive1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Contingent1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Relationship_Officer_History)
                .WithRequired(e => e.tbl_Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Staff)
                .HasForeignKey(e => e.RelationshipOfficerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff>()
                .HasMany(e => e.tbl_Loan_Revolving1)
                .WithRequired(e => e.tbl_Staff1)
                .HasForeignKey(e => e.RelationshipManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff_JobTitle>()
                .HasMany(e => e.tbl_Staff)
                .WithRequired(e => e.tbl_Staff_JobTitle)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff_JobTitle>()
                .HasMany(e => e.tbl_Temp_Staff)
                .WithRequired(e => e.tbl_Staff_JobTitle)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff_Rank>()
                .HasMany(e => e.tbl_Staff)
                .WithRequired(e => e.tbl_Staff_Rank)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Staff_Rank>()
                .HasMany(e => e.tbl_Temp_Staff)
                .WithRequired(e => e.tbl_Staff_Rank)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_State>()
                .Property(e => e.CollateralSearchChargeAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_State>()
                .HasMany(e => e.tbl_City)
                .WithRequired(e => e.tbl_State)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_State>()
                .HasMany(e => e.tbl_Solicitor_State_Mapping)
                .WithRequired(e => e.tbl_State)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Customer)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Loan_Application_Detail)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Sub_Sector>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Sub_Sector)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Tax>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Tax>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithOptional(e => e.tbl_Tax)
                .HasForeignKey(e => e.PrimaryTaxId);

            modelBuilder.Entity<tbl_Tax>()
                .HasMany(e => e.tbl_Charge_Fee1)
                .WithOptional(e => e.tbl_Tax1)
                .HasForeignKey(e => e.SecondaryTaxId);

            modelBuilder.Entity<tbl_Tax>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithOptional(e => e.tbl_Tax)
                .HasForeignKey(e => e.PrimaryTaxId);

            modelBuilder.Entity<tbl_Tax>()
                .HasMany(e => e.tbl_Temp_Charge_Fee1)
                .WithOptional(e => e.tbl_Tax1)
                .HasForeignKey(e => e.SecondaryTaxId);

            modelBuilder.Entity<tbl_Call_Memo_Limit>()
                .Property(e => e.MinimumAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Call_Memo_Limit>()
                .Property(e => e.MaximumAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Call_Memo_Type>()
                .HasMany(e => e.tbl_Call_Memo)
                .WithRequired(e => e.tbl_Call_Memo_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Casa>()
                .Property(e => e.AvailableBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Casa>()
                .Property(e => e.ExistingLienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Casa>()
                .Property(e => e.LienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Casa>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Casa>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .Property(e => e.CollateralValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .Property(e => e.CamRefNumber)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Casa)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Documents)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Item_Policy)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Deposit)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Gaurantee)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Policy)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Plant_And_Equipment)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Marketable_Security)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Miscellaneous)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_PreciousMetal)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Immovable_Property)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Stock)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Collateral_Vehicle)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Loan_Collateral_Mapping)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Policy)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Plant_And_Equipment)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_PreciousMetal)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Stock)
                .WithRequired(e => e.tbl_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Deposit>()
                .Property(e => e.ExistingLienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Deposit>()
                .Property(e => e.LienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Deposit>()
                .Property(e => e.AvailableBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Deposit>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Deposit>()
                .Property(e => e.MaturityAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Deposit>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Documents>()
                .Property(e => e.DocumentCategory)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Documents>()
                .Property(e => e.DocumentType)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Documents>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Gaurantee>()
                .Property(e => e.InstitutionName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Gaurantee>()
                .Property(e => e.GuarantorAddress)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Gaurantee>()
                .Property(e => e.GuaranteeValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Gaurantee>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.PropertyAddress)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.OpenMarketValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.CollateralValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.ForcedSaleValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.StampToCover)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.OriginalValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.AvailableValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.CollateralUsableAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.Longitude)
                .HasPrecision(12, 9);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .Property(e => e.Latitude)
                .HasPrecision(12, 9);

            modelBuilder.Entity<tbl_Collateral_Immovable_Property>()
                .HasOptional(e => e.tbl_Collateral_Immovable_Property1)
                .WithRequired(e => e.tbl_Collateral_Immovable_Property2);

            modelBuilder.Entity<tbl_Collateral_Item_Policy>()
                .Property(e => e.SumInsured)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.DealAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.LienUsableAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.IssuerName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.IssuerReferenceNumber)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.UnitValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Marketable_Security>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Miscellaneous>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Miscellaneous>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Miscellaneous>()
                .HasMany(e => e.tbl_Collateral_Miscellaneous_Notes)
                .WithOptional(e => e.tbl_Collateral_Miscellaneous)
                .HasForeignKey(e => e.MiscellaneousId);

            modelBuilder.Entity<tbl_Collateral_Plant_And_Equipment>()
                .Property(e => e.MachineName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Plant_And_Equipment>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Plant_And_Equipment>()
                .Property(e => e.YearOfManufacture)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Collateral_Plant_And_Equipment>()
                .Property(e => e.YearOfPurchase)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Collateral_Plant_And_Equipment>()
                .Property(e => e.ReplacementValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Policy>()
                .Property(e => e.PremiumAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Policy>()
                .Property(e => e.PolicyAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Policy>()
                .Property(e => e.InsuranceCompanyName)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Collateral_Policy>()
                .Property(e => e.InsurerAddress)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Policy>()
                .Property(e => e.InsurerDetails)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Policy>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_PreciousMetal>()
                .Property(e => e.PreciousMetalName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_PreciousMetal>()
                .Property(e => e.ValuationAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_PreciousMetal>()
                .Property(e => e.PreciousMetalForm)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_PreciousMetal>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_SeniorityOfClaims>()
                .Property(e => e.SeniorityOfClaims)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_SeniorityOfClaims>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Stock>()
                .Property(e => e.MarketPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Stock>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Stock>()
                .Property(e => e.SharesSecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Stock>()
                .Property(e => e.ShareValueAmountToUse)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Collateral_Customer)
                .WithRequired(e => e.tbl_Collateral_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Temp_Collateral_Customer)
                .WithRequired(e => e.tbl_Collateral_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Collateral_Type_Sub)
                .WithRequired(e => e.tbl_Collateral_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Collateral_Valuebase_Type)
                .WithRequired(e => e.tbl_Collateral_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Loan_Application_Collateral)
                .WithRequired(e => e.tbl_Collateral_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Product_CollateralType)
                .WithRequired(e => e.tbl_Collateral_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Temp_Product_CollateralType)
                .WithRequired(e => e.tbl_Collateral_Type)
                .HasForeignKey(e => e.CollateralTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type>()
                .HasMany(e => e.tbl_Temp_Product_CollateralType1)
                .WithRequired(e => e.tbl_Collateral_Type1)
                .HasForeignKey(e => e.CollateralTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type_Sub>()
                .HasMany(e => e.tbl_Collateral_Plant_And_Equipment)
                .WithRequired(e => e.tbl_Collateral_Type_Sub)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type_Sub>()
                .HasMany(e => e.tbl_Temp_Collateral_Immovable_Property)
                .WithRequired(e => e.tbl_Collateral_Type_Sub)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Type_Sub>()
                .HasMany(e => e.tbl_Temp_Collateral_Plant_And_Equipment)
                .WithRequired(e => e.tbl_Collateral_Type_Sub)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Valuebase_Type>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Collateral_Valuebase_Type>()
                .HasMany(e => e.tbl_Collateral_Plant_And_Equipment)
                .WithRequired(e => e.tbl_Collateral_Valuebase_Type)
                .HasForeignKey(e => e.ValueBaseTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Valuebase_Type>()
                .HasMany(e => e.tbl_Temp_Collateral_Plant_And_Equipment)
                .WithRequired(e => e.tbl_Collateral_Valuebase_Type)
                .HasForeignKey(e => e.ValueBaseTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Collateral_Valuer_Type>()
                .HasMany(e => e.tbl_Collateral_Valuer)
                .WithOptional(e => e.tbl_Collateral_Valuer_Type)
                .HasForeignKey(e => e.ValuerTypeId);

            modelBuilder.Entity<tbl_Collateral_Vehicle>()
                .Property(e => e.ResaleValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Vehicle>()
                .Property(e => e.LastValuationAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Vehicle>()
                .Property(e => e.InvoiceValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Collateral_Vehicle>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Credit_Appraisal_Memorandum>()
                .HasMany(e => e.tbl_Credit_Appraisal_Memorandum_Document)
                .WithRequired(e => e.tbl_Credit_Appraisal_Memorandum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Credit_Appraisal_Memorandum>()
                .HasMany(e => e.tbl_Credit_Appraisal_Memorandum_Loan_Detail)
                .WithRequired(e => e.tbl_Credit_Appraisal_Memorandum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Credit_Appraisal_Memorandum_Loan_Detail>()
                .Property(e => e.PrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Limit>()
                .HasMany(e => e.tbl_Limit_Detail)
                .WithRequired(e => e.tbl_Limit)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Limit_Detail>()
                .Property(e => e.MinimumValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Limit_Detail>()
                .Property(e => e.MaximumValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Limit_Metric>()
                .HasMany(e => e.tbl_Limit)
                .WithRequired(e => e.tbl_Limit_Metric)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Limit_Type>()
                .HasMany(e => e.tbl_Limit_Detail)
                .WithRequired(e => e.tbl_Limit_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Limit_Value_Type>()
                .HasMany(e => e.tbl_Limit)
                .WithRequired(e => e.tbl_Limit_Value_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan>()
                .Property(e => e.TeamMISCode)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Loan>()
                .Property(e => e.PrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan>()
                .Property(e => e.EquityContribution)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan>()
                .Property(e => e.OutstandingPrincipal)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan>()
                .Property(e => e.OutstandingInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan>()
                .Property(e => e.ScheduledPrepaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan>()
                .HasMany(e => e.tbl_Loan_Schedule_Daily)
                .WithRequired(e => e.tbl_Loan)
                .HasForeignKey(e => e.LoanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan>()
                .HasMany(e => e.tbl_Loan_Schedule_Periodic)
                .WithRequired(e => e.tbl_Loan)
                .HasForeignKey(e => e.LoanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .Property(e => e.ApplicationAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Application>()
                .Property(e => e.ApprovedAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Credit_Appraisal_Memorandum)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Loan_Condition_Precedent)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Loan_Application_Detail)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Loan_Collateral_Mapping)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Loan_Application_Collateral)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Loan_Guarantor)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application>()
                .HasMany(e => e.tbl_Risk_Assessment)
                .WithRequired(e => e.tbl_Loan_Application)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application_Detail>()
                .Property(e => e.ProposedAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Application_Detail>()
                .Property(e => e.ApprovedAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Application_Detail>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Loan_Application_Detail)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application_Detail>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Loan_Application_Detail)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application_Detail_Status>()
                .HasMany(e => e.tbl_Loan_Application_Detail)
                .WithRequired(e => e.tbl_Loan_Application_Detail_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Application_Status>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_Loan_Application_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.TeamMISCode)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.PrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.ApprovedAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.EquityContribution)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.OutstandingPrincipal)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.OutstandingInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Archive>()
                .Property(e => e.ScheduledPrepaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Camsol>()
                .Property(e => e.AmountAffected)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Contingent>()
                .Property(e => e.TeamMISCode)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Loan_Contingent>()
                .Property(e => e.ContingentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Covenant_Detail>()
                .Property(e => e.CovenantAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Covenant_Type>()
                .HasMany(e => e.tbl_Loan_Covenant_Detail)
                .WithRequired(e => e.tbl_Loan_Covenant_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Fee>()
                .Property(e => e.FeeRateValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Fee>()
                .Property(e => e.FeeDependentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Fee>()
                .Property(e => e.FeeAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Fee>()
                .HasMany(e => e.tbl_Loan_Fee_Schedule)
                .WithRequired(e => e.tbl_Loan_Fee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Fee_Schedule>()
                .Property(e => e.FeeAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Force_Debit>()
                .Property(e => e.DebitAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Force_Debit>()
                .Property(e => e.CreditAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Past_Due>()
                .Property(e => e.DebitAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Past_Due>()
                .Property(e => e.CreditAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Preliminary_Evaluation>()
                .Property(e => e.LoanAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_PrudentialGuideline>()
                .HasMany(e => e.tbl_Loan)
                .WithOptional(e => e.tbl_Loan_PrudentialGuideline)
                .HasForeignKey(e => e.InternalPrudentialGuidelineStatusId);

            modelBuilder.Entity<tbl_Loan_PrudentialGuideline>()
                .HasMany(e => e.tbl_Loan1)
                .WithOptional(e => e.tbl_Loan_PrudentialGuideline1)
                .HasForeignKey(e => e.ExternalPrudentialGuidelineStatusId);

            modelBuilder.Entity<tbl_Loan_PrudentialGuideline>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithOptional(e => e.tbl_Loan_PrudentialGuideline)
                .HasForeignKey(e => e.InternalPrudentialGuidelineStatusId);

            modelBuilder.Entity<tbl_Loan_PrudentialGuideline>()
                .HasMany(e => e.tbl_Loan_Archive1)
                .WithOptional(e => e.tbl_Loan_PrudentialGuideline1)
                .HasForeignKey(e => e.ExternalPrudentialGuidelineStatusId);

            modelBuilder.Entity<tbl_Loan_PrudentialGuideline>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithOptional(e => e.tbl_Loan_PrudentialGuideline)
                .HasForeignKey(e => e.ExternalPrudentialGuidelineStatusId);

            modelBuilder.Entity<tbl_Loan_PrudentialGuideline>()
                .HasMany(e => e.tbl_Loan_Revolving1)
                .WithOptional(e => e.tbl_Loan_PrudentialGuideline1)
                .HasForeignKey(e => e.InternalPrudentialGuidelineStatusId);

            modelBuilder.Entity<tbl_Loan_Review_Operation>()
                .Property(e => e.InterateRate)
                .HasPrecision(18, 0);

            modelBuilder.Entity<tbl_Loan_Review_Operation>()
                .Property(e => e.Prepayment)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Review_Operation>()
                .Property(e => e.OverDraftTopup)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Review_Operation>()
                .Property(e => e.Fee_Charges)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Revolving>()
                .Property(e => e.TeamMISCode)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Loan_Revolving>()
                .Property(e => e.OverdraftLimit)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Category>()
                .HasMany(e => e.tbl_Loan_Schedule_Type)
                .WithRequired(e => e.tbl_Loan_Schedule_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.OpeningBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.StartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.DailyPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.DailyInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.DailyPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.ClosingBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.EndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AccruedInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedOpeningBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedStartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedDailyPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedDailyInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedDailyPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedClosingBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedEndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.AmortisedAccruedInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.Amortised_AmortisedCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.DiscountPremium)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.UnEarnedFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.EarnedFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily>()
                .Property(e => e.BallonAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.OpeningBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.StartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.DailyPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.DailyInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.DailyPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.ClosingBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.EndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AccruedInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedOpeningBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedStartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedDailyPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedDailyInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedDailyPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedClosingBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedEndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.AmortisedAccruedInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.Amortised_AmortisedCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.DiscountPremium)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.UnEarnedFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.EarnedFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Archive>()
                .Property(e => e.BallonAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.OpeningBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.StartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.DailyPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.DailyInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.DailyPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.ClosingBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.EndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AccruedInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedOpeningBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedStartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedDailyPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedDailyInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedDailyPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedClosingBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedEndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.AmortisedAccruedInterest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.Amortised_AmortisedCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.DiscountPremium)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.UnEarnedFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.EarnedFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Daily_Temp>()
                .Property(e => e.BallonAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Irregular_Input>()
                .Property(e => e.PaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.StartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.PeriodPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.PeriodInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.PeriodPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.EndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.AmortisedStartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.AmortisedPeriodPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.AmortisedPeriodInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.AmortisedPeriodPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic>()
                .Property(e => e.AmortisedEndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.StartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.PeriodPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.PeriodInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.PeriodPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.EndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.AmortisedStartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.AmortisedPeriodPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.AmortisedPeriodInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.AmortisedPeriodPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Archive>()
                .Property(e => e.AmortisedEndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.StartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.PeriodPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.PeriodInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.PeriodPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.EndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.AmortisedStartPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.AmortisedPeriodPaymentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.AmortisedPeriodInterestAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.AmortisedPeriodPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Periodic_Temp>()
                .Property(e => e.AmortisedEndPrincipalAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Loan_Schedule_Type>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Loan_Schedule_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Schedule_Type>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Loan_Schedule_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Schedule_Type>()
                .HasMany(e => e.tbl_Loan_Schedule_Type_Product_Type_Mapping)
                .WithRequired(e => e.tbl_Loan_Schedule_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Status>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Loan_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Status>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Loan_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Status>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Loan_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Status>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Loan_Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Transaction_Type>()
                .HasMany(e => e.tbl_Daily_Accrual)
                .WithRequired(e => e.tbl_Loan_Transaction_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Transaction_Type>()
                .HasMany(e => e.tbl_Loan_Force_Debit)
                .WithRequired(e => e.tbl_Loan_Transaction_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Transaction_Type>()
                .HasMany(e => e.tbl_Loan_Past_Due)
                .WithRequired(e => e.tbl_Loan_Transaction_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Type>()
                .HasMany(e => e.tbl_Loan)
                .WithRequired(e => e.tbl_Loan_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Type>()
                .HasMany(e => e.tbl_Loan_Application)
                .WithRequired(e => e.tbl_Loan_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Type>()
                .HasMany(e => e.tbl_Loan_Archive)
                .WithRequired(e => e.tbl_Loan_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Type>()
                .HasMany(e => e.tbl_Loan_Contingent)
                .WithRequired(e => e.tbl_Loan_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Type>()
                .HasMany(e => e.tbl_Loan_Preliminary_Evaluation)
                .WithRequired(e => e.tbl_Loan_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Loan_Type>()
                .HasMany(e => e.tbl_Loan_Revolving)
                .WithRequired(e => e.tbl_Loan_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_LoanApplication_Collateral_Mapping>()
                .HasOptional(e => e.tbl_LoanApplication_Collateral_Mapping1)
                .WithRequired(e => e.tbl_LoanApplication_Collateral_Mapping2);

            modelBuilder.Entity<tbl_MachineValue_Base>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Risk_Assessment_Index>()
                .Property(e => e.Weight)
                .HasPrecision(18, 4);

            modelBuilder.Entity<tbl_Risk_Assessment_Index_Type>()
                .HasMany(e => e.tbl_Risk_Assessment_Index)
                .WithRequired(e => e.tbl_Risk_Assessment_Index_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Risk_Assessment_Title>()
                .HasMany(e => e.tbl_Risk_Assessment)
                .WithRequired(e => e.tbl_Risk_Assessment_Title)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Risk_Assessment_Title>()
                .HasMany(e => e.tbl_Risk_Assessment_Index)
                .WithRequired(e => e.tbl_Risk_Assessment_Title)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Risk_Rating>()
                .Property(e => e.RatesDescription)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Solicitor>()
                .HasMany(e => e.tbl_Solicitor_State_Mapping)
                .WithRequired(e => e.tbl_Solicitor)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Solicitor_State_Mapping>()
                .Property(e => e.CollateralSearchChargeAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Approval>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA_Lien>()
                .Property(e => e.LienCreditAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_CASA_Lien>()
                .Property(e => e.LienDebitAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_COT>()
                .Property(e => e.COTAccountAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_COT>()
                .Property(e => e.COTCreatedBy)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Account_Category>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Account_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Account_Category>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Account_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Account_Category>()
                .HasMany(e => e.tbl_Account_Type)
                .WithRequired(e => e.tbl_Account_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Account_Category>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Account_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Account_Category>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Account_Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Account_Type>()
                .HasMany(e => e.tbl_Chart_Of_Account)
                .WithRequired(e => e.tbl_Account_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Account_Type>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account)
                .WithRequired(e => e.tbl_Account_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Charge_Range>()
                .Property(e => e.Minimum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Charge_Range>()
                .Property(e => e.Maximum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Charge_Range>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Charge_Fee)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Fee)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Finance_Transaction)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Product)
                .WithOptional(e => e.tbl_Chart_Of_Account)
                .HasForeignKey(e => e.PrincipalBalanceGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Product1)
                .WithOptional(e => e.tbl_Chart_Of_Account1)
                .HasForeignKey(e => e.InterestReceivablePayableGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Product2)
                .WithOptional(e => e.tbl_Chart_Of_Account2)
                .HasForeignKey(e => e.InterestIncomeExpenseGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Product3)
                .WithOptional(e => e.tbl_Chart_Of_Account3)
                .HasForeignKey(e => e.PremiumDiscountGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Product4)
                .WithOptional(e => e.tbl_Chart_Of_Account4)
                .HasForeignKey(e => e.DormantGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Product5)
                .WithOptional(e => e.tbl_Chart_Of_Account5)
                .HasForeignKey(e => e.OverdrawnGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Tax)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Collateral_Type)
                .WithOptional(e => e.tbl_Chart_Of_Account)
                .HasForeignKey(e => e.ChargeGLAccountId);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Chart_Of_Account_Currency)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Product)
                .WithOptional(e => e.tbl_Chart_Of_Account)
                .HasForeignKey(e => e.PrincipalBalanceGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Product1)
                .WithOptional(e => e.tbl_Chart_Of_Account1)
                .HasForeignKey(e => e.InterestReceivablePayableGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Product2)
                .WithOptional(e => e.tbl_Chart_Of_Account2)
                .HasForeignKey(e => e.InterestIncomeExpenseGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Product3)
                .WithOptional(e => e.tbl_Chart_Of_Account3)
                .HasForeignKey(e => e.PremiumDiscountGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Product4)
                .WithOptional(e => e.tbl_Chart_Of_Account4)
                .HasForeignKey(e => e.DormantGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Product5)
                .WithOptional(e => e.tbl_Chart_Of_Account5)
                .HasForeignKey(e => e.OverdrawnGL);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Charge_Fee)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Fee)
                .WithRequired(e => e.tbl_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Chart_Of_Account_Class>()
                .HasMany(e => e.tbl_Chart_Of_Account)
                .WithRequired(e => e.tbl_Chart_Of_Account_Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Financial_Statement_Caption>()
                .HasMany(e => e.tbl_Chart_Of_Account)
                .WithRequired(e => e.tbl_Financial_Statement_Caption)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Financial_Statement_Caption>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account)
                .WithRequired(e => e.tbl_Financial_Statement_Caption)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Financial_Statement_Type>()
                .HasMany(e => e.tbl_Customer_FS_Caption)
                .WithRequired(e => e.tbl_Financial_Statement_Type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Charge_Fee>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Chart_Of_Account>()
                .HasMany(e => e.tbl_Temp_Chart_Of_Account_Currency)
                .WithRequired(e => e.tbl_Temp_Chart_Of_Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Casa>()
                .Property(e => e.AvailableBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Casa>()
                .Property(e => e.ExistingLienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Casa>()
                .Property(e => e.LienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Casa>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Casa>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .Property(e => e.CamRefNumber)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Casa)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Documents)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Marketable_Security)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Deposit)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Gaurantee)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Miscellaneous)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Customer>()
                .HasMany(e => e.tbl_Temp_Collateral_Immovable_Property)
                .WithRequired(e => e.tbl_Temp_Collateral_Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Deposit>()
                .Property(e => e.ExistingLienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Deposit>()
                .Property(e => e.LienAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Deposit>()
                .Property(e => e.AvailableBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Deposit>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Deposit>()
                .Property(e => e.MaturityAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Deposit>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Documents>()
                .Property(e => e.DocumentCategory)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Documents>()
                .Property(e => e.DocumentType)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Documents>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Gaurantee>()
                .Property(e => e.InstitutionName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Gaurantee>()
                .Property(e => e.GuarantorAddress)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Gaurantee>()
                .Property(e => e.GuaranteeValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Gaurantee>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.PropertyAddress)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.OpenMarketValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.CollateralValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.ForcedSaleValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.StampToCover)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.OriginalValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.AvailableValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.CollateralUsableAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.Longitude)
                .HasPrecision(12, 9);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .Property(e => e.Latitude)
                .HasPrecision(12, 9);

            modelBuilder.Entity<tbl_Temp_Collateral_Immovable_Property>()
                .HasOptional(e => e.tbl_Temp_Collateral_Immovable_Property1)
                .WithRequired(e => e.tbl_Temp_Collateral_Immovable_Property2);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.DealAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.LienUsableAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.IssuerName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.IssuerReferenceNumber)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.UnitValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Marketable_Security>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Miscellaneous>()
                .Property(e => e.SecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Miscellaneous>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Miscellaneous>()
                .HasMany(e => e.tbl_Temp_Collateral_Miscellaneous_Notes)
                .WithOptional(e => e.tbl_Temp_Collateral_Miscellaneous)
                .HasForeignKey(e => e.MiscellaneousId);

            modelBuilder.Entity<tbl_Temp_Collateral_Plant_And_Equipment>()
                .Property(e => e.MachineName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Plant_And_Equipment>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Plant_And_Equipment>()
                .Property(e => e.YearOfManufacture)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Temp_Collateral_Plant_And_Equipment>()
                .Property(e => e.YearOfPurchase)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Temp_Collateral_Plant_And_Equipment>()
                .Property(e => e.ReplacementValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Policy>()
                .Property(e => e.PremiumAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Policy>()
                .Property(e => e.PolicyAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Policy>()
                .Property(e => e.InsuranceCompanyName)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Temp_Collateral_Policy>()
                .Property(e => e.InsurerAddress)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Policy>()
                .Property(e => e.InsurerDetails)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Policy>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_PreciousMetal>()
                .Property(e => e.PreciousMetalName)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_PreciousMetal>()
                .Property(e => e.MetalType)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_PreciousMetal>()
                .Property(e => e.ValuationAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_PreciousMetal>()
                .Property(e => e.PreciousMetalForm)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_PreciousMetal>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Vehicle>()
                .Property(e => e.ResaleValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Vehicle>()
                .Property(e => e.LastValuationAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Vehicle>()
                .Property(e => e.InvoiceValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Vehicle>()
                .Property(e => e.Remark)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Temp_Product>()
                .HasMany(e => e.tbl_Temp_Product_CollateralType)
                .WithRequired(e => e.tbl_Temp_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Product>()
                .HasMany(e => e.tbl_Temp_Product_Currency)
                .WithRequired(e => e.tbl_Temp_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Product>()
                .HasMany(e => e.tbl_Temp_Product_Charge_Fee)
                .WithRequired(e => e.tbl_Temp_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Product>()
                .HasMany(e => e.tbl_Temp_Product_Fee)
                .WithRequired(e => e.tbl_Temp_Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Product_Charge_Fee>()
                .Property(e => e.RateValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Product_Charge_Fee>()
                .Property(e => e.DependentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Product_Fee>()
                .Property(e => e.RateValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Product_Fee>()
                .Property(e => e.DependentAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Staff>()
                .Property(e => e.Gender)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Temp_Staff>()
                .Property(e => e.GenderOfNOK)
                .IsFixedLength();

            modelBuilder.Entity<tbl_Deal_Classification>()
                .HasMany(e => e.tbl_Product_Type)
                .WithRequired(e => e.tbl_Deal_Classification)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_Temp_Collateral_Stock>()
                .Property(e => e.MarketPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Stock>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Stock>()
                .Property(e => e.SharesSecurityValue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<tbl_Temp_Collateral_Stock>()
                .Property(e => e.ShareValueAmountToUse)
                .HasPrecision(19, 4);
        }
    }
}
