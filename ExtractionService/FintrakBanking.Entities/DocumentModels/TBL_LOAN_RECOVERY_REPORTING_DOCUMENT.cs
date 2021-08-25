using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_REPORTING_DOCUMENT")]
    public partial class TBL_LOAN_RECOVERY_REPORTING_DOCUMENT
    {
        [Key]
        public int LOANRECOVERYREPORTDOCUMENTID { get; set; }
        public string REFERENCEID { get; set; }
        public int TARGETID { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
        public int? OPERATIONID { get; set; }
        public string FILENAME { get; set; }
        public string FILEEXTENSION { get; set; }
        public int FILESIZE { get; set; }
        public byte[] FILEDATA { get; set; }
        public string DESCRIPTION { get; set; }
        public string FILESIZEUNIT { get; set; }


    }
}
