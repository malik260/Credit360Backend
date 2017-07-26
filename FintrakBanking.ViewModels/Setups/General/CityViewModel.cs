namespace FintrakBanking.ViewModels.Setups.General
{
    public class CityViewModel
    {
        public int cityId { get; set; }
        public string cityName { get; set; }
        public int stateId { get; set; }
        public string stateName { get; set; }
        public short cityClassId { get; set; }
        public string cityClassName { get; set; }
        public bool allowedForCollateral { get; set; }
    }
}