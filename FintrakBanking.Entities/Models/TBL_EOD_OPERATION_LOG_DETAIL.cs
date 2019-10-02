namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_EOD_OPERATION_LOG_DETAIL")]
    public partial class TBL_EOD_OPERATION_LOG_DETAIL
    {
        [Key]
        public int EODOPERATIONLOGDETAILID { get; set; }

        public int EODOPERATIONLOGID { get; set; }

        public int? EODOPERATIONID { get; set; }

        public DateTime? STARTDATETIME { get; set; }
        
        public DateTime? ENDDATETIME { get; set; }
        
        public int? EODSTATUSID { get; set; }
        
        public string ERRORINFORMATION { get; set; }

        public string REFERENCENUMBER { get; set; }

        public DateTime? EODDATE { get; set; }

        public int? EODUSERID { get; set; }
    }
}
