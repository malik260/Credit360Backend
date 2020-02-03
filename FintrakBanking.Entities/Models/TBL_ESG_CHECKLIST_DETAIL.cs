namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_CHECKLIST_DETAIL")]
    public partial class TBL_ESG_CHECKLIST_DETAIL
    {
        [Key]
        public int ESGCHECKLISTDETAILID { get; set; }

        public int ESGCHECKLISTDEFINITIONID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }
        public int CHECKLIST_TYPEID { get; set; }
        public short ESGCLASSID { get; set; }
        public short ESGTYPEID { get; set; }
        public short CHECKLISTSTATUSID { get; set; }
        public string DESCRIPTION { get; set; }
        public string COMMENT_ { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
