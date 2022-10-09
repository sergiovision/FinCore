using System.Data.Common;
using NHibernate.Dialect;

namespace BusinessLogic.SQLDriver;

public class CustomDialect : SQLiteDialect
{
    public CustomDialect()
    {
        
    }
    
    public override NHibernate.Dialect.Schema.IDataBaseSchema GetDataBaseSchema(DbConnection connection)
    {
        var item = new CustomMetaData(connection, this);
        return item;
    }

}