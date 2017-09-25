namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Fee")]
    public partial class tbl_Loan_Fee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Fee()
        {
            tbl_Loan_Fee_Schedule = new HashSet<tbl_Loan_Fee_Schedule>();
        }

        [Key]
        public int LoanChargeFeeId { get; set; }

        public int LoanId { get; set; }

        public short ProductTypeId { get; set; }

        public int ChargeFeeId { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeRateValue { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeDependentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeAmount { get; set; }

        public bool IsIntegralFee { get; set; }

        public bool IsRecurring { get; set; }

        public short RecurringPaymentDay { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Charge_Fee tbl_Charge_Fee { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Fee_Schedule> tbl_Loan_Fee_Schedule { get; set; }
    }
}
