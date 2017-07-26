namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Account_KYC_Item")]
    public partial class tbl_Customer_Account_KYC_Item
    {
        [Key]
        public int CustomerAccountKYCItemId { get; set; }

        public short? KYCItemId { get; set; }

        public int CustomerId { get; set; }

        [StringLength(100)]
        public string AccountNumber { get; set; }

        public bool? Provided { get; set; }

        public bool? Deferred { get; set; }

        public bool? Waived { get; set; }

        public bool? Disapproved { get; set; }

        public bool? Approved { get; set; }

        public bool? DateApproved { get; set; }

        public bool? ApprovedBy { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_KYC_Item tbl_KYC_Item { get; set; }
    }
}
