using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBTerminalMap : ClassMap<DBTerminal>
    {
        public DBTerminalMap()
        {
            Table("terminal");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Account).Column("AccountId");
            Map(x => x.Accountnumber).Column("AccountNumber");
            Map(x => x.Broker).Column("Broker").Not.Nullable();
            Map(x => x.Fullpath).Column("Fullpath").Not.Nullable();
            Map(x => x.Codebase).Column("Codebase");
            Map(x => x.Retired).Column("Retired");
            Map(x => x.Demo).Column("Demo");
            Map(x => x.Stopped).Column("Stopped");
        }
    }
}