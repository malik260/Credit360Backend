namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DOCUMENT_CATEGORY_TYPE")]
    public partial class TBL_DOCUMENT_CATEGORY_TYPE
    {
        [Key]
        public int DOCUMENTCATEGORYTYPEID { get; set; }

        public int DOCUMENTTYPEID { get; set; }

        public int DOCUMENTCATEGORYID { get; set; }

        public string DESCRIPTION { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}