namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAK_TRAN_PROC_DETAILS")]
    public partial class FINTRAK_TRAN_PROC_DETAILS
    {
        [Key]
        public int SID { get; set; }

        //[StringLength(20)]
        public string BATCH_ID { get; set; }

        public int BATCH_REF_ID { get; set; }

        //[StringLength(10)]
        public string TRAN_TYPE { get; set; }

        //[StringLength(10)]
        public string FLOW_TYPE { get; set; }

        public decimal AMT { get; set; }

        //[StringLength(16)]
        public string DR_ACCT { get; set; }

        //[StringLength(16)]
        public string CR_ACCT { get; set; }

        //[StringLength(4)]
        public string REF_CRNCY_CODE { get; set; }

        //[StringLength(5)]
        public string RATE_CODE { get; set; }

        public decimal RATE { get; set; }

        //[StringLength(50)]
        public string NARRATION { get; set; }

        //[StringLength(255)]
        public string STATUS { get; set; }

        //[StringLength(9)]
        public string TRAN_ID { get; set; }

        //[StringLength(1)]
        public string PSTD_FLG { get; set; }

        public DateTime PSTD_DATE { get; set; }

        public DateTime RCRE_DATE { get; set; }

        //[StringLength(15)]
        public string PSTD_USR_ID { get; set; }

        //[StringLength(1)]
        public string FAIL_FLG { get; set; }

        //[StringLength(1)]
        public string DEL_FLG { get; set; }

        //[StringLength(4)]
        public string FAILURE_REASON_CODE { get; set; }

        //[StringLength(200)]
        public string FAILURE_REASON { get; set; }

        public decimal AMT_COLLECTED { get; set; }

        public decimal LIEN_AMT { get; set; }

        //[StringLength(1)]
        public string LIEN_FLG { get; set; }

        //[StringLength(1)]
        public string TOD_FLG { get; set; }

        public int VALUE_DATE_NUM { get; set; }

        //[StringLength(50)]
        public string LOAN_ACCT { get; set; }

        //[StringLength(4)]
        public string BANK_ID { get; set; }

        //[StringLength(1)]
        public string FINTRAK_FLG { get; set; }
    }
}
