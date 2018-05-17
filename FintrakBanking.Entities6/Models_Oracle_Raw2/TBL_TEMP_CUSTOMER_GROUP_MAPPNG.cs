namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUSTOMER_GROUP_MAPPNG")]
    public partial class TBL_TEMP_CUSTOMER_GROUP_MAPPNG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERGROUPMAPPINGID { get; set; }

        public int CUSTOMERID { get; set; }

        public int CUSTOMERGROUPID { get; set; }

        public int RELATIONSHIPTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int COMPANYID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP_RELATN_TYPE TBL_CUSTOMER_GROUP_RELATN_TYPE { get; set; }
    }
}
