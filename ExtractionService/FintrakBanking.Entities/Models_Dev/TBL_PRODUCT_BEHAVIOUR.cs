namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_BEHAVIOUR")]
    public partial class TBL_PRODUCT_BEHAVIOUR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCT_BEHAVIOURID { get; set; }

        public short PRODUCTID { get; set; }

        public double? COLLATERAL_LCY_LIMIT { get; set; }

        public double? COLLATERAL_FCY_LIMIT { get; set; }

        public decimal? CUSTOMER_LIMIT { get; set; }

        public double? PRODUCT_LIMIT { get; set; }

        public bool? ISINVOICEBASED { get; set; }

        public bool? REQUIRECASAACCOUNT { get; set; }

        public bool? ALLOWFUNDUSAGE { get; set; }

        public bool? ISTEMPORARYOVERDRAFT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
