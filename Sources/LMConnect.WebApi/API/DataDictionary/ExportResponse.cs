namespace LMConnect.WebApi.API.DataDictionary
{
	public class ExportResponse : Response, IFileResponse
	{
		public string OutputFilePath { get; set; }

		public string GetFile()
		{
			return this.OutputFilePath;
		}
	}
}