namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOM_TRANSACTION_BULK")]
    public partial class TBL_CUSTOM_TRANSACTION_BULK
    {
        [Key]
        public int BULKTRANSACTIONID  { get; set; }

        public int SID { get; set; }

        [Required]
        //[StringLength(50)]
        public string BATCHID { get; set; }
      
        public int BATCHREFID { get; set; }

        //[StringLength(50)]
        public string DEBITACCOUNT { get; set; }

        //[StringLength(50)]
        public string CREDITACCOUNT { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        //[StringLength(50)]
        public string SOURCEREFERENCENUMBER { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMOUNT { get; set; }

        //[StringLength(50)]
        public string TRANSACTIONTYPE { get; set; }

        //[StringLength(50)]
        public string FLOWTYPE { get; set; }
        
        [Required]
        //[StringLength(500)]
        public string DESCRIPTION { get; set; }

        //[Column(TypeName = "date")]
        public DateTime VALUEDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime POSTEDDATE { get; set; }

        public int COMPANYID { get; set; }

        public int SOURCEBRANCHID { get; set; }

        public int DESTINATIONBRANCHID { get; set; }

        //[StringLength(50)]
        public string CURRENCYCODE { get; set; }

        //[StringLength(50)]
        public string CURRENCYRATECODE { get; set; }

        public double CURRENCYRATE { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public bool ISPOSTED { get; set; }

        public string POSTEDBY { get; set; }

        //[StringLength(1)]
        public string FORCEDEBITACCOUNT  { get; set; }

        //[Column(TypeName = "date")]
        public int VALUEDATENUMBER { get; set; }

        //[StringLength(10)]
        public string BANKID { get; set; }

        public decimal AMOUNTCOLLECTED { get; set; }

        public int PRODUCTID { get; set; }

        public int CURRENCYID { get; set; }

        public int DEBITGLACCOUNTID { get; set; }

        public int CREDITGLACCOUNTID { get; set; }

        public int? DEBITCASAACCOUNTID { get; set; }

        public int? CREDITCASAACCOUNTID { get; set; }

        public int? LOANID { get; set; }
        
    }
}
