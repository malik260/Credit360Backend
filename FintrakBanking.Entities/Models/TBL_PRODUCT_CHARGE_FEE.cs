namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_CHARGE_FEE")]
    public partial class TBL_PRODUCT_CHARGE_FEE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_CHARGE_FEE()
        {
            TBL_PRODUCT_CHARGE_FEE_CUSTOMER = new HashSet<TBL_PRODUCT_CHARGE_FEE_CUSTOMER>();
        }

        [Key]
        public int PRODUCTFEEID { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CHARGEFEEID { get; set; }

        [Column(TypeName = "money")]
        public decimal RATEVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal? DEPENDENTAMOUNT { get; set; }

        public bool CANBEREVIEWED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool? DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_CHARGE_FEE_CUSTOMER> TBL_PRODUCT_CHARGE_FEE_CUSTOMER { get; set; }
    }
}
