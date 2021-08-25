namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_COMPANY_DIREC_TYP")]
    public partial class TBL_CUSTOMER_COMPANY_DIREC_TYP
    {
        public TBL_CUSTOMER_COMPANY_DIREC_TYP()
        {
            TBL_CUSTOMER_COMPANY_DIRECTOR = new HashSet<TBL_CUSTOMER_COMPANY_DIRECTOR>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYDIRECTORYTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string COMPANYDIRECTORYTYPENAME { get; set; }

        public virtual ICollection<TBL_CUSTOMER_COMPANY_DIRECTOR> TBL_CUSTOMER_COMPANY_DIRECTOR { get; set; }
    }
}
