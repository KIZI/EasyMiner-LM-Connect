using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using LMConnect.WebApi.API;

namespace LMConnect.WebApi
{
	public class ResponseMediaTypeFormatter : MediaTypeFormatter
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
			return type.IsSubclassOf(typeof(Request)) || type == typeof(Request);
		}

		public override bool CanWriteType(Type type)
		{
			return type.IsSubclassOf(typeof(Response)) || type == typeof(Response);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, System.Net.TransportContext transportContext, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => WriteResponse(value as Response, writeStream), cancellationToken);
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => ReadRequest(type, readStream, content), cancellationToken);
		}

		private object ReadRequest(Type type, Stream stream, HttpContent content)
		{
			return Activator.CreateInstance(type, stream, content);
		}

		private void WriteResponse(Response response, Stream stream)
		{
			if (response != null)
			{
				var fileResponse = response as IFileResponse;

				using (var w = new StreamWriter(stream))
				{
					if (fileResponse != null)
					{
						File.OpenRead(fileResponse.GetFile()).CopyTo(stream);
					}
					else
					{
						w.Write(response.Write());    
					}
					
					w.Flush();
					w.Close();
				}
			}
		}
	}
}
