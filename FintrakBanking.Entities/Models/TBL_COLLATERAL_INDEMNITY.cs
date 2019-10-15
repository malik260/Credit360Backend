using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_INDEMNITY")]
    public  class TBL_COLLATERAL_INDEMNITY
    {
        [Key]
        public int COLLATERALINDEMNITYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public decimal? SECURITYVALUE { get; set; }

        public string FIRSTNAME { get; set; }

        public string MIDDLENAME { get; set; }

        public string LASTNAME { get; set; }

        public string BVN { get; set; }

        public string PHONENUMBER1 { get; set; }

        public string PHONENUMBER2 { get; set; }

        public string ADDRESS { get; set; }

        public string TAXNUMBER { get; set; }

        public string RELATIONSHIP { get; set; }

        public string RELATIONSHIPDURATION { get; set; }

        public string EMAILADRRESS { get; set; }

        public DateTime?  STARTDATE { get; set; }

        public DateTime?  ENDDATE { get; set; }

        public string  REMARK { get; set; }
        public string  DESCRIPTION { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
