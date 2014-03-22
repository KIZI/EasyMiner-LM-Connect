using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using LMConnect.LISpMiner;
using LMConnect.WebApi.API;
using LMConnect.WebApi.API.Requests.DataDictionary;
using LMConnect.WebApi.API.Responses.DataDictionary;

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

			matrix = string.IsNullOrEmpty(matrix) ? "Loans" : matrix;
			template = string.IsNullOrEmpty(template) ? "LMDataSource.Matrix.ARD.Template.PMML" : template;

			var response = new ExportResponse();

			var exporter = this.LISpMiner.Exporter;
			exporter.NoAttributeDisctinctValues = true;
			exporter.NoEscapeSeqUnicode = true;
			exporter.MatrixName = matrix;
			exporter.Output = String.Format("{0}/results_{1}_{2:yyyyMMdd-Hmmss.fff}.xml", this.DataFolder, "DD", DateTime.Now);
			exporter.Template = String.Format(@"{0}\Sewebar\Template\{1}", exporter.LMExecutablesPath, template);
			exporter.Execute();

			response.Status = Status.Success;
			response.OutputFilePath = exporter.Output;

			return response;
		}

		[Filters.NHibernateTransaction]
		public ImportResponse Put(ImportRequest request)
		{
			CheckMinerOwnerShip();

			var response = new ImportResponse
				{
					Id = this.LISpMiner.Id
				};

			if (this.LISpMiner != null && request.DataDictionary != null)
			{
				var dataDictionaryPath = string.Format(@"{0}/DataDictionary_{1:yyyyMMdd-Hmmss}.xml",
														 this.DataFolder,
														 DateTime.Now);

				LMSwbImporter importer = this.LISpMiner.Importer;
				importer.Input = request.WriteDataDictionary(dataDictionaryPath);
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
