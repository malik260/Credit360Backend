namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_PHONECONTACT")]
    public partial class TBL_CUSTOMER_PHONECONTACT
    {
        [Key]
        public int PHONECONTACTID { get; set; }

        [StringLength(20)]
        public string PHONE { get; set; }

        [StringLength(12)]
        public string PHONENUMBER { get; set; }

        public int CUSTOMERID { get; set; }

        public bool ACTIVE { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
