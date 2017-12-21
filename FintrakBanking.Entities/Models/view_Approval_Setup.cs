namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class view_Approval_Setup
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GROUPID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(150)]
        public string GROUPNAME { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYID { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALLEVELID { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(150)]
        public string LEVELNAME { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int POSITION { get; set; }

        [Key]
        [Column(Order = 6, TypeName = "money")]
        public decimal LEVELMAXIMUMAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal? INVESTMENTGRADEAMOUNT { get; set; }

        public int? TENOR { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STAFFID { get; set; }

        [Key]
        [Column(Order = 8, TypeName = "money")]
        public decimal STAFFMAXIMUMAMOUNT { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(50)]
        public string STAFFCODE { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(50)]
        public string STAFFFIRSTNAME { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(50)]
        public string STAFFLASTNAME { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(50)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OPERATIONID { get; set; }

        [Key]
        [Column(Order = 14)]
        [StringLength(150)]
        public string OPERATIONNAME { get; set; }
    }
}
