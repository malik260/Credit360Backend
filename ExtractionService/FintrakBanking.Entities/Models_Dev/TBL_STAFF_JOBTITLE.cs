namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STAFF_JOBTITLE")]
    public partial class TBL_STAFF_JOBTITLE
    {
        public TBL_STAFF_JOBTITLE()
        {
            TBL_STAFF = new HashSet<TBL_STAFF>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int JOBTITLEID { get; set; }

        [Required]
        [StringLength(50)]
        public string JOBTITLENAME { get; set; }

        public int COMPANYID { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
