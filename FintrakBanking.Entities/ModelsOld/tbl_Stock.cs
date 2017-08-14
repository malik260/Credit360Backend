namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("treasury.tbl_Stock")]
    public partial class tbl_Stock
    {
        [Key]
        public int StockId { get; set; }

        [Required]
        [StringLength(50)]
        public string StockCode { get; set; }

        [Required]
        [StringLength(150)]
        public string StockName { get; set; }

        public bool IsQuoted { get; set; }

        public int CompanyId { get; set; }

        public int SectorId { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }
    }
}
