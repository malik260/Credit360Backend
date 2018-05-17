namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_COVENANT_TYPE")]
    public partial class TBL_LOAN_COVENANT_TYPE
    {
        public TBL_LOAN_COVENANT_TYPE()
        {
            TBL_LOAN_APPLICATION_COVENANT = new HashSet<TBL_LOAN_APPLICATION_COVENANT>();
            TBL_LOAN_COVENANT_DETAIL = new HashSet<TBL_LOAN_COVENANT_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COVENANTTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string COVENANTTYPENAME { get; set; }

        public int REQUIREAMOUNT { get; set; }

        public int REQUIREFREQUENCY { get; set; }

        public int REQUIRECASAACCOUNT { get; set; }

        public int ISCLEANUPCYCLE { get; set; }

        public int COMPANYID { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }

        public virtual ICollection<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }
    }
}
