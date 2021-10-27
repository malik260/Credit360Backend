namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_BULK_INTEREST_REVIEW")]
    public partial class TBL_LOAN_BULK_INTEREST_REVIEW
    {
        [Key]
        public int BULKINTERESTRATEREVIEWID { get; set; }

        public int COMPANYID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        public short PRODUCTPRICEINDEXID { get; set; }

        public double OLDINTERESTRATE { get; set; }

        public double NEWINTERESTRATE { get; set; }

        public bool ISPROCESSED { get; set; }

        public DateTime? PROCESSSTARTTIME { get; set; }

        public DateTime? PROCESSENDTIME { get; set; }

        public int CREATEDBY { get; set; }

        //[Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_PRODUCT_PRICE_INDEX TBL_PRODUCT_PRICE_INDEX { get; set; }
    }
}
