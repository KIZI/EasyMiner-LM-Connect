using System;
using System.Linq;
using System.Web.Http;
using LMConnect.Key.Configurations;
using LMConnect.Key.Repositories;
using LMConnect.WebApi.Security;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using Owin;

namespace LMConnect.Console
{
    internal class Server
    {
        public ISessionFactory SessionFactory { get; private set; }

        /// <summary>
        /// This code configures Web API. The Startup class is specified as a type
        /// parameter in the WebApp.Start method.  
        /// </summary>
        /// <param name="appBuilder">IAppBuilder</param>
        public void Configuration(IAppBuilder appBuilder)
        {
            ISessionManager sessionManager = new NHibernateSessionManager();
            // TODO: what context to use?
            // sessionManager.Configuration.CurrentSessionContext<WebSessionContext>();
            sessionManager.Configuration.CurrentSessionContext<ThreadStaticSessionContext>();

            SessionFactory = sessionManager.BuildSessionFactory();

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            var security = new AuthenticationConfiguration
			{
				DefaultAuthenticationScheme = "Basic",
			};

			security.AddBasicAuthentication(OnValidationDelegate);

            var lm = new LMConnect.Environment
            {
                DataPath = string.Format(@"{0}\..\..\Data", AppDomain.CurrentDomain.BaseDirectory),
                LMPoolPath = string.Format(@"{0}\..\..\Data\LMs", AppDomain.CurrentDomain.BaseDirectory),
                LMPath = string.Format(@"{0}\..\..\Libs\{1}", AppDomain.CurrentDomain.BaseDirectory, "LISp Miner"),
                PCGridSettings = null,
                TimeLog = false,
            };

            WebApi.Config.Register(config, SessionFactory, lm);

            config.MessageHandlers.Add(new AuthenticationHandler(security));
            config.Filters.Add(new SecurityExceptionFilter());

            appBuilder.UseWebApi(config);
        }

        private LMConnect.Key.User OnValidationDelegate(string userName, string password)
        {
            using (ISession session = SessionFactory.OpenSession())
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
