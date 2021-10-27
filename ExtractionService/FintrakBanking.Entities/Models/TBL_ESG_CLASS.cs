namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_CLASS")]
    public partial class TBL_ESG_CLASS
    {
        [Key]
        public short ESGCLASSID { get; set; }

        //[StringLength(100)]
        public string ESGCLASSNAME { get; set; }
    }
}
