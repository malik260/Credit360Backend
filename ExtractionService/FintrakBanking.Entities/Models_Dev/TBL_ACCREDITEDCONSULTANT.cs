namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_ACCREDITEDCONSULTANT")]
    public partial class TBL_ACCREDITEDCONSULTANT
    {
        public TBL_ACCREDITEDCONSULTANT()
        {
            TBL_ACCREDITEDCONSULTANT_STATE = new HashSet<TBL_ACCREDITEDCONSULTANT_STATE>();
            TBL_JOB_REQUEST_DETAIL = new HashSet<TBL_JOB_REQUEST_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCREDITEDCONSULTANTID { get; set; }

        [StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string FIRMNAME { get; set; }

        public int? ACCREDITEDCONSULTANTTYPEID { get; set; }

        public int? COMPANYID { get; set; }

        public int? CITYID { get; set; }

        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [StringLength(50)]
        public string SOLICITORBVN { get; set; }

        public short? COUNTRYID { get; set; }

        [StringLength(500)]
        public string EMAILADDRESS { get; set; }

        [StringLength(500)]
        public string PHONENUMBER { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        [StringLength(500)]
        public string CORECOMPETENCE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_ACCREDITEDCONSULTANT_STATE> TBL_ACCREDITEDCONSULTANT_STATE { get; set; }

        public virtual TBL_ACCREDITEDCONSULTANT_TYPE TBL_ACCREDITEDCONSULTANT_TYPE { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST_DETAIL> TBL_JOB_REQUEST_DETAIL { get; set; }
    }
}
