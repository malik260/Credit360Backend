namespace FintrakBanking.ViewModels.Setups.General
{
    public class DepartmentViewModel : GeneralEntity
    {
        public short DepartmentId { get; set; }
        public short? BranchId { get; set; }
        public string BranchName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public short? departmentUnitId { get; set; }
        public string departmentUnitName { get; set; }
        public string departmentUnitEmail { get; set; }
    }

    public class DepartmentCustomersViewModel : DepartmentViewModel
    {
        public string roleName;

        public int customertId { get; set; }
        public int staffId { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string middlename { get; set; }
        public string fullname { get { return $"{ lastname}  {firstname} {middlename}"; } }
        public string jobTitleName { get; set; }
        public string RoleName { get; set; }
    }
}