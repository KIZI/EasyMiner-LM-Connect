using System;

namespace LMConnect.LISpMiner
{
	partial class LISpMiner
	{
		private LMSwbImporter CreateImporter()
		{
			return new LMSwbImporter(this, this.Metabase.ConnectionString, this.LMPrivatePath);
		}

		private LMSwbExporter CreateExporter()
		{
			return new LMSwbExporter(this, this.Metabase.ConnectionString, this.LMPrivatePath);
		}

		public string ExportDataDictionary(string matrix, string template = null)
		{
			matrix = string.IsNullOrEmpty(matrix) ? "Loans" : matrix;
			template = string.IsNullOrEmpty(template) ? "LMDataSource.Matrix.ARD.Template.PMML" : template;

			using (var exporter = this.CreateExporter())
			{
				exporter.NoAttributeDisctinctValues = true;
				exporter.NoEscapeSeqUnicode = true;
				exporter.MatrixName = matrix;
				exporter.Output = string.Format("{0}/results_{1}_{2:yyyyMMdd-Hmmss.fff}.xml", GetDataFolder(), "DD", DateTime.Now);
				exporter.Template = string.Format(@"{0}\Sewebar\Template\{1}", exporter.LMExecutablesPath, template);
				exporter.Execute();

				return exporter.Output;
			}
		}

		public void ImportDataDictionary(string dataDictionary)
		{
			using (LMSwbImporter importer = this.CreateImporter())
			{
				var dataDictionaryPath = string.Format(@"{0}/DataDictionary_{1:yyyyMMdd-Hmmss}.xml", GetDataFolder(), DateTime.Now);

				importer.Input = WriteToFile(dataDictionaryPath, dataDictionary);
				importer.NoCheckPrimaryKeyUnique = false;
				importer.Execute();
			}
		}
	}
}
