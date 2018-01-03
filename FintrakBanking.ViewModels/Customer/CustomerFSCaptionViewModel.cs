using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerFSCaptionViewModel : GeneralEntity
    {
        public int fsCaptionId { get; set; }
        public string fsCaptionCode { get; set; }
        public string fsCaptionName { get; set; }
        public short fsCaptionGroupId { get; set; }
        public string fsCaptionGroupName { get; set; }
        public int? parentIdFSCaptionId { get; set; }
        public string parentIdFSCaptionName { get; set; }
        public short? accountCategoryId { get; set; }
        public string accountCategoryName { get; set; }       
        public short fsTypeId { get; set; }
        public string fsTypeName { get; set; }
        public int position { get; set; }
        public string refNote { get; set; }
        public bool isTotalLine { get; set; }
        public string reportColour { get; set; }
        public double multiplier { get; set; }
        public bool isRatio { get; set; }

        //public string fsCaptionGroupName { get; set; }
        //public int companyId { get; set; }
    }
}
