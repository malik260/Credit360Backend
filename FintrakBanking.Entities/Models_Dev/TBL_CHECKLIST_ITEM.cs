namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHECKLIST_ITEM")]
    public partial class TBL_CHECKLIST_ITEM
    {
        public TBL_CHECKLIST_ITEM()
        {
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHECKLISTITEMID { get; set; }

        [Required]
        [StringLength(2000)]
        public string CHECKLISTITEMNAME { get; set; }

        public int RESPONSE_TYPEID { get; set; }

        public bool REQUIREUPLOAD { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual TBL_CHECKLIST_RESPONSE_TYPE TBL_CHECKLIST_RESPONSE_TYPE { get; set; }
    }
}
