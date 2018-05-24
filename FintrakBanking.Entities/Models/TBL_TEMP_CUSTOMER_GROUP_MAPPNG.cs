namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_CUSTOMER_GROUP_MAPPNG")]
    public partial class TBL_TEMP_CUSTOMER_GROUP_MAPPNG
    {
        [Key]
        public int CUSTOMERGROUPMAPPINGID { get; set; }

        public int CUSTOMERID { get; set; }

        public int CUSTOMERGROUPID { get; set; }

        public short RELATIONSHIPTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool? DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int COMPANYID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP_RELATN_TYPE TBL_CUSTOMER_GROUP_RELATN_TYPE { get; set; }
    }
}
