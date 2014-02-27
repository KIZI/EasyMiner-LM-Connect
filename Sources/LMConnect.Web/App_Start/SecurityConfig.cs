using System.Linq;
using System.Web.Http;
using LMConnect.Key.Repositories;
using LMConnect.Web.Security;
using NHibernate;

namespace LMConnect.Web
{
	public class SecurityConfig
	{
		public static void ConfigureGlobal(HttpConfiguration globalConfig)
		{
			globalConfig.MessageHandlers.Add(new AuthenticationHandler(CreateConfiguration()));
			globalConfig.Filters.Add(new SecurityExceptionFilter());
		}

		private static AuthenticationConfiguration CreateConfiguration()
		{
			var config = new AuthenticationConfiguration
			{
				DefaultAuthenticationScheme = "Basic",
			};

			config.AddBasicAuthentication(OnValidationDelegate);

			return config;
		}

		private static LMConnect.Key.User OnValidationDelegate(string userName, string password)
		{
			using (ISession session = MvcApplication.SessionFactory.OpenSession())
			{
				var repo = new NHibernateRepository(session);
				var user = repo.Query<LMConnect.Key.User>()
					.FirstOrDefault(u => u.Username == userName && u.Password == password);

				session.Close();

				return user;
			}
		}
	}
}