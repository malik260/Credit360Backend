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
        public string status { get; set; } 
        public string reason { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public string referenceNumber { get; set; }
    }

    public class OverrideDetailVeiwModel : GeneralEntity
    {
        public string customerCode;

        public int overrideDetailId { get; set; }
        public int customerId { get; set; }
        public short overrideItemId { get; set; }
        public bool isUsed { get; set; }
        public string reason { get; set; }
        public int approvedStatusId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public new int createdBy { set; get; }
        //public DateTime dateTimeCreated { get; set; }
        public string approvalStatus { get; set; }
        public string customerName { get; set; }
        public string itemName { get; set; }
        public short itemId { get; set; }
        public string customerbvn { get; set; }
        public string staffName { get; set; }
        public int operationId { get; set; }
    }

    public class ApproveOverrideVeiwModel : GeneralEntity
    { 
        public int overrideDetailId { get; set; }            
        public string statusComment { get; set; }
        public int approvedStatusId { get; set; } 
        public int operationId { get; set; }
    }
}
