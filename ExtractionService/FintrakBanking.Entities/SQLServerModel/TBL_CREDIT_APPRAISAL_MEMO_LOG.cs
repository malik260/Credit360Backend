namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CREDIT_APPRAISAL_MEMO_LOG")]
    public partial class TBL_CREDIT_APPRAISAL_MEMO_LOG
    {
        [Key]
        public int CAMDOCUMENTATIONLOGID { get; set; }

        [Required]
        public string CAMDOCUMENTATION { get; set; }

        public int APPRAISALMEMORANDUMID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_CREDIT_APPRAISAL_MEMORANDM TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }
    }
}
