namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CONDITIONAL_OPERATOR")]
    public partial class TBL_CONDITIONAL_OPERATOR
    {
        [Key]
        public int CONDITIONALOPERATORID { get; set; }

        [Required]
        //[StringLength(20)]
        public string OPERATORNAME { get; set; }
        

    }
}
