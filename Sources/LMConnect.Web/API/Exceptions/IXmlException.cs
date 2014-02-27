using System.Xml.Linq;

namespace LMConnect.Web.API.Exceptions
{
	interface IXmlException
	{
		XDocument ToXDocument();
	}
}
