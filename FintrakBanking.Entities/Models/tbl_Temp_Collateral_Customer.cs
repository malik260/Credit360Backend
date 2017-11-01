namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_CUSTOMER")]
    public partial class TBL_TEMP_COLLATERAL_CUSTOMER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_TEMP_COLLATERAL_CUSTOMER()
        {
            TBL_TEMP_COLLATERAL_CASA = new HashSet<TBL_TEMP_COLLATERAL_CASA>();
            TBL_TEMP_COLLATERAL_DOCUMENTS = new HashSet<TBL_TEMP_COLLATERAL_DOCUMENTS>();
            TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY = new HashSet<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY>();
            TBL_TEMP_COLLATERAL_DEPOSIT = new HashSet<TBL_TEMP_COLLATERAL_DEPOSIT>();
            TBL_TEMP_COLLATERAL_GAURANTEE = new HashSet<TBL_TEMP_COLLATERAL_GAURANTEE>();
            TBL_TEMP_COLLATERAL_MISCELLANEOUS = new HashSet<TBL_TEMP_COLLATERAL_MISCELLANEOUS>();
            TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY = new HashSet<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>();
        }

        [Key]
        public int COLLATERALCUSTOMERID { get; set; }

        public int COLLATERALTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string COLLATERALCODE { get; set; }

        public short CURRENCYID { get; set; }

        public int COMPANYID { get; set; }

        public bool ALLOWSHARING { get; set; }

        public bool ISLOCATIONBASED { get; set; }

        public int? VALUATIONCYCLE { get; set; }

        public double HAIRCUT { get; set; }

        public int CUSTOMERID { get; set; }

        [StringLength(50)]
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

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_CASA> TBL_TEMP_COLLATERAL_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_DOCUMENTS> TBL_TEMP_COLLATERAL_DOCUMENTS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY> TBL_TEMP_COLLATERAL_MARKETABLE_SECURITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_DEPOSIT> TBL_TEMP_COLLATERAL_DEPOSIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_GAURANTEE> TBL_TEMP_COLLATERAL_GAURANTEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_MISCELLANEOUS> TBL_TEMP_COLLATERAL_MISCELLANEOUS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY> TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY { get; set; }
    }
}
