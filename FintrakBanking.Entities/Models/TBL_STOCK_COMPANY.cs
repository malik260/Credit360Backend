namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("treasury.TBL_STOCK_COMPANY")]
    public partial class TBL_STOCK_COMPANY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_STOCK_COMPANY()
        {
            TBL_STOCK_PRICE = new HashSet<TBL_STOCK_PRICE>();
        }

        [Key]
        public int STOCKID { get; set; }

        [Required]
        [StringLength(10)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(100)]
        public string STOCKNAME { get; set; }

        public int COUNTRYID { get; set; }

        public bool ISACTIVE { get; set; }

        public bool ISQUOTED { get; set; }

        public short SECTORID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }

        public virtual TBL_SECTOR TBL_SECTOR { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STOCK_PRICE> TBL_STOCK_PRICE { get; set; }
    }
}
