using System;

namespace BusinessObjects.BusinessObjects;

public class Person : Idable
{
    public int CountryId { get; set; }
    public DateTime Created { get; set; }
    public int Languageid { get; set; }
    public string Credential { get; set; }
    public string Regip { get; set; }
    public string Mail { get; set; }
    public string Privilege { get; set; }
    public string Uuid { get; set; }
    public int Activated { get; set; }
    public bool Retired { get; set; }
    public int Id { get; set; }
}
