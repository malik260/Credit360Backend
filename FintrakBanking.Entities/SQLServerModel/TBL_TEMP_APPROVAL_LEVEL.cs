namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_APPROVAL_LEVEL")]
    public partial class TBL_TEMP_APPROVAL_LEVEL
    {
        [Key]
        public int TEMPAPPROVALLEVELID { get; set; }

        public int? APPROVALLEVELID { get; set; }

        [Required]
        //[StringLength(150)]
        public string LEVELNAME { get; set; }

        public int GROUPID { get; set; }

        public int? STAFFROLEID { get; set; }

        public int POSITION { get; set; }

        public int? TENOR { get; set; }

        [Column(TypeName = "money")]
        public decimal MAXIMUMAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal? INVESTMENTGRADEAMOUNT { get; set; }

        public int NUMBEROFUSERS { get; set; }

        public int NUMBEROFAPPROVALS { get; set; }

        public int SLAINTERVAL { get; set; }

        public int SLANOTIFICATIONINTERVAL { get; set; }

        public double? FEERATE { get; set; }

        public double? INTERESTRATE { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool ISACTIVE { get; set; }

        public bool CANVIEWDOCUMENT { get; set; }

        public bool CANVIEWUPLOAD { get; set; }

        public bool CANVIEWAPPROVAL { get; set; }

        public bool CANAPPROVE { get; set; }

        public bool CANUPLOAD { get; set; }

        public bool CANEDIT { get; set; }

        public bool CANRECIEVEEMAIL { get; set; }

        public bool CANRECIEVESMS { get; set; }

        public bool CANESCALATE { get; set; }

        public bool CANAPPROVEUNTENORED { get; set; }

        public bool CANRESOLVEDISPUTE { get; set; }

        public bool ROUTEVIASTAFFORGANOGRAM { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        //[StringLength(50)]
        public string OPERATION { get; set; }

        public virtual TBL_APPROVAL_GROUP TBL_APPROVAL_GROUP { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }
    }
}
