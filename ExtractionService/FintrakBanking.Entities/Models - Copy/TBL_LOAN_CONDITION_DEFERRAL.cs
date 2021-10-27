namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_CONDITION_DEFERRAL")]
    public partial class TBL_LOAN_CONDITION_DEFERRAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHECKLISTDEFERRALID { get; set; }

        public int LOANCONDITIONID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        [Required]
        [StringLength(700)]
        public string DEFERRALREASON { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? DEFERREDDATE { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_LOAN_CONDITION_PRECEDENT TBL_LOAN_CONDITION_PRECEDENT { get; set; }
    }
}
