using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_TEMP_LC_USSANCE")]
    public partial class TBL_TEMP_LC_USSANCE
    {
        [Key]
        public int TEMPLCUSSANCEID { get; set; }
        public int LCISSUANCEID { get; set; }
        public int LCUSSANCEID { get; set; }
        public string EXTENSIONREFNUMBER { get; set; }
        public int OLDUSSANCETENOR { get; set; }
        public int NEWUSSANCETENOR { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public int USANCEEXTENSIONAPPLICATIONSTATUSID { get; set; }
        public DateTime OLDLCUSSANCEMATURITYDATE { get; set; }
        public DateTime NEWLCUSSANCEMATURITYDATE { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
