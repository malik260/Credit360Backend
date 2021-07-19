namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKSTAGING.STG_CUSTOMER")]
    public partial class STG_CUSTOMER
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string CUSTOMERCODE { get; set; }

        [StringLength(255)]
        public string BRANCHCODE { get; set; }

        public int? COMPANY { get; set; }

        [StringLength(255)]
        public string TITLE { get; set; }

        [StringLength(255)]
        public string CONTACTADDRESS { get; set; }

        [StringLength(255)]
        public string LASTCONTACTADDRESS { get; set; }

        [StringLength(255)]
        public string FIRSTNAME { get; set; }

        [StringLength(255)]
        public string MIDDLENAME { get; set; }

        [StringLength(255)]
        public string LASTNAME { get; set; }

        [StringLength(255)]
        public string GENDER { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        [StringLength(255)]
        public string NATIONALITY { get; set; }

        [StringLength(255)]
        public string MARITALSTATUS { get; set; }

        [StringLength(255)]
        public string EMAILADDRESS { get; set; }

        [StringLength(255)]
        public string MAIDENNAME { get; set; }

        [StringLength(255)]
        public string OCCUPATION { get; set; }

        [StringLength(255)]
        public string CUSTOMERTYPE { get; set; }

        [StringLength(255)]
        public string RELATIONSHIPOFFICERCODE { get; set; }

        public bool? POLITICALLYEXPOSEDPERSON { get; set; }

        [StringLength(255)]
        public string MISCODE { get; set; }

        [StringLength(255)]
        public string STAFFCODE { get; set; }

        public bool? CUSTOMERSENSITIVITYLEVEL { get; set; }

        [StringLength(255)]
        public string FSCAPTIONGROUPCODE { get; set; }

        [StringLength(255)]
        public string TAXIDNUMBER { get; set; }

        [StringLength(255)]
        public string BUSINESSTAXIDNUMBER { get; set; }

        [StringLength(255)]
        public string ELECTRICMETERNUMBER { get; set; }

        [StringLength(255)]
        public string BANKVERIFICATIONNUMBER { get; set; }

        [StringLength(255)]
        public string OFFICEADDRESS { get; set; }

        [StringLength(255)]
        public string NEARESTLANDMARK { get; set; }

        public DateTime? DATEOFINCORPORATION { get; set; }

        [StringLength(255)]
        public string PAIDUPCAPITAL { get; set; }

        [StringLength(255)]
        public string AUTHORIZEDCAPITAL { get; set; }

        [StringLength(255)]
        public string EMPLOYERDETAILS { get; set; }

        [StringLength(255)]
        public string RCNUMBER { get; set; }

        [StringLength(255)]
        public string SUBSECTORCODE { get; set; }

        [StringLength(255)]
        public string SECTORCODE { get; set; }
    }
}
