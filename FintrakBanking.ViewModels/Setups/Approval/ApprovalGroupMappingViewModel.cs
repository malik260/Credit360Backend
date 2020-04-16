using System;
using System.Collections.Generic;
using System.Text; 

namespace FintrakBanking.ViewModels.Setups.Approval
{
    public class ApprovalGroupMappingViewModel : GeneralEntity
    {

        public int groupOperationMappingId { get; set; }
        public int tempGroupOperationMappingId { get; set; }
        public int operationId { get; set; }
        public string operationName { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public short? productClassId { get; set; }
        public short? productId { get; set; }
        public string productClassName { get; set; }
        public int position { get; set; }
        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        public string systemOperationType { get; set; }
        public string operation { get; set; }
        public bool allowMultipleInitiator { get; set; }
    }
}
