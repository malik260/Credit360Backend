using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PSR_PROJECT_FACILITIES")]
    public class TBL_PSR_PROJECT_FACILITIES
    {
        [Key]
        public int PROJECTFACITYID { get; set; }
        public int PROJECTSITEREPORTID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int? LOANID { get; set; }
        public int? LOANAPPLICATIONDETAILID { get; set; }
        public int? LANSYSTEMTYPEID { get; set; }
        //public int? LOANSYSTEMTYPEID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        
    }
}
