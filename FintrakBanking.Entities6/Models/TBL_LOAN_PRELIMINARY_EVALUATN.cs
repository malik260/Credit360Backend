namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_PRELIMINARY_EVALUATN")]
    public partial class TBL_LOAN_PRELIMINARY_EVALUATN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_PRELIMINARY_EVALUATN()
        {
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
        }

        [Key]
        public int LOANPRELIMINARYEVALUATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRELIMINARYEVALUATIONCODE { get; set; }

        public int COMPANYID { get; set; }

        public short BRANCHID { get; set; }

        public int? CUSTOMERID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        [Required]
        [StringLength(150)]
        public string PROJECTDESCRIPTION { get; set; }

        [Required]
        [StringLength(300)]
        public string CLIENTDESCRIPTION { get; set; }

        [StringLength(300)]
        public string OWNERSHIPSTRUCTURE { get; set; }

        [Required]
        [StringLength(500)]
        public string PROJECTFINANCINGPLAN { get; set; }

        [Required]
        [StringLength(150)]
        public string EXISTINGEXPOSURE { get; set; }

        [Required]
        [StringLength(150)]
        public string BANKROLE { get; set; }

        [Required]
        [StringLength(500)]
        public string COLLATERALARRANGEMENT { get; set; }

        public string RELATEDCOMPANIES { get; set; }

        [Required]
        public string PROPOSEDTERMSANDCONDITIONS { get; set; }

        [Required]
        [StringLength(500)]
        public string IMPLEMENTATIONARRANGEMENTS { get; set; }

        [Required]
        public string MARKETDEMAND { get; set; }

        [Required]
        public string BUSINESSPROFILE { get; set; }

        [Required]
        public string RISKSANDCONCERNS { get; set; }

        [Required]
        [StringLength(500)]
        public string PRUDENT_EXPOSUR_LIMIT_IMPLCATN { get; set; }

        [Required]
        [StringLength(500)]
        public string ENVIRONMENTALIMPACT { get; set; }

        [Required]
        [StringLength(500)]
        public string PORTFOLIOSTRATEGICALIGNMENT { get; set; }

        [Required]
        [StringLength(500)]
        public string COMMERCIALVIABILITYASSESSMENT { get; set; }

        [StringLength(50)]
        public string TAXIDENTIFICATIONNUMBER { get; set; }

        [StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public bool ISCURRENT { get; set; }

        public bool SENTFOREVALUATION { get; set; }

        public bool SENTFORLOANAPPLICATION { get; set; }

        [Column(TypeName = "money")]
        public decimal LOANAMOUNT { get; set; }

        public short? PRODUCTCLASSID { get; set; }

        public short? SUBSECTORID { get; set; }

        public short LOANAPPLICATIONTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual TBL_LOAN_APPLICATION_TYPE TBL_LOAN_APPLICATION_TYPE { get; set; }
    }
}
