using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerGroupViewModel : GeneralEntity
    {
        public int customerGroupId { get; set; }
        public string groupName { get; set; }
        public string groupCode { get; set; }
        public string groupDescription { get; set; }
    }

}
