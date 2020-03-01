namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LC_SHIPPING")]
    public partial class TBL_LC_SHIPPING
    {
        [Key]
        public int LCSHIPPINGID { get; set; }

        public int LCISSUANCEID { get; set; }

        [Required]
        //[StringLength(200)]
        public string PARTYNAME { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PARTYADDRESS { get; set; }

        [Required]
        //[StringLength(100)]
        public string PORTOFDISCHARGE { get; set; }

        [Required]
        //[StringLength(100)]
        public string PORTOFSHIPMENT { get; set; }

        public DateTime LATESTSHIPMENTDATE { get; set; }

        public bool ISPARTSHIPMENTALLOWED { get; set; }

        public bool ISTRANSSHIPMENTALLOWED { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LC_ISSUANCE TBL_LC_ISSUANCE { get; set; }

    }
}
        /*


            

        */
