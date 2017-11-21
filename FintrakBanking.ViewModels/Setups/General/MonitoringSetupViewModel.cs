namespace FintrakBanking.ViewModels.Setups.General
{
    public class MonitoringSetupViewModel : GeneralEntity
    {
        public int monitoringItemId  { get; set; }
        public string monitoringItemName  { get; set; }
        public string messageTemplate  { get; set; }
        public short messageTypeId { get; set; }
        public int notificationPeriod  { get; set; }
        public string messageTypeName  { get; set; }
        public short productId  { get; set; }
        public string productName  { get; set; }
    }

   
}