using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CRMS_REGULATORY")]
    public partial class TBL_CRMS_REGULATORY
    {
        [Key]
        public int CRMSREGULATORYID { get; set; }

        public int COMPANYID { get; set; }

        public int CRMSTYPEID { get; set; }
        [Required]
        //[StringLength(100)]
        public string CODE { get; set; }

        public string DESCRIPTION { get; set; }
        public int? CUSTOMERTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
