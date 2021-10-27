namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_PRICE_INDEX")]
    public partial class TBL_PRODUCT_PRICE_INDEX
    {
        public TBL_PRODUCT_PRICE_INDEX()
        {
            TBL_LOAN_BULK_INTEREST_REVIEW = new HashSet<TBL_LOAN_BULK_INTEREST_REVIEW>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT_PRICE_INDEX_DAILY = new HashSet<TBL_PRODUCT_PRICE_INDEX_DAILY>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTPRICEINDEXID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRICEINDEXNAME { get; set; }

        public decimal PRICEINDEXRATE { get; set; }

        public int DURATION { get; set; }

        public int ALLOWAUTOMATICREPRICING { get; set; }

        [Required]
        [StringLength(500)]
        public string PRICEINDEXDESCRIPTION { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual ICollection<TBL_LOAN_BULK_INTEREST_REVIEW> TBL_LOAN_BULK_INTEREST_REVIEW { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual ICollection<TBL_PRODUCT_PRICE_INDEX_DAILY> TBL_PRODUCT_PRICE_INDEX_DAILY { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
