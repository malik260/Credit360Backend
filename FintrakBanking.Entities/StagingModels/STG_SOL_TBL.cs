namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_SOL_TBL")]
    public partial class STG_SOL_TBL
    {
        [Key]
        //[StringLength(8)]
        public string SOL_ID { get; set; }
        //[StringLength(132)]
        public string BRANCH_NAME { get; set; }
        //[StringLength(3)]
        public string BOD_SYS_DATE { get; set; }
        public string BANK_ID { get; set; }
        
    }
}


