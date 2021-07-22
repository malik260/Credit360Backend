using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_DOC_TEMPLATE_SECTION_ROLE")]
    public partial class TBL_DOC_TEMPLATE_SECTION_ROLE
    {
        [Key]
        public int SECTIONROLEID { get; set; }

        public int TEMPLATESECTIONID { get; set; }

        public int STAFFROLEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
