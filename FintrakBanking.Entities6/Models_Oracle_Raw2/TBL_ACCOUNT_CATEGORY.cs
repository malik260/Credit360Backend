namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_ACCOUNT_CATEGORY")]
    public partial class TBL_ACCOUNT_CATEGORY
    {
        public TBL_ACCOUNT_CATEGORY()
        {
            TBL_ACCOUNT_TYPE = new HashSet<TBL_ACCOUNT_TYPE>();
            TBL_FINANCIAL_STATEMENT_CAPTN = new HashSet<TBL_FINANCIAL_STATEMENT_CAPTN>();
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCOUNTCATEGORYID { get; set; }

        [Required]
        [StringLength(50)]
        public string ACCOUNTCATEGORYNAME { get; set; }

        public virtual ICollection<TBL_ACCOUNT_TYPE> TBL_ACCOUNT_TYPE { get; set; }

        public virtual ICollection<TBL_FINANCIAL_STATEMENT_CAPTN> TBL_FINANCIAL_STATEMENT_CAPTN { get; set; }

        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
    }
}
