using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LC_CASHBUILDUPPLAN")]
    public partial class TBL_LC_CASHBUILDUPPLAN
    {
        [Key]
        public int LCCASHBUILDUPPLANID { get; set; }

        public int LCISSUANCEID { get; set; }

        public decimal AMOUNT { get; set; }

        public int CURRENCYID { get; set; }

        public int CASHBUILDUPREFERENCETYPEID { get; set; }

        public int COLLECTIONCASAACCOUNTID { get; set; }

        public int DAYSINTERVAL { get; set; }

        public DateTime? PLANDATE { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
