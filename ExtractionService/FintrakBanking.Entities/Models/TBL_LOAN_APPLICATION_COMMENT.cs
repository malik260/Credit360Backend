namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_COMMENT")]
    public partial class TBL_LOAN_APPLICATION_COMMENT
    {
        [Key]
        public int LOANCOMMENTID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string COMMENTS { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? CREATEDBY { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
