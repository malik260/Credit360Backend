using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels; 
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerFSCaptionDetailRepository
    {
        IEnumerable<CustomerFSCaptionDetailViewModel> GetCustomerFSCaptionDetail(int customerId);
        CustomerFSCaptionDetailViewModel GetCustomerFSCaptionDetailById(int fsdetailId);

        bool AddCustomerFSCaptionDetail(CustomerFSCaptionDetailViewModel entity);
        bool AddMultipleCustomerFSCaptionDetail(List<CustomerFSCaptionDetailViewModel> entities);

        bool UpdateCustomerFSCaptionDetail(int fsdetailId, CustomerFSCaptionDetailViewModel entity);


        bool DeleteCustomerFSCaptionDetail(int fsdetailId, UserInfo user);
        bool DeleteMultileCustomerFSCaptionDetail(List<int> fsdetailIds, UserInfo user);
    }
}
