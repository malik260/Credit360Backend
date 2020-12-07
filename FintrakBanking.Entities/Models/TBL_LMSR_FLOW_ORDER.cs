namespace FintrakBanking.Entities.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TBL_LMSR_FLOW_ORDER")]
    public partial class TBL_LMSR_FLOW_ORDER
    {
        [Key]
        public short FLOWORDERID         { get; set; }
        public short OPERATIONID         { get; set; }
        public bool REQUIREAPPRAISAL     { get; set; }
        public bool REQUIREOFFERLETTER   { get; set; }
        public bool REQUIREDRAWDOWN { get; set; }
        public bool REQUIREAVAILMENT     { get; set; }
        public bool REQUIREOPERATIONS     { get; set; }
        public int COMPANYID             { get; set; }
        public DateTime DATETIMECREATED  { get; set; }
        public int CREATEDBY             { get; set; }
        public bool DELETED              { get; set; }
        public int? DELETEDBY            { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? UPDATEDBY            { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public string TAG { get; set; }

    }
}