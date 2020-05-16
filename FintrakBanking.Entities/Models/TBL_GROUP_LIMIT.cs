using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_GROUP_LIMIT")]
    public partial class TBL_GROUP_LIMIT
    {
        [Key]
        public int GROUPLIMITID { get; set; }
        public string GROUPNAME { get; set; }
        public decimal GROUPLIMITVALUE { get; set; }
        public string DESCRIPTION { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public int? LIMITNUMBER { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
