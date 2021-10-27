using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_INSURANCE_POLICY_TYPE")]
    public partial class TBL_INSURANCE_POLICY_TYPE
    {

        [Key]
        public int POLICYTYPEID { get; set; }
        public string DESCRIPTION { get; set; }
        public bool VALUATIONREQUIRED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
        public int? DELETEDBY { get; set; }
        public bool DELETED { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
