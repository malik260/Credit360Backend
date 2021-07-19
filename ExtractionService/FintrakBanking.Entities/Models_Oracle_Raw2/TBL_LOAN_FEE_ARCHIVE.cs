namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_FEE_ARCHIVE")]
    public partial class TBL_LOAN_FEE_ARCHIVE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANCHARGEFEEARCHIVEID { get; set; }

        [Required]
        [StringLength(10)]
        public string CHANGEEFFECTIVEDATE { get; set; }

        public int ISAPPLIED { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        [StringLength(500)]
        public string CHANGEREASON { get; set; }

        public int LOANID { get; set; }

        public int LOANSYSTEMTYPEID { get; set; }

        public int CHARGEFEEID { get; set; }

        public int ISPOSTED { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public decimal FEERATEVALUE { get; set; }

        public decimal FEEDEPENDENTAMOUNT { get; set; }

        public decimal FEEAMOUNT { get; set; }

        public decimal EARNEDFEEAMOUNT { get; set; }

        public decimal TAXAMOUNT { get; set; }

        public decimal EARNEDTAXAMOUNT { get; set; }

        public int ISINTEGRALFEE { get; set; }

        public int ISRECURRING { get; set; }

        public int RECURRINGPAYMENTDAY { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }
    }
}
