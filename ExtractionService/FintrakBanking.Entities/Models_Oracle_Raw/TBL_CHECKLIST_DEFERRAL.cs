namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHECKLIST_DEFERRAL")]
    public partial class TBL_CHECKLIST_DEFERRAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHECKLISTDEFERRALID { get; set; }

        public int CONDITIONID { get; set; }

        public DateTime? DEFEREDDATE { get; set; }
    }
}
