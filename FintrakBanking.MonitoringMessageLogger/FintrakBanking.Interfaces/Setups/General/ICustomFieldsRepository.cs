using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface ICustomFieldsRepository
    {
        #region   Custom Fields
        Task<bool> AddCustomField(AddCustomFieldViewModel model);
        Task<bool> UpdateCustomField(AddCustomFieldViewModel model, int id);
        Task<bool> AddCustomFields(List<CustomFieldViewModel> listEntity);
        IEnumerable<CustomFieldViewModel> CustomFieldsByHostPageId(int hostPageId, int companyId);
        Task<bool> DeleteCustomFields(List<CustomFieldViewModel> customFields , UserInfo user);
        Task<bool> UpdateCustomFields(List<CustomFieldViewModel> listEntity);

        #endregion   Custom Fields

       

        #region   host page 
        IEnumerable<HostPageViewModel> GetHostPages();
        IEnumerable<HostPageViewModel> GetHostPagesParentOnly();
        IEnumerable<HostPageViewModel> GetHostPagesChildrenOnly(int parentHostPageId);
        #endregion   host page

        #region   Custom Fields Data
        Task<bool> AddCustomFieldsData(List<CustomFieldsDataViewModel> listEntity);
        Task<bool> DeleteCustomFieldsData(List<CustomFieldsDataViewModel> listEntity, UserInfo user);
        Task<bool> UpdateCustomFieldsData(List<CustomFieldsDataViewModel> listEntity);
        IEnumerable<CustomFieldsDataViewModel> GetCustomFieldsDataByHostPage(int hostPageId,int ownerId, int companyId);
        #endregion   Custom Fields Data

    }
}
