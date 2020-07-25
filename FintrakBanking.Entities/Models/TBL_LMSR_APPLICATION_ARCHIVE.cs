using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LMSR_APPLICATION_ARCHIVE")]
    public partial class TBL_LMSR_APPLICATION_ARCHIVE
    {
        [Key]
        public int LOANAPPLICATIONARCHID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public string APPLICATIONREFERENCENUMBER { get; set; }
        public string RELATEDREFERENCENUMBER { get; set; }
        public int COMPANYID { get; set; }
        public int? CUSTOMERID { get; set; }
        public short BRANCHID { get; set; }
        public int? CUSTOMERGROUPID { get; set; }
        public DateTime APPLICATIONDATE { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime SYSTEMDATETIME { get; set; }
        public short APPROVALSTATUSID { get; set; }
        public short APPLICATIONSTATUSID { get; set; }
        public int? FINALAPPROVAL_LEVELID { get; set; }
        public short? NEXTAPPLICATIONSTATUSID { get; set; }
        public DateTime? APPROVEDDATE { get; set; }
        public DateTime? AVAILMENTDATE { get; set; }
        public bool DISPUTED { get; set; }
        public bool REQUIRECOLLATERAL { get; set; }
        public int? CAPREGIONID { get; set; }
        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
        public int OPERATIONID { get; set; }
        public short? RISKRATINGID { get; set; }
        public short? PRODUCTCLASSID { get; set; }
        public int? PRODUCTID { get; set; }
        public short? LOANAPPLICATIONTYPEID { get; set; }
        public short? PRODUCT_CLASS_PROCESSID { get; set; }
        public decimal? APPROVEDAMOUNT { get; set; }
        public bool? ISPROJECTRELATED { get; set; }
        public bool? ISONLENDING { get; set; }
        public bool? ISINTERVENTIONFUNDS { get; set; }
        public bool? WITHINSTRUCTION { get; set; }
        public bool? DOMICILIATIONNOTINPLACE { get; set; }
        public DateTime ARCHIVEDATE { get; set; }
        public int OWNEDBY { get; set; }
    }
}
