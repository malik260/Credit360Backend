namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_DETL_INV")]
    public partial class TBL_LOAN_APPLICATION_DETL_INV
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int INVOICEID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int PRINCIPALID { get; set; }

        [Required]
        [StringLength(50)]
        public string INVOICENO { get; set; }

        public decimal INVOICE_AMOUNT { get; set; }

        public int INVOICE_CURRENCYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CONTRACTNO { get; set; }

        [StringLength(50)]
        public string PURCHASEORDERNUMBER { get; set; }

        public int? APPROVALSTATUSID { get; set; }

        [StringLength(800)]
        public string APPROVAL_COMMENT { get; set; }

        public int? APPROVEDBY { get; set; }

        public DateTime? APPROVEDDATETIME { get; set; }

        public int REVALIDATED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime CONTRACT_ENDDATE { get; set; }

        public DateTime CONTRACT_STARTDATE { get; set; }

        public DateTime INVOICE_DATE { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRINCIPAL TBL_LOAN_PRINCIPAL { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETL_STA TBL_LOAN_APPLICATION_DETL_STA { get; set; }
    }
}
