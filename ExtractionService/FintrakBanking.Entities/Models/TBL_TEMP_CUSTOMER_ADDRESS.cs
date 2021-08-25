namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_CUSTOMER_ADDRESS")]
    public partial class TBL_TEMP_CUSTOMER_ADDRESS
    {
        [Key]
        public int TEMPADDRESSID { get; set; }

        public int ADDRESSID { get; set; }

        public int CUSTOMERID { get; set; }

        public int STATEID { get; set; }

        public int? LOCALGOVERNMENTID { get; set; }

        public int CITYID { get; set; }

        public short ADDRESSTYPEID { get; set; }

        [Required]
        //[StringLength(200)]
        public string ADDRESS { get; set; }

        //[StringLength(200)]
        public string HOMETOWN { get; set; }

        //[StringLength(200)]
        public string POBOX { get; set; }

        //[StringLength(300)]
        public string NEARESTLANDMARK { get; set; }

        //[StringLength(50)]
        public string ELECTRICMETERNUMBER { get; set; }

        public bool ACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_ADDRESS_TYPE TBL_CUSTOMER_ADDRESS_TYPE { get; set; }

        public virtual TBL_LOCALGOVERNMENT TBL_LOCALGOVERNMENT { get; set; }
        public virtual TBL_STATE TBL_STATE { get; set; }
    }
}
