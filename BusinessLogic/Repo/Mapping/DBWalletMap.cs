using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBWalletMap : ClassMap<DBWallet>
    {
        public DBWalletMap()
        {
            Table("wallet");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Person).Column("PersonId");
            References(x => x.Site).Column("SiteId");
            Map(x => x.Name).Column("Name").Not.Nullable();
            Map(x => x.Shortname).Column("Shortname");
            Map(x => x.Link).Column("Link");
            Map(x => x.Retired).Column("Retired");
        }
    }
}