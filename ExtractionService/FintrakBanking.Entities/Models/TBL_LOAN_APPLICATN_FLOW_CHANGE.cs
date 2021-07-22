using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_APPLICATN_FLOW_CHANGE")]
   public partial  class TBL_LOAN_APPLICATN_FLOW_CHANGE
   {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOveridableMethodsInConstructors")]

        [Key]
        public short FLOWCHANGEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string PLACEHOLDER { get; set; }

        public short? PRODUCTCLASSID { get; set; }

        public short? PRODUCTTYPEID { get; set; }

        public short? PRODUCTID { get; set; }

        public bool ISSKIPPROCESSENABLED { get; set; }

        public bool HASOPERATIONBASEDRAC { get; set; }

        public  int OPERATIONID { get; set; }
        //public int INTERESTPAYMENT { get; set; }

        //[StringLength(500)]
        public string DESTINATIONURL { get; set; }

        //[StringLength(50)]
        public string LABEL { get; set; }

        public bool DELETED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int CREATEDBY { get; set; }

        public int? UPDATEDBY { get; set; }
        public int? DATETIMEUPDATED { get; set; }
        //public int DOCUMENTOPERATION { get; set; }

   }
}
