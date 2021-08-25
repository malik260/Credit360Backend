namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_ACCOUNT_KYC_ITEM")]
    public partial class TBL_CUSTOMER_ACCOUNT_KYC_ITEM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERACCOUNTKYCITEMID { get; set; }

        public int? KYCITEMID { get; set; }

        public int CUSTOMERID { get; set; }

        [StringLength(100)]
        public string ACCOUNTNUMBER { get; set; }

        public int? PROVIDED { get; set; }

        public int? DEFERRED { get; set; }

        public int? WAIVED { get; set; }

        public int? DISAPPROVED { get; set; }

        public int? APPROVED { get; set; }

        public int? DATEAPPROVED { get; set; }

        public int? APPROVEDBY { get; set; }

        [StringLength(100)]
        public string CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_KYC_ITEM TBL_KYC_ITEM { get; set; }
    }
}
