namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_FREQUENCY_TYPE")]
    public partial class TBL_FREQUENCY_TYPE
    {
        public TBL_FREQUENCY_TYPE()
        {
            TBL_CALL_MEMO_LIMIT = new HashSet<TBL_CALL_MEMO_LIMIT>();
            TBL_COLLATERAL_POLICY = new HashSet<TBL_COLLATERAL_POLICY>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN1 = new HashSet<TBL_LOAN>();
            TBL_LOAN2 = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION_COVENANT = new HashSet<TBL_LOAN_APPLICATION_COVENANT>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_ARCHIVE1 = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_ARCHIVE2 = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_COVENANT_DETAIL = new HashSet<TBL_LOAN_COVENANT_DETAIL>();
            TBL_TEMP_COLLATERAL_POLICY = new HashSet<TBL_TEMP_COLLATERAL_POLICY>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_LOAN1 = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_LOAN2 = new HashSet<TBL_TEMP_LOAN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FREQUENCYTYPEID { get; set; }

        [Required]
        [StringLength(20)]
        public string MODE_ { get; set; }

        public decimal VALUE { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        public int? ISVISIBLE { get; set; }

        public int NUMBEROFDAYS { get; set; }

        public virtual ICollection<TBL_CALL_MEMO_LIMIT> TBL_CALL_MEMO_LIMIT { get; set; }

        public virtual ICollection<TBL_COLLATERAL_POLICY> TBL_COLLATERAL_POLICY { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN1 { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN2 { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE1 { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE2 { get; set; }

        public virtual ICollection<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }

        public virtual ICollection<TBL_TEMP_COLLATERAL_POLICY> TBL_TEMP_COLLATERAL_POLICY { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN1 { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN2 { get; set; }
    }
}
