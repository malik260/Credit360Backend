namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_CUSTOMER_EMPLOYER")]
    public partial class TBL_TEMP_CUSTOMER_EMPLOYER
    {
        [Key]
        public int TEMPEMPLOYERID { get; set; }

        [Required]
        //[StringLength(300)]
        public string EMPLOYER_NAME { get; set; }

        public short EMPLOYER_SUB_TYPEID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(500)]
        public string ADDRESS { get; set; }

        public int CITYID { get; set; }

        //[StringLength(50)]
        public string PHONENUMBER { get; set; }

        //[StringLength(50)]
        public string EMAILADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER_EMPLOYER_TYPE_SUB TBL_CUSTOMER_EMPLOYER_TYPE_SUB { get; set; }
    }
}
