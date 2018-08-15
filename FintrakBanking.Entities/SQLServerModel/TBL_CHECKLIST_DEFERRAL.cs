namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CHECKLIST_DEFERRAL")]
    public partial class TBL_CHECKLIST_DEFERRAL
    {
        [Key]
        public int CHECKLISTDEFERRALID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DEFEREDDATE { get; set; }

        public int CONDITIONID { get; set; }
    }
}
