using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.Credit
{

    public class CustomFieldsDataViewModel : GeneralEntity
    {
        public CustomFieldsDataViewModel() {
            customFieldOption = new List<CustomFieldOptionViewModel>();
        }
        public int customFieldsDataId { get; set; }
        public int customFieldId { get; set; }
        public string labelName { get; set; }
        public string controlKey { get; set; }
        public string controlType { get; set; }
        public bool required { get; set; }
        public int itemOrder { get; set; }
        public int ownerId { get; set; }
        public byte[] customFieldDataUpload { get; set; }
        public string hostPage  { get; set; }
        public int hostPageId { get; set; }
        public int parentHostPageId { get; set; }
        public string dataDetails { get; set; } 
        public bool isUpload { get; set; }
        public List<CustomFieldOptionViewModel> customFieldOption { get; set; }

    }
}