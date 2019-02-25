using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_DOC_TEMPLATE_SECTION")]
    public partial class TBL_DOC_TEMPLATE_SECTION
    {
        [Key]
        public int TEMPLATESECTIONID { get; set; }

        public int TEMPLATEID { get; set; }

        public string TITLE { get; set; }

        public string DESCRIPTION { get; set; }

        public string TEMPLATEDOCUMENT { get; set; }

        public int POSITION { get; set; }

        public bool ISDISABLED { get; set; }

        public bool CANEDIT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public string TEMPLATESECTIONCODE { get; set; }
    }
}
