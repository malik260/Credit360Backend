namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHECKLIST_TYPE")]
    public partial class TBL_CHECKLIST_TYPE
    {
        public TBL_CHECKLIST_TYPE()
        {
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
            TBL_CHECKLIST_TYPE_APROV_LEVL = new HashSet<TBL_CHECKLIST_TYPE_APROV_LEVL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CHECKLIST_TYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string CHECKLIST_TYPE_NAME { get; set; }

        public bool ISPRODUCT_BASED { get; set; }

        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual ICollection<TBL_CHECKLIST_TYPE_APROV_LEVL> TBL_CHECKLIST_TYPE_APROV_LEVL { get; set; }
    }
}
