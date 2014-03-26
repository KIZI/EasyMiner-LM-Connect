using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using LMConnect.ODBC;

namespace LMConnect.WebApi.API.Miners
{
	public class RegistrationRequest : Request
	{
		private XDocument _buffer;
		private DbConnection _dbConnection;
		private DbConnection _dbMetabase;
		private readonly HttpContentHeaders _headers;

		public static DbConnection GetDbConnection(string which, XDocument requestBody)
		{
			OdbcDrivers type;
			var conn = (from element in requestBody
							.Elements("RegistrationRequest")
							.Elements(which)
						select element).ToList();

			if (conn.Count == 0)
			{
				if (which == "Metabase")
				{
					return null;
				}

				throw new Exception(String.Format("Database was not correctly defined ({0}).", which));
			}

			var connType = (from element in conn
							select (string)element.Attribute("type")).SingleOrDefault();

			if (connType != null && !connType.EndsWith("Connection"))
			{
				connType = connType + "Connection";
			}

			if (!OdbcDrivers.TryParse(connType, true, out type))
			{
				throw new Exception(String.Format("Database was not correctly defined (type of {0}).", which));
			}

			switch (type)
			{
				case OdbcDrivers.AccessConnection:
					return (from element in conn
							select
								new DbConnection
								{
									Type = type,
									Filename = (string)element.Element("File")
								}).SingleOrDefault();
				case OdbcDrivers.MySqlConnection:
					return (from element in conn
							select
								new DbConnection
								{
									Type = type,
									Server = (string)element.Element("Server"),
									Database = (string)element.Element("Database"),
									Username = (string)element.Element("Username"),
									Password = (string)element.Element("Password")
								}).SingleOrDefault();
			}

			throw new Exception(String.Format("Database was not correctly defined ({0}).", which));
		}

		public DbConnection Metabase
		{
			get { return this._dbMetabase ?? (this._dbMetabase = GetDbConnection("Metabase", this.GetRequest())); }
		}

		public DbConnection DbConnection
		{
			get { return this._dbConnection ?? (this._dbConnection = GetDbConnection("Connection", this.GetRequest())); }
		}

		public bool SharedBinaries
		{
			get
			{
				var attr = (from attribute in this.GetRequest()
							.Elements("RegistrationRequest")
							.Attributes("sharedBinaries")
							select attribute).FirstOrDefault();

				if (attr != null)
				{
					bool parsed;

					if (bool.TryParse(attr.Value, out parsed))
					{
						return parsed;
					}
				}

				return true;
			}
		}

		public NotRegisteredUser Owner
		{
			get { return NotRegisteredUser.FromRequest(this._headers); }
		}

		public RegistrationRequest(Stream input, HttpContent content)
			: base(input, content)
		{
			this._headers = content.Headers;
		}

		private XDocument GetRequest()
		{
			return _buffer ?? (_buffer = XDocument.Load(this.InputStream));
		}
	}
}