using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace LMConnect.WebApi.API.Requests.Application
{
	public class NotRegisteredUser
	{
		internal static NotRegisteredUser FromRequest(HttpContentHeaders headers)
		{
			IEnumerable<string> auth;

			if (headers.TryGetValues("Authorization", out auth))
			{
				return FromBasicAuthToken(auth.FirstOrDefault());
			}

			return null;
		}

		private static NotRegisteredUser FromBasicAuthToken(string basicAuthToken)
		{
			if (!string.IsNullOrEmpty(basicAuthToken))
			{
				basicAuthToken = basicAuthToken.Replace("Basic ", string.Empty);

				Encoding encoding = Encoding.GetEncoding("iso-8859-1");
				string userPass = encoding.GetString(Convert.FromBase64String(basicAuthToken));
				int separator = userPass.IndexOf(':');

				return new NotRegisteredUser(userPass.Substring(0, separator), userPass.Substring(separator + 1));
			}

			return null;
		}

		public string Username { get; private set; }

		public string Password { get; private set; }

		internal NotRegisteredUser(string username, string password)
		{
			this.Username = username;
			this.Password = password;
		}
	}
}