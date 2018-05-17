namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUSTOMER_GROUP")]
    public partial class TBL_TEMP_CUSTOMER_GROUP
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERGROUPID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string GROUPNAME { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string GROUPCODE { get; set; }

        [StringLength(1000)]
        public string GROUPDESCRIPTION { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ISCURRENT { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALSTATUSID { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
