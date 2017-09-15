namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Type")]
    public partial class tbl_Loan_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Type()
        {
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            //tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan_Preliminary_Evaluation = new HashSet<tbl_Loan_Preliminary_Evaluation>();
            //tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LoanTypeId { get; set; }

        [StringLength(50)]
        public string LoanTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }
    }
}
