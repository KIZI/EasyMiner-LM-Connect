using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using LMConnect.LISpMiner;
using LMConnect.WebApi.API;
using LMConnect.WebApi.API.DataDictionary;

namespace LMConnect.WebApi.Controllers
{
	[Authorize]
	public class DataDictionaryController : ApiBaseController
	{
		private void CheckMinerOwnerShip()
		{
			var user = this.GetLMConnectUser();
			var miner = this.Repository.Query<LMConnect.Key.Miner>()
				.FirstOrDefault(m => m.MinerId == this.LISpMiner.Id);

			if ((miner != null && user.Username != miner.Owner.Username) && !this.User.IsInRole("admin"))
			{
				this.ThrowHttpReponseException("Authorized user is not allowed to use this miner.", HttpStatusCode.Forbidden);
			}
		}

		[Filters.NHibernateTransaction]
		public ExportResponse Get(string matrix = null, string template = null)
		{
			CheckMinerOwnerShip();

			return new ExportResponse
			{
				Status = Status.Success,
				OutputFilePath = this.LISpMiner.ExportDataDictionary(matrix, template)
			};
		}

		[Filters.NHibernateTransaction]
		public ImportResponse Put(ImportRequest request)
		{
			if (request == null || request.DataDictionary == null)
			{
				throw new ArgumentException("No DataDictionary given.");
			}

			CheckMinerOwnerShip();

			this.LISpMiner.ImportDataDictionary(request.DataDictionary);

			return new ImportResponse
			{
				Id = this.LISpMiner.Id,
				Message = String.Format("Data Dictionary imported to {0}", this.LISpMiner.Id),
				Status = Status.Success
			};
		}
	}
}
