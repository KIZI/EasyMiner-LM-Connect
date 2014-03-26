using System;
using System.IO;
using System.Text.RegularExpressions;

namespace LMConnect.LISpMiner
{
	public class ExportTaskDefinition
	{
		private static readonly string InvalidChars = string.Format(@"[{0}]+", Regex.Escape(new String(Path.GetInvalidFileNameChars())));

		public const string DefaultTemplate = "4ftMiner.Task.Template.PMML";

		public virtual string TaskName { get; private set; }

		public string TaskFileName
		{
			get { return Regex.Replace(this.TaskName, InvalidChars, "_"); }
		}

		public string Template { get; protected set; }

		public string Alias { get; protected set; }

		public bool HasAlias
		{
			get { return !string.IsNullOrEmpty(this.Alias); }
		}

		public ExportTaskDefinition(string taskName, string template, string alias)
		{
			this.TaskName = taskName;
			this.Template = string.IsNullOrEmpty(template) ? DefaultTemplate : template;
			this.Alias = alias;
		}
	}
}