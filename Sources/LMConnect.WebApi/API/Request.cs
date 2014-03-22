using System.IO;
using System.Net.Http;

namespace LMConnect.WebApi.API
{
	public class Request
	{
		protected readonly Stream InputStream;

		/// <summary>
		/// Called from <see cref="ResponseMediaTypeFormatter" /> via Activator.CreateInstance.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="content"></param>
		protected Request(Stream input, HttpContent content)
		{
			this.InputStream = input;
		}
	}
}