using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.DocumentModels
{
  public  partial class TBL_LOAN_CONTINGENT_USAGE_DOCS
    {
        [Key]
        public int DOCUMENTID { get; set; }

        public int CONTINGENTLOANUSAGEID { get; set; }

        [Required]
        //[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public DateTime DATECREATED { get; set; }

        public int CREATEDBY { get; set; }

        public string PHYSICALFILENUMBER { get; set; }

        public string PHYSICALLOCATION { get; set; }
    }
}
