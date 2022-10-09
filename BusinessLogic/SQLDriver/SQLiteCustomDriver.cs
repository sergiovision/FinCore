using Autofac;
using BusinessLogic.BusinessObjects;
using NHibernate;
using NHibernate.Driver;

namespace BusinessLogic.SQLDriver;

using System.Data;
using System.Data.Common;

/// <summary>
/// NHibernate driver for the System.Data.SQLite data provider for .NET.
/// </summary>
/// <remarks>
/// <para>
/// In order to use this driver you must have the System.Data.SQLite.dll assembly available
/// for NHibernate to load. This assembly includes the SQLite.dll or SQLite3.dll libraries.
/// </para>
/// <para>
/// You can get the System.Data.SQLite.dll assembly from
/// <a href="https://system.data.sqlite.org/">https://system.data.sqlite.org/</a>
/// </para>
/// <para>
/// Please check <a href="https://www.sqlite.org/">https://www.sqlite.org/</a> for more information regarding SQLite.
/// </para>
/// </remarks>
public class SQLiteCustomDriver : ReflectionBasedDriver
{
	private static XTradeConfig config;

	/// <summary>
	/// Initializes a new instance of <see cref="SQLite20Driver"/>.
	/// </summary>
	/// <exception cref="HibernateException">
	/// Thrown when the <c>SQLite.NET</c> assembly can not be loaded.
	/// </exception>
	public SQLiteCustomDriver() : base(
		"Microsoft.Data.Sqlite",
		"Microsoft.Data.Sqlite",
		"Microsoft.Data.Sqlite.SqliteConnection",
		"Microsoft.Data.Sqlite.SqliteCommand")
	{
	}

	public override DbConnection CreateConnection()
	{
		if (config == null)
			config = MainService.thisGlobal.Container.Resolve<XTradeConfig>();
		string connString = config.ConnectionString();

		var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
		
		connection.StateChange += Connection_StateChange;
		return connection;
	}
	
	public override DbCommand CreateCommand()
	{
		var command = new Microsoft.Data.Sqlite.SqliteCommand();
		return command;
	}


	private static void Connection_StateChange(object sender, StateChangeEventArgs e)
	{
		if ((e.OriginalState == ConnectionState.Broken || e.OriginalState == ConnectionState.Closed || e.OriginalState == ConnectionState.Connecting) &&
			e.CurrentState == ConnectionState.Open)
		{
			var connection = (DbConnection)sender;
			using (var command = connection.CreateCommand())
			{
				// Activated foreign keys if supported by SQLite.  Unknown pragmas are ignored.
				command.CommandText = "PRAGMA foreign_keys = ON";
				command.ExecuteNonQuery();
			}
		}
	}

	public override IResultSetsCommand GetResultSetsCommand(NHibernate.Engine.ISessionImplementor session)
	{
		return new BasicResultSetsCommand(session);
	}

	public override bool UseNamedPrefixInSql => true;

	public override bool UseNamedPrefixInParameter => true;

	public override string NamedPrefix => "@";

	public override bool SupportsMultipleOpenReaders => false;

	public override bool SupportsMultipleQueries => true;

	public override bool SupportsNullEnlistment => false;

	public override bool HasDelayedDistributedTransactionCompletion => true;
	
}

