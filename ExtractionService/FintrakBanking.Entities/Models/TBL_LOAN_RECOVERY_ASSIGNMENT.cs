using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_ASSIGNMENT")]
    public partial class TBL_LOAN_RECOVERY_ASSIGNMENT
    {
        [Key]
        public int LOANASSIGNID { get; set; }
        public int LOANID { get; set; }
        public string APPLICATIONREFERENCENUMBER { get; set; }
        public string CUSTOMERID { get; set; }
        public int ACCREDITEDCONSULTANT { get; set; }
        public DateTime DATEASSIGNED { get; set; }
        public int CREATEDBY { get; set; }
        public int? OPERATIONID { get; set; }
        public bool OPERATIONCOMPLETED { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        public decimal? TOTALAMOUNTRECOVERY { get; set; }
        public string REFERENCEID { get; set; }
        public bool ISFULLYRECOVERED { get; set; }
        public DateTime? EXPCOMPLETIONDATE { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public string SOURCE { get; set; }
        public string LOANREFERENCE { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int PRODUCTID { get; set; }
        public int? PRODUCTCLASSID { get; set; }
        public string ASSIGNMENTTYPE { get; set; }
        public bool ISMAILSENT { get; set; }
    }
}
