namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CHECKLIST_TYPE_APROV_LEVL")]
    public partial class TBL_CHECKLIST_TYPE_APROV_LEVL
    {
        [Key]
        public int CHECKLISTTYPE_APPROVALLEVEL { get; set; }

        public short CHECKLIST_TYPEID { get; set; }

        public int APPROVALLEVELID { get; set; }

        public bool CANVALIDATE { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_CHECKLIST_TYPE TBL_CHECKLIST_TYPE { get; set; }
    }
}
