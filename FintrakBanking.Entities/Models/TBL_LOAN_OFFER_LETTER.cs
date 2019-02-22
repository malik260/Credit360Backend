using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_OFFER_LETTER")]
  public  class TBL_LOAN_OFFER_LETTER
    {
        [Key]
        public int OFFERLETTERID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public bool ISLMS { get; set; }

        public string OFFERLETTERCLAUSES { get; set; }

        public string OFFERLETTERACCEPTANCE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool? ISACCEPTED { get; set; }

        public bool ISFINAL { get; set; }

    }
}
