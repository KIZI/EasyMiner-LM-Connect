using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace LMConnect.LISpMiner
{
	public class RunTaskDefinition : ExportTaskDefinition
	{
		// TODO: compile XPaths
		private static string GetTaskName(string task)
		{
			using (var stream = new StringReader(task))
			{
				var xpath = new XPathDocument(stream);
				var docNav = xpath.CreateNavigator();

				if (docNav.NameTable == null)
				{
					throw new Exception("Task definition has no NameTable.");
				}

				var nsmgr = new XmlNamespaceManager(docNav.NameTable);
				nsmgr.AddNamespace("guha", "http://keg.vse.cz/ns/GUHA0.1rev1");
				nsmgr.AddNamespace("pmml", "http://www.dmg.org/PMML-4_0");

				XPathNavigator node = docNav.SelectSingleNode("/pmml:PMML/*/@modelName", nsmgr);

				if (node == null)
				{
					throw new Exception("Task name could not be fount in task defintion.");
				}

				return node.Value;
			}
		}

		private string _taskName;

		public string Task { get; private set; }

		public override string TaskName
		{
			get { return _taskName ?? (_taskName = GetTaskName(this.Task)); }
		}

		public RunTaskDefinition(string task, string template, string alias)
			: base(null, template, alias)
		{
			this.Task = task;
		}
	}
}