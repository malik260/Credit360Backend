namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_SCHEMECODE_TBL")]
    public partial class STG_SCHEMECODE_TBL
    {
        [Key]
        //[StringLength(5)]
        public string SCHEME_CODE { get; set; }
        //[StringLength(100)]
        public string SCHEME_DESCRIPTION { get; set; }
        //[StringLength(3)]
        public string SCHEME_TYPE { get; set; }
    }
}

