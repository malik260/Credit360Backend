using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels; 
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerFSCaptionRepository
    {
        IEnumerable<CustomerFSCaptionViewModel> GetCustomerFSCaption(short fsCaptionGroupId);
        CustomerFSCaptionViewModel GetCustomerFSCaptionById(int fsCaptionId);
        IEnumerable<CustomerFSCaptionViewModel> GetUnmappedCustomerFSCaption(short fsCaptionGroupId, int customerId, DateTime fsDate);

        bool AddCustomerFSCaption(CustomerFSCaptionViewModel entity);       
        bool UpdateCustomerFSCaption(int fsCaptionId, CustomerFSCaptionViewModel entity);
        bool DeleteCustomerFSCaption(int fsCaptionId, UserInfo user);
    }
}
