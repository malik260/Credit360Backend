namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CREDIT_TEMPLATE")]
    public partial class TBL_CREDIT_TEMPLATE
    {
        [Key]
        public int CREDITTEMPLATEID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(250)]
        public string TEMPLATETITLE { get; set; }

        [Required]
        public string TEMPLATEDOCUMENT { get; set; }

        public int APPROVALLEVELID { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
