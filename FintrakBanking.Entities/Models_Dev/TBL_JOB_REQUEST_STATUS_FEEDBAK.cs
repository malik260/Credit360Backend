namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_JOB_REQUEST_STATUS_FEEDBAK")]
    public partial class TBL_JOB_REQUEST_STATUS_FEEDBAK
    {
        public TBL_JOB_REQUEST_STATUS_FEEDBAK()
        {
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short JOB_STATUS_FEEDBACKID { get; set; }

        [Required]
        [StringLength(100)]
        public string JOB_STATUS_FEEDBACK_NAME { get; set; }

        public short REQUESTSTATUSID { get; set; }

        public short JOBTYPEID { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        public virtual TBL_JOB_REQUEST_STATUS TBL_JOB_REQUEST_STATUS { get; set; }

        public virtual TBL_JOB_TYPE TBL_JOB_TYPE { get; set; }
    }
}
