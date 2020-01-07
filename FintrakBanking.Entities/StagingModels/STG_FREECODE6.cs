namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_FREECODE6")]
    public partial  class STG_FREECODE6
    {
        [Key]
        //[StringLength(10)]
        public string FREECODE6 { get; set; }
        //[StringLength(255)]
        public string REF_DESC { get; set; }
        //[StringLength(2)]
        public string DEL_FLG { get; set; }
        //[StringLength(2)]
        public string BANK_ID { get; set; }
    }
}
