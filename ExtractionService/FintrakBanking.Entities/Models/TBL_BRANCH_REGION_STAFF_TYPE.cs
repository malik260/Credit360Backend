namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_BRANCH_REGION_STAFF_TYPE")]
    public partial class TBL_BRANCH_REGION_STAFF_TYPE
    {
        [Key]
        public int REGIONSTAFFTYPEID { get; set; }

        [Required]
        //[StringLength(200)]
        public string REGIONSTAFFTYPENAME { get; set; }
     
    }
}
