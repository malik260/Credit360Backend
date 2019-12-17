namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_PRICE_INDEX")]
    public partial class TBL_PRODUCT_PRICE_INDEX
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_PRICE_INDEX()
        {
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_LOAN_BULK_INTEREST_REVIEW = new HashSet<TBL_LOAN_BULK_INTEREST_REVIEW>();
            TBL_PRODUCT_PRICE_INDEX_DAILY = new HashSet<TBL_PRODUCT_PRICE_INDEX_DAILY>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        public short PRODUCTPRICEINDEXID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(50)]
        public string PRICEINDEXNAME { get; set; }

        public double PRICEINDEXRATE { get; set; }

        public int DURATION { get; set; }

        public bool ALLOWAUTOMATICREPRICING { get; set; }

        [Required]
        //[StringLength(500)]
        public string PRICEINDEXDESCRIPTION { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_BULK_INTEREST_REVIEW> TBL_LOAN_BULK_INTEREST_REVIEW { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_PRICE_INDEX_DAILY> TBL_PRODUCT_PRICE_INDEX_DAILY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
