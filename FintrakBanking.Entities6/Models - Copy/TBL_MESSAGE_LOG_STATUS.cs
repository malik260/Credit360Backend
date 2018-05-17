namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_MESSAGE_LOG_STATUS")]
    public partial class TBL_MESSAGE_LOG_STATUS
    {
        public TBL_MESSAGE_LOG_STATUS()
        {
            TBL_MESSAGE_LOG = new HashSet<TBL_MESSAGE_LOG>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MESSAGESTATUSID { get; set; }

        [Required]
        [StringLength(50)]
        public string MESSAGESTATUSNAME { get; set; }

        public virtual ICollection<TBL_MESSAGE_LOG> TBL_MESSAGE_LOG { get; set; }
    }
}
