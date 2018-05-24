namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_CUSTOMER_COMPANYINFO")]
    public partial class TBL_TEMP_CUSTOMER_COMPANYINFO
    {
        [Key]
        public int TEMPCOMPANYINFOMATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        [StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        [StringLength(200)]
        public string COMPANYNAME { get; set; }

        [StringLength(200)]
        public string COMPANYWEBSITE { get; set; }

        [StringLength(200)]
        public string COMPANYEMAIL { get; set; }

        [StringLength(200)]
        public string REGISTEREDOFFICE { get; set; }

        [StringLength(200)]
        public string ANNUALTURNOVER { get; set; }

        [StringLength(200)]
        public string CORPORATEBUSINESSCATEGORY { get; set; }

        public int? PAIDUPCAPITAL { get; set; }

        public int? AUTHORISEDCAPITAL { get; set; }

        //[Column(TypeName = "money")]
        public decimal? SHAREHOLDER_FUND { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
