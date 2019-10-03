namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_EOD_OPERATION_LOG")]
    public partial class TBL_EOD_OPERATION_LOG
    {
        [Key]
        public int EODOPERATIONLOGID { get; set; }

        [Required]
        public int EODOPERATIONID { get; set; }
        
        public DateTime? STARTDATETIME { get; set; }
        
        public DateTime? ENDDATETIME { get; set; }
        
        public DateTime? EODDATE { get; set; }
        
        public int? EODSTATUSID { get; set; }

        public int? COMPANYID { get; set; }
        public string ERRORINFORMATION { get; set; }

    }
}
