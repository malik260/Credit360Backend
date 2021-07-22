namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_SECTOR_CODE")]
    public partial class STG_SECTOR_CODE
    {
        [Key]
        //[StringLength(10)]
        public string SECTOR_CODE { get; set; }
        //[StringLength(255)]
        public string REF_DESC { get; set; }
        //[StringLength(2)]
        public string DEL_FLG { get; set; }
        //[StringLength(2)]
        public string BANK_ID { get; set; }
    }
}
