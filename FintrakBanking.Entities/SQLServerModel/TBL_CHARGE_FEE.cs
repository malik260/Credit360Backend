namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CHARGE_FEE")]
    public partial class TBL_CHARGE_FEE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CHARGE_FEE()
        {
            TBL_CHARGE_FEE_DETAIL = new HashSet<TBL_CHARGE_FEE_DETAIL>();
            TBL_CHARGE_RANGE = new HashSet<TBL_CHARGE_RANGE>();
            TBL_CUSTOMER_PRODUCT_FEE = new HashSet<TBL_CUSTOMER_PRODUCT_FEE>();
            TBL_LOAN_APPLICATION_DETL_FEE = new HashSet<TBL_LOAN_APPLICATION_DETL_FEE>();
            TBL_LOAN_FEE_ARCHIVE = new HashSet<TBL_LOAN_FEE_ARCHIVE>();
            TBL_LOAN_FEE = new HashSet<TBL_LOAN_FEE>();
            TBL_PRODUCT_CHARGE_FEE = new HashSet<TBL_PRODUCT_CHARGE_FEE>();
            TBL_TEMP_PRODUCT_CHARGE_FEE = new HashSet<TBL_TEMP_PRODUCT_CHARGE_FEE>();
        }

        [Key]
        public int CHARGEFEEID { get; set; }

        [Required]
        //[StringLength(150)]
        public string CHARGEFEENAME { get; set; }

        public short FEEINTERVALID { get; set; }

        public short? PRODUCTTYPEID { get; set; }

        public short FEETARGETID { get; set; }

        public short? FEEAMORTISATIONTYPEID { get; set; }

        public bool ISINTEGRALFEE { get; set; }

        public bool INCLUDECUTOFFDAY { get; set; }

        public short? CUTOFFDAY { get; set; }

        public int? OPERATIONID { get; set; }

        [Column(TypeName = "money")]
        public decimal? AMOUNT { get; set; }

        public double? RATE { get; set; }

        public short FEETYPEID { get; set; }

        public bool? RECURRING { get; set; }

        public int COMPANYID { get; set; }

        public int? CRMSREGULATORYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CRMS_REGULATORY TBL_CRMS_REGULATORY { get; set; }

        public virtual TBL_FEE_AMORTISATION_TYPE TBL_FEE_AMORTISATION_TYPE { get; set; }

        public virtual TBL_FEE_INTERVAL TBL_FEE_INTERVAL { get; set; }

        public virtual TBL_FEE_TARGET TBL_FEE_TARGET { get; set; }

        public virtual TBL_FEE_TYPE TBL_FEE_TYPE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_RANGE> TBL_CHARGE_RANGE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_PRODUCT_FEE> TBL_CUSTOMER_PRODUCT_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_FEE> TBL_LOAN_APPLICATION_DETL_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_FEE_ARCHIVE> TBL_LOAN_FEE_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_FEE> TBL_LOAN_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_CHARGE_FEE> TBL_PRODUCT_CHARGE_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT_CHARGE_FEE> TBL_TEMP_PRODUCT_CHARGE_FEE { get; set; }
    }
}
