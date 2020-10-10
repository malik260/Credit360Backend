namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_ACCREDITEDCONSULTANT")]
    public partial class TBL_TEMP_ACCREDITEDCONSULTANT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_TEMP_ACCREDITEDCONSULTANT()
        {
            TBL_TEMP_ACCREDITEDCONSULTANT_STATE = new HashSet<TBL_TEMP_ACCREDITEDCONSULTANT_STATE>();
        }
        [Key]
        public int TEMPACCREDITEDCONSULTANTID { get; set; }

        public int ACCREDITEDCONSULTANTID { get; set; }

        //[StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        //[StringLength(100)]
        public string NAME { get; set; }

        //[StringLength(100)]
        public string FIRMNAME { get; set; }

        public int? ACCREDITEDCONSULTANTTYPEID { get; set; }

        public int? COMPANYID { get; set; }

        public short? CITYID { get; set; }

        //[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        //[StringLength(50)]
        public string SOLICITORBVN { get; set; }

        public short? COUNTRYID { get; set; }

        //[StringLength(500)]
        public string EMAILADDRESS { get; set; }

        public string CATEGORY { get; set; }
        public DateTime? DATEOFENGAGEMENT { get; set; }

        //[StringLength(500)]
        public string PHONENUMBER { get; set; }

        //[StringLength(500)]
        public string ADDRESS { get; set; }

        //[StringLength(500)]
        public string CORECOMPETENCE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
         
        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public string OPERATION { get; set; }
        public bool? ISCURRENT { get; set; }
        public string STAFFCODE { get; set; }
        public string AGENTCATEGORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_ACCREDITEDCONSULTANT_STATE> TBL_TEMP_ACCREDITEDCONSULTANT_STATE { get; set; }
        public virtual TBL_ACCREDITEDCONSULTANT_TYPE TBL_ACCREDITEDCONSULTANT_TYPE { get; set; }

    }
}
