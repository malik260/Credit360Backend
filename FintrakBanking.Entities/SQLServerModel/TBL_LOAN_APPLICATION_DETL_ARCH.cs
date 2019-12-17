namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION_DETL_ARCH")]
    public partial class TBL_LOAN_APPLICATION_DETL_ARCH
    {
        [Key]
        public int APPLICATIONDETAIL_ARCHIVE_ID { get; set; }

        [Column(TypeName = "date")]
        public DateTime ARCHIVEDATE { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PROPOSEDPRODUCTID { get; set; }

        public int PROPOSEDTENOR { get; set; }

        public double PROPOSEDINTERESTRATE { get; set; }

        [Column(TypeName = "money")]
        public decimal PROPOSEDAMOUNT { get; set; }

        public short APPROVEDPRODUCTID { get; set; }

        public int APPROVEDTENOR { get; set; }

        public double APPROVEDINTERESTRATE { get; set; }

        [Column(TypeName = "money")]
        public decimal APPROVEDAMOUNT { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        public short SUBSECTORID { get; set; }

        public short STATUSID { get; set; }

        [Required]
        //[StringLength(500)]
        public string LOANPURPOSE { get; set; }

        public bool HASDONECHECKLIST { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
