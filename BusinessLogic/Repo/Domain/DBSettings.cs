using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBSettings : BaseEntity<DBSettings>
    {
        public virtual int Id { get; set; }
        public virtual string Propertyname { get; set; }
        public virtual string Value { get; set; }
        public virtual string Description { get; set; }
    }
}