using System;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CustomFieldsViewModel : GeneralEntity
    {
        public int collateralCustomFieldId { get; set; }
        public int collateralTypeId { get; set; }    
        public string labelName { get; set; }
        public string controlType { get; set; }
        public bool required { get; set; }
        public int itemOrder { get; set; }
        public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }
        public int? actedOnBy { get; set; }
        public string collateralTypeName { get; set; }
    }
}
