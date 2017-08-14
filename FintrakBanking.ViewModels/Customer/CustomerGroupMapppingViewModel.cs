using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerGroupMapppingViewModel : GenaralEntity
    {
        public int customerGroupMappingId { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string customerType { get; set; }
        public int customerGroupId { get; set; }
        public short relationshipTypeId { get; set; }
        public string relationshipTypeName { get; set; }
        
        
        
    }
}
