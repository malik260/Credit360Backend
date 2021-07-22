namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CASA")]
    public partial class TBL_CASA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CASA()
        {
            TBL_CASA_OVERDRAFT = new HashSet<TBL_CASA_OVERDRAFT>();
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION_COVENANT = new HashSet<TBL_LOAN_APPLICATION_COVENANT>();
            TBL_LMSR_APPLICATION_COVENANT = new HashSet<TBL_LMSR_APPLICATION_COVENANT>();
            TBL_LOAN_APPLICATION_DETAIL = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETAIL1 = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            //TBL_LMSR_APPLICATION_DETAIL = new HashSet<TBL_LMSR_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETL_BG = new HashSet<TBL_LOAN_APPLICATION_DETL_BG>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_ARCHIVE1 = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_COVENANT_DETAIL = new HashSet<TBL_LOAN_COVENANT_DETAIL>();
            TBL_LOAN_RECOVERY_PLAN = new HashSet<TBL_LOAN_RECOVERY_PLAN>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN1 = new HashSet<TBL_LOAN>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
        }

        [Key]
        public int CASAACCOUNTID { get; set; }

        [Required]
        //[StringLength(50)]
        public string PRODUCTACCOUNTNUMBER { get; set; }

        [Required]
        //[StringLength(100)]
        public string PRODUCTACCOUNTNAME { get; set; }

        public int CUSTOMERID { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public short BRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public bool ISCURRENTACCOUNT { get; set; }

        public int? TENOR { get; set; }

        public decimal? INTERESTRATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? EFFECTIVEDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? TERMINALDATE { get; set; }

        public int? ACTIONBY { get; set; }

        public DateTime? ACTIONDATE { get; set; }

        public short ACCOUNTSTATUSID { get; set; }

        public int? OPERATIONID { get; set; }

        //[Column(TypeName = "money")]
        public decimal AVAILABLEBALANCE { get; set; }

        //[Column(TypeName = "money")]
        public decimal LEDGERBALANCE { get; set; }

        public int? RELATIONSHIPOFFICERID { get; set; }

        public int? RELATIONSHIPMANAGERID { get; set; }

        //[StringLength(50)]
        public string MISCODE { get; set; }

        //[StringLength(50)]
        public string TEAMMISCODE { get; set; }

        //[Column(TypeName = "money")]
        public decimal? OVERDRAFTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal? OVERDRAFTINTERESTRATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? OVERDRAFTEXPIRYDATE { get; set; }

        public bool? HASOVERDRAFT { get; set; }

        //[Column(TypeName = "money")]
        public decimal LIENAMOUNT { get; set; }

        public bool HASLIEN { get; set; }

        public short POSTNOSTATUSID { get; set; }

        //[StringLength(50)]
        public string OLDPRODUCTACCOUNTNUMBER1 { get; set; }

        //[StringLength(50)]
        public string OLDPRODUCTACCOUNTNUMBER2 { get; set; }

        //[StringLength(50)]
        public string OLDPRODUCTACCOUNTNUMBER3 { get; set; }

        //[StringLength(50)]
        public string REFRESHBATCHID { get; set; }

        public DateTime? LASTREFRESHDATETIME { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public short? APROVALSTATUSID { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA_OVERDRAFT> TBL_CASA_OVERDRAFT { get; set; }

        public virtual TBL_CASA_ACCOUNTSTATUS TBL_CASA_ACCOUNTSTATUS { get; set; }

        public virtual TBL_CASA_POSTNOSTATUS TBL_CASA_POSTNOSTATUS { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }
        public virtual ICollection<TBL_LMSR_APPLICATION_COVENANT> TBL_LMSR_APPLICATION_COVENANT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }
        //public virtual ICollection<TBL_LMSR_APPLICATION_DETAIL> TBL_LMSR_APPLICATION_DETAIL { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL1 { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_BG> TBL_LOAN_APPLICATION_DETL_BG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_RECOVERY_PLAN> TBL_LOAN_RECOVERY_PLAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

    }
}
