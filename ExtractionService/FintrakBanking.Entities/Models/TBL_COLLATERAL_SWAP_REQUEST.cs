using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_SWAP_REQUEST")]
    public partial class TBL_COLLATERAL_SWAP_REQUEST
    {
        [Key]
        public int COLLATERALSWAPID { get; set; }
        [ForeignKey("TBL_LOAN_COLLATERAL_MAPPING")]
        public int LOANCOLLATERALMAPPINGID { get; set; }
        [ForeignKey("TBL_LOAN_APPLICATION_COLLATERL")]
        public int LOANAPPCOLLATERALID { get; set; }
        public int OLDCOLLATERALID { get; set; }
        public int NEWCOLLATERALID { get; set; }
        public int? CUSTOMERID { get; set; }
        public string SWAPREF { get; set; }
        public int? COLLATERALSWAPSTATUSID { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
        public virtual TBL_LOAN_COLLATERAL_MAPPING TBL_LOAN_COLLATERAL_MAPPING { get; set; }
        public virtual TBL_LOAN_APPLICATION_COLLATERL TBL_LOAN_APPLICATION_COLLATERL { get; set; }


    }
}
