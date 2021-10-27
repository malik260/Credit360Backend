namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_BEHAVIOUR")]
    public partial class TBL_PRODUCT_BEHAVIOUR
    {
        [Key]
        public int PRODUCT_BEHAVIOURID { get; set; }

        public short PRODUCTID { get; set; }

        public double? COLLATERAL_LCY_LIMIT { get; set; }

        public double? COLLATERAL_FCY_LIMIT { get; set; }

        [Column(TypeName = "money")]
        public decimal? CUSTOMER_LIMIT { get; set; }

        public double? PRODUCT_LIMIT { get; set; }

        public bool? ISINVOICEBASED { get; set; }

        public bool? REQUIRECASAACCOUNT { get; set; }

        public bool? ALLOWFUNDUSAGE { get; set; }

        public bool? ISTEMPORARYOVERDRAFT { get; set; }

        public int? CRMSREGULATORYID { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_CRMS_REGULATORY TBL_CRMS_REGULATORY { get; set; }
    }
}
