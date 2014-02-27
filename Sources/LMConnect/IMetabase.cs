using LMConnect.ODBC;

namespace LMConnect
{
	interface IMetabase
	{
		void SetDatabaseDsnToMetabase(OdbcConnection database);
	}
}
