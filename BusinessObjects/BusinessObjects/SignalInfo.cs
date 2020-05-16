
namespace BusinessObjects
{
    public class SignalInfo
    {
        public int Id { get; set; }

        public long Flags { get; set; }

        public long ObjectId { get; set; }

        public string RaiseDateTime { get; set; }

        public string Name { get; set; }

        public int Value { get; set; }

        public bool Handled { get; set; }

        public object Data { get; set; }
    }
}