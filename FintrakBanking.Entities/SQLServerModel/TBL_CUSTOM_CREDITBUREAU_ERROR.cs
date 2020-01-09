namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_CREDITBUREAU_ERROR")]
    public partial class TBL_CUSTOM_CREDITBUREAU_ERROR
    {
        [Key]
        public int CODEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string BUREAUTYPE { get; set; }

        [Required]
        //[StringLength(50)]
        public string ERRORCODE { get; set; }

        [Required]
        //[StringLength(500)]
        public string DESCRIPTION { get; set; }
    }
}
