using Knx.Integration;
using Knx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EtsProjectTranslator
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			string file = null;
			string outputFile = null;
			var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (args.Length == 0)
			{
				do
				{
					var ofd = new OpenFileDialog();
					ofd.Filter = "Knx Project Files|*.knxproj";
					ofd.Multiselect = false;
					ofd.Title = "Choose a suitable project file";
					ofd.RestoreDirectory = false;
					if (ofd.ShowDialog() == DialogResult.OK && ofd.CheckFileExists)
					{
						file = ofd.FileName;
					}
					else
						return;
				} while (file == null);
			}
			else
			{
				file = Path.Combine(dir, args[0]);

				if (!File.Exists(file))
				{
					Console.WriteLine("Invalid filename");
					return;
				}
			}
			if (args.Length < 2)
			{

				do
				{
					SaveFileDialog sfd = new SaveFileDialog();
					sfd.Title = "Save KNX Router Configuration";
					sfd.Filter = "XML files|*.xml";
					sfd.RestoreDirectory = false;
					if (sfd.ShowDialog() == DialogResult.OK)
					{
						outputFile = sfd.FileName;
					}
					else
						return;
				} while (outputFile == null);
			}
			else
			{
				outputFile = Path.Combine(dir, args[1]);
			}
			using (Stream s = File.Open(file, FileMode.Open))
			{
				var store = ProjectStore.Load(s);
				var model = ObjectModelFactory.BuildModel(store);
				var doc = Translator.Translate(model);
				File.WriteAllText(outputFile, doc.ToString());
				Process.Start(outputFile);
			}
		}
	}
}
