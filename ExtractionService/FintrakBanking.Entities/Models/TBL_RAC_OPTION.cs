namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RAC_OPTION")]
    public partial class TBL_RAC_OPTION
    {
        [Key]
        public int RACOPTIONID { get; set; }

        [Required]
        //[StringLength(200)]
        public string OPTIONNAME { get; set; }
       // public bool DELETED { get; set; }

        // public bool DELETE { get; set; }

    }
}
        /*

        public virtual DbSet<TBL_RAC_OPTION> TBL_RAC_OPTION { get; set; }

        */
