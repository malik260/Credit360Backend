namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUSTOMER")]
    public partial class TBL_TEMP_CUSTOMER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCUSTOMERID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public int BRANCHID { get; set; }

        public int COMPANYID { get; set; }

        [StringLength(50)]
        public string TITLE { get; set; }

        [Required]
        [StringLength(200)]
        public string FIRSTNAME { get; set; }

        [StringLength(200)]
        public string MIDDLENAME { get; set; }

        [StringLength(200)]
        public string LASTNAME { get; set; }

        [StringLength(10)]
        public string GENDER { get; set; }

        [StringLength(200)]
        public string PLACEOFBIRTH { get; set; }

        [StringLength(20)]
        public string NATIONALITY { get; set; }

        public int? MARITALSTATUS { get; set; }

        [StringLength(100)]
        public string EMAILADDRESS { get; set; }

        [StringLength(200)]
        public string MAIDENNAME { get; set; }

        [StringLength(200)]
        public string SPOUSE { get; set; }

        [StringLength(100)]
        public string OCCUPATION { get; set; }

        public int? CUSTOMERTYPEID { get; set; }

        public int? RELATIONSHIPOFFICERID { get; set; }

        public int ISPOLITICALLYEXPOSED { get; set; }

        public int ISINVESTMENTGRADE { get; set; }

        public int ISREALATEDPARTY { get; set; }

        [StringLength(200)]
        public string MISCODE { get; set; }

        [StringLength(200)]
        public string MISSTAFF { get; set; }

        [StringLength(150)]
        public string ACTEDONBY { get; set; }

        public int ACCOUNTCREATIONCOMPLETE { get; set; }

        public int CREATIONMAILSENT { get; set; }

        public int CUSTOMERSENSITIVITYLEVELID { get; set; }

        public int? SUBSECTORID { get; set; }

        public int? FSCAPTIONGROUPID { get; set; }

        [StringLength(50)]
        public string TAXNUMBER { get; set; }

        [StringLength(50)]
        public string CUSTOMERBVN { get; set; }

        [StringLength(50)]
        public string CUSTOMERNIN { get; set; }

        public int? RISKRATINGID { get; set; }

        public int? VALIDATED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public DateTime? DATEVALIDATED { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
