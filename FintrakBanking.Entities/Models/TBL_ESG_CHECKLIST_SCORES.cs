namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_CHECKLIST_SCORES")]
    public partial class TBL_ESG_CHECKLIST_SCORES
    {
        [Key]
        public int CHECKLISTSCORESID { get; set; }

        //public int CHECKLISTITEMID { get; set; }

        public int CHECKLISTSTATUSID { get; set; }

        //[StringLength(100)]
        public string GRADE { get; set; }
        public int? SCORE { get; set; }
        public int CHECKLIST_TYPEID { get; set; }
        public string SCOREWEIGHT { get; set; }
        public string SCORECOLORCODE { get; set; }

        public string STATUSNAME { get; set; }

        public int? CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
