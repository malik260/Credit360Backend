namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_STATUS")]
    public partial class TBL_LOAN_APPLICATION_STATUS
    {
        public TBL_LOAN_APPLICATION_STATUS()
        {
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION1 = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPLICATIONSTATUSID { get; set; }

        [Required]
        [StringLength(100)]
        public string APPLICATIONSTATUSNAME { get; set; }

        public int POSITION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION1 { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }
    }
}
