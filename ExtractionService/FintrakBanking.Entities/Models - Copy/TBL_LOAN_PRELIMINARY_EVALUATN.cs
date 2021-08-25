namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_PRELIMINARY_EVALUATN")]
    public partial class TBL_LOAN_PRELIMINARY_EVALUATN
    {
        public TBL_LOAN_PRELIMINARY_EVALUATN()
        {
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANPRELIMINARYEVALUATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRELIMINARYEVALUATIONCODE { get; set; }

        public int COMPANYID { get; set; }

        public int BRANCHID { get; set; }

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

        public int APPROVALSTATUSID { get; set; }

        public int ISCURRENT { get; set; }

        public int SENTFOREVALUATION { get; set; }

        public int SENTFORLOANAPPLICATION { get; set; }

        public decimal LOANAMOUNT { get; set; }

        public int? PRODUCTCLASSID { get; set; }

        public int? SUBSECTORID { get; set; }

        public int LOANAPPLICATIONTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual TBL_LOAN_APPLICATION_TYPE TBL_LOAN_APPLICATION_TYPE { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }
    }
}
