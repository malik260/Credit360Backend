namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_CUSTOMER")]
    public partial class TBL_TEMP_CUSTOMER
    {
        [Key]
        public int TEMPCUSTOMERID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public short BRANCHID { get; set; }

        public int COMPANYID { get; set; }

        //[StringLength(50)]
        public string TITLE { get; set; }

        [Required]
        //[StringLength(200)]
        public string FIRSTNAME { get; set; }

        //[StringLength(200)]
        public string MIDDLENAME { get; set; }

        //[StringLength(200)]
        public string LASTNAME { get; set; }

        //[StringLength(10)]
        public string GENDER { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEOFBIRTH { get; set; }

        //[StringLength(200)]
        public string PLACEOFBIRTH { get; set; }

        //[StringLength(20)]
        public string NATIONALITY { get; set; }

        public int? MARITALSTATUS { get; set; }

        //[StringLength(100)]
        public string EMAILADDRESS { get; set; }

        //[StringLength(200)]
        public string MAIDENNAME { get; set; }

        //[StringLength(200)]
        public string SPOUSE { get; set; }

        //[StringLength(100)]
        public string OCCUPATION { get; set; }

        public short? CUSTOMERTYPEID { get; set; }

        public int? RELATIONSHIPOFFICERID { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public bool ISREALATEDPARTY { get; set; }

        //[StringLength(200)]
        public string MISCODE { get; set; }

        //[StringLength(200)]
        public string MISSTAFF { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEACTEDON { get; set; }

        //[StringLength(150)]
        public string ACTEDONBY { get; set; }

        public bool ACCOUNTCREATIONCOMPLETE { get; set; }

        public bool CREATIONMAILSENT { get; set; }

        public short CUSTOMERSENSITIVITYLEVELID { get; set; }

        public short? SUBSECTORID { get; set; }

        public short? FSCAPTIONGROUPID { get; set; }

        //[StringLength(50)]
        public string TAXNUMBER { get; set; }

        //[StringLength(50)]
        public string CUSTOMERBVN { get; set; }

        //[StringLength(50)]
        public string CUSTOMERNIN { get; set; }

        public short? RISKRATINGID { get; set; }

        public bool? VALIDATED { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEVALIDATED { get; set; }

        //[StringLength(50)]
        public string PROSPECTCUSTOMERCODE { get; set; }

        public bool? ISPROSPECT { get; set; }

        public int? CRMSLEGALSTATUSID { get; set; }

        public int? CRMSCOMPANYSIZEID { get; set; }

        public int? CRMSRELATIONSHIPTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
