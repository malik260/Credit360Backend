namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_FINANCE_TRANSACTION")]
    public partial class TBL_FINANCE_TRANSACTION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TRANSACTIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string BATCHCODE { get; set; }

        [StringLength(50)]
        public string REVERSAL_BATCHCODE { get; set; }

        public int GLACCOUNTID { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string SOURCEREFERENCENUMBER { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public decimal DEBITAMOUNT { get; set; }

        public decimal CREDITAMOUNT { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public int COMPANYID { get; set; }

        public int SOURCEBRANCHID { get; set; }

        public int DESTINATIONBRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public double CURRENCYRATE { get; set; }

        public DateTime POSTEDDATETIME { get; set; }

        public bool ISAPPROVED { get; set; }

        public int POSTEDBY { get; set; }

        public int APPROVEDBY { get; set; }

        public DateTime APPROVEDDATETIME { get; set; }

        public int SOURCEAPPLICATIONID { get; set; }

        public DateTime? APPROVEDDATE { get; set; }

        public DateTime? POSTEDDATE { get; set; }

        public DateTime? VALUEDATE { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }
    }
}
