namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_PRUDENTIALGUIDELINE")]
    public partial class TBL_LOAN_PRUDENTIALGUIDELINE
    {
        public TBL_LOAN_PRUDENTIALGUIDELINE()
        {
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRUDENTIALGUIDELINESTATUSID { get; set; }

        [StringLength(250)]
        public string STATUSNAME { get; set; }

        public int PRUDENTIALGUIDELINETYPEID { get; set; }

        public int? INTERNALMINIMUM { get; set; }

        public int? INTERNALMAXIMUM { get; set; }

        public int? EXTERNALMINIMUM { get; set; }

        public int? EXTERNALMAXIMUM { get; set; }

        [StringLength(500)]
        public string NARRATION { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual TBL_LOAN_PRUDENT_GUIDE_TYPE TBL_LOAN_PRUDENT_GUIDE_TYPE { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }
    }
}
