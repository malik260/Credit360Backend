namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_CUSTOMER_PHONCONTACT")]
    public partial class TBL_TEMP_CUSTOMER_PHONCONTACT
    {
        [Key]
        public int TEMPPHONECONTACTID { get; set; }

        public int PHONECONTACTID { get; set; }

        [StringLength(20)]
        public string PHONE { get; set; }

        [StringLength(12)]
        public string PHONENUMBER { get; set; }

        public int CUSTOMERID { get; set; }

        public bool ACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
