using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerRepository
    {
        CustomerViewModels GetCustomer(int custormerId);
        IEnumerable<CustomerViewModels> GetCustomerInGroupByGroupId(int groupId);
        IEnumerable<CustomerViewModels> GetCustomerGeneralInfoByLoanId(int loanApplicationId);

        IEnumerable<CustomerViewModels> GetCustomerByBranchId(int branchId);

        IEnumerable<CustomerViewModels> GetCustomerByCompanyId(int companyId);

        IEnumerable<CustomerTypeViewModels> GetCustomerType();

        IEnumerable<CustomerSupplierTypeViewModels> GetClientSupplierType();

        IEnumerable<CustomerIdentificationModeTypeViewModels> GetIdentificationMode();

        IEnumerable<CompanyDirectorTypeViewModels> GetDirectorsTypes();

        IEnumerable<CustomerAddressTypeViewModels> GetCustomerAddressType();

        IEnumerable<CustomerRiskRatingViewModels> GetCustomerRiskRating();

        IEnumerable<CustomerViewModels> GetCustomerByTypeId(int customerTypeId);


        dynamic GetCustomerAndType(int custormerId);

        dynamic GetCustomerRating(int custormerId);

        bool AddCustomer(CustomerViewModels entity);

        //Task<bool> UpdateCustomer(int customerId, CustomerViewModels entity);
        bool UpdateCustomer(int customerId, CustomerViewModels entity);

        Task<bool> DeleteCustomer(int customerId, UserInfo user);

        bool DeleteChild(int childId);

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
        IQueryable<CustomerSearchItemViewModels> CustomerSearchRealTime(int companyId, string search);
        IEnumerable<CustomerViewModels> SearchRandomCustomerBySearchQuery(string searchQuery);
        IEnumerable<KYCDocumentTypeViewModel> GetKYCDocumentType();


        #region Single Customer Information By CustomerID
        CustomerViewModels GetSingleCustomerGeneralInfo(string customerCode);
        CustomerCompanyInfomationViewModels GetSingleCustomerCompanyInfo(int customerId);
        IEnumerable<CustomerAddressViewModels> GetSingleCustomerAddressInfo(int customerId);
        IEnumerable<CustomerPhoneContactViewModels> GetSingleCustomerPhoneContactInfo(int customerId);
        IEnumerable<CustomerBvnViewModels> GetSingleCustomerBVNInfo(int customerId);
        IEnumerable<CustomerIdentificationViewModels> GetSingleCustomerIdentificationInfo(int customerId);
        IEnumerable<CustomerEmploymentHistoryViewModels> GetSingleCustomerEmploymentHistoryInfo(int customerId);
        IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerDirectorInfo(int customerId, short directorTypeId);
        IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId, short customerTypeId);
        IEnumerable<CustomerClientOrSupplierViewModels> GetSingleCustomerClientOrSupplierInfo(int customerId, short clientTypeId);
        IEnumerable<CustomerChildrenViewModel> GetSingleCustomerChildrenInfo(int customerId);
        IEnumerable<CustomerCompanyBeneficiaryViewModels> GetShareholderUltimateBeneficial(int companyDirectorId);
        IEnumerable<CasaViewModel> GetCustomerCASAInformation(int customerId);
        IEnumerable<CustomerNextOfKinViewModels> GetSingleCustomerNextOfKinInfo(int customerId);
        #endregion

        #region  Customer Information Validation
        bool ValidateCustomerCode(string customerCode);
        bool ValidateCustomerBVN(int customerId, string customerBvn);
        bool ValidateCustomerRCnumber(int customerId, string rcNumber);
        bool ValidateCustomerTIN(int customerId, string tin);
        bool ValidateCustomerEmail(int customerId, string email);

        //TBL_CUSTOMER_CLIENT_SUPPLIER
        bool ValidateClientSupplierEmail(int customerId, string email);
        bool ValidateClientSupplierRCnumber(int customerId, string rcNumber);
        bool ValidateClientSupplierTIN(int customerId, string taxNumber);
        #endregion
    }
}