namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_EDUCATIONLEVELTYP")]
    public partial class TBL_CUSTOMER_EDUCATIONLEVELTYP
    {
        [Key]
        public short EDUCATIONLEVELTYPEID { get; set; }

        [Required]
        //[StringLength(200)]
        public string EDUCATIONLEVEL { get; set; }
    }
}
