namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUST_CLIENT_SUPPLIER")]
    public partial class TBL_TEMP_CUST_CLIENT_SUPPLIER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCLIENT_SUPPLIERID { get; set; }

        public int? CLIENT_SUPPLIERID { get; set; }

        public int CUSTOMERID { get; set; }

        public int CUSTOMERTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string FIRSTNAME { get; set; }

        [StringLength(100)]
        public string MIDDLENAME { get; set; }

        [StringLength(100)]
        public string LASTNAME { get; set; }

        [StringLength(50)]
        public string TAX_NUMBER { get; set; }

        [StringLength(50)]
        public string REGISTRATION_NUMBER { get; set; }

        public int? HAS_CASA_ACCOUNT { get; set; }

        [StringLength(50)]
        public string CASA_ACCOUNTNO { get; set; }

        [StringLength(200)]
        public string BANKNAME { get; set; }

        [StringLength(500)]
        public string NATURE_OF_BUSINESS { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        public int CLIENT_SUPPLIERTYPEID { get; set; }

        [StringLength(100)]
        public string CONTACT_PERSON { get; set; }

        public int CREATEDBY { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public DateTime DATECREATED { get; set; }
    }
}
