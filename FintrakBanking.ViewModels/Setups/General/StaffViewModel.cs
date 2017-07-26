using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class StaffInfoViewModel : GenaralEntity
    {
        public int StaffId { get; set; }
        public string StaffCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public int JobTitleId { get; set; }
        public int RankId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string NameOfNok { get; set; }
        public string PhoneOfNok { get; set; }
        public string EmailOfNok { get; set; }
        public string AddressOfNok { get; set; }
        public string GenderOfNok { get; set; }
        public string NokrelationShip { get; set; }
        public string Comment { get; set; }
        public byte[] Staffsignature { get; set; }
        public short? BranchId { get; set; }
        public string BranchName { get; set; }
        public int? MisinfoId { get; set; }
        public string MisInfoCode { get; set; }
        public short? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? StateId { get; set; }
        public int CityId { get; set; }
        public string StateName { get; set; }
        public short CustomerSensitivityLevel { get; set; }
        public string SensitivityLevel { get; set; }
        public string CreatedByStaffName { get; set; }
        public bool IsUpdate { get; set; }
        public short ApprovalStatusId { get; set; }

        public string StaffFullName { get { return this.FirstName + " " + this.MiddleName.Trim() + " " + this.LastName; } }
    }

    public class StaffViewModel
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
    }

    public class simpleStaffModel
    {
        public int staffId { get; set; }
        public string staffCode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string fullName { get { return $"{this.firstName} {this.middleName.Trim()} {this.lastName} - {this.staffCode}"; } }
    }

    public class StaffDetailsModel : StaffInfoViewModel
    {
        public string JobTitle { get; set; }
        public string Rank { get; set; }
        public string MisInfo { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string company { get; set; }

    }

    //public class StaffDetailsModel
    //{
    //    public int StaffId { get; set; }
    //    public string StaffCode { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string MiddleName { get; set; }
    //    public int JobTitleId { get; set; }
    //    public string JobTitle { get; set; }
    //    public int RankId { get; set; }
    //    public string Rank { get; set; }
    //    public string Phone { get; set; }
    //    public string Email { get; set; }
    //    public string Address { get; set; }
    //    public DateTime DateOfBirth { get; set; }
    //    public string Gender { get; set; }
    //    public string NameOfNok { get; set; }
    //    public string PhoneOfNok { get; set; }
    //    public string EmailOfNok { get; set; }
    //    public string AddressOfNok { get; set; }
    //    public string GenderOfNok { get; set; }
    //    public string NokrelationShip { get; set; }
    //    public string Comment { get; set; }
    //    public byte[] Staffsignature { get; set; }
    //    public short BranchId { get; set; }
    //    public string BranchName { get; set; }
    //    public int MisinfoId { get; set; }
    //    public string MisInfo { get; set; }
    //    public short DepartmentId { get; set; }
    //    public string DepartmentName { get; set; }
    //    public int StateId { get; set; }
    //    public int CityId { get; set; }
    //    public string City { get; set; }
    //    public string State { get; set; }
    //    public short CustomerSensitivityLevel { get; set; }
    //    public string SensitivityLevel { get; set; }
    //    public int companyId { get; set; }
    //    public string company { get; set; }

    //    public string StaffFullName { get { return this.FirstName + " " + this.MiddleName.Trim() + " " + this.LastName; } }

    //}


}