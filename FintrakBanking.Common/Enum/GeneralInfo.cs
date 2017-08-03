using System;

namespace FintrakBanking.Common.Enum
{ 
    public  enum AuditTypeEnum 
    {
        LoggedIn = 1, LoggedOut = 2, StaffApproved = 3, StaffUpdated = 4,
        FeeAdded = 5, ProductCollateralAdded = 6, ProductCollateralDeleted = 7,
        CustomerGroupAdded = 8, CustomerGroupDeleted = 9, CustomerGroupUpdated = 10,
        CustomerGroupMappingAdded = 11, CustomerGroupMappingDeleted = 12, CustomerGroupMappingUpdated = 13,
        CollateralTypeAdded = 14, CollateralTypeDeleted = 15, CollateralTypeUpdated = 16,
        CollateralCategoryAdded = 14, CollateralCategoryDeleted = 15, CollateralCategoryUpdated = 16,
 
        CustomerAdded = 17, CustomerUpdated = 18, CustomerDeleted = 19,
        CustomerAddressAdded = 20, CustomerAddressUpdated = 21, CustomerAddressDeleted = 22,
        StaffDeleted = 23, 

        CustomerFSCaptionGroupAdded = 24, CustomerFSCaptionGroupUpdated = 25,

        AccountCategoryAdded = 24, ChartOfAccountDeleted = 25, ChartOfAccountUpdated = 26, ChartOfAccountInitiated = 27,
        ProductTypeAdded = 28, ProductTypeDeleted = 29, ProductTypeUpdated = 30,
        ProductGroupAdded = 31, ProductGroupDeleted = 32, ProductGroupUpdated = 33,
        ProductAdded = 34, ProductUpdated =35,
        AccountTypeAdded = 36, AccountTypeUpdated = 37,
        UserAdded = 38 , UserUpdated = 39, 
        UserGroupAdded = 40, UserGroupUpdated = 41,
        BranchAdded = 42, BranchUpdated = 43, BranchDeleted = 44,
        RiskAssessmentIndexAdd = 45, RiskAssessmentIndexUpdate = 46, RiskAssessmentIndexDelete = 47,
        CustomerFSCaptionAdded = 48, CustomerFSCaptionUpdated = 49, CustomerFSCaptionDeleted = 50,
        CustomerFSCaptionDetailAdded = 51, CustomerFSCaptionDetailUpdated = 52, CustomerFSCaptionDetailDeleted = 53,
        RiskAssessmentTitleAdd = 54, RiskAssessmentTitleUpdate = 55, RiskAssessmentTitleDelete = 56,
        CustomFieldAdd = 57, CustomFieldUpdate = 58, CustomFieldDelete = 59,
        LoanCovenantDetailAdd = 68, LoanCovenantDetailUpdate = 69, LoanCovenantDetailDelete= 70,
        LoanCovenantTypeAdd = 71, LoanCovenantTypeUpdate = 72, LoanCovenantTypeDelete = 73,
        CustomerFSRatioCaptionAdded = 60, CustomerFSRatioCaptionUpdated = 61, CustomerFSRatioCaptionDeleted = 62,
        CustomerFSRatioDetailAdded = 63, CustomerFSRatioDetailUpdated = 64, CustomerFSRatioDetailDeleted = 65,
        CurrencyRateAdded = 66, CurrencyRateUpdated =67, 
        CustomFieldDatadAdd = 74, CustomFieldDataUpdate = 75, CustomFieldDataDelete = 76, 
        LoanApplication = 74,     
        LoanChecklistAdded = 75, LoanChecklistUpdated = 76, LoanChecklistDeleted = 77,
        LoanChecklistDefinitionAdded = 78, LoanChecklistDefinitionUpdated = 79, LoanChecklistDefinitionDeleted = 80,
        LimitAdded = 81, LimitUpdated = 82, LimitDeleted = 83,
        LimitDetailAdded = 84, LimitDetailUpdated = 85, LimitDetailDeleted = 86,
        ChecklistItemAdded = 87, ChecklistItemUpdated = 88, ChecklistItemDeleted = 89,

        ApprovalGroupMappingAdded = 93, ApprovalGroupMappingUpdated = 94, ApprovalGroupMappingDeleted = 95,
        ApprovalGroupAdded = 90, ApprovalGroupUpdated = 91, ApprovalGroupDeleted = 92,
        ApprovalLevelAdded = 96, ApprovalLevelUpdated = 97, ApprovalLevelDeleted =98,
        ApprovalLevelStaffAdded = 99, ApprovalLevelStaffUpdated = 100, ApprovalLevelStaffDeleted = 101,
        ApprovalStatusUpdated = 102,
        ProductPriceIndexAdded = 103, ProductPriceIndexUpdated = 104, ProductPriceIndexDeleted = 105,
        CreateStaffInitiated = 107, AccountApproved = 108, ProductApproved = 109,
        ProductFeeInitiated = 112, ProductFeeAdded = 113, ProductFeeUpdated =114, ProductFeeDeleted = 115,
        TaxAdded = 116, TaxUpdated = 117, TaxDeleted = 118,
         
        ChargeFeeAdded = 119, ChargeFeeUpdated = 120, ChargeFeeDeleted = 121,
    };


    public static class General
    {
        public static DateTime DefaultDate = new DateTime(1900, 1, 1);
    }

}