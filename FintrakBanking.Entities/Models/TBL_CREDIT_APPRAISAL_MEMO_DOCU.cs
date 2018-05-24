namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CREDIT_APPRAISAL_MEMO_DOCU")]
    public partial class TBL_CREDIT_APPRAISAL_MEMO_DOCU
    {
        [Key]
        public int CAMDOCUMENTATIONID { get; set; }

        [Required]
        public string CAMDOCUMENTATION { get; set; }

        public int APPRAISALMEMORANDUMID { get; set; }

        public int APPROVALLEVELID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_CREDIT_APPRAISAL_MEMORANDM TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }
    }
}
