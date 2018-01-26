using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.CASA
{
    public class OverrideItemVeiwModel
    {

        public int itemId { get; set; }
        public string itemName { get; set; }

    }

    public class OverrideDetailVeiwModel
    {
        public string customerCode;

        public int overrideDetailId { get; set; }
        public int customerId { get; set; }
        public short overrideItemId { get; set; }
        public bool isUsed { get; set; }
        public int approvedStatusId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public int createdBy { set; get; }
        public DateTime dateTimeCreated { get; set; }
    }
}
