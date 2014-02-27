using NHibernate;
using NHibernate.Cfg;

namespace LMConnect.Key.Configurations
{
	public interface ISessionManager
	{
		Configuration Configuration { get; }

		void CreateDatabase();

		void UpdateDatabase();

		ISessionFactory BuildSessionFactory();
	}
}