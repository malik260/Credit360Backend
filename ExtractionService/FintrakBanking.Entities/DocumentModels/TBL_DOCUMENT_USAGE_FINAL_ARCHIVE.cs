using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.DocumentModels
{
    [Table("TBL_DOCUMENT_USAGE_FINAL_ARCHIVE")]
    public partial class TBL_DOCUMENT_USAGE_FINAL_ARCHIVE
    {
        [Key] 
        public int DOCUMENTUSAGEFINALID { get; set; }
        public int DOCUMENTUSAGEID { get; set; }
        public DateTime? ARCHIVEDATE { get; set; }
        public int DOCUMENTUPLOADID { get; set; }

        public int TARGETID { get; set; }

        public string TARGETCODE { get; set; }

        public string TARGETREFERENCENUMBER { get; set; }

        public string DOCUMENTCODE { get; set; }

        public string DOCUMENTTITLE { get; set; }

        public string CUSTOMERCODE { get; set; }

        public int OPERATIONID { get; set; }

        public int? APPROVALSTATUSID { get; set; }

        public int? DOCUMENTSTATUSID { get; set; }

        public bool ISPRIMARYDOCUMENT { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
