using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.DocumentModels
{
    [Table("TBL_TEMP_DOC_INVOICE")]
    public class TBL_TEMP_DOC_INVOICE
    {
        [Key]
        public int DOCUMENTID { get; set; }

        public int TARGETID { get; set; }

        [Required]
        //[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int CREATEDBY { get; set; }
    }
}
