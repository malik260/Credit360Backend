namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CHECKLIST_DETAIL")]
    public partial class TBL_CHECKLIST_DETAIL
    {
        public TBL_CHECKLIST_DETAIL()
        {
            //CHECKLISTSTATUSID2 = false;
            //CHECKLISTSTATUSID3 = false;
        }

        [Key]
        public long CHECKLISTID { get; set; }

        public int CHECKLISTDEFINITIONID { get; set; }

        //[StringLength(500)]
        public string REMARK { get; set; }

        public int CHECKEDBY { get; set; }

        public short TARGETTYPEID { get; set; }

        public int TARGETID { get; set; }

        public int? TARGETID2 { get; set; }

        public short ? CHECKLISTSTATUSID { get; set; }

        public bool? CHECKLISTSTATUSID2 { get; set; }

        public bool? CHECKLISTSTATUSID3 { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? DEFEREDDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? INITIAL_DEFEREDDATE { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHECKLIST_DEFINITION TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual TBL_CHECKLIST_STATUS TBL_CHECKLIST_STATUS { get; set; }

        public virtual TBL_CHECKLIST_TARGETTYPE TBL_CHECKLIST_TARGETTYPE { get; set; }
    }
}
