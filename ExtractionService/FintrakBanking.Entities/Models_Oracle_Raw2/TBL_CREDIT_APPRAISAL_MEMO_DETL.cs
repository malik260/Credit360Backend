namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CREDIT_APPRAISAL_MEMO_DETL")]
    public partial class TBL_CREDIT_APPRAISAL_MEMO_DETL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MEMORANDUMLOANDETAILID { get; set; }

        public int APPRAISALMEMORANDUMID { get; set; }

        public decimal PRINCIPALAMOUNT { get; set; }

        public decimal INTERESTRATE { get; set; }

        public int TENOR { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_CREDIT_APPRAISAL_MEMORANDM TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }
    }
}
