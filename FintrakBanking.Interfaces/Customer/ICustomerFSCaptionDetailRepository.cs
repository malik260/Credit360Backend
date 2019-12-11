using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerFSCaptionDetailRepository
    {
        #region Customer FS Detail

        IEnumerable<CustomerFSCaptionDetailViewModel> GetMappedCustomerFsCaptionDetail(int customerId, short fsCaptionGroupId, DateTime fsDate);
        IEnumerable<CustomerFSCaptionDetailViewModel> GetAllMappedCustomerFsCaptionDetail(int customerId, DateTime fsDate);
        IEnumerable<CustomerFSCaptionDetailViewModel> GetMappedCustomerFsCaptions(int customerId);
        CustomerFSCaptionDetailViewModel GetCustomerFSCaptionDetailById(int fsdetailId);

        bool AddCustomerFSCaptionDetail(CustomerFSCaptionDetailViewModel entity);
        bool AddMultipleCustomerFSCaptionDetail(List<CustomerFSCaptionDetailViewModel> entities);

        bool UpdateCustomerFSCaptionDetail(int fsdetailId, CustomerFSCaptionDetailViewModel entity);

        bool DeleteCustomerFSCaptionDetail(int fsdetailId, UserInfo user);
        bool DeleteMultipleCustomerFSCaptionDetail(List<int> fsdetailIds, UserInfo user);

        #endregion Customer FS Detail

        #region Customer Group FS Detail

        IEnumerable<CustomerGroupFSCaptionDetailViewModel> GetMappedCustomerGroupFsCaptionDetail(int customerGroupId, short fsCaptionGroupId, DateTime fsDate);
        IEnumerable<CustomerGroupFSCaptionDetailViewModel> GetMappedCustomerGroupFsCaptions(int customerGroupId);
        CustomerGroupFSCaptionDetailViewModel GetCustomerGroupFSCaptionDetailById(int fsdetailId);

        bool AddCustomerGroupFSCaptionDetail(CustomerGroupFSCaptionDetailViewModel entity);
        bool AddMultipleCustomerGroupFSCaptionDetail(List<CustomerGroupFSCaptionDetailViewModel> entities);

        bool UpdateCustomerGroupFSCaptionDetail(int fsdetailId, CustomerGroupFSCaptionDetailViewModel entity);

        bool DeleteCustomerGroupFSCaptionDetail(int fsdetailId, UserInfo user);
        bool DeleteMultipleCustomerGroupFSCaptionDetail(List<int> fsdetailIds, UserInfo user);

        #endregion Customer Group FS Detail
    }
}
