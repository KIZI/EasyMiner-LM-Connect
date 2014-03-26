using System.IO;
using System.Net.Http;

namespace LMConnect.WebApi.API.DataDictionary
{
	public class ImportRequest : Request
	{
		private string _dataDictionary;

		public string DataDictionary
		{
			get { return _dataDictionary ?? (_dataDictionary = ReadDataDictionary()); }
		}

		public ImportRequest(Stream input, HttpContent content)
			: base(input, content)
		{
		}

		private string ReadDataDictionary()
		{
			using (var input = new StreamReader(this.InputStream))
			{
				return input.ReadToEnd();
			}
		}
	}
}