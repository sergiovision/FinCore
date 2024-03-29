﻿namespace BusinessObjects.BusinessObjects;

public class MetaSymbol : Idable
{
    public virtual string Name { get; set; }
    public virtual string Description { get; set; }
    public virtual string C1 { get; set; }
    public virtual string C2 { get; set; }
    public virtual int? Typ { get; set; }
    public virtual bool Retired { get; set; }
    public virtual int Id { get; set; }
}
