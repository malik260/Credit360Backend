namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_SOURCE")]
    public partial class TBL_JOB_SOURCE
    {


        [Key]
        public int JOBSOURCEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string JOBSOURCENAME { get; set; }


    }
}
