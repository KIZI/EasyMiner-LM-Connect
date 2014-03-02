using System;
using System.IO;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace LMConnect.Key.Configurations
{
	public class NHibernateSessionManager : ISessionManager
	{
		private Configuration _cfg;

		protected string ConfigurationXmlPath { get; private set; }

		public Configuration Configuration
		{
			get
			{
				if (_cfg == null)
				{
					_cfg = new Configuration();
					_cfg.Configure(this.ConfigurationXmlPath);

					// SQLite relative path to config file itself
					if (_cfg.Properties["connection.driver_class"] == "NHibernate.Driver.SQLite20Driver" &&
						_cfg.Properties["connection.connection_string"] == "Data Source=key.sqlite")
					{
						var cfgFolder = Path.GetDirectoryName(this.ConfigurationXmlPath);

						if (cfgFolder != null)
						{
							var basePath = Path.GetFullPath((new Uri(cfgFolder + "\\..\\..\\Data")).LocalPath);
							var connectionString = string.Format("Data Source={0}\\key.sqlite", basePath);

							_cfg.Properties["connection.connection_string"] = connectionString;
						}
					}
				}

				return _cfg;
			}
		}

		public NHibernateSessionManager() :
			this(string.Format("{0}\\hibernate.cfg.xml", AppDomain.CurrentDomain.BaseDirectory))
		{
		}

		public NHibernateSessionManager(string cfg)
		{
			this.ConfigurationXmlPath = cfg;
		}

		public void CreateDatabase()
		{
			var schemaExport = new SchemaExport(Configuration);
			schemaExport.Create(false, true);
		}

		public void CreateDatabase(string filename)
		{
			var schemaExport = new SchemaExport(Configuration);
			schemaExport.SetOutputFile(filename).Execute(false, true, false);
		}

		public void UpdateDatabase()
		{
			var schemaExport = new SchemaUpdate(Configuration);
			schemaExport.Execute(false, true);
		}

		public ISessionFactory BuildSessionFactory()
		{
			return Configuration.BuildSessionFactory();
		}
	}
}
