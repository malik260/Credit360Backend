using FintrakBanking.ViewModels.CASA;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerGroupMappingViewModel : GeneralEntity
    {
        
        public int customerGroupMappingId { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string customerType { get; set; }
        public int customerGroupId { get; set; }
        public short relationshipTypeId { get; set; }
        public string relationshipTypeName { get; set; }
        public string customerGroupName { get; set; }
        public string customerGroupCode { get; set; }
        public string productAccountNumber { get; set; }
        public string accountHolder { get; set; }
        public short branchId { get; set; }
        public bool isBlackListed { get; set; }
    }
}
