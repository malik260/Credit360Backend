namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Staff")]
    public partial class tbl_Temp_Staff
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        [StringLength(50)]
        public string StaffCode { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        public int JobTitleId { get; set; }

        public int RankId { get; set; }

        [StringLength(100)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(1)]
        public string Gender { get; set; }

        [StringLength(100)]
        public string NameOfNOK { get; set; }

        [StringLength(100)]
        public string PhoneOfNOK { get; set; }

        [StringLength(100)]
        public string EmailOfNOK { get; set; }

        [StringLength(100)]
        public string AddressOfNOK { get; set; }

        [StringLength(1)]
        public string GenderOfNOK { get; set; }

        [StringLength(100)]
        public string NOKRelationShip { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }

        public byte[] Staffsignature { get; set; }

        public short? BranchId { get; set; }

        public int? MISInfoId { get; set; }

        public short? DepartmentId { get; set; }

        public int? StateId { get; set; }

        public int? CityId { get; set; }

        public short CustomerSensitivityLevel { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool IsCurrent { get; set; }

        public short ApprovalStatusId { get; set; }

        public virtual tbl_Approval_Status tbl_Approval_Status { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Department tbl_Department { get; set; }

        public virtual tbl_MIS_Info tbl_MIS_Info { get; set; }

        public virtual tbl_Staff_JobTitle tbl_Staff_JobTitle { get; set; }

        public virtual tbl_Staff_Rank tbl_Staff_Rank { get; set; }

        public virtual tbl_State tbl_State { get; set; }
    }
}
