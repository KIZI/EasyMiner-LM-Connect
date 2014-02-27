using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using LMConnect.LISpMiner;
using LMConnect.Web.API;
using LMConnect.Web.API.Requests.DataDictionary;
using LMConnect.Web.API.Responses.DataDictionary;

namespace LMConnect.Web.Controllers
{
	[Authorize]
	[APIErrorHandler]
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
		public ExportResponse Get()
		{
			CheckMinerOwnerShip();

			var request = new ExportRequest(this);

			var response = new ExportResponse();

			var exporter = this.LISpMiner.Exporter;
			exporter.NoAttributeDisctinctValues = true;
			exporter.NoEscapeSeqUnicode = true;
			exporter.MatrixName = request.MatrixName;
			exporter.Output = String.Format("{0}/results_{1}_{2:yyyyMMdd-Hmmss.fff}.xml", request.DataFolder, "DD", DateTime.Now);
			exporter.Template = String.Format(@"{0}\Sewebar\Template\{1}", exporter.LMExecutablesPath,
											  request.GetTemplate("LMDataSource.Matrix.ARD.Template.PMML"));
			exporter.Execute();

			response.Status = Status.Success;
			response.OutputFilePath = exporter.Output;

			return response;
		}

		[Filters.NHibernateTransaction]
		public ImportResponse Put()
		{
			CheckMinerOwnerShip();

			var request = new ImportRequest(this);

			var response = new ImportResponse
				{
					Id = this.LISpMiner.Id
				};

			if (this.LISpMiner != null && request.DataDictionary != null)
			{
				LMSwbImporter importer = this.LISpMiner.Importer;
				importer.Input = request.DataDictionaryPath;
				importer.NoCheckPrimaryKeyUnique = false;
				importer.Execute();

				response.Message = String.Format("Data Dictionary imported to {0}", importer.LISpMiner.Id);
				response.Status = Status.Success;

				return response;
			}

			throw new Exception("No DataDictionary given.");
		}
	}
}
