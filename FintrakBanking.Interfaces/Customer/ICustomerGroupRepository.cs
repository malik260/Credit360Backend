using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerGroupRepository
    {
        #region tbl_Customer Group Repository
        bool AddCustomerGroup(CustomerGroupViewModel entity);
        IEnumerable<CustomerGroupViewModel> GetCustomerGroup();
        CustomerGroupViewModel GetCustomerGroupByCustomerId(int customerGroupId);
        bool UpdateCustomerGroup(int groupId, CustomerGroupViewModel entity);
        bool DeleteCustomerGroup(int groupId, UserInfo user);

        #endregion

        #region tbl_Customer Group Mapping repository
        bool AddCustomerGroupMapping( CustomerGroupMapppingViewModel entity);

        bool AddMultipleCustomerGroupMapping(List<CustomerGroupMapppingViewModel> customerGroups);


        IEnumerable<CustomerGroupMapppingViewModel> GetCustomerGroupMapping();
        IEnumerable<CustomerGroupMapppingViewModel> GetCustomerGroupMappingByGroupId(int customerGroupId);
        CustomerGroupMapppingViewModel GetCustomerGroupMappingByGroupMapId(int groupMapId);
        IEnumerable<LookupViewModel> GetCustomerGroupRelationshipTypes();

        bool UpdateCustomerGroupMapping(int groupMapId, CustomerGroupMapppingViewModel entity);
        bool DeleteCustomerGroupMapping(int groupMapId, UserInfo user);

        #endregion
    }
}
