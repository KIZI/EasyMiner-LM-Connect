using System.IO;
using System.Net.Http;
using System.Xml.Linq;

namespace LMConnect.WebApi.API.Tasks
{
    public class TaskUpdateRequest : Request
    {
        private XDocument _buffer;

		internal bool IsCancelation
		{
			get
			{
				var doc = this.GetRequest();

				if (doc.Root != null && doc.Root.Name == "CancelationRequest")
				{
					return true;
				}

				return false;
			}
		}

	    public TaskUpdateRequest(Stream stream, HttpContent content)
			: base(stream, content)
	    {
		    
	    }

	    private XDocument GetRequest()
        {
            return _buffer ?? (_buffer = XDocument.Load(this.InputStream));
        }
    }
}