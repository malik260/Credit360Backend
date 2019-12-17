using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace FintrakBanking.Entities.DocumentModels
{

    [Table("TBL_DOCUMENT_TYPE")]
    public partial class TBL_DOCUMENT_TYPE
    {
        [Key]
        public int DOCUMENTTYPEID { get; set; }

        public int DOCUMENTCATEGORYID { get; set; }

        [Required]
        ////[StringLength(250)]
        public string DOCUMENTTYPENAME { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual ICollection<TBL_DOCUMENT_UPLOAD> TBL_DOCUMENT_UPLOAD { get; set; }
        public virtual ICollection<TBL_DOC_MAPPING> TBL_DOC_MAPPING { get; set; }

        public virtual TBL_DOCUMENT_CATEGORY TBL_DOCUMENT_CATEGORY { get; set; }

    }
}