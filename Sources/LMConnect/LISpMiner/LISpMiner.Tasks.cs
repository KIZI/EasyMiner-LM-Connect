using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using LMConnect.Exceptions;
using log4net;

namespace LMConnect.LISpMiner
{
	public partial class LISpMiner
	{
		// TODO: compile XPaths
		private const string XPathStatus = "/*/*/TaskSetting/Extension/TaskState";
		private const string XPathNumberOfRules = "/*/*/@numberOfRules";
		private const string XPathHypothesesCountMax = "/*/*/TaskSetting/Extension/HypothesesCountMax";

		private static readonly ILog Log = LogManager.GetLogger(typeof(LISpMiner));

		private static string RemoveInvalidXmlChars(string input)
		{
			return new string(input.Where(value =>
				(value >= 0x0020 && value <= 0xD7FF) ||
				(value >= 0xE000 && value <= 0xFFFD) ||
				value == 0x0009 ||
				value == 0x000A ||
				value == 0x000D).ToArray());
		}

		private static void GetInfo(string xmlPath, out string status, out int numberOfRules, out int hypothesesCountMax)
		{
			using (var reader = new StreamReader(xmlPath, System.Text.Encoding.UTF8))
			{
				var xml = RemoveInvalidXmlChars(reader.ReadToEnd());
				var document = XDocument.Parse(xml);

				var statusAttribute = ((IEnumerable<object>)document.XPathEvaluate(XPathStatus)).FirstOrDefault() as XElement;
				var numberOfRulesAttribute = ((IEnumerable<object>)document.XPathEvaluate(XPathNumberOfRules)).FirstOrDefault() as XAttribute;
				var hypothesesCountMaxAttribute = ((IEnumerable<object>)document.XPathEvaluate(XPathHypothesesCountMax)).FirstOrDefault() as XElement;

				if (statusAttribute == null)
				{
					throw new InvalidTaskResultXml("TaskState cannot be resolved.", xmlPath, XPathStatus);
				}

				status = statusAttribute.Value;

				if (numberOfRulesAttribute == null || !Int32.TryParse(numberOfRulesAttribute.Value, out numberOfRules))
				{
					throw new InvalidTaskResultXml("NumberOfRulesAttribute cannot be resolved.", xmlPath, XPathNumberOfRules);
				}

				if (hypothesesCountMaxAttribute == null ||
					!Int32.TryParse(hypothesesCountMaxAttribute.Value, out hypothesesCountMax))
				{
					throw new InvalidTaskResultXml("HypothesesCountMax cannot be resolved.", xmlPath, XPathHypothesesCountMax);
				}
			}
		}

		private static string WriteToFile(string path, string content)
		{
			if (!File.Exists(path))
			{
				// save importing task XML
				using (var file = new StreamWriter(path))
				{
					file.Write(content);
				}
			}

			return path;

			//if (!File.Exists(path))
			//{
			//	using (var file = File.CreateText(path))
			//	{
			//		file.Write(this.DataDictionary);
			//		file.Close();
			//	}
			//}

			//return path;
		}

		private string GetDataFolder()
		{
			var dataFolder = string.Format("{0}/xml", this.LMPrivatePath);

			if (!Directory.Exists(dataFolder))
			{
				Directory.CreateDirectory(dataFolder);
			}

			return dataFolder;
		}

		private LMTaskPooler CreateTaskPooler()
		{
			return new LMTaskPooler(this, this.Metabase.ConnectionString, this.LMPrivatePath);
		}

		public LMProcPooler CreateProcPooler()
		{
			return new LMProcPooler(this, this.Metabase.ConnectionString, this.LMPrivatePath);
		}

		public LMGridPooler CreateGridPooler()
		{
			return new LMGridPooler(this, this.Metabase.ConnectionString, this.LMPrivatePath, this.Environment.PCGridSettings);
		}

		private ITaskLauncher CreateTaskLauncher(Type taskType)
		{
			if (taskType == typeof (LMTaskPooler))
			{
				return this.CreateTaskPooler();
			}
			else if (taskType == typeof (LMProcPooler))
			{
				return this.CreateProcPooler();
			}
			else if (taskType == typeof (LMGridPooler))
			{
				return this.CreateGridPooler();
			}

			throw new ArgumentException("Uknown taskType");
		}

		public IEnumerable<string> GetTasks()
		{
			return new[]
			{
				"TODO: List of all Tasks for given miner"
			};
		}

		private string Run(ITaskLauncher launcher, RunTaskDefinition definition)
		{
			if (launcher == null || definition == null)
			{
				throw new ArgumentException("No LISpMiner instance or task defined");
			}

			var taskName = definition.TaskName;
			var taskFileName = definition.TaskFileName;
			var dataFolder = GetDataFolder();

			using(var exporter = this.CreateExporter())
			{
				var status = "Not generated";

				exporter.Output = string.Format("{0}/results_{1}_{2:yyyyMMdd-Hmmss}.xml", dataFolder, taskFileName, DateTime.Now);
				exporter.Template = string.Format(@"{0}\Sewebar\Template\{1}", exporter.LMExecutablesPath, definition.Template);
				exporter.TaskName = taskName;
				exporter.NoEscapeSeqUnicode = true;

				try
				{
					// try to export results
					exporter.Execute();

					if (!File.Exists(exporter.Output))
					{
						throw new LISpMinerException("Task possibly does not exist but no appLog generated");
					}

					int numberOfRules;
					int hypothesesCountMax;

					GetInfo(exporter.Output, out status, out numberOfRules, out hypothesesCountMax);
				}
				catch (LISpMinerException ex)
				{
					// task was never imported - does not exists. Therefore we need to import at first.
					Log.Debug(ex);

					// import task
					using (var importer = this.CreateImporter())
					{
						var taskPath = string.Format("{0}/task_{1}_{2:yyyyMMdd-Hmmss}.xml", dataFolder, taskFileName, DateTime.Now);

						importer.Input = WriteToFile(taskPath, definition.Task);
						importer.NoCheckPrimaryKeyUnique = true;

						if (definition.HasAlias)
						{
							importer.Alias = String.Format(@"{0}\Sewebar\Template\{1}", importer.LMExecutablesPath, definition.Alias);
						}

						importer.Execute();
					}
				}

				switch (status)
				{
					// * Not Generated (po zadání úlohy nebo změně v zadání)
					case "Not generated":

					// * Interrupted (přerušena -- buď kvůli time-outu nebo max počtu hypotéz)
					case "Interrupted":
						// run task - generate results
						if (launcher.Status == ExecutableStatus.Ready)
						{
							launcher.TaskName = taskName;
							launcher.ShutdownDelaySec = 0;

							launcher.Execute();
						}
						else
						{
							Log.Debug("Waiting for result generation");
						}

						// run export once again to refresh results and status
						if (status != "Interrupted")
						{
							exporter.Execute();
						}
						break;

					// * Running (běží generování)
					case "Running":

					// * Waiting (čeká na spuštění -- pro TaskPooler, zatím neimplementováno)
					case "Waiting":
						break;

					// * Solved (úspěšně dokončena)
					case "Solved":
					case "Finnished":
					default:
						break;
				}

				if (!File.Exists(exporter.Output))
				{
					throw new Exception("Results generation did not succeed.");
				}

				return exporter.Output;
			}
		}

		public string Run(Type type, RunTaskDefinition definition)
		{
			var launcher = this.CreateTaskLauncher(type);

			return this.Run(launcher, definition);
		}

		public string Run<T>(RunTaskDefinition definition) where T : class, ITaskLauncher
		{
			ITaskLauncher launcher = this.CreateTaskLauncher(typeof(T));

			return this.Run(launcher, definition);
		}

		public string Export(ExportTaskDefinition definition)
		{
			if (definition == null || string.IsNullOrEmpty(definition.TaskName))
			{
				throw new ArgumentException("No LISpMiner instance or task defined.");
			}

			var exporter = this.CreateExporter();

			try
			{
				var output = string.Format("{0}/results_{1}_{2:yyyyMMdd-Hmmss}.xml", GetDataFolder(), definition.TaskFileName, DateTime.Now);

				exporter.Output = output;
				exporter.Template = string.Format(@"{0}\Sewebar\Template\{1}", exporter.LMExecutablesPath, definition.Template);
				exporter.TaskName = definition.TaskName;
				exporter.NoEscapeSeqUnicode = true;

				// try to export results
				exporter.Execute();

				if (!File.Exists(output))
				{
					throw new LISpMinerException("Results generation did not succeed. Task possibly does not exist but no appLog generated.");
				}

				return output;
			}
			finally
			{
				// clean up
				exporter.Output = String.Empty;
				exporter.Template = String.Empty;
				exporter.TaskName = String.Empty;
			}
		}

		private void Cancel(ITaskLauncher pooler, string taskName)
		{
			if (pooler == null)
			{
				throw new ArgumentException();
			}

			if (string.IsNullOrEmpty(taskName))
			{
				CancelAll(pooler);
			}
			else
			{
				using(pooler)
				{
					// cancel task
					pooler.TaskCancel = true;
					pooler.TaskName = taskName;
					pooler.Execute();
				}
			}
		}

		public void Cancel(Type type, string taskName)
		{
			ITaskLauncher pooler = CreateTaskLauncher(type);

			this.Cancel(pooler, taskName);
		}

		public void Cancel<T>(string taskName) where T : class, ITaskLauncher
		{
			ITaskLauncher pooler = CreateTaskLauncher(typeof(T));

			this.Cancel(pooler, taskName);
		}

		private void CancelAll(ITaskLauncher pooler)
		{
			if (pooler == null)
			{
				throw new ArgumentException();
			}

			using(pooler)
			{
				pooler.CancelAll = true;
				pooler.Execute();
			}
		}

		public void CancelAll(Type type)
		{
			ITaskLauncher pooler = CreateTaskLauncher(type);

			this.CancelAll(pooler);
		}

		public void CancelAll<T>() where T : class, ITaskLauncher
		{
			ITaskLauncher pooler = CreateTaskLauncher(typeof(T));

			this.CancelAll(pooler);
		}

		public void CancelAll()
		{
			// cancel all
			using (var pooler = this.CreateTaskPooler())
			{
				pooler.CancelAll = true;
				pooler.Execute();
			}

			using (var pooler = this.CreateProcPooler())
			{
				pooler.CancelAll = true;
				pooler.Execute();
			}

			using (var pooler = CreateGridPooler())
			{
				pooler.CancelAll = true;
				pooler.Execute();
			}
		}
	}
}
