using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_TEMP_LMSR_APPLTN_CANCELTN")]
    public partial class TBL_TEMP_LMSR_APPLTN_CANCELTN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [Key]
        public int TEMPAPPLICATIONCANCELLATIONID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public short APPLICATIONSTATUSID { get; set; }
        public string CANCELLATIONREASON { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime DATETIMEUPDATED { get; set; }
        public bool ISCURRENT { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public virtual TBL_LMSR_APPLICATION TBL_LMSR_APPLICATION { get; set; }
    }
}
