namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_PRODUCT_BEHAVIOUR")]
    public partial class TBL_TEMP_PRODUCT_BEHAVIOUR
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCT_BEHAVIOURID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PRODUCTID { get; set; }

        public double? LCY_LIMIT { get; set; }

        public double? FCY_LIMIT { get; set; }

        [Column(TypeName = "money")]
        public decimal? CUSTOMER_LIMIT { get; set; }

        public double? PRODUCT_LIMIT { get; set; }
    }
}
