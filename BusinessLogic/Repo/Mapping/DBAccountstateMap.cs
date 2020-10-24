using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBAccountstateMap : ClassMap<DBAccountstate>
    {
        public DBAccountstateMap()
        {
            Table("accountstate");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Account).Column("AccountId");
            Map(x => x.Date).Column("Date").Not.Nullable();
            Map(x => x.Balance).Column("Balance").Not.Nullable();
            Map(x => x.Comment).Column("Comment");
        }
    }
}