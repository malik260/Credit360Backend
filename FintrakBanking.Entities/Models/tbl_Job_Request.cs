namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Job_Request")]
    public partial class tbl_Job_Request
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Job_Request()
        {
            tbl_Job_Request_Document_Mapping = new HashSet<tbl_Job_Request_Document_Mapping>();
        }

        [Key]
        public int JobRequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string JobRequestCode { get; set; }

        public short JobTypeId { get; set; }

        public int SenderStaffId { get; set; }

        public int ReceiverStaffId { get; set; }

        public int? ReassignedTo { get; set; }

        public bool IsReassigned { get; set; }

        public bool IsAcknowledged { get; set; }

        public int TargetId { get; set; }

        public int StaffApprovalGroupId { get; set; }

        public int OperationsId { get; set; }

        public short RequestStatusId { get; set; }

        [StringLength(700)]
        public string SenderComment { get; set; }

        [StringLength(50)]
        public string ResponseComment { get; set; }

        [Column(TypeName = "date")]
        public DateTime ArrivalDate { get; set; }

        public DateTime SystemArrivalDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ReassignedDate { get; set; }

        public DateTime? SystemReassignedDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ResponseDate { get; set; }

        public DateTime? SystemResponseDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? AcknowledgementDate { get; set; }

        public DateTime? SystemAcknowledgementDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Job_Request_Document_Mapping> tbl_Job_Request_Document_Mapping { get; set; }
         
        public virtual tbl_Job_Request_Status tbl_Job_Request_Status { get; set; }

        public virtual tbl_Job_Type tbl_Job_Type { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }
    }
}
