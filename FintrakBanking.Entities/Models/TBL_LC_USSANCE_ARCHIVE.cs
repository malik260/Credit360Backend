using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LC_USSANCE_ARCHIVE")]
    public partial class TBL_LC_USSANCE_ARCHIVE
    {
        [Key]
        public int LCUSSANCEARCHIVEID { get; set; }
        public int LCUSSANCEID { get; set; }
        public int LCISSUANCEID { get; set; }
        public int? USSANCETENOR { get; set; }
        public decimal USSANCEAMOUNT { get; set; }
        public decimal USSANCEAMOUNTLOCAL { get; set; }
        public double? USSANCERATE { get; set; }
        public int? USANCEAMOUNTCURRENCYID { get; set; }
        public DateTime? LCUSSANCEEFFECTIVEDATE { get; set; }
        public DateTime? LCUSSANCEMATURITYDATE { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public int? CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public DateTime ARCHIVEDATE { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? USANCEAPPLICATIONSTATUSID { get; set; }
        public int? USANCEAPPROVALSTATUSID { get; set; }
        public string USANCEREF { get; set; }
        public int ARCHIVINGOPERATIONID { get; set; }
    }
}
