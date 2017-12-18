namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION_DETAIL")]
    public partial class TBL_LOAN_APPLICATION_DETAIL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_APPLICATION_DETAIL()
        {
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION_COLLATERAL = new HashSet<TBL_LOAN_APPLICATION_COLLATERAL>();
            TBL_LOAN_APPLICATION_DETAIL_EDU = new HashSet<TBL_LOAN_APPLICATION_DETAIL_EDU>();
            TBL_LOAN_APPLICATION_DETAIL_INV = new HashSet<TBL_LOAN_APPLICATION_DETAIL_INV>();
            TBL_LOAN_APPLICATION_DETAIL_TRA = new HashSet<TBL_LOAN_APPLICATION_DETAIL_TRA>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_APPLICATION_DETL_LOG = new HashSet<TBL_LOAN_APPLICATION_DETL_LOG>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_BOOKING_REQUEST = new HashSet<TBL_LOAN_BOOKING_REQUEST>();
            TBL_LOAN_COLLATERAL_MAPPING = new HashSet<TBL_LOAN_COLLATERAL_MAPPING>();
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
        }

        [Key]
        public int LOANAPPLICATIONDETAILID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PROPOSEDPRODUCTID { get; set; }

        public int PROPOSEDTENOR { get; set; }

        public double PROPOSEDINTERESTRATE { get; set; }

        [Column(TypeName = "money")]
        public decimal PROPOSEDAMOUNT { get; set; }

        public short APPROVEDPRODUCTID { get; set; }

        public int APPROVEDTENOR { get; set; }

        public double APPROVEDINTERESTRATE { get; set; }

        [Column(TypeName = "money")]
        public decimal APPROVEDAMOUNT { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        public short SUBSECTORID { get; set; }

        public short STATUSID { get; set; }

        [Required]
        [StringLength(500)]
        public string LOANPURPOSE { get; set; }

        public bool HASDONECHECKLIST { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_COLLATERAL> TBL_LOAN_APPLICATION_COLLATERAL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL_EDU> TBL_LOAN_APPLICATION_DETAIL_EDU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL_INV> TBL_LOAN_APPLICATION_DETAIL_INV { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL_TRA> TBL_LOAN_APPLICATION_DETAIL_TRA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_LOG> TBL_LOAN_APPLICATION_DETL_LOG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_BOOKING_REQUEST> TBL_LOAN_BOOKING_REQUEST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_COLLATERAL_MAPPING> TBL_LOAN_COLLATERAL_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }
    }
}
