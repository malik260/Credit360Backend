using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PSR_COMMENT_IMAGES")]
    public partial class TBL_PSR_COMMENT_IMAGES
    {
        [Key]
        public int PSRCOMMENTIMAGEID { get; set; }

        public byte[] FILEDATA { get; set; }
        public string IMAGECAPTION { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int PROJECTSITEREPORTID { get; set; }
        public string FILENAME { get; set; }
        public string FILEEXTENSION { get; set; }
        public int FILESIZE { get; set; }
        public string FILESIZEUNIT { get; set; }
    }
}
