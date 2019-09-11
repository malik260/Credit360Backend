using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_OPERATION_CAMSOL_LIST")]

    public class TBL_OPERATION_CAMSOL_LIST
    {
        [Key]
        public int OPERATIONCAMSOLLISTID { get; set; }
        [ForeignKey("TBL_LOAN_CAMSOL")]
        public int LOAN_CAMSOLID { get; set; }
        public int OPERATIONID { get; set; }
        public int TARGETID { get; set; }
        public int POSITION { get; set; }

        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public virtual TBL_LOAN_CAMSOL TBL_LOAN_CAMSOL { get; set; }
    }
}
