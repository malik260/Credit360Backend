using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ORIGINAL_DOCUMENT_RELEASE")]
    public partial class TBL_ORIGINAL_DOCUMENT_RELEASE
    {
        [Key]
        public int ORIGINALDOCUMENTRELEASEID { get; set; }
        [Required]
        public int ORIGINALDOCUMENTAPPROVALID { get; set; }
        [Required]
        public int DOCUMENTUPLOADID { get; set; }
        public int? DOCSUBMISSIONOPERATIONID { get; set; }
        public short APPROVALSTATUSID { get; set; }
        public int COMPANYID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime? APPROVALDATE { get; set; }
        public int? COLLATERALCUSTOMERID { get; set; }

        public int NUMBEROFTIMESAPPROVE { get; set; }
        public short? PERFECTIONSTATUSID { get; set; }
        public short? LITIGATIONSTATUSID { get; set; }
        public bool? ISONAMCONLIST { get; set; }
    }
}
