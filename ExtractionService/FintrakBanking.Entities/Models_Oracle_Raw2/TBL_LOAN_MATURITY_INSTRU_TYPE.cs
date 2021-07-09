namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_MATURITY_INSTRU_TYPE")]
    public partial class TBL_LOAN_MATURITY_INSTRU_TYPE
    {
        public TBL_LOAN_MATURITY_INSTRU_TYPE()
        {
            TBL_LOAN_MATURITY_INSTRUCTION = new HashSet<TBL_LOAN_MATURITY_INSTRUCTION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int INSTRUCTIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string INSTRUCTIONTYPENAME { get; set; }

        public virtual ICollection<TBL_LOAN_MATURITY_INSTRUCTION> TBL_LOAN_MATURITY_INSTRUCTION { get; set; }
    }
}
