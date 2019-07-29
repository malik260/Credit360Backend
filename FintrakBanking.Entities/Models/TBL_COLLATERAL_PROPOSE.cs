using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
   public class TBL_COLLATERAL_PROPOSE
    {
        public int COLLATERALPROPOSEID { get; set; }
        public int COLLATERALCUSTOMERID  { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public decimal COLLATERALVLAUE { get; set; }
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
