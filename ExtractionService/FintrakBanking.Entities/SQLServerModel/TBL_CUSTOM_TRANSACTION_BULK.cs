namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_TRANSACTION_BULK")]
    public partial class TBL_CUSTOM_TRANSACTION_BULK
    {
        [Key]
        public int BULKTRANSACTIONID { get; set; }

        public int SID { get; set; }

        [Required]
        //[StringLength(50)]
        public string BATCHID { get; set; }

        public int BATCHREFID { get; set; }

        [Required]
        //[StringLength(50)]
        public string DEBITACCOUNTID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CREDITACCOUNTID { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        //[StringLength(50)]
        public string SOURCEREFERENCENUMBER { get; set; }

        [Column(TypeName = "money")]
        public decimal AMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal? AMOUNTCOLLECTED { get; set; }

        [Required]
        //[StringLength(50)]
        public string TRANSACTIONTYPE { get; set; }

        //[StringLength(50)]
        public string FLOWTYPE { get; set; }

        [Required]
        //[StringLength(500)]
        public string DESCRIPTION { get; set; }

        [Column(TypeName = "date")]
        public DateTime VALUEDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime POSTEDDATE { get; set; }

        public int COMPANYID { get; set; }

        public short SOURCEBRANCHID { get; set; }

        public short DESTINATIONBRANCHID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CURRENCYCODE { get; set; }

        //[StringLength(10)]
        public string CURRENCYRATECODE { get; set; }

        public double CURRENCYRATE { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public bool ISPOSTED { get; set; }

        [Required]
        //[StringLength(50)]
        public string POSTEDBY { get; set; }

        [Required]
        //[StringLength(1)]
        public string FORCEDEBITACCOUNT { get; set; }

        public short VALUEDATENUMBER { get; set; }

        [Required]
        //[StringLength(10)]
        public string BANKID { get; set; }
    }
}
