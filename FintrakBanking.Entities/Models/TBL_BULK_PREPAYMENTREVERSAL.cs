using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_BULK_PREPAYMENTREVERSAL")]
    public partial class TBL_BULK_PREPAYMENTREVERSAL
    {
        [Key]
        public int BULK_PREPAYMENTREVERSALID { get; set; }
        public int BATCHID { get; set; }
        public string LOANREFERENCENUMBER { get; set; }
        public Decimal AMOUNT { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public DateTime? PROCESSDATE { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int? CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? CUSTOMERID { get; set; }

    }
}
