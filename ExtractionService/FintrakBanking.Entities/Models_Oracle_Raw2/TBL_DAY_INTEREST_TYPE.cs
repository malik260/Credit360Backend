namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DAY_INTEREST_TYPE")]
    public partial class TBL_DAY_INTEREST_TYPE
    {
        public TBL_DAY_INTEREST_TYPE()
        {
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DAYINTERESTTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string DAYINTERESTTYPENAME { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }
    }
}
