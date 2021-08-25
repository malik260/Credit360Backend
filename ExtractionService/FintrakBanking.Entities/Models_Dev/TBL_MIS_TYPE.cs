namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_MIS_TYPE")]
    public partial class TBL_MIS_TYPE
    {
        public TBL_MIS_TYPE()
        {
            TBL_MIS_INFO = new HashSet<TBL_MIS_INFO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short MISTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string MISTYPE { get; set; }

        [StringLength(50)]
        public string CATEGORY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual ICollection<TBL_MIS_INFO> TBL_MIS_INFO { get; set; }
    }
}
