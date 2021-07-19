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
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCUSTOMERID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERID { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BRANCHID { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYID { get; set; }

        [StringLength(50)]
        public string TITLE { get; set; }

        [Key]
        [Column(Order = 5)]
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

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ISPOLITICALLYEXPOSED { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ISINVESTMENTGRADE { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ISREALATEDPARTY { get; set; }

        [StringLength(200)]
        public string MISCODE { get; set; }

        [StringLength(200)]
        public string MISSTAFF { get; set; }

        [StringLength(150)]
        public string ACTEDONBY { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCOUNTCREATIONCOMPLETE { get; set; }

        [Key]
        [Column(Order = 10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREATIONMAILSENT { get; set; }

        [Key]
        [Column(Order = 11)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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

        [Key]
        [Column(Order = 12)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREATEDBY { get; set; }

        [Key]
        [Column(Order = 13)]
        public DateTime DATETIMECREATED { get; set; }

        [Key]
        [Column(Order = 14)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ISCURRENT { get; set; }

        [Key]
        [Column(Order = 15)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALSTATUSID { get; set; }

        public DateTime? DATEVALIDATED { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
