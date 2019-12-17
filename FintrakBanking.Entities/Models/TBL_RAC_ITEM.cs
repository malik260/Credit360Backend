namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RAC_ITEM")]
    public partial class TBL_RAC_ITEM
    {
        public TBL_RAC_ITEM()
        {
            TBL_RAC_DEFINITION = new HashSet<TBL_RAC_DEFINITION>();
        }

        [Key]
        public int RACITEMID { get; set; }

        [Required]
        //[StringLength(500)]
        public string CRITERIA { get; set; }

        [Required]
        //[StringLength(500)]
        public string DESCRIPTION { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public virtual ICollection<TBL_RAC_DEFINITION> TBL_RAC_DEFINITION { get; set; }

    }
}
