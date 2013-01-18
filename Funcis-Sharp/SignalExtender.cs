using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncisSharp
{
	public class SignalExtender
	{
		const string signalSetup = @".Extender.SendAll()
	(signal, name) =>
		.Extendee.Handle(""loaded"", signal, name)

.Extendee.Request()
	.Extender.SendAll()
		(signal, name) =>
			.Extendee.Handle(""loaded"", signal, name)

.Extender.Listen()
	(event, signal, name) =>
		.Extendee.Handle(event, signal, name)";

		public static void Register(Funcis funcis)
		{
			var node = funcis.CreateNode("You", new string[] { "Extendee" });
			File.WriteAllText(Path.Combine(funcis.GetWatchedPath(), "signalSetup.is"), signalSetup.Replace("\r\n", "\n"));
			node["Handle"] = new FuncEx((sig, args, cb) =>
			{
				if (args.Count < 3)
					return;
				string ev = (string)args[0];
				string signal = (string)args[1];
				string name = (string)args[2];
				try
				{
					if (ev == "loaded")
					{
						File.WriteAllText(Path.Combine(funcis.GetWatchedPath(), name), signal);
					}
					else if (ev == "removed")
					{
						File.Delete(Path.Combine(funcis.GetWatchedPath(), name));
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			});
			node["Request"] = new FuncEx((sig, args, cb) =>
			{
				cb(new Newtonsoft.Json.Linq.JArray());
			});
		}
	}
}
