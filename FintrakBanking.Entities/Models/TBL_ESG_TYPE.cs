namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_TYPE")]
    public partial class TBL_ESG_TYPE
    {
        [Key]
        public short ESGTYPEID { get; set; }

        //[StringLength(100)]
        public string ESGTYPENAME { get; set; }
  
    }
}
