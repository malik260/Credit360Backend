namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_REPAYMENT_TERM")]
    public partial class TBL_REPAYMENT_TERM
    {
        [Key]
        public int REPAYMENTSCHEDULEID { get; set; }

        [Required]
        //[StringLength(1000)]
        public string REPAYMENTTERMDETAIL { get; set; }

        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
