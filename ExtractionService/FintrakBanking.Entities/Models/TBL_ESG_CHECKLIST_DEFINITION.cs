namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_CHECKLIST_DEFINITION")]
    public partial class TBL_ESG_CHECKLIST_DEFINITION
    {
        [Key]
        public int ESGCHECKLISTDEFINITIONID { get; set; }

        public int CHECKLISTITEMID { get; set; }
        public int? ESGCATEGORYID { get; set; }
        public int? ESGSUBCATEGORYID { get; set; }
        public bool ISCOMPULSORY { get; set; }
        public string ITEMDESCRIPTION { get; set; }
        public int YESCHECKLISTSCORESID { get; set; }
        public int NOCHECKLISTSCORESID { get; set; }
        public int CHECKLIST_TYPEID { get; set; }
        public int? SECTORID { get; set; }
        //public int SCORE { get; set; }
        public int COMPANYID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
