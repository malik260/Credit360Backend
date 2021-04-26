using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_WORKFLOW_NOTIFICATION")]
    public partial class TBL_WORKFLOW_NOTIFICATION
    {
        [Key]
        public int WORKFLOWNOTIFICATIONID { get; set; }
        public int GROUPOPERATIONMAPPINGID { get; set; }
        public int APPROVALLEVELID { get; set; }
        public int? PROCEEDINGACTIONSALERTTITLEID { get; set; }
        public int? POOLALERTTITLEID { get; set; }
        public int? OWNERALERTTITLEID { get; set; }
        public int? PENDINGAPPROVALALERTTITLEID { get; set; }
        public bool INCLUDEPOOLINNOTIFICATION { get; set; }
        public bool NOTIFYOFPROCEEDINGWORKFLOWACTIONS { get; set; }
        public bool NOTIFYONWER { get; set; }
        public bool NOTIFYOFPENDINGAPPROVALS { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
