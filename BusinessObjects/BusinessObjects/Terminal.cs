namespace BusinessObjects.BusinessObjects;

public class Terminal : Idable
{
    public long AccountNumber { get; set; }
    public string Broker { get; set; }
    public string FullPath { get; set; }
    public string CodeBase { get; set; }
    public bool Retired { get; set; }
    public bool Stopped { get; set; }
    public string Currency { get; set; }
    public int Id { get; set; }
}
