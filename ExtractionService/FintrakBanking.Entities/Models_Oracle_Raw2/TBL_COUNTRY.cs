namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COUNTRY")]
    public partial class TBL_COUNTRY
    {
        public TBL_COUNTRY()
        {
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
            TBL_PUBLIC_HOLIDAY = new HashSet<TBL_PUBLIC_HOLIDAY>();
            TBL_REGION = new HashSet<TBL_REGION>();
            TBL_STATE = new HashSet<TBL_STATE>();
            TBL_STOCK_COMPANY = new HashSet<TBL_STOCK_COMPANY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COUNTRYID { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(10)]
        public string COUNTRYCODE { get; set; }

        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }

        public virtual ICollection<TBL_PUBLIC_HOLIDAY> TBL_PUBLIC_HOLIDAY { get; set; }

        public virtual ICollection<TBL_REGION> TBL_REGION { get; set; }

        public virtual ICollection<TBL_STATE> TBL_STATE { get; set; }

        public virtual ICollection<TBL_STOCK_COMPANY> TBL_STOCK_COMPANY { get; set; }
    }
}
