namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DOCUMENT_CATEGORY")]
    public partial class TBL_DOCUMENT_CATEGORY
    {
        [Key]
        public int DOCUMENTCATEGORYID { get; set; }

        [Required]
        //////[StringLength(250)]
        public string DOCUMENTCATEGORYNAME { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual ICollection<TBL_DOCUMENT_TYPE> TBL_DOCUMENT_TYPE { get; set; }

    }
}