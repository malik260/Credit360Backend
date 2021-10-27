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

        public int PRODUCTID { get; set; }

        public decimal? COLLATERAL_LCY_LIMIT { get; set; }

        public decimal? COLLATERAL_FCY_LIMIT { get; set; }

        public decimal? CUSTOMER_LIMIT { get; set; }

        public decimal? PRODUCT_LIMIT { get; set; }

        public int? ISINVOICEBASED { get; set; }

        public int? REQUIRECASAACCOUNT { get; set; }

        public int? ALLOWFUNDUSAGE { get; set; }

        public int? ISTEMPORARYOVERDRAFT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
