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
	internal class ResponseMediaTypeFormatter : MediaTypeFormatter
	{
		private readonly string appXml = "application/xml";
		private readonly string textPlain = "text/plain";
		private readonly string textXml = "text/xml";

		public ResponseMediaTypeFormatter()
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(appXml));
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(textPlain));
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(textXml));
		}

		public override bool CanReadType(Type type)
		{
			return false;
		}

		public override bool CanWriteType(Type type)
		{
			return type.IsSubclassOf(typeof(Response)) || type == typeof(Response);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, System.Net.TransportContext transportContext, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => WriteResponse(value as Response, writeStream), cancellationToken);
		}

		private void WriteResponse(Response response, Stream stream)
		{
			if (response != null)
			{
				var fileResponse = response as IFileResponse;

				if (fileResponse != null)
				{
					File.OpenRead(fileResponse.GetFile()).CopyToAsync(stream);
				}
				else
				{
					response.WriteToStream(stream);
				}
			}
		}
	}
}
