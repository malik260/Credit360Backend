using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CURRENCY_LIMIT")]
    public partial class TBL_CURRENCY_LIMIT
    {
        [Key]
        public int CURRENCYLIMITID { get; set; }
        public string CURRENCYLIMITNAME { get; set; }
        public decimal CURRENCYLIMITVALUE { get; set; }
        public string DESCRIPTION { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
