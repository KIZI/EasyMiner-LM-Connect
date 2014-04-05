using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LMConnect.Utils;

namespace LMConnect
{
	public class Manager
	{
		private const string VersionPath = "http://lispminer.vse.cz/download/index.php";
		private const string FilesPath = "http://lispminer.vse.cz/files/exe/";
		private const string PCGrid = "PCGrid.120706.VSE.zip";
		private const string LMCore = "LISp-Miner.Core.zip";

		protected string[] Packages =
		{
			LMCore,
			"LM.GridPooler.zip",
			"http://lispminer.vse.cz/files/tgs/" + PCGrid
		};

		protected string[] DataPackages =
		{
			"Barbora.zip"
		};

		public string TargetPath { get; private set; }

		public string CurrentVersionPath { get; private set; }

		public string Version { get; private set; }

		public DateTime ReleaseDate { get; private set; }

		private WebClient Client { get; set; }

		private Dictionary<string, ConsoleLine> Lines { get; set; }

		private string DataFolder
		{
			get { return string.Format("{0}\\..\\Data\\", this.TargetPath); }
		}

		public Manager(string targetPath)
		{
			this.Lines = new Dictionary<string, ConsoleLine>();

			this.TargetPath = targetPath;
			this.Client = new WebClient();

			var page = this.Client.DownloadString(VersionPath);

			Match match = Regex.Match(page, "<p>The LISp-Miner System, version <a href=\"relnotes.php\"><b>(.*)</b></a> from (.*) available.</p>");

			if (match.Success)
			{
				this.Version = match.Groups[1].ToString();
				this.ReleaseDate = DateTime.Parse(match.Groups[2].ToString());
			}
		}

		public string Update()
		{
			var tasks = new List<Task>();
			var directory = string.Format("{0}\\LISp Miner {1}", this.TargetPath, this.ReleaseDate.ToString("yyyy.MM.dd"));
			var current = string.Format("{0}\\LISp Miner", this.TargetPath);

			this.CurrentVersionPath = directory;

			ConsoleLine.Append("Updating LISp Miner to version {0} from {1}", this.Version, this.ReleaseDate.ToShortDateString());
			ConsoleLine.Append("\tto destination: {0}", Path.GetFullPath(this.TargetPath));
			
			ConsoleLine.Append();

			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
			}

			Directory.CreateDirectory(directory);

			foreach (var package in Packages)
			{
				string source;
				string name;
				string destination;

				GetPackageInfo(package, directory, out name, out source, out destination);

				var line = ConsoleLine.Append("Downloading {0} ...", name);

				this.Lines.Add(name, line);

				var task = this.DownloadPackageAsync(destination, name, source);

				tasks.Add(task);
			}

			if (!Directory.Exists(this.DataFolder))
			{
				Directory.CreateDirectory(this.DataFolder);
			}

			foreach (var package in DataPackages)
			{
				string source;
				string name;
				string destination;

				GetPackageInfo(package, this.DataFolder, out name, out source, out destination);

				var line = ConsoleLine.Append("Downloading {0} ...", name);

				this.Lines.Add(name, line);

				var task = this.DownloadPackageAsync(destination, name, source);

				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

			if (Directory.Exists(current))
			{
				Directory.Delete(current, true);
			}

			#region Setting LISp Miner version {0} as current.

			ConsoleLine.Append("Setting LISp Miner version {0} as current.", this.Version);

			DirectoryUtil.Copy(directory, current);

			#endregion

			return ConsoleLine.GetBuffer();
		}

		private void GetPackageInfo(string package, string defaultDestination, out string name, out string url, out string destination)
		{
			if (package.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				name = Path.GetFileName(package);
				url = package;
			}
			else
			{
				name = package;
				url = string.Format("{0}/{1}", FilesPath, package);
			}

			destination = name == PCGrid ? this.TargetPath : defaultDestination;
		}

		private async Task<Task> DownloadPackageAsync(string directory, string package, string source)
		{
			using (var client = new WebClient())
			{
				var zip = String.Format("{0}\\{1}", directory, package);

				client.DownloadProgressChanged += (o, args) => this.Lines[package].Update("Downloading {0} ({1} %) ...", package, args.ProgressPercentage);

				Task task = client.DownloadFileTaskAsync(source, zip);

				await task;

				this.Lines[package].Update("Unpacking {0} ...", package);

				ZipUtil.Unzip(directory, zip);

				File.Delete(zip);

				if (package == LMCore)
				{
					// Copy LMEmpty to data
					const string LMEmpty = "LMEmpty.mdb";
					const string LMEmptyUS = "LMEmptyUS.mdb";

					File.Copy(string.Format("{0}\\DBCon\\{1}", directory, LMEmpty), string.Format("{0}\\{1}", this.DataFolder, LMEmpty));
					File.Copy(string.Format("{0}\\DBCon\\{1}", directory, LMEmptyUS), string.Format("{0}\\{1}", this.DataFolder, LMEmptyUS));
				}

				this.Lines[package].Update("Finished {0} ...", package);

				return task;
			}
		}

		private class ConsoleLine
		{
			private static readonly StringBuilder Buffer = new StringBuilder();
			private static readonly Object obj = new Object();

			public static ConsoleLine Append(string format, params object[] args)
			{
				Buffer.AppendFormat(format, args);

				return new ConsoleLine(format, args);
			}

			public static void Append()
			{
				Buffer.AppendLine();

				Console.WriteLine();
			}

			public static string GetBuffer()
			{
				return Buffer.ToString();
			}

			private readonly string _spaces;

			private int Row { get; set; }

			private ConsoleLine(string format, params object[] args)
			{
				Console.CursorVisible = false;

				this.Row = Console.CursorTop;

				for (int i = 0; i < 10; i++)
				{
					_spaces += " ";
				}

				this.Write(format, args);
			}

			private void Write(string format, params object[] args)
			{
				Console.WriteLine(format + _spaces, args);
			}

			public void Update(string format, params object[] args)
			{
				lock (obj)
				{
					int tmpRow = Console.CursorTop;
					int tmpCol = Console.CursorLeft;

					Console.SetCursorPosition(0, this.Row);

					Write(format, args);

					Console.SetCursorPosition(tmpCol, tmpRow);
				}
			}
		}
	}
}
