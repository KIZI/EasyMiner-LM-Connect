using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LMConnect.Client
{
    public class Client
    {
        internal HttpClient HttpClient { get; private set; }

        public string Server { get; private set; }

        public string Application { get; private set; }

        public Client(string server, string app = "LMConnect")
        {
            this.Server = server;
            this.Application = app;

            this.HttpClient = new HttpClient
            {
                BaseAddress = new Uri(this.Server)
            };

            this.HttpClient.DefaultRequestHeaders.Authorization = Helpers.GetAnonymousUser();
        }

        protected string GetUrl(string resource, string id = null)
        {
            if (id != null)
            {
                resource = string.Format(resource, id);
            }

            return string.Format("{0}/{1}", this.Application, resource);
        }

        public async Task<Miner> RegisterAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Miner> GetMinerAsync(string id)
        {
            XDocument result = await this.HttpClient.GetXDocumentAsync(GetUrl("miners/{0}", id));

            XAttribute attr = ((IEnumerable) result.XPathEvaluate("/response[@status='success']/@id"))
                .Cast<XAttribute>()
                .FirstOrDefault();

            if (attr != null && !string.IsNullOrEmpty(attr.Value))
            {
                return new Miner(attr.Value);
            }

            // TODO: handle errors read from result
            throw new Exception();
        }
    }
}
