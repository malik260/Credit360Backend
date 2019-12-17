namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_GL_SUBHEAD_TBL")]
    public partial class STG_GL_SUBHEAD_TBL
    {
        [Key]
        //[StringLength(6)]
        public string GL_SUB_HEAD_CODE { get; set; }
        //[StringLength(3)]
        public string CRNCY_CODE { get; set; }
        //[StringLength(6)]
        public string SCHM_CODE { get; set; }
    }
}



