using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class MessageLogViewModel
    {
        public int? targetId;

        public string ReferenceCode { get; set; }

        public int MessageId { get; set; }

        public string FromAddress { get; set; }

        public string ToAddress { get; set; }

        public string MessageBody { get; set; }
        public string operationMethod { get; set; }

        public string MessageSubject { get; set; }

        public short MessageStatusId { get; set; }

        public short MessageTypeId { get; set; }

        public DateTime DateTimeReceived { get; set; }

        public DateTime SendOnDateTime { get; set; }

        public DateTime? DateTimeSent { get; set; }

        public string GatewayResponse { get; set; }
    }
}