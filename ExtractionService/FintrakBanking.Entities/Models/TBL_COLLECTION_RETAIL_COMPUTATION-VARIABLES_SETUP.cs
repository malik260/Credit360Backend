using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP")]
    public class TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP
    {
        [Key]
        public int COMPUTATIONVARIABLEID { get; set; }
        public decimal VAT { get; set; }
        public decimal WHT { get; set; }
        public decimal COMMISSIONPAYABLE { get; set; }
        public decimal COMMISSIONPAYABLELIMIT { get; set; }
        public int COMMISSIONRATEEXTERNAL { get; set; }
        public decimal RECOVEREDAMOUNTBELOW { get; set; }
        public decimal RECOVEREDAMOUNTABOVE { get; set; }
        public decimal RECOVEREDAMOUNTEXTERNALBELOW { get; set; }
        public decimal RECOVEREDAMOUNTEXTERNALABOVE { get; set; }
        public int COMMISSIONRATEEXTERNALTWO { get; set; }
        public bool DELETED { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
    }
}
