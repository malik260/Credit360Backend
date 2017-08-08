using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public partial class CustomFieldOptionViewModel
    {
        public int customFieldOptionsId { get; set; }
        public int customFieldId { get; set; }
        public string optionsKey { get; set; }
        public string optionsValue { get; set; }
    }

    public class CustomFieldViewModel : GeneralEntity
    {
        public CustomFieldViewModel()
        {
            customFieldOption = new List<CustomFieldOptionViewModel>();
        }

        public int customFieldId { get; set; }
        public int hostPageId { get; set; }
        public string labelName { get; set; }
        public string controlKey { get; set; }
        public string controlType { get; set; }
        public bool required { get; set; }
        public int itemOrder { get; set; }
        public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }
        public int? actedOnBy { get; set; }
        public string collateralTypeName { get; set; }
        public List<CustomFieldOptionViewModel> customFieldOption { get; set; }
    }


    public class HostPageViewModel
    {
        public int hostPageId { get; set; }
        public string hostPage { get; set; }
        public int parentHostPageId { get; set; }
    }

    public class AddCustomFieldViewModel : GeneralEntity
    {
        //public AddCustomFieldViewModel()
        //{
        //    customFieldOption = new List<CustomFieldOptionViewModel>();
        //}

        public int hostPageId { get; set; }
        public string labelName { get; set; }
        public string controlType { get; set; }
        public bool required { get; set; }
        public int itemOrder { get; set; }
        //public List<CustomFieldOptionViewModel> customFieldOption { get; set; }
    }
}