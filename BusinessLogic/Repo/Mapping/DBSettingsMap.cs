using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBSettingsMap : ClassMap<DBSettings>
    {
        public DBSettingsMap()
        {
            Table("settings");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("ID");
            Map(x => x.Propertyname).Column("Propertyname").Not.Nullable();
            Map(x => x.Value).Column("Value");
            Map(x => x.Description).Column("Description");
        }
    }
}