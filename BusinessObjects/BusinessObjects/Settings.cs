namespace BusinessObjects.BusinessObjects;
public class Settings : Idable
{
    public int Id { get; set; }
    public bool Retired { get; set; }

    public string Propertyname { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }
}
