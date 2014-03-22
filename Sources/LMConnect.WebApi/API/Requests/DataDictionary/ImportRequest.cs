using System;
using System.IO;
using System.Net.Http;

namespace LMConnect.WebApi.API.Requests.DataDictionary
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

		public string WriteDataDictionary(string path)
		{
			if (!File.Exists(path))
			{
				using (var file = File.CreateText(path))
				{
					file.Write(this.DataDictionary);
					file.Close();
				}
			}

			return path;
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