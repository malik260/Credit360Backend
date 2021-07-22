namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_COLLATERAL_MISCELLAN")]
    public partial class TBL_TEMP_COLLATERAL_MISCELLAN
    {
        [Key]
        public int TEMPCOLLATERALMISCELLANEOUSID { get; set; }
        [Required]
        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public bool ISOWNEDBYCUSTOMER { get; set; }
        [Required]
        //[StringLength(50)]
        public string NAMEOFSECURITY { get; set; }

        //[Column(TypeName = "money")]
        public decimal SECURITYVALUE { get; set; }

        //[StringLength(100)]
        public string NOTE { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
