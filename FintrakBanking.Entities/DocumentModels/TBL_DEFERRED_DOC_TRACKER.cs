using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.DocumentModels
{
    [Table ("TBL_DEFERRED_DOC_TRACKER")]
    public class TBL_DEFERRED_DOC_TRACKER
    {
        [Key]
       public int DEFERREDDOCID { get; set; }
       public int DOCUMENTCATEGORYID { get; set; }
       public int DOCUMENTTYPEID { get; set; }
       public int LOANAPPLICATIONID { get; set; }
       public DateTime DUEDATE { get; set; }
       public int CREATEDBY { get; set; }
       public DateTime DATETIMECREATED { get; set; }
       public bool SUBMITTED { get; set; }
       public DateTime? DATETIMESUBMITTED { get; set; }
       public int LASTUPDATEDBY { get; set; }
       public DateTime? DATETIMEUPDATED { get; set; }
       public bool DELETED { get; set; }
       public int DELETEDBY { get; set; }
       public DateTime? DATETIMEDELETED { get; set; }
    }
}
