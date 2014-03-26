using System.IO;
using System.Net.Http;

namespace LMConnect.WebApi.API.Tasks
{
	public class TaskRequest : Request
	{
		private string _task;

		public string Task
		{
			get
			{
				return this._task ?? (this._task = this.ReadTask());
			}
		}

		public TaskRequest(Stream input, HttpContent content)
			: base(input, content)
		{
		}

		private string ReadTask()
		{
			using (var input = new StreamReader(this.InputStream))
			{
				return input.ReadToEnd();
			}
		}
	}
}