using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{

    [Table("TBL_TEMP_APPROVAL_LEVEL_STAFF")]
    public partial class TBL_TEMP_APPROVAL_LEVEL_STAFF
    {
        [Key]
        public int TEMPSTAFFLEVELID { get; set; }

        public int STAFFLEVELID { get; set; }

        public int STAFFID { get; set; }

        public int APPROVALLEVELID { get; set; }

        //[Column(TypeName = "money")]
        public decimal MAXIMUMAMOUNT { get; set; }

        public short PROCESSVIEWSCOPEID { get; set; }

        public int POSITION { get; set; }

        public bool CANVIEWDOCUMENT { get; set; }

        public bool CANVIEWUPLOAD { get; set; }

        public bool CANVIEWAPPROVAL { get; set; }

        public bool CANAPPROVE { get; set; }

        public bool CANUPLOAD { get; set; }

        public bool CANEDIT { get; set; }

        public bool VETOPOWER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }
        public string OPERATION { get; set; }

        //public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        //    public virtual TBL_STAFF TBL_STAFF { get; set; }
    }

}
