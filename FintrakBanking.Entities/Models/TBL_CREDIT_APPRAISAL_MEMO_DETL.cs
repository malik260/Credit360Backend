namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CREDIT_APPRAISAL_MEMO_DETL")]
    public partial class TBL_CREDIT_APPRAISAL_MEMO_DETL
    {
        [Key]
        public int MEMORANDUMLOANDETAILID { get; set; }

        public int APPRAISALMEMORANDUMID { get; set; }

        [Column(TypeName = "money")]
        public decimal PRINCIPALAMOUNT { get; set; }

        public double INTERESTRATE { get; set; }

        public int TENOR { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_CREDIT_APPRAISAL_MEMORANDM TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }
    }
}
