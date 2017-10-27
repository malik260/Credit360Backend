namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Approval_Status")]
    public partial class tbl_Approval_Status
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Approval_Status()
        {
            tbl_Approval_Trail = new HashSet<tbl_Approval_Trail>();
            tbl_Loan_Application_Detail = new HashSet<tbl_Loan_Application_Detail>();
            tbl_Loan_Collateral_Mapping = new HashSet<tbl_Loan_Collateral_Mapping>();
            tbl_Loan_Fee = new HashSet<tbl_Loan_Fee>();
            tbl_Loan_Preliminary_Evaluation = new HashSet<tbl_Loan_Preliminary_Evaluation>();
            tbl_Temp_Charge_Fee = new HashSet<tbl_Temp_Charge_Fee>();
            tbl_Temp_Chart_Of_Account = new HashSet<tbl_Temp_Chart_Of_Account>();
            tbl_Temp_Collateral_Customer = new HashSet<tbl_Temp_Collateral_Customer>();
            tbl_Temp_Customer_Group_Mapping = new HashSet<tbl_Temp_Customer_Group_Mapping>();
            tbl_Temp_Customer_Group = new HashSet<tbl_Temp_Customer_Group>();
            tbl_Temp_Fee = new HashSet<tbl_Temp_Fee>();
            tbl_Temp_Product = new HashSet<tbl_Temp_Product>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ApprovalStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string ApprovalStatusName { get; set; }

        public bool ForDisplay { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application_Detail> tbl_Loan_Application_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Collateral_Mapping> tbl_Loan_Collateral_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Fee> tbl_Loan_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Chart_Of_Account> tbl_Temp_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Customer> tbl_Temp_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Customer_Group_Mapping> tbl_Temp_Customer_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Customer_Group> tbl_Temp_Customer_Group { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Fee> tbl_Temp_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product> tbl_Temp_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
    }
}
