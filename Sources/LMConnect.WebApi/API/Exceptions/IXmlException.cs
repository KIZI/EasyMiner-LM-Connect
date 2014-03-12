using System.Xml.Linq;

namespace LMConnect.WebApi.API.Exceptions
{
	interface IXmlException
	{
		XDocument ToXDocument();
	}
}
