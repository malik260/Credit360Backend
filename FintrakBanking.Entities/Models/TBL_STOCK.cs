namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("treasury.TBL_STOCK")]
    public partial class TBL_STOCK
    {
        [Key]
        public int STOCKID { get; set; }

        [Required]
        [StringLength(50)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(150)]
        public string STOCKNAME { get; set; }

        public bool ISQUOTED { get; set; }

        public int COMPANYID { get; set; }

        public int SECTORID { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
