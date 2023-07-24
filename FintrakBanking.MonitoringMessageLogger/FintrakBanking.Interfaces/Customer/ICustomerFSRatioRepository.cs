using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerFSRatioRepository
    {
        #region FS Ratio Caption
        IEnumerable<CustomerFSRatioCaptionViewModel> GetFSRatioCaption(int companyId);
        IEnumerable<CustomerFSRatioCaptionViewModel> GetFSRatioCaptionByFSCaptionGroupId(int companyId, int fSCaptionGroupId);
        List<CustomerFSRatioCaptionViewModel> GetFSRatioCaptionById(short ratioCaptionId);
        bool AddFSRatioCaption(CustomerFSRatioCaptionViewModel model);
        bool UpdateFSRatioCaption(short fsCaptionId, CustomerFSRatioCaptionViewModel model);
        bool DeleteFSRatioCaption(short fsCaptionId, UserInfo user);
        #endregion

        #region FS Ratio Detail
        bool AddFSRatioDetail(CustomerFSRatioDetailViewModel model);
        bool AddMultipleFSRatioDetail(List<CustomerFSRatioDetailViewModel> model);
        IEnumerable<CustomerFSRatioDetailViewModel> GetFSRatioDetail(short ratioCaptionId, short fsCaptionGroupId, int companyId);
        CustomerFSRatioDetailViewModel GetFSRatioDetailById(int ratioDetailId);
        bool UpdateFSRatioDetail(int ratioDetailId, CustomerFSRatioDetailViewModel model);
        bool DeleteFSRatioDetail(int ratioDetailId, UserInfo user);
        bool DeleteMultipleFSRatioDetail(List<int> ratioDetailId, UserInfo user);
        IEnumerable<CustomerFSRatioDivisorTypeViewModel> GetAllDivisorType();
        IEnumerable<CustomerFSRatioValueTypeViewModel> GetAllValueType();
        #endregion

        List<CustomerFSRatioCaptionReportViewModel> GetCustomerFSRatioValues(int customerId);

        decimal CalculateFSRatioValueForDerived(CustomerFSCaptionDetailViewModel entity);
    }
}