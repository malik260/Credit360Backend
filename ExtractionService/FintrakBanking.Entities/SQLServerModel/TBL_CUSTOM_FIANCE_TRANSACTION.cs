namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_FIANCE_TRANSACTION")]
    public partial class TBL_CUSTOM_FIANCE_TRANSACTION
    {
        [Key]
        public int CUSTOMTRANSACTIONID { get; set; }

        //[StringLength(50)]
        public string BATCHCODE { get; set; }

        //[StringLength(50)]
        public string ACCOUNTID { get; set; }

        //[StringLength(50)]
        public string AMOUNT { get; set; }

        //[StringLength(10)]
        public string CURRENCYCODE { get; set; }

        //[StringLength(500)]
        public string NARRATION { get; set; }

        public int OPERATIONID { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool CONSUMED { get; set; }

        public DateTime? DATETIMECONSUMED { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }
    }
}
