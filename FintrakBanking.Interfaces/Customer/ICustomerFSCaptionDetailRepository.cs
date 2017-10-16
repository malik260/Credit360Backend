using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels; 
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerFSCaptionDetailRepository
    {
        IEnumerable<CustomerFSCaptionDetailViewModel> GetMappedCustomerFsCaptionDetail(int customerIdshort, short fsCaptionGroupId, DateTime fsDate);
        CustomerFSCaptionDetailViewModel GetCustomerFSCaptionDetailById(int fsdetailId);

        bool AddCustomerFSCaptionDetail(CustomerFSCaptionDetailViewModel entity);
        bool AddMultipleCustomerFSCaptionDetail(List<CustomerFSCaptionDetailViewModel> entities);

        bool UpdateCustomerFSCaptionDetail(int fsdetailId, CustomerFSCaptionDetailViewModel entity);


        bool DeleteCustomerFSCaptionDetail(int fsdetailId, UserInfo user);
        bool DeleteMultileCustomerFSCaptionDetail(List<int> fsdetailIds, UserInfo user);
    }
}
