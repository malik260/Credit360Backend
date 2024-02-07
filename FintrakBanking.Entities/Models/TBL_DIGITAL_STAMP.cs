using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_DIGITAL_STAMP")]
    public class TBL_DIGITAL_STAMP
    {
        [Key]
        public int DIGITALSTAMPID { get; set; }
        public int STAFFROLEID { get; set; }
        public string STAMPNAME { get; set; }
        public string DIGITALSTAMP { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public int UPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
       

    }
}
