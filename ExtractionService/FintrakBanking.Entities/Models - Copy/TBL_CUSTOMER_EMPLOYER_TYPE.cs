namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_EMPLOYER_TYPE")]
    public partial class TBL_CUSTOMER_EMPLOYER_TYPE
    {
        public TBL_CUSTOMER_EMPLOYER_TYPE()
        {
            TBL_CUSTOMER_EMPLOYER_TYPE_SUB = new HashSet<TBL_CUSTOMER_EMPLOYER_TYPE_SUB>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EMPLOYER_TYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string EMPLOYER_TYPE_NAME { get; set; }

        public virtual ICollection<TBL_CUSTOMER_EMPLOYER_TYPE_SUB> TBL_CUSTOMER_EMPLOYER_TYPE_SUB { get; set; }
    }
}
