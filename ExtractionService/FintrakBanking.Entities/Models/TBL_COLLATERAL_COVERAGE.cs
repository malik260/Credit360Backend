using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
   public class TBL_COLLATERAL_COVERAGE
    {
        [Key]
        public int COLLATERALCOVERAGEID { get; set; }
        public int COLLATERALSUBTYPEID { get; set; }
        public int COVERAGE { get; set; }
        public int? CURRENCYID { get; set; }
        public int? INTERESTRATECOVERAGE { get; set; }
        public int? COMPANYID { get; set; }
        public int? CREATEDBY { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public bool? DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
