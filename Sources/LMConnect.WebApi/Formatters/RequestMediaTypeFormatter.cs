using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using LMConnect.WebApi.API;

namespace LMConnect.WebApi.Formatters
{
	internal class RequestMediaTypeFormatter : MediaTypeFormatter
	{
		private readonly string appXml = "application/xml";
		private readonly string textPlain = "text/plain";
		private readonly string textXml = "text/xml";

		public RequestMediaTypeFormatter()
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(appXml));
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(textPlain));
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(textXml));
		}

		public override bool CanReadType(Type type)
		{
			return type.IsSubclassOf(typeof(Request)) || type == typeof(Request);
		}

		public override bool CanWriteType(Type type)
		{
			return false;
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => ReadRequest(type, readStream, content), cancellationToken);
		}

		private object ReadRequest(Type type, Stream stream, HttpContent content)
		{
			return Activator.CreateInstance(type, stream, content);
		}
	}
}
