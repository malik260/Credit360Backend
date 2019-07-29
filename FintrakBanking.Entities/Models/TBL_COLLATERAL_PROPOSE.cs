using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
   public class TBL_COLLATERAL_PROPOSE
    {
        [Key]
        public int COLLATERALPROPOSEID { get; set; }
        public int COLLATERALCUSTOMERID  { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public decimal COLLATERALVALUE { get; set; }
        public decimal PROPOSELOANVALUE { get; set; }
        public decimal COLLATERALCOVERAGE { get; set; }
        public decimal BALANCEAVAILABLE { get; set; }
        public int COLLATERALUSESAGESTATUSID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime DATETIMEUPDATED { get; set; }
        public bool DELETED  { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime DATETIMEDELETED { get; set; }
    }
}
