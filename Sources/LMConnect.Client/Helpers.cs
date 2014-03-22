using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LMConnect.Client
{
	internal static class Helpers
	{
		public static async Task<XDocument> GetXDocumentAsync(this HttpClient client, string uri)
		{
			HttpResponseMessage result = await client.GetAsync(uri);

			if (result.IsSuccessStatusCode && result.Content != null)
			{
				string content = await result.Content.ReadAsStringAsync();
				return XDocument.Parse(content);
			}

			throw new Exception();
		}

		public static async Task<XDocument> PostXDocumentAsync(this HttpClient client, string uri, HttpContent content)
		{
			HttpResponseMessage result = await client.PostAsync(uri, content);

			if (result.IsSuccessStatusCode && result.Content != null)
			{
				string responseContent = await result.Content.ReadAsStringAsync();
				return XDocument.Parse(responseContent);
			}

			throw new Exception();
		}

		public static AuthenticationHeaderValue GetAnonymousUser()
		{
			return GetUser("anonymous", string.Empty);
		}

		public static AuthenticationHeaderValue GetUser(string username, string password)
		{
			return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				System.Text.Encoding.ASCII.GetBytes(
					string.Format("{0}:{1}", username, password))));
		}
	}
}