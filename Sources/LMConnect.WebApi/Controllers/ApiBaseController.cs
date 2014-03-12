using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LMConnect.Key.Repositories;
using LMConnect.WebApi.API;

namespace LMConnect.WebApi.Controllers
{
	public class ApiBaseController : ApiController
	{
		protected const string PARAMS_GUID = "minerId";
		private const string AnonymousUserName = "anonymous";

		private LISpMiner.LISpMiner _miner;
		private IRepository _repository;
		private LMConnect.Key.User _user;

		protected virtual IRepository Repository
		{
			get { return _repository ?? (_repository = new NHibernateRepository(Config.SessionFactory.GetCurrentSession())); }
		}

		public LISpMiner.LISpMiner LISpMiner
		{
			get
			{
				if (this._miner == null)
				{
					if (this.ControllerContext.RouteData.Values[PARAMS_GUID] == null)
					{
						// we need a context
						throw new Exception(string.Format("{0} is required parameter in this context", PARAMS_GUID));
					}

					var guid = this.ControllerContext.RouteData.Values[PARAMS_GUID] as string;

					if (guid == null)
					{
						throw new Exception(String.Format("Not specified which LISpMiner to work with."));
					}

					if (!Config.Environment.Exists(guid))
					{
						throw new Exception(String.Format("Requested LISpMiner with ID {0}, does not exists", guid));
					}

					this._miner = Config.Environment.GetMiner(guid);
				}

				return this._miner;
			}
		}

		protected LMConnect.Key.User GetAnonymousUser()
		{
			return this.Repository.Query<LMConnect.Key.User>()
				.First(u => u.Username == AnonymousUserName);
		}

		/// <summary>
		/// Searches database for authorized LMConnect.Key.User.
		/// </summary>
		/// <returns>Authorized LMConnect.Key.User.</returns>
		protected LMConnect.Key.User GetLMConnectUser()
		{
			if (this.User == null)
			{
				return null;
			}

			return this.Repository.Query<LMConnect.Key.User>()
				.FirstOrDefault(u => u.Username == (this.User.Identity.Name) /* && u.Password == password */);
		}

		protected LMConnect.Key.User LMConnectUser
		{
			get
			{
				return this._user ?? (this._user = this.GetLMConnectUser());
			}
		}

		protected T ThrowHttpReponseException<T>(string message = null, HttpStatusCode code = HttpStatusCode.InternalServerError)
		{
			if (!string.IsNullOrEmpty(message))
			{
				var body = new Response(message)
					{
						Status = Status.Failure
					};

				var reponse = ControllerContext.Request.CreateResponse(code, body);

				throw new HttpResponseException(reponse);
			}

			throw new HttpResponseException(code);
		}

		protected Response ThrowHttpReponseException(string message = null, HttpStatusCode code = HttpStatusCode.InternalServerError)
		{
			return this.ThrowHttpReponseException<Response>(message, code);
		}

		protected T ThrowHttpReponseException<T>(HttpStatusCode code = HttpStatusCode.InternalServerError)
		{
			return this.ThrowHttpReponseException<T>(null, code);
		}
	}
}