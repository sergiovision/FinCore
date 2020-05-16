
namespace BusinessObjects
{
    public class ExpertInfo
    {
        public string Account { get; set; }
        public long Magic { get; set; }
        public long Flags { get; set; }
        public string ChartTimeFrame { get; set; }
        public string Symbol { get; set; }
        public string EAName { get; set; }
        public string Data { get; set; }
        public bool   IsMaster { get; set; }
        public string Orders { get; set; }
        public string Reason { get; set; }
    }
}