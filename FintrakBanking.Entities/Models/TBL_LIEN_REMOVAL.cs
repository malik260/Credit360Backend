using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LIEN_REMOVAL")]
    public partial class TBL_LIEN_REMOVAL
    {
        public readonly int COMPANYID;

        [Key]
        public int UNFREEZELIENACCOUNTID { get; set; }
        public int CASALIENACCOUNTID { get; set; }
        public string LOANREFERENCENUMBER { get; set; }
        public string FILENAME { get; set; }
        public string FILEEXTENSION { get; set; }
        public int FILESIZE { get; set; }
        public string FILESIZEUNIT { get; set; }
        public byte[] FILEDATA { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? REQUESTDATE { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public bool OPERATIONCOMPLETED { get; set; }
        public int OPERATIONID { get; set; }
    }
}
