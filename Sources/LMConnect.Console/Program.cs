using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LMConnect.Client;

namespace LMConnect.Console
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string command = string.Empty;
			string module = string.Empty;
			string[] parameters = args.Skip(2).ToArray();

			if (args.Length == 1)
			{
				command = args[0].ToLower();
			}
			else if (args.Length > 1)
			{
				module = args[0].ToLower();
				command = args[1].ToLower();
			}

			switch (module)
			{
				case "lm":
					LM(command, parameters);
					break;
				case "key":
					ManageDatabase(command, parameters);
					break;
                case "client":
			        RunClient(command, parameters).Wait();
                    break;
				default:
					Help();
					break;
			}

			System.Console.WriteLine();

			System.Console.WriteLine("Done.");
		}

	    private static void Help()
		{
			System.Console.WriteLine("Usage: {0} [module] [command] [arguments]", Path.GetFileName(Assembly.GetEntryAssembly().Location));
			System.Console.WriteLine();
			System.Console.WriteLine("modules:");
			System.Console.WriteLine("\tLM");
			System.Console.WriteLine("\t\tupdate [LM_LIB_PATH]");
			System.Console.WriteLine("\tKey");
			System.Console.WriteLine("\t\tcreate [nHibernate config]");
			System.Console.WriteLine("\t\tupdate [nHibernate config]");
			System.Console.WriteLine("\t\tinit [nHibernate config]");
			System.Console.WriteLine("\t\tmigrate [nHibernate configFrom] [nHibernate configTo]");
		}

		#region LM module 

		private static void LM(string command, string[] args)
		{
			switch (command)
			{
				case "update":
					Update(args.Length >= 1 ? args[0] : null);
					break;
				case "remove":
					Remove();
					break;
				default:
					Help();
					break;
			}
		}

		private static void Update(string lmLibPath)
		{
			try
			{
				var manager = new Manager(lmLibPath);

				manager.Update();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);
				System.Console.WriteLine();
				System.Console.WriteLine(ex.StackTrace);
			}
		}

		private static void Remove()
		{
			var env = new LMConnect.Environment
					{
						//LMPoolPath = String.Format(@"{0}", @"C:\LMs\"),
						LMPoolPath = String.Format(@"{0}", @"c:\LMs"),
						LMPath = String.Format("{0}/../{1}", System.AppDomain.CurrentDomain.BaseDirectory, "LISp Miner"),
					};

			foreach (var path in Directory.GetDirectories(env.LMPoolPath))
			{
				try
				{
					var directory = new DirectoryInfo(path);
					var lm = new LISpMiner.LISpMiner(directory, env);

					lm.Dispose();

					System.Console.WriteLine(lm.Id);
				}
				catch (Exception ex)
				{
					System.Console.WriteLine(String.Format("skipping {0} {1}", path, ex.Message));
				}
			}
		}

		#endregion

		#region Key module

		private static void ManageDatabase(string command, string[] args)
		{
			DatabaseManager databaseManager;

			if (args.Length > 0)
			{
				string cfg = args[0];
				
				if (!File.Exists(cfg))
				{
					cfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg);
					// normalize
					cfg = Path.GetFullPath((new Uri(cfg)).LocalPath);
				}
				
				databaseManager = new DatabaseManager(cfg);
			} 
			else
			{
				databaseManager = new DatabaseManager();
			}
			
			switch (command)
			{
				case "update":
					databaseManager.Update();
					break;
				case "create":
					databaseManager.Create();
					break;
				case "init":
					databaseManager.Init();
					break;
				case "migrate":
					if (args.Length > 1)
					{
						databaseManager.Migrate(args[1]);
					}
					else
					{
						Help();
					}
					break;
				default:
					Help();
					break;
			}
		}

		#endregion

        #region Client

        private static async Task RunClient(string command, string[] parameters)
        {
            try
            {
                var client = new LMConnect.Client.Client("http://localhost");

                Miner result = await client.GetMinerAsync("P0YF0OFlXkW2fdy9HPZg5A");

                System.Console.WriteLine("OK - {0}.", result.Id);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("NOK {0}.", ex.Message);
            }

            System.Console.ReadLine();
        }

        #endregion
    }
}
