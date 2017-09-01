namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Address")]
    public partial class tbl_Customer_Address
    {
        [Key]
        public int AddressId { get; set; }

        public int CustomerId { get; set; }

        public int StateId { get; set; }

        public int CityId { get; set; }

        public int AddressTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(200)]
        public string HomeTown { get; set; }

        [StringLength(20)]
        public string POBox { get; set; }

        [StringLength(300)]
        public string NearestLandmark { get; set; }

        [StringLength(50)]
        public string ElectricMeterNumber { get; set; }

        public bool Active { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}
