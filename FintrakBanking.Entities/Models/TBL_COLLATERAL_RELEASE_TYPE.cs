namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_RELEASE_TYPE")]
    public partial class TBL_COLLATERAL_RELEASE_TYPE
    {
       
        [Key]
        public int COLLATERALRELEASETYPEID { get; set; }

        //[Required]
        ////[StringLength(150)]
        public string COLLATERALRELEASETYPENAME { get; set; }

     
    }
}
