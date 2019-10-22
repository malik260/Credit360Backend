using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_APPLICATIONDETAIL_LIEN")]
    public partial class TBL_APPLICATIONDETAIL_LIEN
    {
        [Key]
        public int APPLICATIONDETAILLIENID { get; set; }
        public int APPLICATIONDETAILID { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }
        public decimal AMOUNT { get; set; }
        public string ACCOUNTNO { get; set; }
        public bool ISRELEASED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    } 
}
