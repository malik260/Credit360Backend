namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DEFINED_FUNCTION")]
    public partial class TBL_DEFINED_FUNCTION
    {
        [Key]
        public int DEFINEDFUNCTIONID { get; set; }

        [Required]
        //[StringLength(50)]
        public string FUNCTIONNAME { get; set; }

        //[StringLength(250)]
        public string DESCRIPTION { get; set; }

        public bool ISSYSTEMDEFINED { get; set; }
        

    }
}
