using System;
using System.Configuration;
using System.IO;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using LMConnect.Key.Configurations;
using log4net;

namespace LMConnect.Web
{
	public class MvcApplication : System.Web.HttpApplication
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));
		private static LMConnect.Environment _env;
		public static ISessionFactory SessionFactory { get; private set; }

		#region Properties

		public static LMConnect.Environment Environment
		{
			get
			{
				if (_env == null)
				{
					string dataPath = GetAppPathSetting("LMConnect-DataPath",
						String.Format(@"{0}\..\..\Data", AppDomain.CurrentDomain.BaseDirectory));

					string pcGridPath = GetAppPathSetting("LMConnect-PCGrid-Bin",
						String.Format(@"{0}\..\..\Libs\PCGrid\Binaries", AppDomain.CurrentDomain.BaseDirectory));

					string pcGridKeyStore = GetAppPathSetting("LMConnect-PCGrid-Keystore",
						String.Format(@"{0}\..\..\Libs\SewebarConnect.jks", AppDomain.CurrentDomain.BaseDirectory));

					string pcGridHostname = GetAppSetting("LMConnect-PCGrid-Hostname",
						String.Empty);

					string pcGridPort = GetAppSetting("LMConnect-PCGrid-Port",
						String.Empty);

					string pcGridAlias = GetAppSetting("LMConnect-PCGrid-Alias",
						String.Empty);

					string pcGridPassword = GetAppSetting("LMConnect-PCGrid-Password",
						String.Empty);

					var pcGridSettings = new PCGridSettings(
						binaries: pcGridPath,
						keystore: pcGridKeyStore,
						hostname: pcGridHostname, 
						port: pcGridPort, 
						alias: pcGridAlias,
						password: pcGridPassword);

					_env = new LMConnect.Environment
					{
						DataPath = dataPath,
						LMPoolPath = GetAppPathSetting("LMConnect-PoolPath", String.Format(@"{0}\..\..\Data\LMs", AppDomain.CurrentDomain.BaseDirectory)),
						LMPath = GetAppPathSetting("LMConnect-LMPath", String.Format(@"{0}\..\..\Libs\{1}", AppDomain.CurrentDomain.BaseDirectory, "LISp Miner")),
						PCGridSettings = pcGridSettings,
						TimeLog = GetAppSetting("LMConnect-TimeLog", false),
					};
				}

				return _env;
			}
		}

		#endregion

		protected static string GetAppSetting(string setting, string defaultValue)
		{
			var value = ConfigurationManager.AppSettings[setting];
			return String.IsNullOrEmpty(value) ? defaultValue : value;
		}

		protected static string GetAppPathSetting(string setting, string defaultValue)
		{
			var value = ConfigurationManager.AppSettings[setting];
			return Path.GetFullPath(String.IsNullOrEmpty(value) ? defaultValue : value);
		}

		protected static bool GetAppSetting(string setting, bool defaultValue)
		{
			var value = ConfigurationManager.AppSettings[setting];
			bool parse;

			if (bool.TryParse(value, out parse))
			{
				return parse;
			}

			return defaultValue;
		}

		protected static void RegisterExisting()
		{
			var env = MvcApplication.Environment;

			if (!Directory.Exists(env.LMPoolPath)) return;

			foreach (var path in Directory.GetDirectories(env.LMPoolPath))
			{
				try
				{
					var directory = new DirectoryInfo(path);
					var lm = new LISpMiner.LISpMiner(directory, env);

					if (!env.Exists(lm.Id))
					{
						env.Register(lm);
					}
				}
				catch
				{
					continue;
				}
			}
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			SecurityConfig.ConfigureGlobal(GlobalConfiguration.Configuration);

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			// Register exisitng LMs
			RegisterExisting();

			// Load logging info
			log4net.Config.XmlConfigurator.Configure();

			ISessionManager sessionManager = new NHibernateSessionManager();
			sessionManager.Configuration.CurrentSessionContext<WebSessionContext>();

			SessionFactory = sessionManager.BuildSessionFactory();
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			Log.Error(Server.GetLastError());
		}
	}
}