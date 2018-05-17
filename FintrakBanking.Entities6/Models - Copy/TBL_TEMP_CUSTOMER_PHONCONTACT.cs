namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUSTOMER_PHONCONTACT")]
    public partial class TBL_TEMP_CUSTOMER_PHONCONTACT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPPHONECONTACTID { get; set; }

        public int PHONECONTACTID { get; set; }

        [StringLength(20)]
        public string PHONE { get; set; }

        [StringLength(12)]
        public string PHONENUMBER { get; set; }

        public int CUSTOMERID { get; set; }

        public int ACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
