namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Message_Log")]
    public partial class tbl_Message_Log
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        [StringLength(1000)]
        public string FromAddress { get; set; }

        [Required]
        [StringLength(1000)]
        public string ToAddress { get; set; }

        [Required]
        public string MessageBody { get; set; }

        [Required]
        [StringLength(1000)]
        public string MessageSubject { get; set; }

        public short MessageStatusId { get; set; }

        public short MessageTypeId { get; set; }

        public DateTime DateTimeReceived { get; set; }

        public DateTime SendOnDateTime { get; set; }

        public DateTime? DateTimeSent { get; set; }

        [StringLength(2000)]
        public string GatewayResponse { get; set; }

        public int? OperationId { get; set; }

        public int? TargetId { get; set; }

        public virtual tbl_Message_Log_Status tbl_Message_Log_Status { get; set; }

        public virtual tbl_Message_Log_Type tbl_Message_Log_Type { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }
    }
}
