namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LMSR_APPLICATION")]
    public partial class TBL_LMSR_APPLICATION
    {
        public TBL_LMSR_APPLICATION()
        {
            TBL_LMSR_APPLICATION_DETAIL = new HashSet<TBL_LMSR_APPLICATION_DETAIL>();
        }

        [Key]
        public int LOANAPPLICATIONID { get; set; }

        [Required]
        //[StringLength(50)]
        public string APPLICATIONREFERENCENUMBER { get; set; }

        //[StringLength(50)]
        public string RELATEDREFERENCENUMBER { get; set; }

        public int COMPANYID { get; set; }

        public int? CUSTOMERID { get; set; }

        public short BRANCHID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        [Column(TypeName = "date")]
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

        [Column(TypeName = "date")]
        public DateTime? APPROVEDDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime? AVAILMENTDATE { get; set; }

        public bool DISPUTED { get; set; }

        public bool REQUIRECOLLATERAL { get; set; }

        public int? CAPREGIONID { get; set; }
        public string CRMSCODE { get; set; }
        public bool? CRMSVALIDATED { get; set; }

        public DateTime? CRMSDATE { get; set; }

        //public int? PROPOSEDTENOR { get; set; }

        //public int? PROPOSEDINTEREST { get; set; }

        // public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        // public virtual TBL_CASA TBL_CASA { get; set; }

        // public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        // public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        // public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        // public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        // public virtual TBL_PRODUCT_CLASS_PROCESS TBL_PRODUCT_CLASS_PROCESS { get; set; }

        // public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual ICollection<TBL_LMSR_APPLICATION_DETAIL> TBL_LMSR_APPLICATION_DETAIL { get; set; }
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
        public decimal TOTALEXPOSUREAMOUNT { get; set; }
        public bool ISAGRICRELATED { get; set; }
    }
}