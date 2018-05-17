namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_SECTOR")]
    public partial class TBL_SECTOR
    {
        public TBL_SECTOR()
        {
            TBL_STOCK_COMPANY = new HashSet<TBL_STOCK_COMPANY>();
            TBL_SUB_SECTOR = new HashSet<TBL_SUB_SECTOR>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SECTORID { get; set; }

        [Required]
        [StringLength(200)]
        public string NAME { get; set; }

        [StringLength(10)]
        public string CODE { get; set; }

        public decimal? LOAN_LIMIT { get; set; }

        public virtual ICollection<TBL_STOCK_COMPANY> TBL_STOCK_COMPANY { get; set; }

        public virtual ICollection<TBL_SUB_SECTOR> TBL_SUB_SECTOR { get; set; }
    }
}
