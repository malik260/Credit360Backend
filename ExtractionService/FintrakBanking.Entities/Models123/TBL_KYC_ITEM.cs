namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_KYC_ITEM")]
    public partial class TBL_KYC_ITEM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_KYC_ITEM()
        {
            TBL_CUSTOMER_ACCOUNT_KYC_ITEM = new HashSet<TBL_CUSTOMER_ACCOUNT_KYC_ITEM>();
        }

        [Key]
        public short KYCITEMID { get; set; }

        public short? PRODUCTID { get; set; }

        public int DISPLAYORDER { get; set; }

        [Required]
        [StringLength(500)]
        public string ITEM { get; set; }

        public bool ISMANDATORY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_ACCOUNT_KYC_ITEM> TBL_CUSTOMER_ACCOUNT_KYC_ITEM { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
