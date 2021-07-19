using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_DOC_TEMPLATE")]
    public partial class TBL_DOC_TEMPLATE
    {
        [Key]
        public int TEMPLATEID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(100)]
        public string TEMPLATENAME { get; set; }

        public int STAFFROLEID { get; set; }

        public int OPERATIONID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
