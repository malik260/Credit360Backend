namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DAY_COUNT_CONVENTION")]
    public partial class TBL_DAY_COUNT_CONVENTION
    {
        public TBL_DAY_COUNT_CONVENTION()
        {
            TBL_DAILY_ACCRUAL = new HashSet<TBL_DAILY_ACCRUAL>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DAYCOUNTCONVENTIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string DAYCOUNTCONVENTIONNAME { get; set; }

        public int DAYSINAYEAR { get; set; }

        public int INUSE { get; set; }

        public virtual ICollection<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
