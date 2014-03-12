using System.Web.Http;
using NHibernate;

namespace LMConnect.WebApi
{
	public static class Config
	{
        internal static ISessionFactory SessionFactory { get; private set; }

	    internal static LMConnect.Environment Environment { get; private set; }

	    public static void Register(HttpConfiguration config, ISessionFactory sessionFactory, LMConnect.Environment environment)
		{
			config.Routes.MapHttpRoute(
				name: "MinersApi",
				routeTemplate: "miners/{minerId}/{controller}/{taskType}/{taskName}/{action}",
				defaults: new
					{
						controller = "Miners",
                        minerId = RouteParameter.Optional,
                        taskType = RouteParameter.Optional,
                        taskName = RouteParameter.Optional,
                        action = RouteParameter.Optional
					}
			);

			config.Routes.MapHttpRoute(
				name: "UsersApi",
				routeTemplate: "users/{username}/{controller}/{id}",
				defaults: new
				{
					controller = "Users",
                    username = RouteParameter.Optional,
                    id = RouteParameter.Optional
				}
			);

			config.Formatters.Remove(config.Formatters.JsonFormatter);
			config.Formatters.Remove(config.Formatters.XmlFormatter);
			config.Formatters.Add(new ResponseMediaTypeFormatter());

	        SessionFactory = sessionFactory;
	        Environment = environment;
		}
	}
}
