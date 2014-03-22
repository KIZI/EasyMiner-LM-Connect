using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace LMConnect.WebApi.API.Requests.Task
{
	public class TaskRequest : Request
	{
		private static readonly string InvalidChars = String.Format(@"[{0}]+", Regex.Escape(new string(Path.GetInvalidFileNameChars())));

		private string _task;
		private string _taskName;
		private string _taskFileName;

		public string Task
		{
			get
			{
				return this._task ?? (this._task = this.ReadTask());
			}
		}

		public string TaskName
		{
			get
			{
				if (this._taskName == null)
				{
					using (var stream = new StringReader(this.Task))
					{
						var xpath = new XPathDocument(stream);
						var docNav = xpath.CreateNavigator();

						if (docNav.NameTable != null)
						{
							var nsmgr = new XmlNamespaceManager(docNav.NameTable);
							nsmgr.AddNamespace("guha", "http://keg.vse.cz/ns/GUHA0.1rev1");
							nsmgr.AddNamespace("pmml", "http://www.dmg.org/PMML-4_0");

							var node = docNav.SelectSingleNode("/pmml:PMML/*/@modelName", nsmgr);
							_taskName = node != null ? node.Value : null;
						}
					}
				}

				return _taskName ?? "task";
			}
		}

		public string TaskFileName
		{
			get
			{
				if (String.IsNullOrEmpty(this._taskFileName))
				{
					this._taskFileName = Regex.Replace(this.TaskName, InvalidChars, "_");
				}

				return _taskFileName;
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

		public string WriteTask(string taskPath)
		{
			if (!File.Exists(taskPath))
			{
				// save importing task XML
				using (var file = new StreamWriter(taskPath))
				{
					file.Write(this.Task);
				}
			}

			return taskPath;
		}
	}
}