using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.CreditLimitValidations;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerRepository
    {
        CustomerViewModels GetCustomer(int custormerId);
        IEnumerable<CustomerViewModels> GetAllProspectiveCustomer();
        IEnumerable<CustomerViewModels> GetCustomerInGroupByGroupId(int groupId);
        IEnumerable<CustomerViewModels> GetCustomerGeneralInfoByLoanId(int loanApplicationId);

        IEnumerable<CustomerViewModels> GetCustomerByBranchId(int branchId);

        IEnumerable<CustomerViewModels> GetCustomerByCompanyId(int companyId);

        IEnumerable<CustomerTypeViewModels> GetCustomerType();
        IEnumerable<CorporateCustomerTypeViewModels> GetCorporateCustomerType();
        IEnumerable<CustomerTypeViewModels> GetCustomerTypeWithHybrid();

        IEnumerable<CustomerSupplierTypeViewModels> GetClientSupplierType();

        IEnumerable<CustomerIdentificationModeTypeViewModels> GetIdentificationMode();

        IEnumerable<CompanyDirectorTypeViewModels> GetDirectorsTypes();

        IEnumerable<CustomerAddressTypeViewModels> GetCustomerAddressType();

        IEnumerable<CustomerRiskRatingViewModels> GetCustomerRiskRating();

        IEnumerable<CustomerViewModels> GetCustomerByTypeId(int customerTypeId);

        IEnumerable<GroupCustomerMembersViewModel> GetCustomerAndType(int custormerId);

        dynamic GetCustomerRating(int custormerId);

        string AddCustomer(CustomerViewModels entity);

        //Task<bool> UpdateCustomer(int customerId, CustomerViewModels entity);
        bool UpdateCustomer(int customerId, CustomerViewModels entity);

        Task<bool> DeleteCustomer(int customerId, UserInfo user);

        bool DeleteChild(int childId, UserInfo user);

        bool DeleteUltimateBeneficial(int companyBeneficialId, UserInfo user);

        bool AddCustomerIdentification(CustomerIdentificationViewModels entity);

        bool AddCustomerEmploymentHistory(CustomerEmploymentHistoryViewModels entity);

        bool AddCustomerBvn(CustomerBvnViewModels entity);

        bool AddCustomerClientSupplier(CustomerClientOrSupplierViewModels entity);

        bool AddCustomerCompanyDirector(CustomerCompanyDirectorsViewModels entity);

       

        bool AddCustomerAddresses(CustomerAddressViewModels entity);

        bool AddCustomerPhoneContact(CustomerPhoneContactViewModels entity);

        bool AddCustomerCompanyInfomation(CustomerCompanyInfomationViewModels entity);

        bool AddCustomerChildren(List<CustomerChildrenViewModel> models, int staffId, short BranchId);

        bool AddCustomerNextOfKin(CustomerNextOfKinViewModels entity);

        IEnumerable<CustomerViewModels> CustomerSearch(int companyId, string search);
        IEnumerable<CustomerViewModels> CustomerSearch(int companyId, CustomerSearchItemViewModels search);
        IEnumerable<CustomerEligibilityViewModels> GetCustomerIBLEligibility(int customerId);
        bool updateIBLEligibility(int iblEligibilityId);
        CustomerEligibilityViewModels EligibilitySearch(int companyId, CustomerEligibilityViewModels search);
        IQueryable<CustomerSearchItemViewModels> CustomerSearchRealTime(int companyId, string search);
        IEnumerable<CustomerViewModels> SearchRandomCustomerBySearchQuery(string searchQuery);
        IEnumerable<CustomerViewModels> SearchRandomCustomersBySearchQuery(string searchQuery);

        IEnumerable<KYCDocumentTypeViewModel> GetKYCDocumentType();
        IEnumerable<LookupViewModel> GetAllCRMSLegalStatus();
        IEnumerable<LookupViewModel> GetAllCRMSCompanySize();
        IEnumerable<LookupViewModel> GetAllCRMSRelationshipType();

        IEnumerable<LookupViewModel> GetAllCRMSLegalStatusByType(int type);
        IEnumerable<LookupViewModel> GetAllCRMSCompanySizeByType(int type);
        IEnumerable<LookupViewModel> GetAllCRMSRelationshipTypeByType(int type);


        #region Single Customer Information By CustomerID
        CustomerViewModels GetSingleCustomerGeneralInfo(string customerCode);
        CustomerViewModels GetSingleCustomerGeneralInfoByCustomerId(int customerId);
        CustomerCompanyInfomationViewModels GetSingleCustomerCompanyInfo(int customerId);
        IEnumerable<CustomerAddressViewModels> GetSingleCustomerAddressInfo(int customerId);
        IEnumerable<CustomerPhoneContactViewModels> GetSingleCustomerPhoneContactInfo(int customerId);
        IEnumerable<CustomerBvnViewModels> GetSingleCustomerBVNInfo(int customerId);
        IEnumerable<CustomerIdentificationViewModels> GetSingleCustomerIdentificationInfo(int customerId);
        IEnumerable<CustomerEmploymentHistoryViewModels> GetSingleCustomerEmploymentHistoryInfo(int customerId);
        CustomerEmploymentHistoryViewModels GetSingleCustomerRelatedEmployer(int customerId);
        IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerDirectorInfo(int customerId, short directorTypeId);
        IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId, short customerTypeId);
        IEnumerable<CustomerClientOrSupplierViewModels> GetSingleCustomerClientOrSupplierInfo(int customerId, short clientTypeId);
        IEnumerable<CustomerChildrenViewModel> GetSingleCustomerChildrenInfo(int customerId);
        IEnumerable<CustomerCompanyBeneficiaryViewModels> GetShareholderUltimateBeneficial(int companyDirectorId);
        IEnumerable<CasaViewModel> GetCustomerCASAInformation(int customerId);
        IEnumerable<CasaViewModel> GetCustomerCASAInformation(string customerCode);
        IEnumerable<CustomerNextOfKinViewModels> GetSingleCustomerNextOfKinInfo(int customerId);
        #endregion

        #region  Customer Information Validation
        bool ValidateCustomerCode(string customerCode);
        bool ValidateCustomerBVN(int customerId, string customerBvn);
        bool ValidateCustomerRCnumber(int customerId, string rcNumber);
        bool ValidateCustomerTIN(int customerId, string tin);
        bool ValidateCustomerEmail(int customerId, string email);
        bool ValidateRelatedPartyEntry(int customerId, int companyDirectorId);

        //TBL_CUSTOMER_CLIENT_SUPPLIER
        bool ValidateClientSupplierEmail(int customerId, string email);
        bool ValidateClientSupplierRCnumber(int customerId, string rcNumber);
        bool ValidateClientSupplierTIN(int customerId, string taxNumber);
        bool CustomerInformationCompleted(int customerId, UserInfo user);

        //TEMP tables 
        bool ValidateCustomerModification(int customerId);
        bool ValidateModifiedCustomerRecord(int customerId);
        bool ValidateModifiedCompanyRecord(int customerId);
        bool ValidateModifiedAddressRecord(int customerId);
        bool ValidateModifiedPhoneRecord(int customerId);
        #endregion
        IEnumerable<CustomerInformationApprovalViemModel> GetAllCustomerInformationAwaitingApproval(int staffId, int companyId);
        int GoForApproval(ApprovalViewModel entity);

        #region Customer Temporary Information 
        CustomerViewModels GetSingleCustomerGeneralInfoByCustomerId(int customerId, int targetId);
        CustomerCompanyInfomationViewModels GetSingleCustomerCompanyInfo(int customerId, int targetId);
        IEnumerable<CustomerAddressViewModels> GetSingleCustomerAddressInfo(int customerId, int targetId);
        IEnumerable<CustomerClientOrSupplierViewModels> GetSingleCustomerClientOrSupplierInfo(int customerId, short clientTypeId, int targetId);
        IEnumerable<CustomerNextOfKinViewModels> GetSingleCustomerNextOfKinInfo(int customerId, int targetId);
        IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerDirectorInfo(int customerId, short directorTypeId, int targetId);
        IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId, short customerTypeId, int targetId);
        IEnumerable<CustomerPhoneContactViewModels> GetSingleCustomerPhoneContactInfo(int customerId, int targetId);
        IEnumerable<CustomerEmploymentHistoryViewModels> GetSingleCustomerEmploymentHistoryInfo(int customerId, int targetId);
        #endregion

        IEnumerable<CustomerRelatedPartyViewModel> GetCustomerRelatedParty(int customerId);
        bool DeleteEmployment(int placeOfWorkId, UserInfo user);
        bool DeleteNextOfKin(int nextOfKinId, UserInfo user);

        bool Deletcontact(int phoneContactId, UserInfo user);
        bool DeleteRelatedParty(int relatedPartyId, UserInfo user);
        bool Deleteaddress(int addressId, UserInfo user);
        bool AddUpdateCustomerRelatedParty(CustomerRelatedPartyViewModel entity);
        bool UpdatePropectToCustomer(int customerId, CustomerViewModels entity);
        IEnumerable<CustomerRelatedDirectorViewModel> DirectorRelatedCustomer(string bvn);
        IEnumerable<CustomerViewModels> GetCustomerGeneralInfoByLMSLoanId(int loanApplicationId);
        IEnumerable<CustomerViewModels> SearchRandomSingleCustomersBySearchQuery(string searchQuery);
        IEnumerable<CustomerViewModels> SearchRandomSingleCorporateCustomersBySearchQuery(string searchQuery);
        IEnumerable<CustomerViewModels> SearchRandomGroupCustomersBySearchQuery(string searchQuery);
        IEnumerable<CustomerViewModels> SearchGroupCustomersBySearchQuery(string searchQuery, int groupId);
        bool GetPoliticallyExposedPerson(string customerCode);
        void UpdateCustomerCollateralId(string customerCode);
        bool refreshCustomerAccount(int customerCode);
        bool UpdateCustomerInformation(string customerCode, string accountNumber, int createdBy);
        bool saveBulkFsCaptionEntries(List<MultipleFsCaptionOutputViewModel> models, UserInfo user);
        Tuple<List<MultipleFsCaptionOutputViewModel>, bool> preBulkFsCaption(byte[] file, UserInfo user, bool isFinal, int customerId);
    }
}