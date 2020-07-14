namespace FintrakBanking.ViewModels.Setups.General
{
    public class DepartmentViewModel : GeneralEntity
    {
        public short departmentId { get; set; }
        public short? BranchId { get; set; }
        public string BranchName { get; set; }
        public string departmentCode { get; set; }
        public string departmentName { get; set; }
        public string description { get; set; }
        public short? unitId { get; set; }
        public string unitName { get; set; }
        public string unitEmail { get; set; }
    }

    public class DepartmentCustomersViewModel : DepartmentViewModel
    {
        public string staffEmail { get; set; }
        public string staffPhone { get; set; }

        public string staffCode { get; set; }

        public short departmentUnitId { get; set; }

        public string departmentUnitName { get; set; }
        public string branchName { get; set; }

        public int customertId { get; set; }
        public new int staffId { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string middlename { get; set; }
        public string fullname { get { return $"{ lastname}  {firstname} {middlename}"; } }
        public string jobTitleName { get; set; }
        public string roleName { get; set; }
    }
}