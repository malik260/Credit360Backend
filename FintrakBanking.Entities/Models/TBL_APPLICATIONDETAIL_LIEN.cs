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
        public float AMOUNT { get; set; }
        public string ACCOUNTNO { get; set; }
        public bool ISRELEASED { get; set; }
    } 
}
