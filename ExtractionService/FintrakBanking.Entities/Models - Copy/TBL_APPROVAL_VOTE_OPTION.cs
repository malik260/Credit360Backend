namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_VOTE_OPTION")]
    public partial class TBL_APPROVAL_VOTE_OPTION
    {
        public TBL_APPROVAL_VOTE_OPTION()
        {
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VOTE_OPTIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string VOTE_OPTION_NAME { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }
    }
}
