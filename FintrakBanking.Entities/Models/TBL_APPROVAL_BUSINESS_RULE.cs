namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_APPROVAL_BUSINESS_RULE")]
    public partial class TBL_APPROVAL_BUSINESS_RULE
    {
        public TBL_APPROVAL_BUSINESS_RULE()
        {
            TBL_APPROVAL_LEVEL = new HashSet<TBL_APPROVAL_LEVEL>();
        }

        [Key]
        public int APPROVALBUSINESSRULEID { get; set; }

        public string DESCRIPTION { get; set; }

        public decimal? MINIMUMAMOUNT { get; set; }
        public decimal? MAXIMUMAMOUNT { get; set; }
        public decimal? PEPAMOUNT { get; set; }

        public bool PEP { get; set; }
        public bool INSIDERRELATED { get; set; }
        public bool PROJECTRELATED { get; set; }
        public bool ONLENDING { get; set; }
        public bool INTERVENTIONFUNDS { get; set; }
        public bool ORRBASEDAPPROVAL { get; set; }
        public bool ISAGRICRELATED { get; set; }
        public bool WITHINSTRUCTION { get; set; }
        public bool DOMICILIATIONNOTINPLACE { get; set; }
        public bool ESRM { get; set; }
        public bool ISFORCONTINGENTFACILITY { get; set; }
        public bool ISFORREVOLVINGFACILITY { get; set; }
        public bool ISFORRENEWAL { get; set; }
        public bool EXEMPTCONTINGENTFACILITY { get; set; }
        public bool EXEMPTREVOLVINGFACILITY { get; set; }
        public bool EXEMPTRENEWAL { get; set; }
        public bool EXCLUDELEVEL { get; set; }

        public int COMPANYID { get; set; }
        public int? TENOR { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_APPROVAL_LEVEL> TBL_APPROVAL_LEVEL { get; set; }
    }
}
