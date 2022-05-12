using System.Data.Common;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace BusinessLogic;

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