namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_FINANCE_TRANSACTION_STAGING")]
    public partial class TBL_FINANCE_TRANSACTION_STAGING
    {
        [Key]
        public int TRANSACTIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string BATCHCODE { get; set; }

        [StringLength(50)]
        public string REVERSAL_BATCHCODE { get; set; }

        public int CREDITGLACCOUNT { get; set; }

        public int DEBITGLACCOUNT { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string SOURCEREFERENCENUMBER { get; set; }

        public decimal AMOUNT { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        //[Column(TypeName = "date")]
        public DateTime VALUEDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime POSTEDDATE { get; set; }

        public int COMPANYID { get; set; }

        public short SOURCEBRANCHID { get; set; }

        public short DESTINATIONBRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public double CURRENCYRATE { get; set; }

        public DateTime POSTEDDATETIME { get; set; }

        public bool ISAPPROVED { get; set; }

        public int POSTEDBY { get; set; }

        public int APPROVEDBY { get; set; }

        //[Column(TypeName = "date")]
        public DateTime APPROVEDDATE { get; set; }

        public DateTime APPROVEDDATETIME { get; set; }

        public short SOURCEAPPLICATIONID { get; set; }

        [Required]
        [StringLength(10)]
        public string TRANSTYPE { get; set; }
    }
}
