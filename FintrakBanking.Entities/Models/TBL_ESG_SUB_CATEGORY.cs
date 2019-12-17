namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_SUB_CATEGORY")]
    public partial class TBL_ESG_SUB_CATEGORY
    {
      
        [Key]
        public short ESGSUBCATEGORYID { get; set; }

        public short ESGCATEGORYID { get; set; }

        //[StringLength(700)]
        public string ESGSUBCATEGORYNAME { get; set; }

    }
}
