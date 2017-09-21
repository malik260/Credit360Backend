using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class JobRequestViewModel : GeneralEntity
    {
        public int targetId { get; set; }
        public short departmentId { get; set; }
        public int jobRequestId { get; set; }
        public string jobRequestCode { get; set; }
        public short jobTypeId { get; set; }
        public int senderStaffId { get; set; }
        public int receiverStaffId { get; set; }
        public int? reassignedTo { get; set; }
        public bool isReassigned { get; set; }
        public bool isAcknowledged { get; set; }
        public int operationsId { get; set; }
        public short requestStatusId { get; set; }
        public string senderComment { get; set; }
        public string responseComment { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime systemArrivalDate { get; set; }
        public DateTime? reassignedDate { get; set; }
        public DateTime? systemReassignedDate { get; set; }
        public DateTime? responseDate { get; set; }
        public DateTime? systemResponseDate { get; set; }
        public DateTime? acknowledgementDate { get; set; }
        public DateTime? systemAcknowledgementDate { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string assignee { get; set; }
    }

    public class JobTypeViewModel : GeneralEntity
    {
        public short jobTypeId { get; set; }
        public string jobTypeName { get; set; }
    }

    public class OperationStaffViewModel : GeneralEntity
    {
        public int id { get; set; }
        public string name { get; set; }
        public int groupId { get; set; }
    }
}
