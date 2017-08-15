namespace FintrakBanking.ViewModels.Setups.General
{
    public class MisInfoViewModel : GeneralEntity
    {
        public int MisinfoId { get; set; }
        public string Miscode { get; set; }
        public string Misname { get; set; }
        public short? MistypeId { get; set; }
        public int? ParentMisinfoId { get; set; }
        public string MisType { get; set; }
    }
}