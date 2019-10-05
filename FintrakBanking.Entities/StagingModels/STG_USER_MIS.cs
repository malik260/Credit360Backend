using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("STG_USER_MIS")]
   public partial class STG_USER_MIS
    {
        [Key]
        public int USERMISID { get; set; }
        public string LOGINID { get; set; }
        public string PROFITCENTERDEFINITIONCODE { get; set; }
        public string PROFITCENTERMISCODE { get; set; }

        public string COSTCENTERDEFINITIONCODE { get; set; }
        public string COSTCENTERMISCODE { get; set; }
        public string ACTIVE { get; set; }
        public string DELETED { get; set; }

        public string CREATEDBY { get; set; }
        public DateTime CREATEDON { get; set; }
        public string UPDATEDBY { get; set; }
        public DateTime UPDATEDON { get; set; }
        public string ROWVERSION { get; set; }
    }
}
