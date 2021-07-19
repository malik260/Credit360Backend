namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_CATEGORY")]
    public partial class TBL_ESG_CATEGORY
    {
        [Key]
        public short ESGCATEGORYID { get; set; }

        public string ESGCATEGORYNAME { get; set; }
    }
}
