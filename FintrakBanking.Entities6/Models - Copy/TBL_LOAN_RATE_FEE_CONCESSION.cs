namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_RATE_FEE_CONCESSION")]
    public partial class TBL_LOAN_RATE_FEE_CONCESSION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONCESSIONID { get; set; }

        public int CONCESSIONTYPEID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int? LOANCHARGEFEEID { get; set; }

        [StringLength(3000)]
        public string CONSESSIONREASON { get; set; }

        public decimal CONCESSION { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETL_FEE TBL_LOAN_APPLICATION_DETL_FEE { get; set; }

        public virtual TBL_LOAN_CONCESSION_TYPE TBL_LOAN_CONCESSION_TYPE { get; set; }
    }
}
