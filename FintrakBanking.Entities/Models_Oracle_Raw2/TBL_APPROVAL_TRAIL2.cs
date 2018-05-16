namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_TRAIL2")]
    public partial class TBL_APPROVAL_TRAIL2
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALTRAILID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TARGETID { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime SYSTEMARRIVALDATETIME { get; set; }

        public DateTime? SYSTEMRESPONSEDATETIME { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REQUESTSTAFFID { get; set; }

        public int? RESPONSESTAFFID { get; set; }

        public int? TOSTAFFID { get; set; }

        public int? RELIEVEDSTAFFID { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYID { get; set; }

        public int? FROMAPPROVALLEVELID { get; set; }

        public int? TOAPPROVALLEVELID { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALSTATEID { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALSTATUSID { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OPERATIONID { get; set; }

        [StringLength(700)]
        public string COMMENT_ { get; set; }

        public int? VOTE { get; set; }

        public DateTime? ARRIVALDATE { get; set; }

        public DateTime? RESPONSEDATE { get; set; }
    }
}
