using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBCountryMap : ClassMap<DBCountry>
    {
        public DBCountryMap()
        {
            Table("country");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Currency).Column("CurrencyId");
            Map(x => x.Code).Column("Code").Not.Nullable();
            Map(x => x.Description).Column("Description");
        }
    }
}