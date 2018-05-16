namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_CONTINGENT_USAGE")]
    public partial class TBL_LOAN_CONTINGENT_USAGE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONTINGENTLOANUSAGEID { get; set; }

        public int CONTINGENTLOANID { get; set; }

        public decimal AMOUNTREQUESTED { get; set; }

        public int APPROVALSTATUSID { get; set; }

        [Required]
        [StringLength(2000)]
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_LOAN_CONTINGENT TBL_LOAN_CONTINGENT { get; set; }
    }
}
