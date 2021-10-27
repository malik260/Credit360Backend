namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_TYPE_UNIT")]
    public partial class TBL_JOB_TYPE_UNIT
    {
        // [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]

        [Key]
        public short JOBTYPEUNITID { get; set; }

        [Required]
        //[StringLength(200)]
        public string UNITNAME { get; set; }

        public short JOBTYPEID { get; set; }


    }
}