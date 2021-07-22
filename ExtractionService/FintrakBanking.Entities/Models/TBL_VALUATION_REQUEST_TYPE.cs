using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_VALUATION_REQUEST_TYPE")]
    public partial class TBL_VALUATION_REQUEST_TYPE
    {
        [Key]
        public int VALUATIONREQUESTTYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string VALUATIONREQUESTTYPE { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public DateTime DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
