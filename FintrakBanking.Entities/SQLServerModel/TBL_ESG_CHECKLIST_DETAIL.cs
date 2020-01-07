namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_ESG_CHECKLIST_DETAIL")]
    public partial class TBL_ESG_CHECKLIST_DETAIL
    {
        [Key]
        public int ESGCHECKLISTDETAILID { get; set; }

        public int ESGCHECKLISTDEFINITIONID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public short ESGCLASSID { get; set; }

        public short ESGTYPEID { get; set; }

        public short CHECKLISTSTATUSID { get; set; }

        //[StringLength(2000)]
        public string DESCRIPTION { get; set; }

        //[StringLength(1000)]
        public string COMMENT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHECKLIST_STATUS TBL_CHECKLIST_STATUS { get; set; }

        public virtual TBL_ESG_CHECKLIST_DEFINITION TBL_ESG_CHECKLIST_DEFINITION { get; set; }

        public virtual TBL_ESG_CLASS TBL_ESG_CLASS { get; set; }

        public virtual TBL_ESG_TYPE TBL_ESG_TYPE { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
