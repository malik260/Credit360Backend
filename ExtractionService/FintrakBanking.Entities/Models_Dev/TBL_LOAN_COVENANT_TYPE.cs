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
        public short COVENANTTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string COVENANTTYPENAME { get; set; }

        public bool REQUIREAMOUNT { get; set; }

        public bool REQUIREFREQUENCY { get; set; }

        public bool REQUIRECASAACCOUNT { get; set; }

        public bool ISCLEANUPCYCLE { get; set; }

        public int COMPANYID { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }

        public virtual ICollection<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }
    }
}
