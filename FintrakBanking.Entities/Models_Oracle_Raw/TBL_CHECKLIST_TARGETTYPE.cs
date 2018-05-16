namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHECKLIST_TARGETTYPE")]
    public partial class TBL_CHECKLIST_TARGETTYPE
    {
        public TBL_CHECKLIST_TARGETTYPE()
        {
            TBL_CHECKLIST_DETAIL = new HashSet<TBL_CHECKLIST_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TARGETTYPEID { get; set; }

        [StringLength(50)]
        public string TARGETTYPENAME { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CHECKLIST_DETAIL> TBL_CHECKLIST_DETAIL { get; set; }
    }
}
