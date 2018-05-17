namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_OPERATION")]
    public partial class TBL_LOAN_OPERATION
    {
        public TBL_LOAN_OPERATION()
        {
            TBL_LOAN_STATUS = new HashSet<TBL_LOAN_STATUS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OPERATIONID { get; set; }

        [Required]
        [StringLength(20)]
        public string NAME { get; set; }

        public virtual ICollection<TBL_LOAN_STATUS> TBL_LOAN_STATUS { get; set; }
    }
}
