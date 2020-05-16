using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBCurrencyMap : ClassMap<DBCurrency>
    {
        public DBCurrencyMap()
        {
            Table("currency");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Name).Column("Name").Not.Nullable();
            Map(x => x.Enabled).Column("Enabled");
        }
    }
}