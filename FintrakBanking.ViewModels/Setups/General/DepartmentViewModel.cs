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
    }
}