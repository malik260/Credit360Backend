namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_TYPE")]
    public partial class TBL_COLLATERAL_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_TYPE()
        {
            TBL_COLLATERAL_CUSTOMER = new HashSet<TBL_COLLATERAL_CUSTOMER>();
            TBL_COLLATERAL_TYPE_SUB = new HashSet<TBL_COLLATERAL_TYPE_SUB>();
            TBL_COLLATERAL_VALUEBASE_TYPE = new HashSet<TBL_COLLATERAL_VALUEBASE_TYPE>();
            TBL_PRODUCT_COLLATERALTYPE = new HashSet<TBL_PRODUCT_COLLATERALTYPE>();
            TBL_TEMP_PRODUCT_COLLATERALTYP = new HashSet<TBL_TEMP_PRODUCT_COLLATERALTYP>();
            TBL_TEMP_COLLATERAL_CUSTOMER = new HashSet<TBL_TEMP_COLLATERAL_CUSTOMER>();
        }

        [Key]
        public int COLLATERALTYPEID { get; set; }

        [Required]
        [StringLength(250)]
        public string COLLATERALTYPENAME { get; set; }

        public int COMPANYID { get; set; }

        [StringLength(500)]
        public string DETAILS { get; set; }

        public bool REQUIREINSURANCEPOLICY { get; set; }

        public bool REQUIREVISITATION { get; set; }

        public int? CHARGEGLACCOUNTID { get; set; }

        public int POSITION { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_TYPE_SUB> TBL_COLLATERAL_TYPE_SUB { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_VALUEBASE_TYPE> TBL_COLLATERAL_VALUEBASE_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_COLLATERALTYPE> TBL_PRODUCT_COLLATERALTYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT_COLLATERALTYP> TBL_TEMP_PRODUCT_COLLATERALTYP { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
