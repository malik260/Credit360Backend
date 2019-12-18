using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_SUGGESTED_CONDITION")]
    public partial class TBL_SUGGESTED_CONDITION
    {
        [Key]
        public int SUGGESTEDCONDITIONID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int SUGGESTIONTYPEID { get; set; }

        [Required]
        //[StringLength(1000)]
        public string DESCRIPTION { get; set; }


        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
