namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_PhoneContact")]
    public partial class tbl_Customer_PhoneContact
    {
        [Key]
        public int PhoneContactId { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(12)]
        public string PhoneNumber { get; set; }

        public int CustomerId { get; set; }

        public bool Active { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}
