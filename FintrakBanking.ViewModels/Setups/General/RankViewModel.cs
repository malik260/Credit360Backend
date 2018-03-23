namespace FintrakBanking.ViewModels.Setups.General
{
    public class RankViewModel : GeneralEntity
    {
        public int rankId { get; set; }
        public string rankName { get; set; }
    }

    public class RoleViewModel : GeneralEntity
    {
        public int staffRoleId { get; set; }
        public string staffRoleName { get; set; }
    }
}