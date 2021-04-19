
namespace BusinessObjects
{
    public class SignalInfo
    {
        public int Id { get; set; }

        public long Flags { get; set; }

        public long ObjectId { get; set; }

        public long ChartId { get; set; }

        public string Sym { get; set; }

        public int Value { get; set; }

        public object Data { get; set; }
    }
}