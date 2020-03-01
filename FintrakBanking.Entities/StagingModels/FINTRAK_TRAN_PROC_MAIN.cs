namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAK_TRAN_PROC_MAIN")]
    public partial class FINTRAK_TRAN_PROC_MAIN
    {
        [Key]
        public int SID { get; set; }

        //[StringLength(20)]
        public string BATCH_ID { get; set; }

        //[StringLength(10)]
        public string TRAN_TYPE { get; set; }

        public DateTime? RCRE_DATE { get; set; }

        //[StringLength(20)]
        public string RCRE_USER { get; set; }

        public decimal? TOTAL_AMT { get; set; }

        public decimal? REC_COUNT { get; set; }

        //[StringLength(3)]
        public string STATUS { get; set; }

        //[StringLength(1)]
        public string PSTD_FLG { get; set; }

        public DateTime? PSTD_DATE { get; set; }

        //[StringLength(15)]
        public string PSTD_USR_ID { get; set; }

        //[StringLength(1)]
        public string DEL_FLG { get; set; }

        //[StringLength(1)]
        public string IS_SELECTED { get; set; }

        //[StringLength(2)]
        public string BANK_ID { get; set; }

        public decimal? TOTAL_AMT_COLLECTED { get; set; }
    }
}
