namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOM_CRCBUREAU_PRODUCT")]
    public partial class TBL_CUSTOM_CRCBUREAU_PRODUCT
    {
        [Key]
        //[StringLength(50)]
        public string PRODUCTCODE { get; set; }

        [Required]
        //[StringLength(200)]
        public string PRODUCTNAME { get; set; }
    }
}
