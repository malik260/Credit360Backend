using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CUSTOMER_ARCHIVE")]
    public class TBL_CUSTOMER_ARCHIVE
    {
        [Key]
        public int CUSTOMERARCHIVEID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        public string CUSTOMERCODE { get; set; }

        public short BRANCHID { get; set; }

        public int COMPANYID { get; set; }

        public string TITLE { get; set; }

        [Required]
        public string FIRSTNAME { get; set; }

        public string MIDDLENAME { get; set; }

        public string LASTNAME { get; set; }

        public string GENDER { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        public string PLACEOFBIRTH { get; set; }

        public int? NATIONALITYID { get; set; }

        public int? MARITALSTATUS { get; set; }

        public string EMAILADDRESS { get; set; }

        public string MAIDENNAME { get; set; }

        public string SPOUSE { get; set; }

        public string OCCUPATION { get; set; }

        public short? CUSTOMERTYPEID { get; set; }

        public int? RELATIONSHIPOFFICERID { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public bool ISREALATEDPARTY { get; set; }

        public string MISCODE { get; set; }

        public string MISSTAFF { get; set; }

        public int? APPROVALSTATUS { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public string ACTEDONBY { get; set; }

        public bool ACCOUNTCREATIONCOMPLETE { get; set; }

        public bool CREATIONMAILSENT { get; set; }

        public short CUSTOMERSENSITIVITYLEVELID { get; set; }

        public short? SUBSECTORID { get; set; }

        public short? FSCAPTIONGROUPID { get; set; }

        public string TAXNUMBER { get; set; }

        public string CUSTOMERBVN { get; set; }

        public string CUSTOMERNIN { get; set; }

        public short? RISKRATINGID { get; set; }

        public bool? VALIDATED { get; set; }

        public DateTime? DATEVALIDATED { get; set; }

        public string PROSPECTCUSTOMERCODE { get; set; }

        public bool ISPROSPECT { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public int? CRMSLEGALSTATUSID { get; set; }

        public int? CRMSCOMPANYSIZEID { get; set; }

        public int? BUSINESSUNTID { get; set; }

        public int? COUNTRYOFRESIDENTID { get; set; }

        public int? NUMBEROFDEPENDENTS { get; set; }

        public int? NUMBEROFLOANSTAKEN { get; set; }

        public decimal? MONTHLYLOANREPAYMENT { get; set; }

        public DateTime? DATEOFRELATIONSHIPWITHBANK { get; set; }

        public int? RELATIONSHIPTYPEID { get; set; }

        public string TEAMLDR { get; set; }

        public string TEAMNPL { get; set; }

        public string CORR { get; set; }

        public decimal? PASTDUEOBLIGATIONS { get; set; }

        public int? CRMSRELATIONSHIPTYPEID { get; set; }
        public string OFFERLETTERTITLE { get; set; }
        public string OFFERLETTERSALUTATION { get; set; }
        public string APIREQUESTID { get; set; }
        public string CUSTOMERRATING { get; set; }
        public string LIABILITYLIMITNUMBER { get; set; }
        public string OWNERSHIP { get; set; }

        public string NAMEOFSIGNATORY { get; set; }
        public string ADDRESSOFSIGNATORY { get; set; }
        public string PHONENUMBEROFSIGNATORY { get; set; }
        public string EMAILOFSIGNATORY { get; set; }
        public string BVNNUMBEROFSIGNATORY { get; set; }

    }
}
