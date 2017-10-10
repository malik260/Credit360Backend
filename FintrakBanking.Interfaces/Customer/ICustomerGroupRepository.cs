using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerGroupRepository
    {
        //IQueryable<KYCItemViewModel> GetKYCItems(int companyId);
        //bool AddKycItem(KYCItemViewModel entity);
        //bool UpdatedKycItem(int KYCItemId ,KYCItemViewModel entity) ;

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

        #endregion tbl_Customer Group Repository

        #region tbl_Customer Group Mapping repository

        bool AddCustomerGroupMapping(CustomerGroupMappingViewModel entity);

        bool AddTempCustomerGroupMapping(CustomerGroupMappingViewModel entity);

        bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> customerGroups);

        IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping();

        IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMappingByGroupId(int customerGroupId);

        CustomerGroupMappingViewModel GetCustomerGroupMappingByGroupMapId(int groupMapId);

        IEnumerable<LookupViewModel> GetCustomerGroupRelationshipTypes();

        bool UpdateCustomerGroupMapping(int groupMapId, CustomerGroupMappingViewModel entity);

        bool UpdateCustomerGroupMappingForApproval(int groupMapId, CustomerGroupMappingViewModel entity);

        bool DeleteCustomerGroupMapping(int groupMapId, UserInfo user);

        IEnumerable<GroupCustomerMembersViewModel> GetGroupMembersByGroupId(int customerGroupId, int companyId);
        IQueryable<CustomerGroupViewModel> SearchForCustomerGroup(int companyId, string searchQuery);

        #endregion tbl_Customer Group Mapping repository
    }
}