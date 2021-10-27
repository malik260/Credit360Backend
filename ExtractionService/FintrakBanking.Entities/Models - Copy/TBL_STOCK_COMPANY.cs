namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STOCK_COMPANY")]
    public partial class TBL_STOCK_COMPANY
    {
        public TBL_STOCK_COMPANY()
        {
            TBL_STOCK_PRICE = new HashSet<TBL_STOCK_PRICE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STOCKID { get; set; }

        [Required]
        [StringLength(10)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(100)]
        public string STOCKNAME { get; set; }

        public int COUNTRYID { get; set; }

        public int ISACTIVE { get; set; }

        public int ISQUOTED { get; set; }

        public int SECTORID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }

        public virtual TBL_SECTOR TBL_SECTOR { get; set; }

        public virtual ICollection<TBL_STOCK_PRICE> TBL_STOCK_PRICE { get; set; }
    }
}
