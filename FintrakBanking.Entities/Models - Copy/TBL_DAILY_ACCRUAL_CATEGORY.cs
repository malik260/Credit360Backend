namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DAILY_ACCRUAL_CATEGORY")]
    public partial class TBL_DAILY_ACCRUAL_CATEGORY
    {
        public TBL_DAILY_ACCRUAL_CATEGORY()
        {
            TBL_DAILY_ACCRUAL = new HashSet<TBL_DAILY_ACCRUAL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CATEGORYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CATEGORYNAME { get; set; }

        public virtual ICollection<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }
    }
}
