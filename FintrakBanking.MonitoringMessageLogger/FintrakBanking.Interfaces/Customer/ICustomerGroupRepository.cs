using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerGroupRepository
    {
         IEnumerable<KYCItemViewModel> GetKYCItems(int companyId);
         bool AddKycItem(KYCItemViewModel entity);
        bool UpdatedKycItem(int KYCItemId ,KYCItemViewModel entity) ;

        #region tbl_Customer Group Repository

        bool AddCustomerGroup(CustomerGroupViewModel entity);

        bool AddTempCustomerGroup(CustomerGroupViewModel entity);

        IEnumerable<CustomerGroupViewModel> GetCustomerGroup();

        CustomerGroupViewModel GetCustomerGroupByCustomerId(int customerGroupId);

        bool UpdateCustomerGroup(int groupId, CustomerGroupViewModel entity);

        bool UpdateCustomerGroupForApproval(int groupId, CustomerGroupViewModel entity);

        bool DeleteCustomerGroup(int groupId, UserInfo user);

        IEnumerable<CustomerGroupViewModel> GetCustomerGroupsAwaitingApprovals(int staffId, int companyId);

        bool GoForApproval(ApprovalViewModel entity);
        bool GoForGroupMappingApproval(ApprovalViewModel entity);

        IEnumerable<CustomerGroupViewModel> CustomerGroupSearch(string search);

        IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapsAwaitingApprovals(int staffId, int companyId);

        #endregion tbl_Customer Group Repository

        #region tbl_Customer Group Mapping repository
        bool DoesGroupNameExist(string groupName, string groupCode);

        bool AddCustomerGroupMapping(CustomerGroupMappingViewModel entity);

        bool AddTempCustomerGroupMapping(CustomerGroupMappingViewModel entity);

        //bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> customerGroups);
        bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> customerGroups, int createdBy, short userBranchId, int companyId);

        bool AddCustomerGroupRelationshipTypes(LookupViewModel model);

        IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping();

        IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMappingByGroupId(int customerGroupId);

        CustomerGroupMappingViewModel GetCustomerGroupMappingByGroupMapId(int groupMapId);

        IEnumerable<LookupViewModel> GetCustomerGroupRelationshipTypes();

        bool UpdateCustomerGroupMapping(int groupMapId, CustomerGroupMappingViewModel entity);

        bool UpdateCustomerGroupMappingForApproval(int groupMapId, CustomerGroupMappingViewModel entity);

        bool DeleteCustomerGroupMapping(int groupMapId, UserInfo user);

        IEnumerable<GroupCustomerMembersViewModel> GetGroupMembersByGroupId(int customerGroupId, int companyId);
        IQueryable<CustomerGroupViewModel> SearchForCustomerGroupRealtime(int companyId, string searchQuery);
        CustomerGroupViewModel GetCustomerGroupDetailsByGroupId(int customerGroupId);
        IEnumerable<CustomerGroupViewModel> SearchForCustomerGroup(int companyId, string searchQuery);
        IEnumerable<CustomerGroupMappingViewModel> GetAllCustomerGroupMappingByGroupId(int customerGroupId);
        #endregion tbl_Customer Group Mapping repository
        List<CurrentCustomerExposure> GetGroupExposureByCustomerId(int customerId, int companyId);
        List<CurrentCustomerExposure> GetGroupExposureByGroupId(int customerGroupId, int companyId);

        IEnumerable<CustomerGroupViewModel> GetAllTempCustomerGroups();
        IEnumerable<CustomerGroupMappingViewModel> GetTempCustomerGroupMappingByGroupId(int customerGroupId);
    }
}