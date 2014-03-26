using System.IO;
using System.Net.Http;
using LMConnect.WebApi.Formatters;

namespace LMConnect.WebApi.API
{
	public class Request
	{
		protected readonly Stream InputStream;

		/// <summary>
		/// Called from <see cref="RequestMediaTypeFormatter" /> via Activator.CreateInstance.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="content"></param>
		protected Request(Stream input, HttpContent content)
		{
			this.InputStream = input;
		}
	}
}