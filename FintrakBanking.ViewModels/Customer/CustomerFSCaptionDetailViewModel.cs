using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerFSCaptionDetailViewModel : GeneralEntity
    {
        public int fsdetailId { get; set; }
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public int fsCaptionId { get; set; }
        public string fsCaptionName { get; set; }
        public int fsCaptionPosition { get; set; }
        public string accountCategoryName { get; set; }
        public string fsTypeName { get; set; }
        public DateTime fsDate { get; set; }
        public decimal amount { get; set; }
        public string fsGroupName { get; set; }
        public string textValue { get; set; }

    }

    public class CustomerGroupFSCaptionDetailViewModel: CustomerFSCaptionDetailViewModel
    {
        public int customerGroupId { get; set; }
        public string customerGroupCode { get; set; }
    }
}
