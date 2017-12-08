namespace FintrakBanking.Entities.Models
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PRODUCTID { get; set; }

        public double? LCY_LIMIT { get; set; }

        public double? FCY_LIMIT { get; set; }

        [Column(TypeName = "money")]
        public decimal? CUSTOMER_LIMIT { get; set; }

        public double? PRODUCT_LIMIT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
