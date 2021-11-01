using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
    [Table("STG_SUB_BASICTRANSACTION")]
    public class STG_SUB_BASICTRANSACTION
    {
        [Key]
        public int ID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public string APPLICATIONREFERENCENUMBER { get; set; }
        public string RELATEDREFERENCENUMBER { get; set; }
        public int SUBSIDIARYID { get; set; }
        public int? CUSTOMERID { get; set; }
        public string COUNTRYCODE { get; set; }
        public long? CUSTOMERGLOBALID { get; set; }
        public DateTime APPLICATIONDATE { get; set; }
        public double INTERESTRATE { get; set; }
        public int APPLICATIONTENOR { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public int APPROVALLEVELID { get; set; }
        public int APPROVALLEVELGLOBALCODE { get; set; }
        public int? TOSTAFFID { get; set; }
        public short APPLICATIONSTATUSID { get; set; }
        public string OPERATIONNAME { get; set; }
        public string PRODUCTCLASSNAME { get; set; }
        public string PRODUCTNAME { get; set; }
        public string PRODUCT_CLASS_PROCESS{ get; set; }
        public string LOANAPPLICATIONTYPENAME { get; set; }
        public string FIRSTNAME { get; set; }
        public string MIDDLENAME { get; set; }
        public string LASTNAME { get; set; }
        public DateTime? SYSTEMARRIVALDATETIME { get; set; }
        public string BUSINESSUNITSHORTCODE { get; set; }
        public decimal APPLICATIONAMOUNT { get; set; }
        public decimal TOTALEXPOSUREAMOUNT { get; set; }
        public int CREATEDBY { get; set; }
        public string CREATEDBYNAME { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime? SYSTEMDATETIME { get; set; }
    }
}
