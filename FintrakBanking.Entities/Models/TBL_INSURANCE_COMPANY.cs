
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_INSURANCE_COMPANY")]
    public partial class TBL_INSURANCE_COMPANY
    {
        [Key]
        public int INSURANCECOMPANYID { get; set; }

        [Required]
        //[StringLength(200)]
        public string COMPANYNAME { get; set; }


        [Required]
        //[StringLength(50)]
        public string ADDRESS { get; set; }

        [Required]
        //[StringLength(150)]
        public string CONTACTEMAIL { get; set; }

        [Required]
        //[StringLength(20)]
        public string PHONENUMBER { get; set; }

        public int COMPANYID { get; set; }

        public int? CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }

    }
}
