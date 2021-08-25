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
        public int? riskRatingId { get; set; }
        public string riskRating { get; set; }
        public string comment { get; set; }
        public int operationId { get; set; }
        public int approvalStatusId { get; set; }
        public string customerGroupName { get; set; }
        public string customerGroupCode { get; set; }
        public List<CustomerGroupMappingViewModel> customerGroupMappings { get; set; }
        public string customers { get; set; }

        public string groupAddress { get; set; }
        public string groupContactPerson { get; set; }
    }

}
