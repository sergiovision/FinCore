using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBAccountMap : ClassMap<DBAccount>
    {
        public DBAccountMap()
        {
            Table("account");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Currency).Column("CurrencyId");
            References(x => x.Wallet).Column("WalletId");
            References(x => x.Terminal).Column("TerminalId");
            References(x => x.Person).Column("PersonId");
            Map(x => x.Number).Column("Number").Not.Nullable();
            Map(x => x.Description).Column("Description");
            Map(x => x.Balance).Column("Balance");
            Map(x => x.Equity).Column("Equity");
            Map(x => x.Lastupdate).Column("LastUpdate");
            Map(x => x.Retired).Column("Retired").Not.Nullable();
            Map(x => x.Typ).Column("Typ").Not.Nullable();
        }
    }
}