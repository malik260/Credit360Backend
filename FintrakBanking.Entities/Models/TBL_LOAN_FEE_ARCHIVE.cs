namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_FEE_ARCHIVE")]
    public partial class TBL_LOAN_FEE_ARCHIVE
    {
        [Key]
        public int LOANCHARGEFEEARCHIVEID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime CHANGEEFFECTIVEDATE { get; set; }

        public bool ISAPPLIED { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        //[StringLength(500)]
        public string CHANGEREASON { get; set; }

        public int LOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int CHARGEFEEID { get; set; }

        public bool ISPOSTED { get; set; }

        public short APPROVALSTATUSID { get; set; }

        //[Column(TypeName = "money")]
        public decimal FEERATEVALUE { get; set; }

        //[Column(TypeName = "money")]
        public decimal FEEDEPENDENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal FEEAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal EARNEDFEEAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal TAXAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal EARNEDTAXAMOUNT { get; set; }

        public bool ISINTEGRALFEE { get; set; }

        public bool ISRECURRING { get; set; }

        public short RECURRINGPAYMENTDAY { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }
    }
}
