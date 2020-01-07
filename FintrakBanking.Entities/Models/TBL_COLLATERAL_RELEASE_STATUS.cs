namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_RELEASE_STATUS")]
    public partial class TBL_COLLATERAL_RELEASE_STATUS
    {
       
        [Key]
        public int COLLATERALRELEASESTATUSID { get; set; }

        //[Required]
        ////[StringLength(150)]
        public string COLLATERALRELEASESTATUSNAME { get; set; }

     
    }
}
