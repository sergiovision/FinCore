using System;


namespace BusinessLogic.Repo
{
    public class DBPerson : BaseEntity<DBPerson>
    {
        public virtual int Id { get; set; }
        public virtual DBCountry Country { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual int Languageid { get; set; }
        public virtual string Credential { get; set; }
        public virtual string Regip { get; set; }
        public virtual string Mail { get; set; }
        public virtual string Privilege { get; set; }
        public virtual string Uuid { get; set; }
        public virtual int Activated { get; set; }
        public virtual bool Retired { get; set; }
    }
}