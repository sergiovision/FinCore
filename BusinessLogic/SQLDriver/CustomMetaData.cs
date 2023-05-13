using System;
using System.Data.Common;
using NHibernate.Dialect;
using NHibernate.Dialect.Schema;

namespace BusinessLogic.SQLDriver;

public class CustomMetaData : SQLiteDataBaseMetaData
{
    [Obsolete("Obsolete")]
    public CustomMetaData(DbConnection connection) : base(connection)
    {
    }

    public CustomMetaData(DbConnection connection, Dialect dialect) : base(connection, dialect)
    {
        
    }

    public override bool IncludeDataTypesInReservedWords => false;
}