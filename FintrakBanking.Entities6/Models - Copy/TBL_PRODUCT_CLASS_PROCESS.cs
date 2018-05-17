namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_CLASS_PROCESS")]
    public partial class TBL_PRODUCT_CLASS_PROCESS
    {
        public TBL_PRODUCT_CLASS_PROCESS()
        {
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_PRODUCT_CLASS = new HashSet<TBL_PRODUCT_CLASS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCT_CLASS_PROCESSID { get; set; }

        [Required]
        [StringLength(100)]
        public string PRODUCT_CLASS_PROCESS_NAME { get; set; }

        public int USE_AMOUNT_LIMIT { get; set; }

        public decimal? MAXIMUM_AMOUNT { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_PRODUCT_CLASS> TBL_PRODUCT_CLASS { get; set; }
    }
}
