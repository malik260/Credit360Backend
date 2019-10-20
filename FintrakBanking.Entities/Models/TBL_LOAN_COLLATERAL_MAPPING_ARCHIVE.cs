using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATRL_MAPPING_ARCHIVE")]
    public partial class TBL_LOAN_COLLATERAL_MAPPING_ARCHIVE
    {
        [Key]
        public int LOANCOLLATERALMAPPINGARCHIVEID { get; set; }
        [ForeignKey("TBL_LOAN_COLLATERAL_MAPPING")]
        int LOANCOLLATERALMAPPINGID { get; set; }
        [ForeignKey("TBL_LOAN_APPLICATION_COLLATERL")]
        public int LOANAPPCOLLATERALID { get; set; }
        [ForeignKey("TBL_COLLATERAL_SWAP_REQUEST")]
        public int COLLATERALSWAPID { get; set; }
        [ForeignKey("TBL_COLLATERAL_CUSTOMER")]
        public int COLLATERALCUSTOMERID { get; set; }
        [ForeignKey("TBL_COLLATERAL_CUSTOMER")]
        public int NEWCOLLATERALCUSTOMERID { get; set; }

        public int LOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public bool ISRELEASED { get; set; }

        public short? RELEASEAPPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
