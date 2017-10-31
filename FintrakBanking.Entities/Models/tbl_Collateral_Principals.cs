namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_PRINCIPALS")]
    public partial class TBL_COLLATERAL_PRINCIPALS
    {
        [Key]
        public short PRINCIPALSID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRINCIPALSREGNUMBER { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        public int? COMPANYID { get; set; }

        public short? CITYID { get; set; }

        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [StringLength(50)]
        public string PRINCIPALSBVN { get; set; }

        public short? COUNTRYID { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
