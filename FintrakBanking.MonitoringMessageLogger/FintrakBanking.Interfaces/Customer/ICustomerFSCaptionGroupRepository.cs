using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerFSCaptionGroupRepository
    {
        IEnumerable<CustomerFSCaptionGroupViewModel> GetCustomerFSCaptionGroup();
        IEnumerable<CustomerFSCaptionGroupViewModel> GetCustomerFSCaptionGroupWithoutRatio();
        CustomerFSCaptionGroupViewModel GetCustomerFSCaptionGroupById(short fsCaptionGroupId);
        bool AddCustomerFSCaptionGroup(CustomerFSCaptionGroupViewModel entity);       
        bool UpdateCustomerFSCaptionGroup(short groupId, CustomerFSCaptionGroupViewModel entity);
        bool DeleteCustomerFSCaptionGroup(int fsCaptionId, UserInfo user);
    }
}
