namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_ACCOUNT_KYC_ITEM")]
    public partial class TBL_CUSTOMER_ACCOUNT_KYC_ITEM
    {
        [Key]
        public int CUSTOMERACCOUNTKYCITEMID { get; set; }

        public short? KYCITEMID { get; set; }

        public int CUSTOMERID { get; set; }

        [StringLength(100)]
        public string ACCOUNTNUMBER { get; set; }

        public bool? PROVIDED { get; set; }

        public bool? DEFERRED { get; set; }

        public bool? WAIVED { get; set; }

        public bool? DISAPPROVED { get; set; }

        public bool? APPROVED { get; set; }

        public bool? DATEAPPROVED { get; set; }

        public bool? APPROVEDBY { get; set; }

        [StringLength(100)]
        public string CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_KYC_ITEM TBL_KYC_ITEM { get; set; }
    }
}
