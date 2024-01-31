namespace Core.Settings
{
    public class FidoSetting
    {
        public string ServerName { get; set; }
        public string ServerDomain { get; set; }
        public string Origin { get; set; }
        public int TimestampDriftTolerance { get; set; }
        public string MDSAccessKey { get; set; }
    }
}