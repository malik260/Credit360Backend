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
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int INVOICEID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANAPPLICATIONDETAILID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRINCIPALID { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string INVOICENO { get; set; }

        [Key]
        [Column(Order = 4)]
        public decimal INVOICE_AMOUNT { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short INVOICE_CURRENCYID { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(50)]
        public string CONTRACTNO { get; set; }

        [StringLength(50)]
        public string PURCHASEORDERNUMBER { get; set; }

        public short? APPROVALSTATUSID { get; set; }

        [StringLength(800)]
        public string APPROVAL_COMMENT { get; set; }

        public int? APPROVEDBY { get; set; }

        public DateTime? APPROVEDDATETIME { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REVALIDATED { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREATEDBY { get; set; }

        [Key]
        [Column(Order = 9)]
        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        [Key]
        [Column(Order = 10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime CONTRACT_ENDDATE { get; set; }

        public DateTime CONTRACT_STARTDATE { get; set; }

        public DateTime INVOICE_DATE { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRINCIPAL TBL_LOAN_PRINCIPAL { get; set; }
    }
}
