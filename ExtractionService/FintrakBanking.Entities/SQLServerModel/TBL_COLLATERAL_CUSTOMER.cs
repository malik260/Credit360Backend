namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_CUSTOMER")]
    public partial class TBL_COLLATERAL_CUSTOMER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_CUSTOMER()
        {
            TBL_COLLATERAL_CASA = new HashSet<TBL_COLLATERAL_CASA>();
            TBL_COLLATERAL_ITEM_POLICY = new HashSet<TBL_COLLATERAL_ITEM_POLICY>();
            TBL_COLLATERAL_DEPOSIT = new HashSet<TBL_COLLATERAL_DEPOSIT>();
            TBL_COLLATERAL_GAURANTEE = new HashSet<TBL_COLLATERAL_GAURANTEE>();
            TBL_COLLATERAL_POLICY = new HashSet<TBL_COLLATERAL_POLICY>();
            TBL_COLLATERAL_PLANT_AND_EQUIP = new HashSet<TBL_COLLATERAL_PLANT_AND_EQUIP>();
            TBL_COLLATERAL_MKT_SECURITY = new HashSet<TBL_COLLATERAL_MKT_SECURITY>();
            TBL_COLLATERAL_MISCELLANEOUS = new HashSet<TBL_COLLATERAL_MISCELLANEOUS>();
            TBL_COLLATERAL_PRECIOUSMETAL = new HashSet<TBL_COLLATERAL_PRECIOUSMETAL>();
            TBL_COLLATERAL_IMMOVE_PROPERTY = new HashSet<TBL_COLLATERAL_IMMOVE_PROPERTY>();
            TBL_COLLATERAL_STOCK = new HashSet<TBL_COLLATERAL_STOCK>();
            TBL_COLLATERAL_VEHICLE = new HashSet<TBL_COLLATERAL_VEHICLE>();
            TBL_LOAN_APPLICATION_COLLATERL = new HashSet<TBL_LOAN_APPLICATION_COLLATERL>();
            TBL_LOAN_COLLATERAL_MAPPING = new HashSet<TBL_LOAN_COLLATERAL_MAPPING>();
            TBL_LOANAPPLICATION_COLTRL_MAP = new HashSet<TBL_LOANAPPLICATION_COLTRL_MAP>();
        }

        [Key]
        public int COLLATERALCUSTOMERID { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public short COLLATERALSUBTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string COLLATERALCODE { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        public int COMPANYID { get; set; }

        public bool ALLOWSHARING { get; set; }

        public bool ISLOCATIONBASED { get; set; }

        public int? VALUATIONCYCLE { get; set; }

        [Column(TypeName = "money")]
        public decimal COLLATERALVALUE { get; set; }

        public double HAIRCUT { get; set; }

        public int CUSTOMERID { get; set; }

        //[StringLength(50)]
        public string CAMREFNUMBER { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int APPROVALSTATUS { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_CASA> TBL_COLLATERAL_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_ITEM_POLICY> TBL_COLLATERAL_ITEM_POLICY { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_DEPOSIT> TBL_COLLATERAL_DEPOSIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_GAURANTEE> TBL_COLLATERAL_GAURANTEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_POLICY> TBL_COLLATERAL_POLICY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_PLANT_AND_EQUIP> TBL_COLLATERAL_PLANT_AND_EQUIP { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_MKT_SECURITY> TBL_COLLATERAL_MKT_SECURITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_MISCELLANEOUS> TBL_COLLATERAL_MISCELLANEOUS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_PRECIOUSMETAL> TBL_COLLATERAL_PRECIOUSMETAL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_IMMOVE_PROPERTY> TBL_COLLATERAL_IMMOVE_PROPERTY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_STOCK> TBL_COLLATERAL_STOCK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_VEHICLE> TBL_COLLATERAL_VEHICLE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_COLLATERL> TBL_LOAN_APPLICATION_COLLATERL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_COLLATERAL_MAPPING> TBL_LOAN_COLLATERAL_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOANAPPLICATION_COLTRL_MAP> TBL_LOANAPPLICATION_COLTRL_MAP { get; set; }
    }
}
